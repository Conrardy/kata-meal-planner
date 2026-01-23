# Stage 1: Restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS restore
WORKDIR /src

# Copy only project files for better caching
COPY src/Domain/MealPlanner.Domain/MealPlanner.Domain.csproj src/Domain/MealPlanner.Domain/
COPY src/Application/MealPlanner.Application/MealPlanner.Application.csproj src/Application/MealPlanner.Application/
COPY src/Infrastructure/MealPlanner.Infrastructure/MealPlanner.Infrastructure.csproj src/Infrastructure/MealPlanner.Infrastructure/
COPY src/Api/MealPlanner.Api/MealPlanner.Api.csproj src/Api/MealPlanner.Api/
COPY MealPlanner.sln .

RUN dotnet restore MealPlanner.sln

# Stage 2: Build application
FROM restore AS build
ARG CONFIGURATION=Release

COPY src/ src/

RUN dotnet build src/Api/MealPlanner.Api/MealPlanner.Api.csproj \
    -c ${CONFIGURATION} \
    --no-restore

# Stage 3: Publish application
FROM build AS publish
ARG CONFIGURATION=Release

RUN dotnet publish src/Api/MealPlanner.Api/MealPlanner.Api.csproj \
    -c ${CONFIGURATION} \
    --no-build \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 4: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

# Install required packages for globalization
RUN apk add --no-cache icu-libs

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "MealPlanner.Api.dll"]
