# See https://aka.ms/customizecontainer to learn how to customize your debug container
# and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image used when running from VS or in production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image - used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file from the subfolder and restore
COPY ["ImageApi/ImageApi.csproj", "ImageApi/"]
RUN dotnet restore "ImageApi/ImageApi.csproj"

# Copy the rest of the source
COPY . .

# Build the project
WORKDIR "/src/ImageApi"
RUN dotnet build "ImageApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/ImageApi"
RUN dotnet publish "ImageApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Listen on 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ImageApi.dll"]
