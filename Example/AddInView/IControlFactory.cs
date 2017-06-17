using System;
using System.AddIn.Pipeline;
using System.Windows;

namespace Demo.AddInView
{
    [AddInBase]
    public interface IControlFactory : IDisposable
    {
        FrameworkElement GetControl();
    }
}
