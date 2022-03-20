using System.Linq;
using System.Threading.Tasks;
using AddTimeEntry.DTO;
using NUnit.Framework;

namespace AddTimeTests
{
    public class AddTimeEntryTests : TestBase
    {
        [Test]
        public async Task Is_Service_Working()
        {
            var timeEntry = new TimeEntryDataVerseDto
            {
                StartOn = "02/02/2022",
                EndOn = "02/02/2022"
            };

            var result = await AddTime.AddTime.Run(HttpPostRequestSetup(timeEntry), Logger);
            Assert.AreEqual(result.IsSuccessStatusCode, true);
        }

        [Test]
        public async Task Add_5_Time_Entries()
        {
            var timeEntry = new TimeEntryDataVerseDto
            {
                StartOn = "02/02/2020",
                EndOn = "02/06/2020"
            };

            var response = await AddTime.AddTime.Run(HttpPostRequestSetup(timeEntry), Logger);
            Assert.AreEqual(response.IsSuccessStatusCode, true);

            var result = await DeserializeMode<object>(response);
            Assert.AreEqual(5, result.Count());
        }
    }
}