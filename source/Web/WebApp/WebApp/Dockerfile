FROM mcr.microsoft.com/dotnet/aspnet:8.4 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.4 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["source/Web/WebApp/WebApp/WebApp.csproj", "source/Web/WebApp/WebApp/"]
COPY ["source/Web/WebApp/WebApp.Client/WebApp.Client.csproj", "source/Web/WebApp/WebApp.Client/"]
RUN dotnet restore "source/Web/WebApp/WebApp/WebApp.csproj"
COPY . .
WORKDIR "/src/source/Web/WebApp/WebApp"
RUN dotnet build "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
