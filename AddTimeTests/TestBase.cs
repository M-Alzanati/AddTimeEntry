using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AddTimeTests
{
    public abstract class TestBase
    {
        protected ILogger Logger;

        protected TestBase()
        { 
            Logger = Mock.Of<ILogger>();
        }

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("Connect", "AuthType=OAuth; Username=MohamedAhmed93@hosnyrent.onmicrosoft.com; Url=https://orge339c78a.crm4.dynamics.com;Password=h@2041993@H;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97; LoginPrompt=Never");
        }

        protected HttpRequestMessage HttpPostRequestSetup(object body)
        {
            var configuration = new HttpConfiguration();
            var reqMessage = new HttpRequestMessage();
            var bodyContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.ASCII, "application/json");
            reqMessage.Content = bodyContent;
            reqMessage.Method = HttpMethod.Post;
            reqMessage.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            return reqMessage;
        }

        protected async Task<IEnumerable<TEntity>> DeserializeMode<TEntity>(HttpResponseMessage message)
        {
            var result = new List<TEntity>();
            var content = await message.Content.ReadAsAsync<IEnumerable<TEntity>>();
            return content;
        }
    }
}
