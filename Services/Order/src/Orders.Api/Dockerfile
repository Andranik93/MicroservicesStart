# Stage 1: the runtime base (lightweight, used for the final production image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: the build environment (heavy SDK, used only to compile).
# Build context is the Order service root (Services/Order), not this folder,
# because Orders.Api depends on three sibling projects.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Orders.Api/Orders.Api.csproj", "src/Orders.Api/"]
COPY ["src/Orders.Application/Orders.Application.csproj", "src/Orders.Application/"]
COPY ["src/Orders.Domain/Orders.Domain.csproj", "src/Orders.Domain/"]
COPY ["src/Orders.Infrastructure/Orders.Infrastructure.csproj", "src/Orders.Infrastructure/"]
RUN dotnet restore "src/Orders.Api/Orders.Api.csproj"
COPY . .
RUN dotnet build "src/Orders.Api/Orders.Api.csproj" -c Release -o /app/build

# Stage 3: the publish step (optimizes the output)
FROM build AS publish
RUN dotnet publish "src/Orders.Api/Orders.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: the final image (combine stage 1 and stage 3)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
