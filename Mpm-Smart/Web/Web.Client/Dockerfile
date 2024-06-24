FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mpm-Smart-Web/Mpm-Smart-Web.Client/Mpm-Smart-Web.Client.csproj", "Mpm-Smart-Web/Mpm-Smart-Web.Client/"]
RUN dotnet restore "Mpm-Smart-Web/Mpm-Smart-Web.Client/Mpm-Smart-Web.Client.csproj"
COPY . .
WORKDIR "/src/Mpm-Smart-Web/Mpm-Smart-Web.Client"
RUN dotnet build "Mpm-Smart-Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mpm-Smart-Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mpm-Smart-Web.Client.dll"]
