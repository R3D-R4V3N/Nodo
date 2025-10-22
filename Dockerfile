# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder

WORKDIR /src

# Copy only the necessary project files 
COPY ["src/Rise.Server/Rise.Server.csproj", "src/Rise.Server/"]
COPY ["src/Rise.Client/Rise.Client.csproj", "src/Rise.Client/"]
COPY ["src/Rise.Domain/Rise.Domain.csproj", "src/Rise.Domain/"]
COPY ["src/Rise.Persistence/Rise.Persistence.csproj", "src/Rise.Persistence/"]
COPY ["src/Rise.Services/Rise.Services.csproj", "src/Rise.Services/"]
COPY ["src/Rise.Shared/Rise.Shared.csproj", "src/Rise.Shared/"]

# Restore dependencies for the server
RUN dotnet restore "src/Rise.Server/Rise.Server.csproj"

# Copy all source code
COPY . .

# Build and publish the client first (to wwwroot)
RUN dotnet publish "src/Rise.Client/Rise.Client.csproj" -c Release -o /app/client

# Build and publish the server
RUN dotnet publish "src/Rise.Server/Rise.Server.csproj" -c Release -o /app/server --no-restore

# Copy client wwwroot files to server wwwroot
RUN cp -r /app/client/wwwroot/* /app/server/wwwroot/

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Copy published output from builder
COPY --from=builder /app/server .

# Create logs directory
RUN mkdir -p /app/Logs

# Expose port
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD dotnet --version || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Rise.Server.dll"]
