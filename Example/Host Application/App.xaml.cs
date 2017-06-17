using Demo.HostView;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Example.Reparenting.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class HostApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AddInStore.Rebuild(Environment.CurrentDirectory);
        }
    }
}
