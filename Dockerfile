# Use the ASP.NET 8.0 image for Linux
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app


# Use the .NET SDK 8.0 image for Linux
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebTV/WebTV.csproj", "WebTV/"]
RUN dotnet restore "./WebTV/WebTV.csproj"
COPY . .
WORKDIR "/src/WebTV"
RUN dotnet build "./WebTV.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebTV.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 7246
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "WebTV.dll"]
