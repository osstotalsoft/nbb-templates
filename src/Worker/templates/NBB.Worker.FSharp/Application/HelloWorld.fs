namespace NBB.Worker.Application

open System
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp

module HelloWorld =
    type Command =
        { id: Guid }
        interface ICommand

    let handle (_command: Command) : Effect<unit option> =
        effect {
            return failwith "Not implemented"
        }

