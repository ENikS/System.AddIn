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

            Task.Factory.StartNew(Initialize);
        }

        #endregion


        #region Implementation

        /// <summary>
        /// Create AppDomains and controls in parallel
        /// </summary>
        private void Initialize()
        {
            var addIns = AddInStore.FindAddIns(typeof(IControlFactory), Environment.CurrentDirectory)
                                   .AsParallel()
                                   .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                   .WithMergeOptions(ParallelMergeOptions.AutoBuffered)
                                   .Select(GetControlFromFactory)
                                   .ToArray();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var tuple in addIns)
                {
                    _factories.Add(tuple.Item1);
                    ((Panel)Content).Children.Add(tuple.Item2);
                }
            }));

        }

        private Tuple<IDisposable, FrameworkElement> GetControlFromFactory(AddInToken token)
        {
            IControlFactory factory = token.Activate<IControlFactory>(AppDomain.CreateDomain(token.Name + " App Domain"));

            return new Tuple<IDisposable, FrameworkElement>(factory, factory.GetControl());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (FrameworkElement element in ((Panel)Content).Children)
            {
                element.RaiseEvent(new RoutedEventArgs(UnloadedEvent));
            }

            base.OnClosing(e);

            _factories.AsParallel().ForAll((factory) => factory.Dispose());
        }

        #endregion
    }
}
