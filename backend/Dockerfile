FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AdReport.sln* ./
COPY AdReport.slnx* ./
COPY AdReport.API/AdReport.API.csproj AdReport.API/
COPY AdReport.Application/AdReport.Application.csproj AdReport.Application/
COPY AdReport.Domain/AdReport.Domain.csproj AdReport.Domain/
COPY AdReport.Infrastructure/AdReport.Infrastructure.csproj AdReport.Infrastructure/

RUN dotnet restore AdReport.API/AdReport.API.csproj

COPY . .
RUN dotnet publish AdReport.API/AdReport.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p wwwroot/reports wwwroot/logos

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AdReport.API.dll"]
