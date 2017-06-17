using System;
using System.AddIn.Pipeline;
using System.Windows;
using Demo.Contracts;
using Demo.HostView;

namespace Demo.HostSideAdapters
{
    [HostAdapter]
    public class ControlFactoryContractToViewAdapter : IControlFactory
    {

        IControlFactoryContract _contract;
        ContractHandle _handle;

        public ControlFactoryContractToViewAdapter(IControlFactoryContract contract)
        {
            _contract = contract;
            _handle = new ContractHandle(_contract);
        }

        public FrameworkElement GetControl()
        {
            var contract = _contract.GetControl();
            var handle = new ContractHandle(contract);

            var element = FrameworkElementAdapters.ContractToViewAdapter(contract);
            element.Unloaded += (s, e) => handle.Dispose();

            return element;
        }

        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}
