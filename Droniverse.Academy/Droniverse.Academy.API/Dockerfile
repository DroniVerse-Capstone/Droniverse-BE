# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Droniverse.Academy/Droniverse.Academy.API/Droniverse.Academy.API.csproj", "Droniverse.Academy/Droniverse.Academy.API/"]
COPY ["Droniverse.Academy/Droniverse.Academy.Application/Droniverse.Academy.Application.csproj", "Droniverse.Academy/Droniverse.Academy.Application/"]
COPY ["Droniverse.Academy/Droniverse.Academy.Domain/Droniverse.Academy.Domain.csproj", "Droniverse.Academy/Droniverse.Academy.Domain/"]
COPY ["Droniverse.Academy/Droniverse.Academy.Infrastructure/Droniverse.Academy.Infrastructure.csproj", "Droniverse.Academy/Droniverse.Academy.Infrastructure/"]
RUN dotnet restore "./Droniverse.Academy/Droniverse.Academy.API/Droniverse.Academy.API.csproj"
COPY . .
WORKDIR "/src/Droniverse.Academy/Droniverse.Academy.API"
RUN dotnet build "./Droniverse.Academy.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Droniverse.Academy.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Droniverse.Academy.API.dll"]