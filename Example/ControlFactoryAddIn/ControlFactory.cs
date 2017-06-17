using AddIn.Controls;
using Demo.AddInView;
using System;
using System.AddIn;
using System.AddIn.Pipeline;
using System.Threading;
using System.Windows;

namespace Demo.AddIn
{
    [AddIn("Control Factory", Description = "Demo implementation of control factory")]
    [QualificationData("Isolation", "NewAppDomain")]
    public class ControlFactory : IControlFactory
    {
        #region Fields

        private readonly ManualResetEvent _ready = new ManualResetEvent(false);

        #endregion


        #region Constructor

        public ControlFactory()
        {
            if (null != Application.Current)
                return;

            Thread thread = new Thread(InitializeAppDomain);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AddIn WPF Thread";
            thread.Start();

            _ready.WaitOne();
        }

        #endregion


        #region Implementation

        private void InitializeAppDomain()
        {
            new Application();
            Application.Current.Dispatcher.BeginInvoke(new Action(() => _ready.Set()));
            Application.Current.Run();
        }
        
        #endregion


        #region IControlFactory

        public FrameworkElement GetControl()
        {
            return new CubeControl();
        }

        #endregion


        #region IDisposable

        public void Dispose()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(Application.Current.Shutdown));
        }

        #endregion
    }
}
