﻿### Local
- Build project
- Install template: 
```console
  dotnet new -i <Your_project_path>\src\Worker
```
- Display command line options:
```console
  dotnet new nbbworker -h
```

- Use template:
```console
  dotnet new nbbworker -n MyTest.Worker
```


### Nuget
- Build project
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
- Install template
```console
  dotnet new -i NBB.Worker::* --nuget-source <Your_nuget_repo_url>
```
- Use template:
```console
  dotnet new nbbworker -n MyTest.Worker
```

### Uninstall
This will reset all templates:
```console
  dotnet new --debug:reinit
``` 