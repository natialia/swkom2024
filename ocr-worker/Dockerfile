FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    libleptonica-dev \
    libtesseract-dev \
    libc6-dev \
    libjpeg62-turbo-dev \
    libgdiplus \
    ghostscript \
    libpng-dev

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ocr-worker.dll"]