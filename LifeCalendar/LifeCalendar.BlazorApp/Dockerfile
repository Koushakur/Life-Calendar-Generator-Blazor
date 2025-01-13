FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LifeCalendar/LifeCalendar.BlazorApp/LifeCalendar.BlazorApp.csproj", "LifeCalendar.BlazorApp/"]
RUN dotnet restore "LifeCalendar.BlazorApp/LifeCalendar.BlazorApp.csproj"
COPY . .
WORKDIR "/src/LifeCalendar.BlazorApp"
RUN dotnet build "LifeCalendar/LifeCalendar.BlazorApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LifeCalendar/LifeCalendar.BlazorApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LifeCalendar.BlazorApp.dll"]
