# Примеры использования HassLanguage DSL

## Простые примеры

### 1. Базовый пример (`examples/basic-example.hass`)

Минимальный пример с одной комнатой и простой автоматизацией:

```c
zone "TestHome" {
  area "TestArea" test {
    device "TestDevice" test {
      entities: [
        binary_sensor test_sensor { id: "binary_sensor.test"; }
      ];
    }
  }
}

automation "Simple test" {
  when test.test.test_sensor == "on" {
    call notify.telegram("Sensor activated!");
  }
}
```

### 2. Простой пример (`examples/simple-example.hass`)

Более полный пример с несколькими комнатами и автоматизациями:

- Кухня с освещением и датчиками движения
- Гостиная с климат-контролем
- Автоматизация включения света при движении
- Автоматизация кондиционера при высокой температуре
- Уведомления при экстремальных условиях

### 3. Сложный пример (`examples/complex-example.hass`)

Демонстрирует:
- Комбинированные условия (`all for 20m`)
- Диапазоны времени (через полночь: `23:00..06:00`)
- Диапазоны значений (`temp in 20.0..24.0`)
- Декораторы (`@mode`, `@cooldown`, `@edge`, `@max_age`)
- Wait с timeout

## Как запустить примеры

### Через консольное приложение

```powershell
# Использовать встроенный пример
dotnet run --project src/HassLanguage/HassLanguage.csproj

# Или указать файл с примером
dotnet run --project src/HassLanguage/HassLanguage.csproj -- examples/simple-example.hass
```

### Через Docker

```powershell
docker run --rm -v "${PWD}:/workspace" -w /workspace mcr.microsoft.com/dotnet/sdk:8.0 `
    dotnet run --project src/HassLanguage/HassLanguage.csproj -- examples/simple-example.hass
```

## Структура примеров

Все примеры следуют одной структуре:

1. **Определение zone** - корневой контейнер
2. **Определение areas** - зоны в доме
3. **Определение devices** - устройства в комнатах
4. **Определение entities** - конкретные сущности (лампы, датчики)
5. **Определение automations** - правила автоматизации

## Основные конструкции

### Иерархия
```
zone "Name" alias {
  area "Name" alias {
    device "Name" alias {
      entities: [
        entity_type alias { id: "entity.id"; }
      ];
    }
  }
}
```

### Автоматизация
```c
automation "Name" {
  when условие {
    call действие();
    wait условие for время timeout время;
  }
}
```

### Условия
```c
// Простое условие
when sensor.temp > 25.0 { ... }

// С удержанием
when sensor.temp > 25.0 for 30m { ... }

// Комбинированные (все)
when all for 20m {
  условие1;
  условие2;
} { ... }

// Комбинированные (любое)
when any {
  условие1;
  условие2;
} { ... }
```

### Декораторы
```c
@mode(restart)
@cooldown(10s)
automation "Name" {
  @edge(rising)
  @debounce(2s)
  when условие { ... }
}
```
