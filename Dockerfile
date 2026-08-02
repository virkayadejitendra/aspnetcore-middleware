FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PartnerDataSharingMiddlewareDemo.slnx ./
COPY PartnerDataSharing.Api/PartnerDataSharing.Api.csproj PartnerDataSharing.Api/
COPY PartnerDataSharing.Api.Tests/PartnerDataSharing.Api.Tests.csproj PartnerDataSharing.Api.Tests/
RUN dotnet restore PartnerDataSharingMiddlewareDemo.slnx

COPY . .
RUN dotnet publish PartnerDataSharing.Api/PartnerDataSharing.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PartnerDataSharing.Api.dll"]
