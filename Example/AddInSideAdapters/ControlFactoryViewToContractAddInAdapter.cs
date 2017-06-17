using Demo.Contracts;
using System;
using System.AddIn.Contract;
using System.AddIn.Pipeline;
using System.Windows;

namespace Demo.AddInSideAdapters
{
    [AddInAdapter]
    public class ControlFactoryViewToContractAddInAdapter : ContractBase, IControlFactoryContract
    {
        #region Fields
        
        private AddInView.IControlFactory _factory;

        #endregion


        #region Constructor

        public ControlFactoryViewToContractAddInAdapter(AddInView.IControlFactory factory)
        {
            _factory = factory;
        }

        #endregion


        #region IControlFactoryContract

        public INativeHandleContract GetControl()
        {
            INativeHandleContract value = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var element = _factory.GetControl();
                value = FrameworkElementAdapters.ViewToContractAdapter(element);
            }));

            return value;
        }

        #endregion


        #region ContractBase

        protected override void OnFinalRevoke()
        {
            base.OnFinalRevoke();

            _factory.Dispose();
        }
        
        #endregion
    }
}
