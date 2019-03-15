using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDigitalTwins
{
    public static class ApiExtension
    {
        public static bool IsEmpty(this string that)
        {
            if (that == null) return true;
            if (string.IsNullOrWhiteSpace(that)) return true;
            return false;
        }

        #region TypesClient

        private static Dictionary<ExtendedTypeCreateCategory, TypeSingleton> typeSingletons = new Dictionary<ExtendedTypeCreateCategory, TypeSingleton>();

        public static TypeSingleton Get(this TypesClient that, ExtendedTypeCreateCategory category, Guid spaceId)
        {
            if (!typeSingletons.ContainsKey(category))
            {
                typeSingletons.Add(category, new TypeSingleton(that, category, spaceId));
            }
            return typeSingletons[category];
        }

        #endregion

        #region SpacesClient

        private static SpacesSingleton spacesSingletons;

        public static SpacesSingleton Get(this SpacesClient that)
        {
            if (spacesSingletons == null)
            {
                spacesSingletons = new SpacesSingleton(that);
            }
            return spacesSingletons;
        }
        
        #endregion

        #region DevicesClient

        private static DevicesSingleton devicesSingletons;

        public static DevicesSingleton Get(this DevicesClient that)
        {
            if (devicesSingletons == null)
            {
                devicesSingletons = new DevicesSingleton(that);
            }
            return devicesSingletons;
        }

        #endregion
    }

    public class TypeSingleton
    {
        private TypesClient client;
        private ExtendedTypeCreateCategory category;
        private Guid spaceId;

        public TypeSingleton(TypesClient client, ExtendedTypeCreateCategory category, Guid spaceId) => (this.client, this.category, this.spaceId) = (client, category, spaceId);

        public async Task<int> IdOf(string name)
        {
            var ids = await client.RetrieveAsync(names: name);
            if (ids.Count == 0)
            {
                var newId = await client.CreateAsync(new ExtendedTypeCreate
                {
                    Category = category,
                    Name = name,
                    SpaceId = spaceId
                });
                return newId;
            }
            else
            {
                return ids.First().Id;
            }
        }
    }


    public class SpacesSingleton
    {
        private SpacesClient client;

        public SpacesSingleton(SpacesClient client) => (this.client) = (client);

        public async Task<Guid> IdOf(string name, int typeId)
        {
            var ids = await client.RetrieveAsync(
                name: name);
            if (ids.Count == 0)
            {
                throw new ArgumentException($"Space not found");
            }
            else
            {
                return ids.First().Id;
            }
        }
    }

    public class DevicesSingleton
    {
        private DevicesClient client;

        public DevicesSingleton(DevicesClient client) => (this.client) = (client);

        public async Task<Guid> IdOf(string name, int typeId)
        {
            var ids = await client.RetrieveAsync(
                names: name, types: $"{typeId}");
            if (ids.Count == 0)
            {
                throw new ArgumentException($"Space not found");
            }
            else
            {
                return ids.First().Id;
            }
        }
    }
}
