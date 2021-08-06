[<AutoOpen>]
module ServiceCollectionExtensions

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open NBB.Worker.Application
open NBB.Messaging.Host
open NBB.Messaging.Host.Builder
open NBB.Messaging.Host.MessagingPipeline
open NBB.Messaging.Abstractions
open NBB.Application.Mediator.FSharp
open NBB.Core.Effects
open System
open NBB.Worker.Messaging
#if NatsMessagingTransport
open NBB.Messaging.Nats
open System.Reflection
open Microsoft.Extensions.Logging
#if OpenTracing
open NBB.Messaging.OpenTracing.Subscriber
open NBB.Messaging.OpenTracing.Publisher
open OpenTracing
open OpenTracing.Noop
open Jaeger
open Jaeger.Samplers
open Jaeger.Reporters
open Jaeger.Senders.Thrift
open OpenTracing.Util
#endif
#endif

type IServiceCollection with
    member this.AddWorkerServices(configuration: IConfiguration) =

        // Application
        this |> WriteApplication.addServices |> ignore

        // Messaging
#if NatsMessagingTransport
        this
            .AddMessageBus()
            .AddNatsTransport(configuration)
        |> ignore
#endif

        this.Decorate<IMessageBusPublisher, SamplePublisherDecorator>() |> ignore

#if OpenTracing
        this.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>()
        |> ignore
#endif
        this.AddMessagingHost
            (fun hostBuilder ->
                hostBuilder.Configure
                    (fun configBuilder ->
                        configBuilder
                            .AddSubscriberServices(fun config ->
                                config.AddTypes(typeof<HelloWorld.Command>)
                                |> ignore)
                            .WithDefaultOptions()
                            .UsePipeline(fun pipelineBuilder ->
                                pipelineBuilder
                                    .UseCorrelationMiddleware()
                                    .UseExceptionHandlingMiddleware()
#if OpenTracing
                                    .UseMiddleware<OpenTracingMiddleware>()
#endif
#if Resiliency
                                    .UseDefaultResiliencyMiddleware()
#endif
                                    .UseMiddleware<SampleSubscriberPipelineMiddleware>()
                                    .UseEffectMiddleware(fun m ->
                                        m
                                        |> Mediator.sendMessage
                                        |> EffectExtensions.ToUnit)
                                |> ignore)
                        |> ignore)
                |> ignore)
        |> ignore


#if OpenTracing
        // OpenTracing
        this.AddOpenTracing() |> ignore

        this.AddSingleton<ITracer>(fun (serviceProvider : IServiceProvider) ->
            let isEnabled = configuration.GetValue<bool>("OpenTracing:Jeager:IsEnabled")
            if not isEnabled then
                NoopTracerFactory.Create();
            else
                let serviceName = Assembly.GetEntryAssembly().GetName().Name
                let loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
                let tracer = 
                    Tracer.Builder(serviceName)
                        .WithLoggerFactory(loggerFactory)
                        .WithSampler(ConstSampler true)
                        .WithReporter(
                            RemoteReporter.Builder()
                                .WithSender(
                                    UdpSender(
                                        configuration.GetValue<string>("OpenTracing:Jeager:AgentHost"),
                                        configuration.GetValue<int>("OpenTracing:Jeager:AgentPort"), 0))
                                .Build())
                        .Build() :> ITracer

                GlobalTracer.Register(tracer) |> ignore

                tracer
        )
#endif
