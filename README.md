# RaceTiming

[![CI](https://github.com/SomeoneUnlicensed/RaceTiming/actions/workflows/ci.yml/badge.svg)](https://github.com/SomeoneUnlicensed/RaceTiming/actions/workflows/ci.yml)

Backend хронометража спортивного мероприятия: арки на трассе шлют события прохождения точек,
API копит их и строит результат участника — без мгновенного `DNF` из-за одного опоздавшего
или потерянного запроса.

**Стек:** ASP.NET Core 8 Web API · EF Core 8 · SQLite · Swagger/OpenAPI · xUnit

## Ключевая бизнес-логика: `Unknown`, а не мгновенный `DNF`

Пока нет события `FINISH`, участник не становится `DNF` автоматически. Статус считается так:

| Условие | Статус |
|---|---|
| Есть проход через checkpoint типа `Finish` | `Finished` |
| `Finish` ещё нет, но окно результатов (`Event.ResultsCutoff` после `Event.StartTime`) не истекло | `Unknown` |
| `Finish` нет, и окно истекло | `Dnf` |

Если `FINISH` придёт позже с задержкой, статус пересчитывается в `Finished`, даже если дедлайн
уже прошёл. Правило — чистая функция [`ResultStatusCalculator`](src/RaceTiming.Api/Services/ResultStatusCalculator.cs),
покрыта unit-тестами на все граничные случаи.

## Идемпотентность

Повторная доставка одного и того же события арки (потеря ACK, повторная отправка батча) не
должна создавать вторую запись. Это гарантируется на уровне БД: уникальный индекс по
`(ParticipantId, CheckpointId, Timestamp)` в [`RaceTimingDbContext`](src/RaceTiming.Api/Data/RaceTimingDbContext.cs).
[`CheckpointPassRecorder`](src/RaceTiming.Api/Services/CheckpointPassRecorder.cs) перед вставкой
одним запросом подтягивает уже существующие ключи для всех участников батча и отфильтровывает
дубликаты — без отдельного `SELECT` на каждую из потенциально тысячи записей.

## Пачка событий и `Task.WhenAll`

Эндпоинт `POST /api/checkpoints/batch` принимает массив событий одним запросом — ровно сценарий
«арка потеряла связь, затем прислала 500 результатов разом». Внутри один `SaveChangesAsync`
с `CancellationToken`, а не цикл с отдельным сохранением на каждую запись.

`DbContext` не потокобезопасен, поэтому запись в БД — один последовательный `SaveChangesAsync`.
`Task.WhenAll` используется только после него, для независимых [каналов уведомлений](src/RaceTiming.Api/Services/NotificationChannels.cs)
(аудит-лог, инвалидация кэша), которые с `DbContext` не работают.

## Эндпоинты

```
GET  /api/events/{id}
GET  /api/events/{id}/participants
GET  /api/participants/{bibNumber}?eventId=      # eventId опционален, нужен только если номер не уникален глобально
GET  /api/results/{eventId}
POST /api/checkpoints/pass                        # одно событие в реальном времени
POST /api/checkpoints/batch                        # пачка событий (burst-загрузка)
```

Два пересекающихся batch-эндпоинта из ТЗ (`/api/checkpoints/events` и `/api/checkpoints/{id}/batch`)
объединены в один `POST /api/checkpoints/batch`: арка знает своё имя чекпоинта, но не наш
внутренний `id`, а батч принадлежит событию, а не отдельной точке трассы.

Пример запроса на точку прохода:

```json
POST /api/checkpoints/pass
{
  "eventId": 1,
  "participantNumber": 1042,
  "checkpoint": "finish",
  "timestamp": "2026-10-04T18:42:13Z"
}
```

Пример результата:

```json
GET /api/results/1
[
  {
    "bibNumber": 1042,
    "name": "Иван Петров",
    "status": "Finished",
    "finishTime": "2026-10-04T18:42:13+00:00",
    "splits": [
      { "checkpoint": "start", "timestamp": "..." },
      { "checkpoint": "5km", "timestamp": "..." },
      { "checkpoint": "10km", "timestamp": "..." },
      { "checkpoint": "finish", "timestamp": "2026-10-04T18:42:13+00:00" }
    ]
  }
]
```

## Запуск

```bash
dotnet run --project src/RaceTiming.Api
```

Swagger UI: `http://localhost:5009/swagger`. При первом запуске база (SQLite-файл
`racetiming.db`) создаётся миграциями и наполняется демо-данными — см.
[`DbInitializer`](src/RaceTiming.Api/Data/DbInitializer.cs). Старт события привязан к моменту
запуска, поэтому сценарий `Unknown` воспроизводится при каждом запуске.

## Тесты

```bash
dotnet test
```

- `ResultStatusCalculatorTests` — unit-тесты правила `Unknown`/`Finished`/`Dnf`, включая
  граничные случаи (ровно на дедлайне, поздний финиш после дедлайна).
- `CheckpointIdempotencyTests` — интеграционные тесты через `WebApplicationFactory` поверх
  реального приложения (те же middleware и DI, что в проде) и изолированной in-memory SQLite:
  повтор одного события, дубликат внутри батча, неизвестные bib/checkpoint не роняют запрос.

## Структура

```
src/RaceTiming.Api/
  Domain/            # Event, Participant, Checkpoint, CheckpointPass, enums
  Data/               # DbContext, миграции, сид демо-данных
  Dtos/               # контракты запросов/ответов
  Services/           # бизнес-логика: статус, запись проходов, уведомления
  Controllers/        # тонкие контроллеры поверх сервисов
  Middleware/         # маппинг доменных исключений в ProblemDetails
tests/RaceTiming.Tests/
```
