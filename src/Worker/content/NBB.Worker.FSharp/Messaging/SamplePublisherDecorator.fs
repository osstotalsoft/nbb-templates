namespace NBB.Worker.Messaging

open Microsoft.Extensions.Logging
open NBB.Messaging.Abstractions
open System.Threading
open System

type SamplePublisherDecorator(inner: IMessageBusPublisher, logger: ILogger<SamplePublisherDecorator>) =
    interface IMessageBusPublisher with
        member _.PublishAsync<'T>(message: 'T, options : MessagingPublisherOptions, cancellationToken : CancellationToken) =
            let opts = if (isNull options) then MessagingPublisherOptions.Default else options

            let newCustomizer (outgoingEnvelope : MessagingEnvelope) =
                outgoingEnvelope.SetHeader("SampleHeader", "SampleValue")
                if (not(isNull opts.EnvelopeCustomizer)) then opts.EnvelopeCustomizer.Invoke(outgoingEnvelope)

            logger.LogDebug("Message {@Message} was sent.", message)
            inner.PublishAsync(message, MessagingPublisherOptions(EnvelopeCustomizer = Action<MessagingEnvelope>(newCustomizer), TopicName = opts.TopicName), cancellationToken)          


