using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDigitalTwins
{
    partial class Program
    {
        internal static AppSettings appSettings;

        static Random random = new Random();

        static HttpClient httpClient;
        static Clients clients;
        static SpacesSingleton spaces;
        static DevicesSingleton devices;
        static Guid rootId;

        static TypeSingleton spaceTypes;
        static TypeSingleton deviceTypes;
        static TypeSingleton sensorTypes;
        static TypeSingleton sensorDataTypes;
        static TypeSingleton spaceBlobType;
        static TypeSingleton spaceBlobSubType;

        static async Task Setup()
        {
            appSettings = new ConfigurationBuilder()
                                     .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                     .AddJsonFile("appSettings.json")
                                     .Build()
                                     .Get<AppSettings>();

            var logger = new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger("DigitalTwins");

            httpClient = new HttpClient(new LoggingHttpHandler(logger))
            {
                BaseAddress = new Uri($"{appSettings.BaseUrl}/api/v1.0/"),
            };
            var accessTokenFilename = ".accesstoken";
            var accessToken = System.IO.File.Exists(accessTokenFilename) ? System.IO.File.ReadAllText(accessTokenFilename) : null;
            if (accessToken != null)
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }

            // just a random query to check if authorized
            if (!(await httpClient.GetAsync("ontologies")).IsSuccessStatusCode)
            {
                var authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(appSettings.Authority);
                var codeResult = await authContext.AcquireDeviceCodeAsync(appSettings.Resource, appSettings.ClientId);
                Console.WriteLine(codeResult.Message);
                accessToken = (await authContext.AcquireTokenByDeviceCodeAsync(codeResult)).AccessToken;
                System.IO.File.WriteAllText(accessTokenFilename, accessToken);
                if (httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    httpClient.DefaultRequestHeaders.Remove("Authorization");
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            }

            clients = new Clients(httpClient);

            spaces = clients.SpacesClient.Get();
            devices = clients.DevicesClient.Get();

            var ids = await clients.SpacesClient.RetrieveAsync(name: "root");
            if (ids.Any())
            {
                rootId = ids.First().Id;
            }
            else
            {
                rootId = await clients.SpacesClient.CreateAsync(new SpaceCreate
                {
                    Name = "root",
                    TypeId = 59 // Tenant
                });
            }

            spaceTypes = clients.TypesClient.Get(ExtendedTypeCreateCategory.SpaceType, rootId);
            deviceTypes = clients.TypesClient.Get(ExtendedTypeCreateCategory.DeviceType, rootId);
            sensorTypes = clients.TypesClient.Get(ExtendedTypeCreateCategory.SensorType, rootId);
            sensorDataTypes = clients.TypesClient.Get(ExtendedTypeCreateCategory.SensorDataType, rootId);
            spaceBlobType = clients.TypesClient.Get(ExtendedTypeCreateCategory.SpaceBlobType, rootId);
            spaceBlobSubType = clients.TypesClient.Get(ExtendedTypeCreateCategory.SpaceBlobSubtype, rootId);
        }
    }
}
