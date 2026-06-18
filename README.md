# Payment Processor

## Requisitos

* Docker Desktop
* Docker Compose

## Ejecución

Desde la carpeta payment_processor:

```bash
docker compose up -d --build
```

## API

Disponible en:

http://localhost:5000

## Detener servicios

```bash
docker compose down
```

## Reiniciar completamente la base de datos

```bash
docker compose down -v
docker compose up --build
```
