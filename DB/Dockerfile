FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MyBackendApp.csproj .
RUN dotnet restore MyBackendApp.csproj
COPY . .
RUN dotnet publish MyBackendApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0 
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyBackendApp.dll"]
