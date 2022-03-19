using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddTimeEntry.DTO;
using AddTimeEntry.Helper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Xrm.Sdk;

namespace AddTimeEntry
{
    public static class AddTimeEntry
    {
        [FunctionName("AddTimeEntry")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, 
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<TimeEntryDataVerseDto>();

            if (!DateTimeOffset.TryParse(data.StartOn, out var startOnDate))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Start Date Is Not Valid, Please Use This Format mm/dd/yyyy");
            }

            if (!DateTimeOffset.TryParse(data.EndOn, out var endOnDate))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "End Date Is Not Valid, Please Use This Format mm/dd/yyyy");
            }

            if (startOnDate > endOnDate)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "End Date Should Be Greater Than Start Date");
            }

            var serviceClient = Dynamic365ServiceHelper.ConnectToService(log);
            var result = CreateTimeEntries(serviceClient, startOnDate, endOnDate, log);
            return result ? req.CreateResponse(HttpStatusCode.OK, "Done") : req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private static bool CreateTimeEntries(IOrganizationService service, DateTimeOffset startOn, DateTimeOffset endOn, TraceWriter log)
        {
            try
            {
                for (var temp = startOn; temp <= endOn; temp = temp.AddDays(1))
                {
                    var timeEntry = new Entity("msdyn_timeentry")
                    {
                        ["msdyn_start"] = temp.DateTime,
                        ["msdyn_end"] = temp.DateTime,
                        ["msdyn_duration"] = 0,
                    };

                    var timeEntryId = service.Create(timeEntry);
                    log.Info($"Created {timeEntry.LogicalName} entity Id {timeEntryId}.");
                }

                return true;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                return false;
            }
        }
    }
}
