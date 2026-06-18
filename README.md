# Payment Processor

## Requisitos

* Docker Desktop
* Docker Compose

## Ejecución

Desde la carpeta PaymentAPI:

```bash
docker compose up --build
```

## API

Disponible en:

http://localhost:4001

## Detener servicios

```bash
docker compose down
```

## Reiniciar completamente la base de datos

```bash
docker compose down -v
docker compose up --build
```
