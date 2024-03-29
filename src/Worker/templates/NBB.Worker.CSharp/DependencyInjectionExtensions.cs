﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Worker.Messaging;
using NBB.Worker.Application;
using NBB.Messaging.Host;
using NBB.Messaging.Abstractions;
#if NatsMessagingTransport
using NBB.Messaging.Nats;
#endif
#if RusiMessagingTransport
#endif
using System.Reflection;
using Microsoft.Extensions.Logging;
#if OpenTracing
using NBB.Messaging.OpenTracing.Subscriber;
using NBB.Messaging.OpenTracing.Publisher;
using OpenTracing;
using OpenTracing.Noop;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Reporters;
using Jaeger.Senders.Thrift;
using OpenTracing.Util;
#endif


namespace NBB.Worker
{
    public static class DependencyInjectionExtensions
    {
        public static void AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MediatR 
            services.AddMediatR(typeof(HelloWorld.Command).Assembly);
            services.Scan(scan => scan.FromAssemblyOf<HelloWorld.Handler>()
                .AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Messaging
#if NatsMessagingTransport
            services.AddMessageBus().AddNatsTransport(configuration);
#endif
#if RusiMessagingTransport
            services.AddMessageBus().AddRusiTransport(configuration);
#endif            

            services.Decorate<IMessageBusPublisher, SamplePublisherDecorator>();

#if OpenTracing
            services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();
#endif
            services.AddMessagingHost(
                configuration, 
                hostBuilder => hostBuilder
                .Configure(configBuilder => configBuilder
                    .AddSubscriberServices(selector =>
                    {
                        selector
                            .FromMediatRHandledCommands()
                            .AddAllClasses();
                    })
                    .WithDefaultOptions()
                    .UsePipeline(pipelineBuilder => pipelineBuilder
                        .UseCorrelationMiddleware()
                        .UseExceptionHandlingMiddleware()
#if OpenTracing
                        .UseMiddleware<OpenTracingMiddleware>()
#endif
#if Resiliency
                        .UseDefaultResiliencyMiddleware()
#endif
                        .UseMediatRMiddleware()
                        .UseMiddleware<SampleSubscriberPipelineMiddleware>())));

#if OpenTracing
            // OpenTracing
            services.AddOpenTracing();

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                if (!configuration.GetValue<bool>("OpenTracing:Jeager:IsEnabled"))
                {
                    return NoopTracerFactory.Create();
                }

                string serviceName = Assembly.GetEntryAssembly().GetName().Name;

                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                ITracer tracer = new Tracer.Builder(serviceName)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(new ConstSampler(true))
                    .WithReporter(new RemoteReporter.Builder()
                        .WithSender(new UdpSender(
                            configuration.GetValue<string>("OpenTracing:Jeager:AgentHost"),
                            configuration.GetValue<int>("OpenTracing:Jeager:AgentPort"), 0))
                        .Build())
                    .Build();

                GlobalTracer.Register(tracer);

                return tracer;
            });
#endif
        }
    }
}
