FROM mcr.microsoft.com/dotnet/sdk:5.0
EXPOSE 80
EXPOSE 443

COPY . /app
WORKDIR /app

RUN dotnet restore
RUN dotnet build
RUN dotnet publish -c Release -o /app/publish

ENV UPLOADR_PATH=./publish/uploadr.json

ENTRYPOINT ["dotnet", "./publish/UploadR.dll", "/Database:Hostname=uploadr-postgres"]
