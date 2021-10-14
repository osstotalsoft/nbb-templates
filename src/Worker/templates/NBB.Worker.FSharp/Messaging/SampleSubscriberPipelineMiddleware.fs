namespace NBB.Worker.Messaging

open Microsoft.Extensions.Logging
open NBB.Core.Pipeline
open NBB.Messaging.Abstractions
open System
open System.Threading
open System.Threading.Tasks

type SampleSubscriberPipelineMiddleware(logger: ILogger<SampleSubscriberPipelineMiddleware>) =
    interface IPipelineMiddleware<MessagingContext> with
        member _.Invoke(context: MessagingContext, cancellationToken: CancellationToken, next: Func<Task>) =
            logger.LogDebug("Message {@Message} was received.", context.MessagingEnvelope.Payload)
            next.Invoke()
