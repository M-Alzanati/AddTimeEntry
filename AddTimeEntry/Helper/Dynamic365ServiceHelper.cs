using System;
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
                var connectionString = GetConnectionStringFromAppConfig("Connect", log);    // Read connection string from environment variables
                if (string.IsNullOrEmpty(connectionString)) return null;

                var svc = new CrmServiceClient(connectionString);   // Open a connection with service
                service = svc.OrganizationWebProxyClient ?? (IOrganizationService)svc.OrganizationServiceProxy;

                if (service != null)
                {
                    var userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId; // Getting user_Id
                    if (userid != Guid.Empty)
                    {
                        log.Info("Connection Established Successfully...");
                        log.Info($"UserId: {userid}");
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
