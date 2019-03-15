using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDigitalTwins
{
    public class LoggingHttpHandler : DelegatingHandler
    {
        private ILogger logger;

        public LoggingHttpHandler(ILogger logger)
            : base(new HttpClientHandler())
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            logger.LogTrace($"Request: {request.Method} {request.RequestUri}");

            var response = await base.SendAsync(request, cancellationToken);

            const int maxContentLength = 200;
            var content = await response.Content?.ReadAsStringAsync();
            var contentMaxLength = content == null || content.Length < maxContentLength
                ? content
                : content.Substring(0, maxContentLength - 3) + "...";
            var contentDisplay = contentMaxLength == null ? "" : $", {contentMaxLength}";
            logger.LogTrace($"Response Status: {(int)response.StatusCode}, {response.StatusCode} {contentDisplay}");

            return response;
        }
    }
}