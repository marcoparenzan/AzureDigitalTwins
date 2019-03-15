// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AzureDigitalTwins
{
    public class AppSettings {
        // Note: this is a constant because it is the same for every user authorizing
        // against the Digital Twins Apis
        private static string DigitalTwinsAppId = "0b07f429-9f4b-4714-9392-cc5e8e80c8b0";

        public string AADInstance { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Resource { get; set; } = DigitalTwinsAppId;
        public string Tenant { get; set; }
        public string BaseUrl { get; set; }
        public string Authority => AADInstance + Tenant;
        public string EventGridUri { get; set; }
        public string EventGridKey { get; set; }
        public string ServiceBusConnectionString { get; set; }
    }
}