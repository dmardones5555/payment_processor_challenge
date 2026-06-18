FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY PaymentWorker/PaymentWorker.csproj PaymentWorker/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Integrations/Integrations.csproj Integrations/
COPY Persistence/Persistence.csproj Persistence/

RUN dotnet restore PaymentWorker/PaymentWorker.csproj

COPY . .

RUN dotnet publish PaymentWorker/PaymentWorker.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PaymentWorker.dll"]