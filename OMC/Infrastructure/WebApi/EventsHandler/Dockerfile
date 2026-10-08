#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# The status dashboard is opt-in: build with --build-arg BUILD_DASHBOARD=true to bake it into the
# image (and set DASHBOARD_ENABLED=true at runtime to serve it). Without it, the Node build below
# is skipped entirely and the image ships no dashboard at all.
ARG BUILD_DASHBOARD=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Statically exports the dashboard (client-only pages, no Node server needed at
# runtime) so it can be baked straight into this image.
FROM node:22-alpine AS dashboard-build-true
WORKDIR /dashboard
COPY dashboard/package.json dashboard/package-lock.json ./
RUN npm ci
COPY dashboard/ .
RUN npm run build

# No dashboard: an empty output folder, so the final COPY below has nothing to copy.
FROM busybox AS dashboard-build-false
RUN mkdir -p /dashboard/out

# Picks one of the two stages above; BuildKit never builds the one that isn't used.
FROM dashboard-build-${BUILD_DASHBOARD} AS dashboard-build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["OMC/Infrastructure/WebApi/EventsHandler/EventsHandler.csproj", "OMC/Infrastructure/WebApi/EventsHandler/"]
RUN dotnet restore "OMC/Infrastructure/WebApi/EventsHandler/EventsHandler.csproj"
COPY . .
WORKDIR "/src/OMC/Infrastructure/WebApi/EventsHandler"
RUN dotnet build "EventsHandler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventsHandler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=dashboard-build /dashboard/out ./wwwroot
ENTRYPOINT ["dotnet", "OMC.EventsHandler.dll"]