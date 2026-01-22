FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HassLanguage.sln .
COPY src/ ./src/

RUN dotnet restore
RUN dotnet build --no-restore -c Release

FROM build AS publish
RUN dotnet publish src/HassLanguage/HassLanguage.csproj --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HassLanguage.dll"]
