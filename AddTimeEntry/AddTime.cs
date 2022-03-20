using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddTime.Helper;
using AddTimeEntry.DTO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AddTime
{
    public static class AddTime
    {
        private const string MsdynTimeentry = "msdyn_timeentry";
        private const string MsdynStart = "msdyn_start";
        private const string MsdynEnd = "msdyn_end";
        private const string MsdynDuration = "msdyn_duration";
        private const string MsdynDescription = "msdyn_description";
        private const string MsdynTimeentryid = "msdyn_timeentryid";

        /// <summary>
        /// This function is used to create time entries in dynamics365 service
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("AddTime")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var data = await req.Content.ReadAsAsync<TimeEntryDataVerseDto>();  // Serialize request payload

            if (!DateTimeOffset.TryParse(data.StartOn, out var startOnDate))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Start Date Is Not Valid, Please Use This Format mm/dd/yyyy");
            }

            if (!DateTimeOffset.TryParse(data.EndOn, out var endOnDate))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "End Date Is Not Valid, Please Use This Format mm/dd/yyyy");
            }

            if (startOnDate > endOnDate)    // End date should be greater than start date
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "End Date Should Be Greater Than Start Date");
            }

            var serviceClient = Dynamic365ServiceHelper.ConnectToService(log);  // Connect to service
            if (serviceClient == null)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, "Azure function can't access Dynamic365 Service");
            }

            var result = CreateTimeEntries(serviceClient, startOnDate, endOnDate, log); // Create time entries 
            return result == null
                ? req.CreateResponse(HttpStatusCode.InternalServerError)
                : req.CreateResponse(HttpStatusCode.OK, result);
        }

        private static IEnumerable<Guid> CreateTimeEntries(IOrganizationService service, DateTimeOffset startOn, DateTimeOffset endOn, ILogger log)
        {
            var result = new List<Guid>();  // return list of added entries

            try
            {
                log.LogInformation("Retrieving old time entries to ensure no duplicates in data.");
                var oldDates = RetrieveOldDates(service);    // Get old dates to ensure there is no duplicates

                for (var temp = startOn; temp <= endOn; temp = temp.AddDays(1))
                {
                    if (oldDates.Any(r => r == temp)) continue; // Skip the new date if it was already in time entries

                    var timeEntry = new Entity(MsdynTimeentry)
                    {
                        [MsdynStart] = temp.DateTime,
                        [MsdynEnd] = temp.DateTime,
                        [MsdynDuration] = 0,
                        [MsdynDescription] = "Test Time Entry"
                    };

                    var timeEntryId = service.Create(timeEntry);    // Create new date in time entries
                    result.Add(timeEntryId);
                    log.LogInformation($"Created {timeEntry.LogicalName} entity_Id {timeEntryId}.");
                }

                return result;
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
                return null;
            }
        }

        private static List<DateTime> RetrieveOldDates(IOrganizationService service)
        {
            var oldEntities = service.RetrieveMultiple(new QueryExpression(MsdynTimeentry));    // Getting old entities from service
           
            var oldDates = oldEntities?.Entities?.Select(r =>
            {
                var entity = service.Retrieve(r.LogicalName, (Guid) r[MsdynTimeentryid], new ColumnSet(MsdynStart));    // Get start date because start_date == end_date
                return DateTime.Parse(entity[MsdynStart].ToString());
            }).ToList();

            return oldDates;
        }
    }
}
