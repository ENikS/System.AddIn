﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Interop;

namespace Hosting.AddInSideAdapters
{
    public class NativeHandleAddInAdapter : ContractBase, INativeHandleContract
    {
        #region Fields

        private HwndSource hwndSource;
        private ContractHandle observer;

        #endregion


        #region Constructors


        public NativeHandleAddInAdapter(FrameworkElement root, ContractHandle adapter, string name = null)
        {
            Debug.Assert(null != root);
            Debug.Assert(null != adapter);
            Debug.Assert(adapter.Contract is IObserverContract); 
            root.VerifyAccess();

            observer = adapter; 

            var parameters = new HwndSourceParameters(name ?? "AddInWindow")
            {
                ParentWindow = new IntPtr(-3),
                WindowStyle = 0x40000000
            };

            hwndSource = new HwndSource(parameters)
            {
                RootVisual = root,
                SizeToContent = SizeToContent.Manual
            };

            if (hwndSource.CompositionTarget != null)
                hwndSource.CompositionTarget.BackgroundColor = Colors.White;

            hwndSource.AddHook(HwndSourceHook);
            
            CommandManager.AddCanExecuteHandler(root, CanExecuteRoutedEventHandler);
            CommandManager.AddExecutedHandler(root, ExecuteRoutedEventHandler);

            // TODO: Refactoring
            Debug.WriteLine("--\t\t\t\t\t\t\t Handle: {0}\t--\tWindow: {1}", hwndSource.Handle.ToString("x8"), parameters.WindowName);
        }


        #endregion

        private IntPtr x;
        private IntPtr y;

        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // TODO: Refactoring
            var name = Enum.GetName(typeof(WindowMessage), msg);
            Debug.WriteLine("--\tTicks: {0} \t Handle: {1} \t Message: {2} \t wParam: {3} \t lParam: {4}",
                Stopwatch.GetTimestamp(), 
                hwnd.ToString("x8"), name ?? msg.ToString("x4"), wParam.ToString("x8"), lParam.ToString("x8"));

            //return IntPtr.Zero;

            switch (msg)
            {
                case 0x9000:
                    x = wParam;
                    y = lParam;
                    break;

                case 0x9001:
                    SetWindowPos(new HandleRef(null, hwndSource.Handle), new HandleRef(null, IntPtr.Zero),
                        (int)x, (int)y, (int)wParam, (int)lParam, 0x4114);
                    break;

                case 0x9002:
                    ShowWindowAsync(new HandleRef(null, hwndSource.Handle), (int)wParam);
                    break;

                case 0x9003:
                    EnableWindow(new HandleRef(null, hwndSource.Handle), IntPtr.Zero != wParam);
                    break;

                case 0x9004:
                    break;


                case 0x9005:
                    SetParent(new HandleRef(null, hwndSource.Handle), new HandleRef(null, wParam));
                    break;

                default:
                    return IntPtr.Zero;
            }

            handled = true;
            return IntPtr.Zero;
        }


        #region RoutedCommands

        public void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            bool isSerializable = (null == e.Parameter) || e.Parameter.GetType().IsSerializable;
            if (!(e.Command is RoutedCommand) || !isSerializable)
                return;

            var command = ((RoutedCommand)e.Command);

