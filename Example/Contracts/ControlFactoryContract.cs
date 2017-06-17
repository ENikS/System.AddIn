using System.AddIn.Contract;
using System.AddIn.Pipeline;

namespace Demo.Contracts
{
    [AddInContract]
    public interface IControlFactoryContract : IContract
    {
        INativeHandleContract GetControl();
    }
}
