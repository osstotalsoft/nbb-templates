using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Messaging
{
    public class SamplePublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ILogger<SamplePublisherDecorator> _logger;

        public SamplePublisherDecorator(IMessageBusPublisher inner, ILogger<SamplePublisherDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public Task PublishAsync<T>(T message, MessagingPublisherOptions options = null, CancellationToken cancellationToken = default)
        {
            options ??= MessagingPublisherOptions.Default;
            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                outgoingEnvelope.SetHeader("SampleHeader", "SampleValue");
                options.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            _logger.LogDebug("Message {@Message} was sent.", message);
            return _inner.PublishAsync(message, options with { EnvelopeCustomizer = NewCustomizer }, cancellationToken);
        }
    }
}