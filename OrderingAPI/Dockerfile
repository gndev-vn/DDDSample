FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9084
EXPOSE 9085

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrderingAPI/OrderingAPI.csproj", "OrderingAPI/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["GrpcShared/GrpcShared.csproj", "GrpcShared/"]
RUN dotnet restore "OrderingAPI/OrderingAPI.csproj"
COPY . .
WORKDIR "/src/OrderingAPI"
RUN dotnet build "./OrderingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrderingAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderingAPI.dll"]
