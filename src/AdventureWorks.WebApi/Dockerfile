FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["AdventureWorks.sln", "."]
COPY ["src/AdventureWorks.WebApi/AdventureWorks.WebApi.csproj", "src/AdventureWorks.WebApi/"]
RUN dotnet restore "src/AdventureWorks.WebApi/AdventureWorks.WebApi.csproj"
COPY . .
WORKDIR "/src/src/AdventureWorks.WebApi"
RUN dotnet build "AdventureWorks.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdventureWorks.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdventureWorks.WebApi.dll"]
