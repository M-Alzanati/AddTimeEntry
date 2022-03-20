using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace AddTime.Helper
{
    public static class Dynamic365ServiceHelper
    {
        public static IOrganizationService ConnectToService(
            ILogger log)
        {
            IOrganizationService service = null;
            try
            {
                var connectionString = GetConnectionStringFromAppConfig("Connect");    // Read connection string from environment variables
                if (string.IsNullOrEmpty(connectionString)) return null;

                var svc = new CrmServiceClient(connectionString);   // Open a connection with service
                service = svc.OrganizationWebProxyClient ?? (IOrganizationService)svc.OrganizationServiceProxy;

                if (service != null)
                {
                    var userid = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId; // Getting user_Id
                    if (userid != Guid.Empty)
                    {
                        log.LogInformation("Connection Established Successfully...");
                        log.LogInformation($"UserId: {userid}");
                    }
                }
                else
                {
                    log.LogInformation("Failed to Established Connection!!!");
                }

                return service;
            }
            catch (Exception ex)
            {
                log.LogError("Cannot create crm service client, please check your connection string", ex);
                return service;
            }
        }

        private static string GetConnectionStringFromAppConfig(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
