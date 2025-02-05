FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5055
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./CryptoBank_WebApi.csproj", "CryptoBank_WebApi/"]
RUN dotnet restore "CryptoBank_WebApi/CryptoBank_WebApi.csproj"
COPY . /src/CryptoBank_WebApi
WORKDIR "/src/CryptoBank_WebApi"
RUN dotnet build "CryptoBank_WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CryptoBank_WebApi.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:5055
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "CryptoBank_WebApi.dll"]
