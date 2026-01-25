# HassLanguage - DSL для умного дома

## Что реализовано

### ✅ Базовая структура проекта
- .NET 8 solution с проектами: Core, Parser, Runtime, HassLanguage (консольное приложение)
- Docker конфигурация для сборки

### ✅ ANTLR4 грамматика
- Lexer и Parser правила для всего синтаксиса DSL
- Поддержка home/area/device/entity иерархии
- Поддержка automation/when/call/wait конструкций
- Декораторы для automation и when
- Выражения с приоритетами операторов

### ✅ AST модель
- Полная модель AST для всех конструкций языка
- Типы данных: Duration, TimeOfDay, Range
- Выражения, условия, действия

### ✅ Семантическая валидация
- Проверка уникальности alias
- Проверка корректности ссылок на entities
- Symbol table для отслеживания всех определений

### ✅ Skeleton Runtime
- Регистрация automations
- Обработка when условий
- Заготовка для wait/for (неблокирующая реализация через state machine)
- Базовая структура для обработки событий

## Что НЕ реализовано (TODO)

1. **Генерация парсера из грамматики** - нужно запустить ANTLR4 tool для генерации кода
2. **Полная реализация expression evaluation** - сейчас возвращает false
3. **Реализация function calls** - нет подключения к Home Assistant API
4. **HTTP actions** - нет HttpClientFactory и обработки http.telegram.post
5. **Event normalization** - нет StateChangedEvent, CommandEvent, ScheduleEvent
6. **RabbitMQ поддержка** - нет multi-replica
7. **Type checking** - базовая проверка типов не реализована
8. **Декораторы runtime логика** - @mode, @cooldown, @lock, @edge, @max_age, @debounce не обрабатываются

## Как собрать

### Через Docker (рекомендуется)

```powershell
.\build.ps1
```

### Вручную (если установлен .NET SDK)

```powershell
dotnet restore HassLanguage.sln
dotnet build HassLanguage.sln -c Release
```

## Как запустить

После сборки:

```powershell
docker run --rm -v "${PWD}:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 `
    dotnet run --project src/HassLanguage/HassLanguage.csproj
```

## Следующие шаги

1. Установить ANTLR4 tool и сгенерировать парсер из HassLanguage.g4
2. Реализовать expression evaluator с поддержкой всех типов
3. Подключить Home Assistant WebSocket API
4. Реализовать HTTP actions с HttpClientFactory
5. Добавить полную поддержку декораторов
6. Реализовать event loop с RabbitMQ
