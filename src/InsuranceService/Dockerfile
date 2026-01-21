# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["InsuranceSystemSolution.sln", "./"]

# Copy project files
COPY ["src/InsuranceService/API/API.csproj", "src/InsuranceService/API/"]
COPY ["src/InsuranceService/Application/Application.csproj", "src/InsuranceService/Application/"]
COPY ["src/InsuranceService/Domain/Domain.csproj", "src/InsuranceService/Domain/"]
COPY ["src/InsuranceService/Infrastructure/Infrastructure.csproj", "src/InsuranceService/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/InsuranceService/API/API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/InsuranceService/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "API.dll"]
