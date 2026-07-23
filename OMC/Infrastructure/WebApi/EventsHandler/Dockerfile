#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Statically exports the dashboard (client-only pages, no Node server needed at
# runtime) so it can be baked straight into this image — same deployment as today.
FROM node:22-alpine AS dashboard-build
WORKDIR /dashboard
COPY dashboard/package.json dashboard/package-lock.json ./
RUN npm ci
COPY dashboard/ .
RUN npm run build

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