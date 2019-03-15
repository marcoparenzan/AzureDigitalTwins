// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureDigitalTwins
{

    partial class Program
    { 
        public static async Task<Guid> CreateUserDefinedFunction(
             CreateUserDefinedFunctionArgs args,
             string js)
        {
            var metadataContent = new StringContent(JsonConvert.SerializeObject(args));
            metadataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var contentContent = new StringContent(js);
            contentContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/javascript");

            var multipartContent = new MultipartFormDataContent("USER_DEFINED_BOUNDARY");
            multipartContent.Add(metadataContent, "metadata");
            multipartContent.Add(contentContent, "contents");
            contentContent.Headers.ContentDisposition.FileName = "udf.js";

            var response = await httpClient.PostAsync($"{appSettings.BaseUrl}/api/v1.0/userdefinedfunctions", multipartContent);
            var idTExt = await response.Content.ReadAsStringAsync();

            return Guid.Parse(idTExt.Trim('"'));
        }
    }

    public class CreateUserDefinedFunctionArgs
    {
        [JsonProperty("spaceId")]
        public Guid SpaceId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("matchers")]
        public IEnumerable<Guid> Matchers { get; set; }
    }
}