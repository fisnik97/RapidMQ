FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RapidMq_Client/RapidMq_Client.csproj", "RapidMq_Client/"]
RUN dotnet restore "RapidMq_Client/RapidMq_Client.csproj"
COPY . .
WORKDIR "/src/RapidMq_Client"
RUN dotnet build "RapidMq_Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RapidMq_Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RapidMq_Client.dll"]
