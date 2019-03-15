using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace AzureDigitalTwins
{
    partial class Program
    {
        private static async Task Browse()
        {
            var usersList = await clients.UsersClient.RetrieveAsync();
            foreach (var user in usersList)
            {
                WriteLine($"[{user.FullName}]");
            }

            var rootSpacesList = await clients.SpacesClient.RetrieveAsync(maxLevel: 1);
            foreach (var space in rootSpacesList)
            {
                WriteLine($"[{space.Name}]");
                await Browse(space, 1);
            }
        }

        private static async Task Browse(SpaceRetrieveWithChildren parentSpace, int baseLevel)
        {
            if (parentSpace.SpacePaths != null)
            {
                var spacePath = "";
                spacePath = string.Join("/", parentSpace.SpacePaths.ToArray());
                var roleAssignmentsList = await clients.RoleAssignmentsClient.RetrieveAsync(spacePath);
                foreach (var roleAssignment in roleAssignmentsList)
                {
                    WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}{roleAssignment.RoleId}");
                }
            }

            var matchersList = await clients.MatchersClient.RetrieveAsync(spaceId: parentSpace.Id);
            foreach (var matcher in matchersList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}[{matcher.Name}]");
            }

            var udfsList = await clients.UserDefinedFunctionsClient.RetrieveAsync(spaceId: parentSpace.Id);
            foreach (var udfs in udfsList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}[{udfs.Name}]");
            }

            var resourcesList = await clients.ResourcesClient.RetrieveAsync(spaceId: parentSpace.Id);
            foreach (var resource in resourcesList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}{resource.FullName}");
            }

            var devicesList = await clients.DevicesClient.RetrieveAsync(spaceId: parentSpace.Id);
            foreach (var device in devicesList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}{device.Name}");
                await Browse(device, baseLevel + 1);
            }
            var spacesList = await clients.SpacesClient.RetrieveAsync(minLevel: baseLevel + 1, maxLevel: baseLevel + 1);
            foreach (var space in spacesList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}[{space.Name}]");
                await Browse(space, baseLevel + 1);
            }
        }

        private static async Task Browse(DeviceRetrieve device, int baseLevel)
        {
            WriteLine($"{string.Empty.PadRight(baseLevel + 1, ' ')}{device.ConnectionString}");
            var sensorsList = await clients.SensorsClient.RetrieveAsync(deviceIds: device.Id.ToString());
            foreach (var sensor in sensorsList)
            {
                WriteLine($"{string.Empty.PadRight(baseLevel, ' ')}{sensor.HardwareId}");
            }
        }
    }
}
