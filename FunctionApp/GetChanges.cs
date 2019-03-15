using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureDigitalTwins.Backend
{
    public static class GetChanges
    {
        [FunctionName("GetChanges")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log,
            [Blob("topologychanges", Connection = "AzureStorage")]
            CloudBlobContainer blobs)
        {
            var response = await blobs.ListBlobsSegmentedAsync(null);

            // return (ActionResult)new OkObjectResult($"Hello");
            var resp = req.CreateResponse(HttpStatusCode.OK, new {
                result = "ok",
                items = response.Results
            }, "application/json");

            if (req.Headers.Contains("Origin"))
            {
                var origin = req.Headers.GetValues("Origin").FirstOrDefault();
                log.LogInformation($"Yes origin: {origin}");
                resp.Headers.Add("Access-Control-Allow-Credentials", "true");
                resp.Headers.Add("Access-Control-Allow-Origin", origin);
                resp.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                resp.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Set-Cookie");
            }
            else
            {
                log.LogInformation($"No origin");
            }

            return resp;
        }
    }
}
