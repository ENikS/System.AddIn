using Demo.HostView;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Reparenting_WPF
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private IList<IDisposable> _factories = new List<IDisposable>();

        #endregion


        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(new Action(FillGrid));
        }

        #endregion


        #region Implementation

        private void FillGrid()
        {
            IEnumerable<AddInToken> tokens = AddInStore.FindAddIns(typeof(IControlFactory), Environment.CurrentDirectory);

            var root = Content;

            foreach (var token in tokens)
            {
                for (var i = 0; i < 4; i++)
                {
                    IControlFactory factory = token.Activate<IControlFactory>(AppDomain.CreateDomain(token.Name + " App Domain " + i));
                    ((Panel)Content).Children.Add(factory.GetControl());
                    _factories.Add(factory);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (FrameworkElement element in ((Panel)Content).Children)
            {
                element.RaiseEvent(new RoutedEventArgs(UnloadedEvent));
            }

            base.OnClosing(e);

            foreach(var factory in _factories)
            {
                factory.Dispose();
            }
        }

        #endregion
    }
}
