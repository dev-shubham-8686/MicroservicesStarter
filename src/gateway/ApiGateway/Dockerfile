FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/gateway/ApiGateway/ApiGateway.csproj", "src/gateway/ApiGateway/"]
COPY ["src/shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "src/shared/Shared.Infrastructure/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
RUN dotnet restore "src/gateway/ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/gateway/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ApiGateway.dll"]

