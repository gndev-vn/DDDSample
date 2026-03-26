FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9088
EXPOSE 9089

FROM mcr.microsoft.com/dotnet/sdk:100 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IdentityAPI/IdentityAPI.csproj", "IdentityAPI/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "IdentityAPI/IdentityAPI.csproj"
COPY . .
WORKDIR "/src/IdentityAPI"
RUN dotnet build "./IdentityAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityAPI.dll"]
