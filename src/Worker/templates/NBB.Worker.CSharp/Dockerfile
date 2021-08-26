FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY NuGet.config .
COPY dependencies.props .
COPY Directory.Build.props .
COPY ["src/NBB.Worker/NBB.Worker.csproj", "src/NBB.Worker/"]
RUN dotnet restore "src/NBB.Worker/NBB.Worker.csproj"
COPY ["src/NBB.Worker/", "src/NBB.Worker/"]
WORKDIR "/src/src/NBB.Worker"
RUN dotnet build "NBB.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NBB.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ARG imageUser=tsuser
ARG imageUserGroup=tsgroup
ARG imageUserId=1375
ARG imageUserGroupId=1375
 
RUN addgroup --system --gid $imageUserGroupId $imageUserGroup && \    
    adduser --system --uid $imageUserId --ingroup $imageUserGroup $imageUser
 
USER $imageUser
 
COPY --chown=$imageUser:$imageUserGroup --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "NBB.Worker.dll"]
