using Microsoft.Extensions.Logging;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Messaging
{
    public class SampleSubscriberPipelineMiddleware: IPipelineMiddleware<MessagingContext>
    {
        private readonly ILogger<SampleSubscriberPipelineMiddleware> _logger;

        public SampleSubscriberPipelineMiddleware(ILogger<SampleSubscriberPipelineMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            _logger.LogDebug("Message {@Message} was received.", context.MessagingEnvelope.Payload);
            await next();
        }
    }
}