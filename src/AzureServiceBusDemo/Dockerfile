FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AzureServiceBusDemo.csproj", "./"]
RUN dotnet restore "AzureServiceBusDemo.csproj"
COPY . .
RUN dotnet publish "AzureServiceBusDemo.csproj" --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AzureServiceBusDemo.dll"]
