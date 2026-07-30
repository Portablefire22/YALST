FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker.csproj", "YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker/"]
COPY ["YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker.Client/YetAnotherLeagueStatTracker.Client.csproj", "YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker.Client/"]
RUN dotnet restore "YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker.csproj"
COPY . .
WORKDIR "/src/YetAnotherLeagueStatTracker/YetAnotherLeagueStatTracker"
RUN dotnet build "./YetAnotherLeagueStatTracker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YetAnotherLeagueStatTracker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YetAnotherLeagueStatTracker.dll"]
