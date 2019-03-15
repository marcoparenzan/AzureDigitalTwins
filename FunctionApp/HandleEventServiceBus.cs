using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace AzureDigitalTwins.Backend
{
    public static class HandleEventServiceBus
    {
        [FunctionName("HandleEventServiceBus")]
        public static async Task Run(
            [ServiceBusTrigger("topologychanges", "function", Connection= "AzureServiceBus")] string ev, 
            ILogger log,
             [SignalR(HubName = "hub", ConnectionStringSetting = "AzureSignalR")]
             IAsyncCollector<SignalRMessage> signalRMessages,
             [Blob("topologychanges", Connection = "AzureStorage")]
             CloudBlobContainer blobs
        )
        {
            log.LogInformation($"{ev}");

            var id = Guid.NewGuid().ToString();
            var name = $"{id}.json";
            var blob = blobs.GetBlockBlobReference(name);
            await blob.UploadTextAsync(ev);

            await signalRMessages.AddAsync(
               new SignalRMessage
               {
                   Target = "topologychanges",
                   Arguments = new object[] { new {
                       Name = name,
                       LastModified = blob.Properties.LastModified
                   } }
               });
        }
    }
}

