FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY PaymentAPI/PaymentAPI.csproj PaymentAPI/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Integrations/Integrations.csproj Integrations/
COPY Persistence/Persistence.csproj Persistence/

RUN dotnet restore PaymentAPI/PaymentAPI.csproj

COPY . .

RUN dotnet publish PaymentAPI/PaymentAPI.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PaymentAPI.dll"]