FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PaymentMock/PaymentMock.csproj PaymentMock/
RUN dotnet restore PaymentMock/PaymentMock.csproj

COPY PaymentMock/ PaymentMock/
WORKDIR /src/PaymentMock
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
RUN groupadd paymentmock && useradd --gid paymentmock --no-create-home paymentmock
COPY --from=build /app/publish .
RUN mkdir -p /app/logs && chown -R paymentmock:paymentmock /app
USER paymentmock

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PaymentMock.dll"]
