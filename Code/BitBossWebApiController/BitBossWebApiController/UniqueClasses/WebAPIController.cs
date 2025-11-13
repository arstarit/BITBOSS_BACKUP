using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitBossWebApiController
{
    public class WebAPIController
    {
        // MAIN 
        private static WebAPIController _instance = null;
        protected WebAPIController()
        {
      

        }             
        public static WebAPIController Instance()
        {
            if (_instance == null)
                _instance = new WebAPIController();
            return _instance;
        }

        // METHODS
        public object transferToEGM(int cashablecentsAmount, int restristedcentsAmount, int nonrestrictedcentsAmount)
        {
           return SASHost.Instance().transferToEGM(cashablecentsAmount,restristedcentsAmount,nonrestrictedcentsAmount);
        }

        public object cashoutToEGM(int cashablecentsAmount, int restristedcentsAmount, int nonrestrictedcentsAmount)
        {
            return SASHost.Instance().cashoutToEGM(cashablecentsAmount, restristedcentsAmount, nonrestrictedcentsAmount);

        }
        
        public object GetPhysicalEGMSettings()
        {
            return MainController.Instance().GetPhysicalEGMSettings();
        }
        public object GetInterfacingSettings()
        {
            return MainController.Instance().GetInterfacingSettings();
        }

        public object getInfoFromPhysicalEGM()
        {
            return PhysicalEGM.Instance().GetInfo();
        }
        public object getMetersFromPhysicalEGM()
        {
            return PhysicalEGM.Instance().GetMeters();
        }
        public object getPhysicalAFTTransactionHistory()
        {
            return AuditSubController.Instance().getPhysicalAFTTransactionHistory();
        }

        public object getCurrentTransfer()
        {
            return PhysicalEGM.Instance().GetCurrentTransfer();
        }

        public object getHostLiveTrace()
        {
            return AuditSubController.Instance().getHostLiveTrace();
        }

        public object getClientLiveTrace()
        {
            return AuditSubController.Instance().getClientLiveTrace();
        }

        public object getHostStatus()
        {
            return AuditSubController.Instance().getHostStatus();
        }
        
        public object Restart()
        {
            return ControlSubController.Instance().Restart();
        }

        public object APICommHealth()
        {
            return ControlSubController.Instance().APICommHealth();
        }

        public object LinksHealth()
        {
            return ControlSubController.Instance().LinksHealth();
        }

        
        public object AFTSettings(dynamic parameters)
        {
            return ControlSubController.Instance().AFTSettings(parameters);
        }

        public void testingEnqueueLP(byte lpcode)
        {

        }
        public void testingGetLastLPPayload(byte lpcode)
        {

        }
    }
}
