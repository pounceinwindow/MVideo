# Запуск проекта

## Через Docker Compose

```bash
docker compose up --build
```

После запуска Swagger доступен по адресу:

```text
http://localhost:8080/swagger
```

## локальный запуск:

```bash
dotnet run --project MVideo.Api
```

Swagger доступен по адресу:

```text
http://localhost:5000/swagger
```

## Переменные окружения

Строку подключения к PostgreSQL можно задать через переменную:

```text
ConnectionStrings__DefaultConnection
```
