# Build context is the repo root (docker-compose.yml passes `context: .`),
# since the API project pulls in Domain/Application/Infrastructure as project
# references and needs the whole solution tree to restore/build.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so `dotnet restore` is cached across
# rebuilds that only touch .cs files, not dependencies.
COPY InventorySystem.slnx .
COPY src/InventorySystem.Domain/InventorySystem.Domain.csproj src/InventorySystem.Domain/
COPY src/InventorySystem.Application/InventorySystem.Application.csproj src/InventorySystem.Application/
COPY src/InventorySystem.Infrastructure/InventorySystem.Infrastructure.csproj src/InventorySystem.Infrastructure/
COPY src/InventorySystem.API/InventorySystem.API.csproj src/InventorySystem.API/
RUN dotnet restore src/InventorySystem.API/InventorySystem.API.csproj

COPY src/ src/
RUN dotnet publish src/InventorySystem.API/InventorySystem.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventorySystem.API.dll"]
