FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS NotificationAPI
WORKDIR /app
COPY . ./
RUN dotnet restore ./Ccs.Ppg.NotificationService.API/Ccs.Ppg.NotificationService.API.csproj
COPY Ccs.Ppg.NotificationService.API/appsecrets-template.json /app/appsecrets.json
COPY Ccs.Ppg.NotificationService.API/appsettings.json /app/appsettings.json
RUN dotnet build --configuration Release ./Ccs.Ppg.NotificationService.API/Ccs.Ppg.NotificationService.API.csproj
EXPOSE 5000
ENTRYPOINT ["dotnet","Ccs.Ppg.NotificationService.API/bin/Release/net8.0/Ccs.Ppg.NotificationService.API.dll"]
