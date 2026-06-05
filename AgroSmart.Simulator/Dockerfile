FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AgroSmart.Simulator/AgroSmart.Simulator.csproj", "AgroSmart.Simulator/"]
RUN dotnet restore "AgroSmart.Simulator/AgroSmart.Simulator.csproj"
COPY . .
WORKDIR /src/AgroSmart.Simulator
RUN dotnet publish "AgroSmart.Simulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgroSmart.Simulator.dll"]
