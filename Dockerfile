# Base image for the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5242
EXPOSE 7239

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["webapi.csproj", "./"]
RUN dotnet restore "webapi.csproj"

# Copy the entire source code
COPY . . 
WORKDIR "/src/webapi"

# Build the project
RUN dotnet build "webapi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app

# Copy published output to the final image
COPY --from=publish /app/publish .

# Add the certificate
RUN mkdir -p /app/certificates
COPY certificates/aspnetapp.pfx /app/certificates/

# Define the entry point
ENTRYPOINT ["dotnet", "webapi.dll"]
