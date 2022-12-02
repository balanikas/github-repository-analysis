FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RepositoryAnalysis/RepositoryAnalysis.csproj", "RepositoryAnalysis/"]

COPY ["Frontend/Frontend.csproj", "Frontend/"]
RUN dotnet restore "Frontend/Frontend.csproj" 
COPY . .
WORKDIR "/src/Frontend"

RUN dotnet build "Frontend.csproj" -c Release -o /app/build 

FROM build AS publish
RUN dotnet publish "Frontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod +x Frontend.dll
ENTRYPOINT ["dotnet", "Frontend.dll"]
