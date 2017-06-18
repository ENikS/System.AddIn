using ControlFactoryAddIn.Controls;
using Demo.AddIn;
using System.AddIn;
using System.Windows;

namespace ControlFactoryAddIn
{
    [AddIn("Info Factory")]
    public class InfoFactory : ControlFactory
    {
        public override FrameworkElement GetControl()
        {
            return new InfoControl();
        }
    }
}
