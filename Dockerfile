FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SearchService.API/SearchService.API.csproj", "src/SearchService.API/"]
COPY ["src/SearchService.Application/SearchService.Application.csproj", "src/SearchService.Application/"]
COPY ["src/SearchService.Infrastructure/SearchService.Infrastructure.csproj", "src/SearchService.Infrastructure/"]
COPY ["src/SearchService.Domain/SearchService.Domain.csproj", "src/SearchService.Domain/"]
RUN dotnet restore "src/SearchService.API/SearchService.API.csproj"
COPY . .
WORKDIR "/src/src/SearchService.API"
RUN dotnet build "SearchService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SearchService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SearchService.API.dll"] 