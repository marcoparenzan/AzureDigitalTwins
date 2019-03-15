using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static System.Console;

namespace AzureDigitalTwins
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            await Setup();

            //
            // setup: Build your spaces graph with Graph Explorer
            //

            // await AddSpaces();

            //await AddIoTHubResource();

            //await UpdateIoTHubResource();

            // last step in the pipeline demo
            //await AddEndpoints();

            //await SetTemperatureSensorValueUDF();

            // last step in creation as IoTHub provisioning can be long
            // await AddBathroomDevicesAndSensors();

            //
            // demo pipeline
            //

            //Task.Run(() => {
            //    WatchData();
            //});

            //await GenerateFakeSensorData();

            //
            // advanced
            //

            await NavigateTree();

            //await UpdateIoTHubResource();

            // await AddBlob(clients, rootId, house01);
        }

        private static async Task AddEndpoints()
        {
            var ids = await clients.EndpointsClient.RetrieveAsync();
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    await clients.EndpointsClient.DeleteAsync(id.Id);
                }
            }

            var topologyChange2Args = new EndpointCreate
            {
                EventTypes = new List<Anonymous2>() { Anonymous2.TopologyOperation },
                ConnectionString = appSettings.ServiceBusConnectionString,
                Type = EndpointCreateType.ServiceBus,
                Path = "topologychanges"
            };
            var topology2 = await clients.EndpointsClient.CreateAsync(topologyChange2Args);

            var topologyChangeArgs = new EndpointCreate
            {
                EventTypes = new List<Anonymous2>() { Anonymous2.TopologyOperation },
                ConnectionString = appSettings.EventGridKey,
                Type = EndpointCreateType.EventGrid,
                Path = appSettings.EventGridUri
            };
            var topology = await clients.EndpointsClient.CreateAsync(topologyChangeArgs);

            var sensorChangeArgs = new EndpointCreate
            {
                EventTypes = new List<Anonymous2>() { Anonymous2.SensorChange },
                ConnectionString = appSettings.EventGridKey,
                Type = EndpointCreateType.EventGrid,
                Path = appSettings.EventGridUri
            };
            var sensorId = await clients.EndpointsClient.CreateAsync(sensorChangeArgs);
        }

        private static async Task WatchData()
        {
            while (true)
            {
                var sss = await clients.SensorsClient.RetrieveAsync(dataTypes: "Temperature", includes: Includes17.Value);
                foreach (var s in sss)
                {
                    WriteLine($"Read {s.DeviceId}-{s.Port}={s.Value.Value}");
                }

                await Task.Delay(1000);
            }
        }

        private static async Task SetTemperatureSensorValueUDF()
        {
            var ids = await clients.MatchersClient.RetrieveAsync(names: "SetTemperatureSensorValue");
            if (ids.Any())
                await clients.MatchersClient.DeleteAsync(ids.First().Id);
            // define the matcher
            var handleTemperatureMatchCreate = new MatcherCreate
            {
                SpaceId = rootId,
                FriendlyName = "SetTemperatureSensorValue",
                Name = "SetTemperatureSensorValue",
                Description = "SetTemperatureSensorValue",
                Conditions = new List<ConditionCreate>()
            };
            // create the matcher
            handleTemperatureMatchCreate.Conditions.Add(
                new ConditionCreate
                {
                    Path = "$.dataType",
                    Comparison = ConditionCreateComparison.Equals,
                    Value = "\"Temperature\"",
                    Target = ConditionCreateTarget.Sensor
                });
            var handleTemperatureMatcherId =
                await clients.MatchersClient.CreateAsync(handleTemperatureMatchCreate);

            var fids = await clients.UserDefinedFunctionsClient.RetrieveAsync(names: "SetTemperatureSensorValue");
            if (fids.Any())
                await clients.UserDefinedFunctionsClient.DeleteAsync(fids.First().Id);

            var handleTemperatureUDFId = await CreateUserDefinedFunction(
                new CreateUserDefinedFunctionArgs
                {
                    SpaceId = rootId,
                    Name = "SetTemperatureSensorValue",
                    Description = "SetTemperatureSensorValue",
                    Matchers = new Guid[] { handleTemperatureMatcherId }
                },
                await System.IO.File.ReadAllTextAsync("SetTemperatureSensorValue.js")
            );

            // UDF descriptor
            //var handleTemperatureUDFCreate = new UserDefinedFunctionCreate
            //{
            //    SpaceId = rootId,
            //    Name = "SetTemperatureSensorValue",
            //    FriendlyName = "SetTemperatureSensorValue",
            //    Description = "SetTemperatureSensorValue",
            //    Matchers = new List<Guid>()
            //};
            //handleTemperatureUDFCreate.Matchers.Add(handleTemperatureMatcherId);

            //var handleTemperatureUDFId =
            //    await clients.UserDefinedFunctionsClient.CreateAsync(
            //        handleTemperatureUDFCreate.ToJson(),
            //        new FileParameter(System.IO.File.OpenRead("SetTemperatureSensorValue.js"), "SetTemperatureSensorValue.js", "text/javascript")
            //    );

            var root = await clients.SpacesClient.RetrieveByIdAsync(rootId, includes: Includes21.FullPath);

            var rids = await clients.RoleAssignmentsClient.RetrieveAsync(root.SpacePaths.First());
            foreach (var rid in rids)
                await clients.RoleAssignmentsClient.DeleteAsync(rid.Id);

            var roleId = await clients.RoleAssignmentsClient.CreateAsync(new RoleAssignmentCreate
            {
                RoleId = Roles.SpaceAdministrator,
                ObjectIdType = RoleAssignmentCreateObjectIdType.UserDefinedFunctionId,
                ObjectId = handleTemperatureUDFId.ToString(),
                Path = root.SpacePaths.First()
            });

            var sss = await clients.SensorsClient.RetrieveAsync(dataTypes: "Temperature");
            foreach (var s in sss)
            {
                var result = await clients.MatchersClient.EvaluateAsync(handleTemperatureMatcherId, s.Id, true);
                var rrrr = await clients.SensorsClient.MatchersAsync(s.Id, includes: Includes19.UserDefinedFunctions);
            }
        }

        private static async Task AddBlob(Guid rootId, ICollection<SpaceRetrieveWithChildren> house01)
        {
            var content = await clients.SpacesClient.RetrieveBlobMetadataAsync(spaceId: house01.First().Id, includes: Includes27.ContentInfo);

            if (content.Any())
                await clients.SpacesClient.DeleteBlobAsync(content.First().Id);

            var x = await clients.TypesClient.RetrieveAsync(categories: "SpaceBlobType", spaceId: rootId);
            var y = await clients.TypesClient.RetrieveAsync(categories: "SpaceBlobSubtype", spaceId: rootId);

            var imageStream = new System.IO.MemoryStream(System.IO.File.ReadAllBytes("image.jpg"));
            var metadataCreate = new BlobMetadataCreate
            {
                ParentId = house01.First().Id,
                Name = "image.jpg",
                Type = "Map",
                Subtype = "GenericMap",
                Sharing = BlobMetadataCreateSharing.None,
                Description = "Front"
            };
            await clients.SpacesClient.CreateBlobAsync(metadataCreate.ToJson(),
                new FileParameter(imageStream, "image.jpg", "image/jpeg"));
        }

        private static async Task NavigateTree()
        {
            var tenants = await clients.SpacesClient.RetrieveAsync(types: "Tenant", name: "Customer Domotics");

            var bathrooms = await clients.SpacesClient.RetrieveAsync(types: "Room", subtypes: "Bathroom");

            var house01 = await clients.SpacesClient.RetrieveAsync(name: "House01", includes: Includes20.ChildSpaces, minLevel: 1, maxLevel: 6);

            var bathrooms2 = await clients.SpacesClient.RetrieveAsync(types: "Room", subtypes: "Bathroom", spaceId: house01.First().Id, traverse: Traverse8.Down);
        }

        private static async Task AddIoTHubResource()
        {
            var args = new SpaceResourceCreate
            {
                Type = SpaceResourceCreateType.IotHub,
                SpaceId = rootId
            };
            await clients.ResourcesClient.CreateAsync(args);
        }

        private static async Task UpdateIoTHubResource()
        {
            var iotHubs = await clients.ResourcesClient.RetrieveAsync(Type2.IotHub);
            var iotHub = await clients.ResourcesClient.RetrieveByIdAsync(iotHubs.First().Id);

            //await clients.ResourcesClient.DeleteAsync(iotHub.Id);

            iotHub.Status = SpaceResourceRetrieveStatus.Running;

            //await clients.ResourcesClient.UpdateAsync(iotHubs.First().Id, new SpaceResourceUpdate {
            //    Status = SpaceResourceUpdateStatus.Running
            //});
        }

        private static async Task AddSpaces()
        {
            var customersProjectId = await clients.SpacesClient.CreateAsync(new SpaceCreate {
                ParentSpaceId = rootId,
                Name = "CustomerProjects"
            });

            // HOUSE01

            var house01Id = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = customersProjectId,
                Name = "House01",
                Type = "Venue"
            });

            var house01BasementId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = house01Id,
                Name = "Basement",
                Type = "Floor"
            });

            var house01BasementBathId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = house01BasementId,
                Name = "Bath",
                Type = "Room",
                Subtype = "Bathroom"
            });

            var house01GroundFloorId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = house01Id,
                Name = "GroundFloor",
                Type = "Floor"
            });

            var house01GroundFloorBathId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = house01GroundFloorId,
                Name = "Bath",
                Type = "Room",
                Subtype = "Bathroom"
            });

            var house01GroundFloorKitchenId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = house01GroundFloorId,
                Name = "Kitchen",
                Type = "Room"
            });

            // OFFICE02

            var office02Id = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = customersProjectId,
                Name = "Office02",
                Type = "Venue"
            });

            var office02BathId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = office02Id,
                Name = "Bath",
                Type = "Room",
                Subtype = "Bathroom"
            });

            var office02OfficeId = await clients.SpacesClient.CreateAsync(new SpaceCreate
            {
                ParentSpaceId = office02Id,
                Name = "Office",
                Type = "Room",
                Subtype = "OfficeRoom"
            });
        }

        private static async Task AddBathroomDevicesAndSensors()
        {
            var bathrooms = await clients.SpacesClient.RetrieveAsync(types: "Room", subtypes: "Bathroom", includes: Includes20.FullPath);
            //var house01 = await clients.SpacesClient.RetrieveAsync(name: "House01", includes: Includes20.ChildSpaces, minLevel: 1, maxLevel: 6);
            //var bathrooms2 = await clients.SpacesClient.RetrieveAsync(types: "Room", subtypes: "Bathroom", spaceId: house01.First().Id, traverse: Traverse8.Down);

            foreach (var bathroom in bathrooms)
            {
                var deviceHardwareId = Guid.NewGuid().ToString();
                var deviceCreate = new DeviceCreate
                {
                    SpaceId = bathroom.Id,
                    FriendlyName = $"{bathroom.Name}-Controller-{deviceHardwareId}",
                    Name = $"{bathroom.Name}-Controller-{deviceHardwareId}",
                    HardwareId = $"{deviceHardwareId}",
                    CreateIoTHubDevice = true,
                    TypeId = await deviceTypes.IdOf("BathroomController")
                };
                var deviceId = await clients.DevicesClient.CreateAsync(deviceCreate);

                var temperatureHardwareId = Guid.NewGuid().ToString();
                var temperatureSensorCreate = new SensorCreate
                {
                    Port = "temp",
                    DeviceId = deviceId,
                    HardwareId = $"{temperatureHardwareId}",
                    DataTypeId = await sensorTypes.IdOf("Temperature")
                };
                var temperatureSensorId = await clients.SensorsClient.CreateAsync(temperatureSensorCreate);

                var humidityHardwareId = Guid.NewGuid().ToString();
                var humiditySensorCreate = new SensorCreate
                {
                    Port = "hum",
                    DeviceId = deviceId,
                    HardwareId = $"{humidityHardwareId}",
                    DataTypeId = await sensorTypes.IdOf("Humidity")
                };
                var humiditySensorId = await clients.SensorsClient.CreateAsync(humiditySensorCreate);
            }
        }

        private static async Task GenerateFakeSensorData()
        {
            var deviceInfos = new List<(Guid DeviceId, DeviceClient DeviceClient)>();
            var devices = await clients.DevicesClient.RetrieveAsync();
            foreach (var dd in devices)
            {
                var d = await clients.DevicesClient.RetrieveByIdAsync(dd.Id, includes: Includes2.ConnectionString);
                deviceInfos.Add((d.Id, Microsoft.Azure.Devices.Client.DeviceClient.CreateFromConnectionString(d.ConnectionString)));
            }

            while (true)
            {
                foreach (var deviceInfo in deviceInfos)
                {
                    var sensors = await clients.SensorsClient.RetrieveAsync(deviceIds: deviceInfo.DeviceId.ToString());

                    foreach (var sensor in sensors)
                    {
                        int randomValue = default;
                        switch (sensor.Port)
                        {
                            case "temp":
                                randomValue = random.Next(15,30);
                                break;
                            case "hum":
                                randomValue = random.Next(40, 100);
                                break;
                        }
                        var creationTimeUtc = DateTime.UtcNow.ToString("o", Thread.CurrentThread.CurrentCulture);
                        var bytes = Encoding.UTF8.GetBytes($"{randomValue}");
                        var eventMessage = new Message(bytes);
                        eventMessage.Properties.Add("DigitalTwins-Telemetry", "1.0");
                        eventMessage.Properties.Add("DigitalTwins-SensorHardwareId", $"{sensor.HardwareId}");
                        eventMessage.Properties.Add("CreationTimeUtc", creationTimeUtc);
                        eventMessage.Properties.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
                        eventMessage.Properties.Add("random-value", $"{randomValue}");

                        WriteLine($"{creationTimeUtc}>{deviceInfo.DeviceId}-{sensor.Port}: {randomValue}");

                        await deviceInfo.DeviceClient.SendEventAsync(eventMessage);
                    }
                }

                await Task.Delay(10000);
            }
        }

        static string OneOf(params string[] args) => args[random.Next(0, args.Length)];

        private static async Task AddRandomForest()
        {
            var regionId = await spaces.IdOf("sea", await spaceTypes.IdOf("Region"));

            for (var i = 0; i < random.Next(5, 10); i++)
            {
                var placeId = await clients.SpacesClient.CreateAsync(new SpaceCreate
                {
                    Name = $"PLACE{i}",
                    TypeId = await spaceTypes.IdOf(OneOf("Restaurant", "Bar")),
                    ParentSpaceId = regionId
                });

                for (var j = 0; j < random.Next(2, 6); j++)
                {
                    var deviceId = await clients.DevicesClient.CreateAsync(new DeviceCreate
                    {
                        Name = $"PLACE{i}DEVICE{j}",
                        HardwareId = $"PLACE{i}DEVICE{j}",
                        TypeId = await deviceTypes.IdOf(OneOf("CookingBlock", "Modular")),
                        SpaceId = placeId
                    });

                    for (var k = 0; k < random.Next(2, 6); k++)
                    {
                        var sensorId = await clients.SensorsClient.CreateAsync(new SensorCreate
                        {
                            Port = $"sensor{k}",
                            HardwareId = $"PLACE{i}DEVICE{j}SENSOR{k}",
                            TypeId = await sensorTypes.IdOf(OneOf("GrillTemperature", "PastaCookerTemperature")),
                            DataTypeId = await sensorDataTypes.IdOf("Temperature"),
                            DeviceId = deviceId
                        });
                    }
                }
            }
        }

        private static async Task DeleteAllSpaces()
        {
            var rootSpacesList = await clients.SpacesClient.RetrieveAsync(maxLevel: 1);
            foreach (var space in rootSpacesList)
            {
                await DeleteSpace(clients, space.Id);
                WriteLine($"[{space.Name}]");
            }
        }

        private static async Task DeleteSpace(Clients clients, Guid spaceId)
        {
            var space = await clients.SpacesClient.RetrieveByIdAsync(spaceId, Includes21.ChildSpaces);

            foreach (var childSpace in space.Children)
            {
                await DeleteSpace(clients, childSpace.Id);
            }

            var matchersList = await clients.MatchersClient.RetrieveAsync(spaceId: spaceId);
            foreach (var matcher in matchersList)
            {
                await clients.MatchersClient.DeleteAsync(matcher.Id);
                WriteLine($"Deleted matcher {matcher.Id}");
            }

            var udfsList = await clients.UserDefinedFunctionsClient.RetrieveAsync(spaceId: spaceId);
            foreach (var udfs in udfsList)
            {
                await clients.UserDefinedFunctionsClient.DeleteAsync(udfs.Id);
                WriteLine($"Deleted udfsList {udfs.Id}");
            }

            var resourcesList = await clients.ResourcesClient.RetrieveAsync(spaceId: spaceId);
            foreach (var resource in resourcesList)
            {
                await clients.ResourcesClient.DeleteAsync(resource.Id);
                WriteLine($"Deleted resource {resource.Id}");
            }

            var devicesList = await clients.DevicesClient.RetrieveAsync(spaceId: spaceId);
            foreach (var device in devicesList)
            {
                await clients.DevicesClient.DeleteAsync(device.Id);
                WriteLine($"Deleted device {device.Id}");
            }

            await clients.SpacesClient.DeleteAsync(spaceId);
        }
    }
}