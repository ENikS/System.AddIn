using Demo.HostView;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Example.Reparenting.WPF
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

            IEnumerable<AddInToken> tokens = AddInStore.FindAddIns(typeof(IControlFactory), Environment.CurrentDirectory);

            Task.Factory.StartNew(() => tokens.AsParallel().ForAll(AddChildToRoot));
        }

        #endregion


        #region Implementation

        private void AddChildToRoot(AddInToken token)
        {
            IControlFactory factory = token.Activate<IControlFactory>(AppDomain.CreateDomain(token.Name + " App Domain"));
            FrameworkElement control = factory.GetControl();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ((Panel)Content).Children.Add(control);
                _factories.Add(factory);
            }));
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
