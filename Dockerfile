FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["IdGenerator/IdGenerator.csproj", "IdGenerator/"]
RUN dotnet restore "IdGenerator/IdGenerator.csproj"
COPY . .
WORKDIR "/src/IdGenerator"
RUN dotnet build "IdGenerator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdGenerator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV REDIS_ADDRESS=127.0.0.1
ENV REDIS_PORT=6379
ENV REDIS_DB=0
ENV IDENTITY_LOW_LIMIT=100000
ENV INTERVAL=86400
ENTRYPOINT ["dotnet", "IdGenerator.dll"]