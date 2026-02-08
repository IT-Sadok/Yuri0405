# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["InsuranceSystemSolution.sln", "./"]

# Copy project files
COPY ["src/PaymentService/API/PaymentService.csproj", "src/PaymentService/API/"]
COPY ["src/PaymentService/Application/Application.csproj", "src/PaymentService/Application/"]
COPY ["src/PaymentService/Domain/Domain.csproj", "src/PaymentService/Domain/"]
COPY ["src/PaymentService/Infrastructure/Infrastructure.csproj", "src/PaymentService/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/PaymentService/API/PaymentService.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/PaymentService/API"
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

ENTRYPOINT ["dotnet", "PaymentService.dll"]
