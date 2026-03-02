FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FileStorageService/FileStorageService.csproj", "FileStorageService/"]
RUN dotnet restore "FileStorageService/FileStorageService.csproj"
COPY . .
WORKDIR "/src/FileStorageService"
RUN dotnet build "FileStorageService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileStorageService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create storage directory
RUN mkdir -p /app/storage

ENTRYPOINT ["dotnet", "FileStorageService.dll"]