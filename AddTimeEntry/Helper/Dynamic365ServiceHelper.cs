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
            try
            {
                var url = "https://orge339c78a.crm4.dynamics.com";
                var userName = "MohamedAhmed93@hosnyrent.onmicrosoft.com";
                var password = "h@2041993@H";
                var authType = "OAuth";
                var appId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
                var redirectUri = "app://58145B91-0C36-4500-8554-080854F2AC97";

                return $"Url = {url};" +
                       $"AuthType = {authType};" +
                       $"UserName = {userName};" +
                       $"Password = {password};" +
                       $"AppId = {appId};" +
                       $"RedirectUri = {redirectUri};" +
                       "LoginPrompt=Never;";
                // return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch (Exception e)
            {
                log.Error("You can set connection data in cds/App.config before running this sample. - Switching to Interactive Mode", e);
                return string.Empty;
            }
        }
    }
}

/*
 * <add name="Connect"
   connectionString="AuthType=OAuth; 
   Username=jsmith@contoso.onmicrosoft.com; 
   Url=https://contosotest.crm.dynamics.com; 
   Password=passcode;
   AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;
   RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;
   TokenCacheStorePath=d:\MyTokenCache;
   LoginPrompt=Auto"/>
   </connectionStrings>
 */