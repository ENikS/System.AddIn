using ControlFactoryAddIn.Controls;
using Demo.AddIn;
using System.AddIn;
using System.Windows;

namespace ControlFactoryAddIn
{
    [AddIn("Data Grid Factory")]
    public class DataGridFactory : ControlFactory
    {
        public override FrameworkElement GetControl()
        {
            return new DataGridControl();
        }
    }
}
