using ControlFactoryAddIn.Controls;
using Demo.AddIn;
using System.AddIn;
using System.Windows;

namespace ControlFactoryAddIn
{
    [AddIn("Web Browser Factory")]
    public class WebBrowserFactory : ControlFactory
    {
        public override FrameworkElement GetControl()
        {
            return new WebBrowserControl();
        }
    }
}
