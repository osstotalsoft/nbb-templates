namespace NBB.Worker.Application

open NBB.Application.Mediator.FSharp
open NBB.Core.Effects.FSharp
open NBB.Core.Effects
open Microsoft.Extensions.DependencyInjection
open NBB.Messaging.Effects


module Middlewares =

    let log =
        fun next req ->
            effect {
                let reqType = req.GetType().FullName
                printfn "Precessing %s" reqType

                let! result = next req
                printfn "Precessed %s" reqType
                return result
            }

module WriteApplication =
    open Middlewares

    open RequestMiddleware  
    open CommandHandler

    let private commandPipeline =
        log
        << handlers [ HelloWorld.handle |> upCast ]

    let private queryPipeline: QueryMiddleware = handlers []

    open EventMiddleware

    let private eventPipeline : EventMiddleware = (*log << *)handlers []
    

    let addServices (services: IServiceCollection) =
        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore
        services.AddMediator(commandPipeline, queryPipeline, eventPipeline)