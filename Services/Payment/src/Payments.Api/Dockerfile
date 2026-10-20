# Stage 1: the runtime base (lightweight, used for the final production image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: the build environment (heavy SDK, used only to compile).
# Build context is the Payment service root (Services/Payment), not this folder,
# because Payments.Api depends on three sibling projects.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Payments.Api/Payments.Api.csproj", "src/Payments.Api/"]
COPY ["src/Payments.Application/Payments.Application.csproj", "src/Payments.Application/"]
COPY ["src/Payments.Domain/Payments.Domain.csproj", "src/Payments.Domain/"]
COPY ["src/Payments.Infrastructure/Payments.Infrastructure.csproj", "src/Payments.Infrastructure/"]
RUN dotnet restore "src/Payments.Api/Payments.Api.csproj"
COPY . .
RUN dotnet build "src/Payments.Api/Payments.Api.csproj" -c Release -o /app/build

# Stage 3: the publish step (optimizes the output)
FROM build AS publish
RUN dotnet publish "src/Payments.Api/Payments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: the final image (combine stage 1 and stage 3)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.Api.dll"]