            e.CanExecute = ((IObserverContract)observer.Contract).CanExecuteRoutedCommand(
                                                            (e.Command is RoutedUICommand) ? ((RoutedUICommand)e.Command).Text
                                                                                           : null,
                                                            command.Name,
                                                            command.OwnerType.AssemblyQualifiedName,
                                                            command.GetHashCode(),
                                                            e.Parameter);
            e.Handled = true;
        }

        public void ExecuteRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            bool isSerializable = (null == e.Parameter) || e.Parameter.GetType().IsSerializable;
            if (!(e.Command is RoutedCommand) || !isSerializable)
                return;

            var command = ((RoutedCommand)e.Command);
            ((IObserverContract)observer.Contract).ExecuteRoutedCommand(
                                             (e.Command is RoutedUICommand) ? ((RoutedUICommand)e.Command).Text
                                                                            : null,
                                             command.Name,
                                             command.OwnerType.AssemblyQualifiedName,
                                             command.GetHashCode(),
                                             e.Parameter);
        }

        #endregion


        #region Implementation



        protected override void OnFinalRevoke()
        {
            if (null != hwndSource)
            {
                CommandManager.RemoveCanExecuteHandler((UIElement)hwndSource.RootVisual, CanExecuteRoutedEventHandler);
                CommandManager.RemoveExecutedHandler((UIElement)hwndSource.RootVisual, ExecuteRoutedEventHandler);
                
                if (!hwndSource.CheckAccess())
                    hwndSource.Dispatcher.Invoke(hwndSource.Dispose);
                else
                    hwndSource.Dispose();
                hwndSource = null;
            }

            if (null != observer)
            {
                observer.Dispose();
                observer = null;
            }
            base.OnFinalRevoke();
        }

        #endregion


        #region Native Methods

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool EnableWindow(HandleRef hWnd, bool enable);

        #endregion


        #region INativeHandleContract

        public IntPtr GetHandle()
        {
            return hwndSource.Handle;
        }

        #endregion

        enum WindowMessage
        {
            WM_ACTIVATE = 6,
            WM_ACTIVATEAPP = 0x1c,
            WM_AFXFIRST = 0x360,
            WM_AFXLAST = 0x37f,
            WM_APP = 0x8000,
            WM_APPCOMMAND = 0x319,
            WM_ASKCBFORMATNAME = 780,
            WM_CANCELJOURNAL = 0x4b,
            WM_CANCELMODE = 0x1f,
            WM_CAPTURECHANGED = 0x215,
            WM_CHANGECBCHAIN = 0x30d,
            WM_CHANGEUISTATE = 0x127,
            WM_CHAR = 0x102,
            WM_CHARTOITEM = 0x2f,
            WM_CHILDACTIVATE = 0x22,
            WM_CLEAR = 0x303,
            WM_CLOSE = 0x10,
            WM_COMMAND = 0x111,
            WM_COMMNOTIFY = 0x44,
            WM_COMPACTING = 0x41,
            WM_COMPAREITEM = 0x39,
            WM_CONTEXTMENU = 0x7b,
            WM_COPY = 0x301,
            WM_COPYDATA = 0x4a,
            WM_CREATE = 1,
            WM_CTLCOLOR = 0x19,
            WM_CTLCOLORBTN = 0x135,
            WM_CTLCOLORDLG = 310,
            WM_CTLCOLOREDIT = 0x133,
            WM_CTLCOLORLISTBOX = 0x134,
            WM_CTLCOLORMSGBOX = 0x132,
            WM_CTLCOLORSCROLLBAR = 0x137,
            WM_CTLCOLORSTATIC = 0x138,
            WM_CUT = 0x300,
            WM_DEADCHAR = 0x103,
            WM_DELETEITEM = 0x2d,
            WM_DESTROY = 2,
            WM_DESTROYCLIPBOARD = 0x307,
            WM_DEVICECHANGE = 0x219,
            WM_DEVMODECHANGE = 0x1b,
            WM_DISPLAYCHANGE = 0x7e,
            WM_DRAWCLIPBOARD = 0x308,
            WM_DRAWITEM = 0x2b,
            WM_DROPFILES = 0x233,
            WM_DWMCOLORIZATIONCOLORCHANGED = 800,
            WM_DWMCOMPOSITIONCHANGED = 0x31e,
            WM_DWMNCRENDERINGCHANGED = 0x31f,
            WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x326,
            WM_DWMSENDICONICTHUMBNAIL = 0x323,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x321,
            WM_ENABLE = 10,
            WM_ENDSESSION = 0x16,
            WM_ENTERIDLE = 0x121,
            WM_ENTERMENULOOP = 0x211,
            WM_ENTERSIZEMOVE = 0x231,
            WM_ERASEBKGND = 20,
            WM_EXITMENULOOP = 530,
            WM_EXITSIZEMOVE = 0x232,
            WM_FONTCHANGE = 0x1d,
            WM_GETDLGCODE = 0x87,
            WM_GETFONT = 0x31,
            WM_GETHOTKEY = 0x33,
            WM_GETICON = 0x7f,
            WM_GETMINMAXINFO = 0x24,
            WM_GETOBJECT = 0x3d,
            WM_GETTEXT = 13,
            WM_GETTEXTLENGTH = 14,
            WM_HANDHELDFIRST = 0x358,
            WM_HANDHELDLAST = 0x35f,
            WM_HELP = 0x53,
            WM_HOTKEY = 0x312,
            WM_HSCROLL = 0x114,
            WM_HSCROLLCLIPBOARD = 0x30e,
            WM_ICONERASEBKGND = 0x27,
            WM_IME_CHAR = 0x286,
            WM_IME_COMPOSITION = 0x10f,
            WM_IME_COMPOSITIONFULL = 0x284,
            WM_IME_CONTROL = 0x283,
            WM_IME_ENDCOMPOSITION = 270,
            WM_IME_KEYDOWN = 0x290,
            WM_IME_KEYLAST = 0x10f,
            WM_IME_KEYUP = 0x291,
            WM_IME_NOTIFY = 0x282,
            WM_IME_REQUEST = 0x288,
            WM_IME_SELECT = 0x285,
            WM_IME_SETCONTEXT = 0x281,
            WM_IME_STARTCOMPOSITION = 0x10d,
            WM_INITDIALOG = 0x110,
            WM_INITMENU = 0x116,
            WM_INITMENUPOPUP = 0x117,
            WM_INPUT = 0xff,
            WM_INPUTLANGCHANGE = 0x51,
            WM_INPUTLANGCHANGEREQUEST = 80,
            WM_KEYDOWN = 0x100,
            WM_KEYFIRST = 0x100,
            WM_KEYLAST = 0x108,
            WM_KEYUP = 0x101,
            WM_KILLFOCUS = 8,
            WM_LBUTTONDBLCLK = 0x203,
            WM_LBUTTONDOWN = 0x201,
            WM_LBUTTONUP = 0x202,
            WM_MBUTTONDBLCLK = 0x209,
            WM_MBUTTONDOWN = 0x207,
            WM_MBUTTONUP = 520,
            WM_MDIACTIVATE = 0x222,
            WM_MDICASCADE = 0x227,
            WM_MDICREATE = 0x220,
            WM_MDIDESTROY = 0x221,
            WM_MDIGETACTIVE = 0x229,
            WM_MDIICONARRANGE = 0x228,
            WM_MDIMAXIMIZE = 0x225,
            WM_MDINEXT = 0x224,
            WM_MDIREFRESHMENU = 0x234,
            WM_MDIRESTORE = 0x223,
            WM_MDISETMENU = 560,
            WM_MDITILE = 550,
            WM_MEASUREITEM = 0x2c,
            WM_MENUCHAR = 0x120,
            WM_MENUSELECT = 0x11f,
            WM_MOUSEACTIVATE = 0x21,
            WM_MOUSEFIRST = 0x200,
            WM_MOUSEHOVER = 0x2a1,
            WM_MOUSEHWHEEL = 0x20e,
            WM_MOUSELAST = 0x20e,
            WM_MOUSELEAVE = 0x2a3,
            WM_MOUSEMOVE = 0x200,
            WM_MOUSEQUERY = 0x9b,
            WM_MOUSEWHEEL = 0x20a,
            WM_MOVE = 3,
            WM_MOVING = 0x216,
            WM_NCACTIVATE = 0x86,
            WM_NCCALCSIZE = 0x83,
            WM_NCCREATE = 0x81,
            WM_NCDESTROY = 130,
            WM_NCHITTEST = 0x84,
            WM_NCLBUTTONDBLCLK = 0xa3,
            WM_NCLBUTTONDOWN = 0xa1,
            WM_NCLBUTTONUP = 0xa2,
            WM_NCMBUTTONDBLCLK = 0xa9,
            WM_NCMBUTTONDOWN = 0xa7,
            WM_NCMBUTTONUP = 0xa8,
            WM_NCMOUSELEAVE = 0x2a2,
            WM_NCMOUSEMOVE = 160,
            WM_NCPAINT = 0x85,
            WM_NCRBUTTONDBLCLK = 0xa6,
            WM_NCRBUTTONDOWN = 0xa4,
            WM_NCRBUTTONUP = 0xa5,
            WM_NCXBUTTONDBLCLK = 0xad,
            WM_NCXBUTTONDOWN = 0xab,
            WM_NCXBUTTONUP = 0xac,
            WM_NEXTDLGCTL = 40,
            WM_NEXTMENU = 0x213,
            WM_NOTIFY = 0x4e,
            WM_NOTIFYFORMAT = 0x55,
            WM_NULL = 0,
            WM_PAINT = 15,
            WM_PAINTCLIPBOARD = 0x309,
            WM_PAINTICON = 0x26,
            WM_PALETTECHANGED = 0x311,
            WM_PALETTEISCHANGING = 0x310,
            WM_PARENTNOTIFY = 0x210,
            WM_PASTE = 770,
            WM_PENWINFIRST = 0x380,
            WM_PENWINLAST = 0x38f,
            WM_POWER = 0x48,
            WM_POWERBROADCAST = 0x218,
            WM_PRINT = 0x317,
            WM_PRINTCLIENT = 0x318,
            WM_QUERYDRAGICON = 0x37,
            WM_QUERYENDSESSION = 0x11,
            WM_QUERYNEWPALETTE = 0x30f,
            WM_QUERYOPEN = 0x13,
            WM_QUERYUISTATE = 0x129,
            WM_QUEUESYNC = 0x23,
            WM_QUIT = 0x12,
            WM_RBUTTONDBLCLK = 0x206,
            WM_RBUTTONDOWN = 0x204,
            WM_RBUTTONUP = 0x205,
            WM_RENDERALLFORMATS = 0x306,
            WM_RENDERFORMAT = 0x305,
            WM_SETCURSOR = 0x20,
            WM_SETFOCUS = 7,
            WM_SETFONT = 0x30,
            WM_SETHOTKEY = 50,
            WM_SETICON = 0x80,
            WM_SETREDRAW = 11,
            WM_SETTEXT = 12,
            WM_SETTINGCHANGE = 0x1a,
            WM_SHOWWINDOW = 0x18,
            WM_SIZE = 5,
            WM_SIZECLIPBOARD = 0x30b,
            WM_SIZING = 0x214,
            WM_SPOOLERSTATUS = 0x2a,
            WM_STYLECHANGED = 0x7d,
            WM_STYLECHANGING = 0x7c,
            WM_SYNCPAINT = 0x88,
            WM_SYSCHAR = 0x106,
            WM_SYSCOLORCHANGE = 0x15,
            WM_SYSCOMMAND = 0x112,
            WM_SYSDEADCHAR = 0x107,
            WM_SYSKEYDOWN = 260,
            WM_SYSKEYUP = 0x105,
            WM_TABLET_ADDED = 0x2c8,
            WM_TABLET_DEFBASE = 0x2c0,
            WM_TABLET_DELETED = 0x2c9,
            WM_TABLET_FLICK = 0x2cb,
            WM_TABLET_MAXOFFSET = 0x20,
            WM_TABLET_QUERYSYSTEMGESTURESTATUS = 0x2cc,
            WM_TCARD = 0x52,
            WM_THEMECHANGED = 0x31a,
            WM_TIMECHANGE = 30,
            WM_TIMER = 0x113,
            WM_UNDO = 0x304,
            WM_UNINITMENUPOPUP = 0x125,
            WM_UPDATEUISTATE = 0x128,
            WM_USER = 0x400,
            WM_USERCHANGED = 0x54,
            WM_VKEYTOITEM = 0x2e,
            WM_VSCROLL = 0x115,
            WM_VSCROLLCLIPBOARD = 0x30a,
            WM_WINDOWPOSCHANGED = 0x47,
            WM_WINDOWPOSCHANGING = 70,
            WM_WININICHANGE = 0x1a,
            WM_WTSSESSION_CHANGE = 0x2b1,
            WM_XBUTTONDBLCLK = 0x20d,
            WM_XBUTTONDOWN = 0x20b,
            WM_XBUTTONUP = 0x20c
        }
    }
}
