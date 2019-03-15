using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

// https://github.com/Azure-Samples/signalr-service-quickstart-serverless-chat
namespace AzureDigitalTwins.Backend
{
    public static class GetSignalRInfo
    {
        [FunctionName("GetSignalRInfo")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log,
            [SignalRConnectionInfo(HubName = "hub", ConnectionStringSetting ="AzureSignalR", UserId="{headers.x-ms-client-principal-id}")]
            SignalRConnectionInfo connectionInfo)
        {
            // return (ActionResult)new OkObjectResult($"Hello");
            var resp = req.CreateResponse(HttpStatusCode.OK, connectionInfo, "application/json");

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
