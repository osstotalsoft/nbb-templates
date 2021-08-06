## Overview

The template can be used to create a new NBB Worker project in C# or F# with the following features:
* Messaging host and messaging bus
* Health checks
* OpenTracing
* SQL logging
* Application mediator
  * MediatR (C#) 
  * NBB.Application.Mediator.FSharp (F#)

## Installation

The template can be installed using the `dotnet new` tool
```console
dotnet new -i NBB.Worker::*
```

## Usage
Create a C# project with default options:
```console
dotnet new nbbworker -n MyTest.Worker
```
Create a F# project with default options:
```console
dotnet new nbbworker -lang F# -n MyTest.Worker
```

### Additional options
The following options are vailable to customize the generated project:

|Flag | Description | Type | Default |
|-----|-------------|------|---------|
| -t\|--title        |                     The name of the project which determines the assembly product name. | string - Optional | Project Title |
| -d\|--description  |                     A description of the project which determines the assembly description. | string - Optional | Project Description |
| -au\|--param:author|                     The name of the author of the project which determines the assembly author, company and copyright information. | st ring - Optional| Project Author|
| -hc\|--health-check|                     A health-check endpoint that returns the status of this service and its dependencies, giving an indication of its health.|bool - Optional|true|
| -r\|--resiliency   |                     Adds default resiliency policy to the messaging host | bool - Optional | true|
| -slcs\|--sql-logging-connection-string|  The connection string to the sql server database.|string - Optional| 
| -ot\|--open-tracing|                     Add Jaeger / OpenTracing support|bool - Optional| true|
| -nr\|--no-restore  |                     Skips the execution of 'dotnet restore' on project creation.|bool - Optional| false|


## Development
### Build
The template projects are compilable C#/ F# projects. You can build them like any other projects.

You can enable/disable features from file `development.props`.

### Install the template from a local folder
- Test the a template without packaging
```console
  dotnet new -i <Your_project_path>\src\Worker
```

### Package and publish template
- Increase version in file "NBB.WorkerNBB.Worker.nuspec"
- Package - use nuget.exe instead of dotnet pack:
```console
  cd <Your_project_path>\src\Worker
  nuget pack
``` 
- Publish package
```console
  cd <Your_project_path>\src\Worker
  nuget push NBB.Worker.*.nupkg -source <Your_nuget_repo_url>
``` 

### Uninstall
This will reset all templates:
```console
  dotnet new --debug:reinit
``` 