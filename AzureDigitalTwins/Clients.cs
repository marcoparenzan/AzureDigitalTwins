using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AzureDigitalTwins
{
    class Clients
    {
        private HttpClient HttpClient { get; }
        public UsersClient UsersClient { get; }
        public SpacesClient SpacesClient { get; }
        public DevicesClient DevicesClient { get; }
        public SensorsClient SensorsClient { get; }
        public ResourcesClient ResourcesClient { get; }
        public EndpointsClient EndpointsClient { get; }
        public OntologiesClient OntologiesClient { get; }
        public PropertyKeysClient PropertyKeysClient { get; }
        public TypesClient TypesClient { get; }
        public UserDefinedFunctionsClient UserDefinedFunctionsClient { get; }
        public MatchersClient MatchersClient { get; }
        public RoleAssignmentsClient RoleAssignmentsClient { get; }

        public Clients(HttpClient httpClient)
        {
            HttpClient = httpClient;
            UsersClient = new UsersClient(HttpClient);
            SpacesClient = new SpacesClient(HttpClient);
            DevicesClient = new DevicesClient(HttpClient);
            SensorsClient = new SensorsClient(HttpClient);
            ResourcesClient = new ResourcesClient(HttpClient);
            EndpointsClient = new EndpointsClient(HttpClient);
            OntologiesClient = new OntologiesClient(HttpClient);
            PropertyKeysClient = new PropertyKeysClient(HttpClient);
            TypesClient = new TypesClient(HttpClient);
            UserDefinedFunctionsClient = new UserDefinedFunctionsClient(HttpClient);
            MatchersClient = new MatchersClient(HttpClient);
            RoleAssignmentsClient = new RoleAssignmentsClient(HttpClient);
        }
    }
}
