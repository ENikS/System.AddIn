using System;
using System.Windows;

namespace Demo.HostView
{
    public interface IControlFactory : IDisposable
    {
        FrameworkElement GetControl();
    }
}
