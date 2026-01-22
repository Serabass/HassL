
> Ты — senior software engineer.
> Твоя задача — начать реализацию **DSL для умного дома** (rules engine), строго по описанию ниже.
> Не упрощай модель, не заменяй концепции “чем-то похожим”. Если что-то неясно — оставляй TODO, но не меняй архитектуру.

---

### 1. Общая цель

Нужно реализовать **язык конфигурации (DSL)** для автоматизаций умного дома, поверх Home Assistant, с такими свойствами:

* C-подобный синтаксис со `{ }`
* человекочитаемый, интуитивный
* event-driven (WebSocket / RabbitMQ)
* неблокирующий (никаких sleep, wait = корутины / state machine)
* поддержка условий “удерживается N времени”
* поддержка декораторов `@decorator(...)`

Стек:

* **C# / .NET 8**
* Парсер: **ANTLR4** либо **Sprache** (первый предпочтительнее потому что новый для меня, второй привычнее)
* Архитектура: `parser → AST → semantic validation → runtime engine`

---

### 2. Иерархия DSL

Строгая иерархия:

```
home
 └─ room(s)
     └─ device(s)
         └─ entities[]
```

Пример:

```c
home "MyFlat" {
  room "Kitchen" kitchen {
    device "Light" light {
      entities: [
        light ceiling { id: "light.kitchen_ceiling"; },
        light counter { id: "light.kitchen_counter"; }
      ];
    }

    device "Sensors" sensors {
      entities: [
        binary_sensor motion { id: "binary_sensor.kitchen_motion"; },
        sensor illuminance    { id: "sensor.kitchen_lux"; unit: "lx"; }
      ];
    }
  }
}
```

* строка в кавычках = display name (для UI/логов)
* идентификатор без кавычек = alias (используется в коде)
* ссылки вида: `kitchen.sensors.motion`

---

### 3. Типы данных (обязательно)

Примитивы:

* `bool`
* `int`
* `float`

  * `int → float` разрешено
  * `float → int` запрещено без явного каста

Строки:

* `string`

Время:

* `Duration` → `30s`, `5m`, `2h`
* `TimeOfDay` → `07:00`, `23:30`
* `DateTime` → `2026-01-22T07:00:00+05:00`

Диапазоны:

* `Range<T>`
* синтаксис: `a..b`
* `Range<TimeOfDay>` поддерживает диапазон через полночь (`23:00..06:00`)

Примеры:

```c
now().time in 23:00..06:00
value(sensor.temp) in 25.0..30.0
```

---

### 4. Automation + when + do

Базовый синтаксис:

```c
automation "Kitchen motion light" {
  when sensors.motion == "on" {
    do light.turn_on(light.ceiling, { brightness: 70 });
    wait sensors.motion == "off" for 40s timeout 10m;
    do light.turn_off(light.ceiling);
  }
}
```

`wait` / `for`:

* **НЕ блокирует поток**
* реализуется как ожидание события + таймер
* если условие стало false → таймер сбрасывается

---

### 5. Условия с удержанием (`for`)

```c
when living.climate.temp > 25.0 for 30m {
  do climate.set_mode(living.climate.ac, "cool");
}
```

Семантика:

* условие должно быть истинным **непрерывно** 30 минут
* любое нарушение → сброс таймера
* срабатывает **один раз** (edge-triggered)

---

### 6. Комбинированные условия

Поддерживаются:

#### AND / OR через блоки

```c
when all for 20m {
  living.climate.temp > 25.0;
  now().time in 09:00..23:00;
  living.windows.main == "closed";
} {
  do climate.set_mode(living.climate.ac, "cool");
}
```

```c
when any {
  living.climate.temp > 28.0;
  living.humidity > 70;
} {
  do notify.telegram("Проветри!");
}
```

#### Сахар

```c
when for 20m {
  A;
  B;
}
```

Эквивалентно:

```c
when all for 20m { A; B; }
```

---

### 7. Декораторы (СТРОГО ОГРАНИЧЕНЫ)

Декораторы разрешены **ТОЛЬКО**:

1. На `automation`
2. На `when`

#### Automation decorators

```c
@mode(restart)
@cooldown(10s)
@lock("bath_light", 2s)
automation "Bathroom motion light" { ... }
```

Поддерживаемые:

* `@mode(single|restart|queued(n)|parallel(n))`
* `@cooldown(duration)`
* `@lock(key, ttl)`

#### When decorators

```c
automation "AC auto" {
  @edge(rising)
  @max_age(5m)
  when living.climate.temp > 25.0 for 30m {
    ...
  }
}
```

Поддерживаемые:

* `@edge(rising|falling|both)`
* `@max_age(duration)`
* `@debounce(duration)`

---

### 8. HTTP action

Нужно действие `do http.request(...)` с именованными клиентами.

Пример:

```c
do http.telegram.post(
  format("/bot{0}/sendMessage", secret("TG_BOT_TOKEN")),
  json({
    chat_id: secret("TG_CHAT_ID"),
    text: "Свет включен"
  })
);
```

Требования:

* HttpClientFactory
* таймауты
* ретраи
* allowlist хостов
* секреты не логируются

---

### 9. Архитектура рантайма

* все события нормализуются:

  * StateChangedEvent
  * CommandEvent
  * ScheduleEvent
* engine = event loop + state machines
* `wait` / `for` = регистрация ожиданий, НЕ блокировка
* поддержка multi-replica (RabbitMQ)

---

### 10. Что нужно сделать в первую очередь

1. ANTLR grammar (lexer + parser)
2. AST модель
3. Семантическая валидация:

   * уникальность alias
   * корректные ссылки
   * типы условий
4. Skeleton runtime:

   * регистрация automations
   * обработка `when`
   * заготовка под `wait / for`

❗ Не реализовывай всё сразу.
❗ Не меняй синтаксис.
❗ Если что-то не реализуешь — оставь TODO с комментарием почему.

---
