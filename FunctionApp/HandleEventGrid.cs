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
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace AzureDigitalTwins.Backend
{
    public static class HandleEventGrid
    {
        [FunctionName("HandleEventGrid")]
        public static async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent, 
            ILogger log,
             [SignalR(HubName = "hub", ConnectionStringSetting = "AzureSignalR")]
             IAsyncCollector<SignalRMessage> signalRMessages,
             [Blob("topologychanges", Connection = "AzureStorage")]
             CloudBlobContainer blobs
        )
        {
            log.LogInformation($"{eventGridEvent.EventType}=>{eventGridEvent.Id}");
            log.LogInformation($"{eventGridEvent}");

            var blob = blobs.GetBlockBlobReference(eventGridEvent.Id);
            await blob.UploadTextAsync(JsonConvert.SerializeObject(eventGridEvent));

            await signalRMessages.AddAsync(
               new SignalRMessage
               {
                   Target = "topologychanges",
                   Arguments = new object[] { eventGridEvent }
               });
        }
    }
}

