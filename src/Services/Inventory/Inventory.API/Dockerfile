FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/Services/Inventory/Inventory.API/Inventory.API.csproj", "Services/Inventory/Inventory.API/"]
# Only include this if the EventBus project actually exists
# COPY ["src/Libs/EventBus/EventBus.csproj", "Libs/EventBus/"]

RUN dotnet restore "Services/Inventory/Inventory.API/Inventory.API.csproj"

# Copy only the inventory service files
COPY ["src/Services/Inventory/Inventory.API/", "Services/Inventory/Inventory.API/"]
# Only include this if the EventBus project actually exists
# COPY ["src/Libs/EventBus/", "Libs/EventBus/"]

WORKDIR "/src/Services/Inventory/Inventory.API"
RUN dotnet build "Inventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventory.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "Inventory.API.dll"]