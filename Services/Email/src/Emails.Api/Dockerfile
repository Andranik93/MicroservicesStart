# Stage 1: the runtime base (lightweight, used for the final production image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: the build environment (heavy SDK, used only to compile).
# Build context is the Email service root (Services/Email), not this folder,
# because Emails.Api depends on three sibling projects.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Emails.Api/Emails.Api.csproj", "src/Emails.Api/"]
COPY ["src/Emails.Application/Emails.Application.csproj", "src/Emails.Application/"]
COPY ["src/Emails.Domain/Emails.Domain.csproj", "src/Emails.Domain/"]
COPY ["src/Emails.Infrastructure/Emails.Infrastructure.csproj", "src/Emails.Infrastructure/"]
RUN dotnet restore "src/Emails.Api/Emails.Api.csproj"
COPY . .
RUN dotnet build "src/Emails.Api/Emails.Api.csproj" -c Release -o /app/build

# Stage 3: the publish step (optimizes the output)
FROM build AS publish
RUN dotnet publish "src/Emails.Api/Emails.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: the final image (combine stage 1 and stage 3)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Emails.Api.dll"]
