# HealthChecker

## Развёртывание

### Требования

- Docker 24+ и Docker Compose v2 (команда `docker compose`)
- Свободные порты: `8080` (API), `5432` (PostgreSQL), `6379` (Redis)

### Запуск в Docker Compose

```bash
git clone <URL-репозитория> AsyncHealthChecker
cd AsyncHealthChecker

docker compose up -d --build   
docker compose ps              
```

После старта:

| Что                | Адрес                         |
| ------------------ | ----------------------------- |
| Swagger UI         | http://localhost:8080/swagger |
| Метрики Prometheus | http://localhost:8080/metrics |

Миграции БД применяются автоматически при старте приложения, вручную ничего создавать не нужно.

Остановка:

```bash
docker compose down -v    
```

### Локальный запуск для разработки

Инфраструктура в Docker, API на хосте:

```bash
docker compose up -d postgres redis
dotnet run --project AsyncHealthChecker.Api
```

Приложение будет доступно на **http://localhost:5245** (Swagger: http://localhost:5245/swagger).

> В Docker порт API — **8080**, при локальном запуске — **5245**.

## Конфигурация

Настройки берутся из `appsettings.json` и переопределяются переменными окружения (вложенные ключи через `__`).

В `docker-compose.yml` для `api` уже заданы строки подключения с хостами `postgres` и `redis`.

Параметры воркера заданы в коде (`HealthCheckWorker.cs`) и меняются только пересборкой: до 20 одновременных проверок, таймаут одной проверки 10 с, опрос пустой очереди раз в 1 с.


---

## Примеры использования API (curl)

Базовый адрес:

```bash
BASE_URL=http://localhost:8080
```

| Метод | Путь | Описание | Коды ответа |
|---|---|---|---|
| `POST` | `/api/v1/task` | Создать задачу проверки списка URL | `201`, `400` |
| `GET` | `/api/v1/task/{taskId}` | Получить статус и результаты | `200`, `404` |

Правила: `urls` обязательно и содержит минимум один элемент, каждый URL абсолютный и начинается с `http://` или `https://`.

Статусы: `Queued` (ждет проверки), `Processing` (проверена часть URL), `Completed` (проверены все) `Failed` (Ошибка при сохранении в бд).

### 1. Создать задачу

```bash
curl -i -X POST "$BASE_URL/api/v1/task" \
  -H "Content-Type: application/json" \
  -d '{
        "urls": [
          "https://github.com",
          "https://example.com",
          "https://nonexistent.invalid"
        ]
      }'
```

Ответ `201 Created` (значения примерные):

```json
{
  "taskId": "3f6c2b1e-8a4d-4c55-9b0e-7d2a1f5e9c11",
  "status": "Queued",
  "urlsCount": 3,
  "createdAt": "2026-09-24T10:15:30.123456Z"
}
```

### 2. Получить статус и результаты

```bash
curl -s "$BASE_URL/api/v1/task/3f6c2b1e-8a4d-4c55-9b0e-7d2a1f5e9c11"
```

Ответ `200 OK` (значения примерные):

```json
{
  "taskId": "3f6c2b1e-8a4d-4c55-9b0e-7d2a1f5e9c11",
  "status": "Completed",
  "totalUrls": 3,
  "processedUrls": 3,
  "results": [
    {
      "url": "https://example.com",
      "statusCode": 200,
      "responseTime": 187.42,
      "isAvailable": true,
      "checkedAt": "2026-09-24T10:15:31.402913Z"
    },
    {
      "url": "https://github.com",
      "statusCode": 200,
      "responseTime": 312.05,
      "isAvailable": true,
      "checkedAt": "2026-09-24T10:15:31.527120Z"
    },
    {
      "url": "https://nonexistent.invalid",
      "statusCode": null,
      "responseTime": null,
      "isAvailable": false,
      "checkedAt": "2026-09-24T10:15:31.910554Z"
    }
  ]
}
```

`statusCode` и `responseTime` равны `null`, если запрос не удался (DNS, таймаут, отказ соединения). `responseTime` в миллисекундах, `isAvailable = true` при коде 2xx.

### 3. Ошибки

Некорректный URL, `400 Bad Request`:

```bash
curl -i -X POST "$BASE_URL/api/v1/task" \
  -H "Content-Type: application/json" \
  -d '{"urls":["not-a-valid-url"]}'
# {"error":"Invalid URL: not-a-valid-url"}
```

Пустой список URL, `400 Bad Request` (стандартная ошибка валидации ASP.NET Core):

```bash
curl -i -X POST "$BASE_URL/api/v1/task" \
  -H "Content-Type: application/json" \
  -d '{"urls":[]}'
  # The field Urls must be a string or array type with a minimum length of '1'.
```

Задача не найдена, `404 Not Found` (пустое тело):

```bash
curl -i "$BASE_URL/api/v1/task/00000000-0000-0000-0000-000000000000"
```


---

### Логи

```bash
docker compose logs -f api
docker compose logs --tail=200 postgres
docker compose logs --tail=200 redis
```

Логи приложения в формате JSON. Ключевые события: `HealthCheckWorker started`, `Task ... queued`, `Processing task ...`, `Failed to check ...` (URL недоступен), `Task ... failed`, `Unhandled error in worker loop`.


