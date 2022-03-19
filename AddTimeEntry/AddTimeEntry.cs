using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddTimeEntry.DTO;
using AddTimeEntry.Helper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AddTimeEntry
{
    public static class AddTimeEntry
    {
        private const string MsdynTimeentry = "msdyn_timeentry";
        private const string MsdynStart = "msdyn_start";
        private const string MsdynEnd = "msdyn_end";
        private const string MsdynDuration = "msdyn_duration";
        private const string MsdynDescription = "msdyn_description";
        private const string MsdynTimeentryid = "msdyn_timeentryid";

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
            if (serviceClient == null)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Azure function can't access Dynamic365 Service");
            }

            var result = CreateTimeEntries(serviceClient, startOnDate, endOnDate, log);
            return result ? req.CreateResponse(HttpStatusCode.OK, "Done") : req.CreateResponse(HttpStatusCode.InternalServerError);
        }

        private static bool CreateTimeEntries(IOrganizationService service, DateTimeOffset startOn, DateTimeOffset endOn, TraceWriter log)
        {
            try
            {
                log.Info("Retrieving old time entries to ensure no duplicates in data.");
                var oldDates = GetOldDates(service);

                for (var temp = startOn; temp <= endOn; temp = temp.AddDays(1))
                {
                    if (oldDates.Any(r => r == temp)) continue;

                    var timeEntry = new Entity(MsdynTimeentry)
                    {
                        [MsdynStart] = temp.DateTime,
                        [MsdynEnd] = temp.DateTime,
                        [MsdynDuration] = 0,
                        [MsdynDescription] = "Test Time Entry"
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

        private static List<DateTime> GetOldDates(IOrganizationService service)
        {
            var oldEntities = service.RetrieveMultiple(new QueryExpression(MsdynTimeentry));
            var oldDates = oldEntities?.Entities?.Select(r =>
            {
                var entity = service.Retrieve(r.LogicalName, (Guid) r[MsdynTimeentryid], new ColumnSet(MsdynStart));
                return DateTime.Parse(entity[MsdynStart].ToString());
            }).ToList();
            return oldDates;
        }
    }
}
