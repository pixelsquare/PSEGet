using System.Linq;
using System.Diagnostics;
using LateBindingHelper;
using PSEGetLib.Interfaces;

namespace PSEGetLib.Service
{
    public class AmibrokerService : IAmibrokerService
    {
        public bool IsAmibrokerInstalled()
        {
            try
            {

                Process[] localByName = Process.GetProcessesByName("broker");
                if (localByName.Any())
                {
                    return true;
                }
                else
                {
                    IOperationInvoker amiInvoker = BindingFactory.CreateAutomationBinding("Broker.Application");
                    try
                    {
                        return amiInvoker != null;
                    }
                    finally
                    {
                        if (amiInvoker != null)
                            amiInvoker.Method("Quit").Invoke();
                    }
                }                
            }
            catch
            {
                return false;
            }
        }
    }
}
