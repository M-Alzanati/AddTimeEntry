using System;
using System.Configuration;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace AddTimeEntry.Helper
{
    public static class Dynamic365ServiceHelper
    {
        public static IOrganizationService ConnectToService(
            TraceWriter log)
        {
            IOrganizationService service = null;
            try
            {
                var connectionString = GetConnectionStringFromAppConfig("Connect", log);
                if (string.IsNullOrEmpty(connectionString)) return null;

                var svc = new CrmServiceClient(connectionString);
                service = svc.OrganizationWebProxyClient ?? (IOrganizationService)svc.OrganizationServiceProxy;

                if (service != null)
                {
                    var userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
                    if (userid != Guid.Empty)
                    {
                        log.Info("Connection Established Successfully...");
                        log.Info($" UserId: {userid}");
                    }
                }
                else
                {
                    log.Info("Failed to Established Connection!!!");
                }

                return service;
            }
            catch (Exception ex)
            {
                log.Error("Cannot create crm service client, please check your connection string", ex);
                return service;
            }
        }

        private static string GetConnectionStringFromAppConfig(string name, TraceWriter log)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}

/*
 * <add name="Connect"
   connectionString="AuthType=OAuth; 
   Username=MohamedAhmed93@hosnyrent.onmicrosoft.com; 
   Url=https://orge339c78a.crm4.dynamics.com; 
   Password=h@2041993@H;
   AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;
   RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;
   LoginPrompt=Never"/>
   </connectionStrings>
 */