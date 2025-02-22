# Steg 1: Definiera basbilden
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN id $APP_UID || (echo "Invalid APP_UID" && exit 1)

# Steg 2: Bygg-steg
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT_NAME=LifeCalendar.BlazorApp
ARG BUILD_CONFIGURATION=Release
WORKDIR /app

# Copy Skia to image
COPY ["Common/libSkiaSharp.so", "./"]

# Dynamiskt kopiera projektet baserat på bygg-argumentet
COPY ["LifeCalendar/$PROJECT_NAME/$PROJECT_NAME.csproj", "$PROJECT_NAME/"]

# Återställ projektet
RUN dotnet restore "$PROJECT_NAME/$PROJECT_NAME.csproj"

# Kopiera hela koden
COPY . .

# Felsök: Lista alla filer i /src för att säkerställa att lösningen finns där
RUN ls -R /app

# Bygg projektet
RUN dotnet build "LifeCalendar/$PROJECT_NAME/$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Steg 3: Publicera applikationen
FROM build AS publish
RUN dotnet publish "LifeCalendar/$PROJECT_NAME/$PROJECT_NAME.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Steg 4: Slutlig bild för att köra applikationenx
FROM base AS final
WORKDIR /app

# Kopiera publicerade filer
COPY --from=publish /app/publish .

# Skapa data- och templates-kataloger
RUN mkdir -p Data
RUN chmod -R a+rwx Data/

# Installera curl
USER root
RUN apt-get update && apt-get install -y curl

# Kör applikationen
ENTRYPOINT ["dotnet", "LifeCalendar.BlazorApp.dll"]

