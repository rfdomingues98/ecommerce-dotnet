FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Order/Order.API/Order.API.csproj", "Services/Order/Order.API/"]
COPY ["src/Libs/EventBus/EventBus.csproj", "Libs/EventBus/"]
RUN dotnet restore "Services/Order/Order.API/Order.API.csproj"
COPY . .
WORKDIR "/src/Services/Order/Order.API"
RUN dotnet build "Order.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.API.dll"]
