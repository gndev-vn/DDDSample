FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
# REST API port
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CatalogAPI/CatalogAPI.csproj", "CatalogAPI/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["GrpcShared/GrpcShared.csproj", "GrpcShared/"]
RUN dotnet restore "CatalogAPI/CatalogAPI.csproj"
COPY . .
WORKDIR "/src/CatalogAPI"
RUN dotnet build "./CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CatalogAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogAPI.dll"]
