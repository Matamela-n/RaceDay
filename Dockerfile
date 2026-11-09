
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RACEDAY_API_PROJECT.csproj ./
RUN dotnet restore RACEDAY_API_PROJECT.csproj

COPY . .
RUN dotnet publish RACEDAY_API_PROJECT.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RACEDAY_API_PROJECT.dll"]
