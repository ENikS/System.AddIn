using ControlFactoryAddIn.Controls;
using Demo.AddIn;
using System.AddIn;
using System.Windows;

namespace ControlFactoryAddIn
{
    [AddIn("Cube Animation Factory")]
    public class CubeAnimationFactory : ControlFactory
    {
        public override FrameworkElement GetControl()
        {
            return new CubeControl();
        }
    }
}
