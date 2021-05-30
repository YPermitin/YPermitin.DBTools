# Набор библиотек для работы с различными СУБД

Набор небольших библиотек и приложений на .NET Core для упрощения работы с некоторыми СУБД в части задач разработки и администрирования.

| Проект | Актуальная версия | Описание |
| ----------- | ----------------- | -------- |
| YY.DBTools.Core | [![NuGet version](https://badge.fury.io/nu/YY.DBTools.Core.svg)](https://badge.fury.io/nu/YY.DBTools.Core) | Базовый пакет |
| YY.DBTools.SQLServer.XEvents | [![NuGet version](https://badge.fury.io/nu/YY.DBTools.SQLServer.XEvents.svg)](https://badge.fury.io/nu/YY.DBTools.SQLServer.XEvents) | Пакет для чтения файлов расширенных событий SQL Server |
| YY.DBTools.SQLServer.XEvents.ToClickHouse | [![NuGet version](https://badge.fury.io/nu/YY.DBTools.SQLServer.XEvents.ToClickHouse.svg)](https://badge.fury.io/nu/YY.DBTools.SQLServer.XEvents.ToClickHouse) | Пакет для экспорта расширенных событий SQL Server в базу ClickHouse |
| YY.DBTools.SQLServer.ExtendedEventsToClickHouse | [последний релиз](https://github.com/YPermitin/YY.DBTools/releases) | Консольное приложение для экспорта расширенных событий SQL Server в ClickHouse |

Последние новости об этой и других разработках, а также выходе других материалов, **[смотрите в Telegram-канале](https://t.me/DevQuietPlace)**.

## Состояние сборки

| Windows |  Linux |
|:-------:|:------:|
| - | ![.NET](https://github.com/YPermitin/YY.DBTools/workflows/.NET/badge.svg) |

## Приложение экспорта расширенных событий SQL Server в ClickHouse

**YY.DBTools.SQLServer.ExtendedEventsToClickHouse** - простое приложение для экспорта данных расширенных событий SQL Server в базу ClickHouse.

Для использования нужно сформировать файл конфигурации:

```json
{
  "ConnectionStrings": {
    "XEventsDatabase": "Host=<АдресСервера>;Port=8123;Username=<Пользователь>;password=<Пароль>;Database=<ИмяБазы>;"
  },
    "WatchMode": {
    "Use": true,
    "Periodicity": 10000
  },
  "Export": {
    "Buffer": {
      "MaxItemCountSize": 20000,
      "MaxSaveDurationMs": 60000,
      "MaxBufferSizeItemsCount": 50000
    }
  },
  "LogSources": [
    {
        "Name": "ИмяЛога №1",
        "Description": "Описание логов №1",
        "SourcePath": "C:\\Logs_1",
        "Portion": 10000,
        "TimeZone": ""
    },
    {
        "Name": "ИмяЛога №2",
        "Description": "Описание логов №2",
        "SourcePath": "C:\\Logs_2",
        "Portion": 10000,
        "TimeZone": ""
    }
  ]
}
```

Секция "ConnectionStrings" содержит строку подключения "XEventsDatabase" к базе данных для экспорта. База будет создана автоматически при первом запуске приложения. Также можно создать ее вручную, главное, чтобы структура была соответствующей. Имя строки подключения "XEventsDatabase" - это значение по умолчанию. Контекст приложения будет использовать ее автоматически, если это не переопределено разработчиком явно.

Секция "WatchMode" содержит настройки режима "наблюдения", в котором библиотеки будут отслеживать появление новых записей в файлах логов. Параметр "Use" включает или отключает этот режим работы, а "Periodicity" указываем в миллисекундах периодичность, с которой выполняется проверка появления новых данных.

Секция "Export.Buffer" содержит настройки работы буфера:

* MaxBufferSizeItemsCount - максимальное количество записей в буфере. По достижению этого размера запись в буфер останавливается.
* MaxItemCountSize - количество записей, по достижению которого выполняется экспорт данных из буфера и очистка буфера от ранее выгруженных данных.
* MaxSaveDurationMs - количество миллисекунд хранения записей в буфере. По истечению этого времени записи из буфера будут отправлены в хранилище в любом случае, независимо от количества записей в нем.

От настроек буфера зависит сколько памяти будет выделено для его работы и как часто будет выполняться запрос экспорта данных. Параметры нужно подбирать индивидуально, но можете для начала использовать стандартные настройки из примера и менять по обстоятельствам.

Секция "LogSources" содержит список параметров обработки технологических журналов, для каждого из которых указываются параметры:

* **Name** - имя источника логов.
* **Description** - описание источника логов.
* **SourcePath** - путь к каталогу с файлами логов Extended Events (*.xel).
* **Portion** - количество записей, передаваемых в одной порции в хранилище.
* **TimeZone** - часовой пояс логов для корректной обработки дат.

Кроме этого, само приложение имеет ряд параметров запуска:

* **config** - путь к файлу конфигурации. Если не указан, то в каталоге приложения ищется файл с именем "appsettings.json".
* **logDirectoryPath** - путь к каталогу, куда приложение будет сохранять логи. По умолчанию создается в том же месте, где и приложение.
* **AllowInteractiveCommands** - включает возможность интерактивной работы с приложением. Например, остановить экспорт по нажатию CTRL + C. По умолчанию включен. В некоторых сценариях может понадобиться отключение.

## Библиотека чтения расширенных событий SQL Server

**YY.DBTools.SQLServer.XEvents** - библиотека для чтения расширенных событий SQL Server из файлов *.xel. Внутри себя использует официальное решение от Microsoft - **[Microsoft.SqlServer.XEvent.XELite](https://www.nuget.org/packages/Microsoft.SqlServer.XEvent.XELite/)**, расширяя ее в части обработки событий и некоторых других моментах.

Пример использования ниже, а также можно посмотреть в примере приложения.

```csharp
using (var reader = new ExtendedEventsReader(eventPath,
    (sender, EventArgs) =>
    {
        // Событие при чтении каждого события
        // EventArgs.EventNumber - номер события
        // EventArgs.EventData - данные события
    },
    (sender, EventArgs) =>
    {
        // Событие при чтении метаданных файла
    },
    (sender, EventArgs) =>
    {
        // Событие перед чтением файла
        // EventArgs.Cancel - признак отмены чтения. Если установить в True, то файл будет пропущен
        // EventArgs.FileName - полный путь к файлу
    },
    (sender, EventArgs) =>
    {
        // Событие после чтением файла
        // EventArgs.FileName - полный путь к файлу
    },
    (sender, EventArgs) =>
    {
        // Событие при возникновении ошибок
        // EventArgs.Exception - информация об исключении
    }))
{
    await reader.StartReadEvents(CancellationToken.None);
}
```

Не обязательно передавать обработчики всех событий.

## Библиотека экспорта расширенный событий SQL Server

**YY.DBTools.SQLServer.ExtendedEventsToClickHouse** - библиотека экспорта расширенных событий SQL Server в ClickHouse.

Позволяет инициализировать базу данных ClickHouse универсальной структуры для хранения расширенных событий и выполнить их экспорт порциями.

Простой пример использования без использования общего буфреа:

```csharp
using (IXEventExportMaster exporter = new XEventExportMaster(
    (e) =>
    {
        // Событие перед отправкой данных
        // e.Cancel - признак отмены отправки. Если установить в True, то данные не будут отправлены в базу.
        // e.Rows - список событий для отправки
    },
    (e) =>
    {
        // Событие после отправки данных
        // e.CurrentPosition - информация о позиции чтения данных в файле с логами
        // e.CurrentPosition.Id - Идентификатор события внутри файла (обычно соответствует номеру события)
        // e.CurrentPosition.EventUUID - идентификатор события
        // e.CurrentPosition.CurrentFileData - путь к файлу данных
        // e.CurrentPosition.EventPeriod - период события
        // e.CurrentPosition.EventNumber - номер события
    },
    (e) =>
    {
        // Событие при возникновении ошибок
        // e.Exception - информация об исключении
    }))
{
    exporter.SetXEventsPath(_settings.XEventsPath);
    IXEventsOnTarget target = new ExtendedEventsOnClickHouse(
            ConnectionString, // Строка подключения к БД
            Portion // Размер порции выгрузки данных
        );
    exporter.SetTarget(target);
    await exporter.StartSendEventsToStorage(CancellationToken.None);
}
```

При первом подключении будет создана база данных ClickHouse для хранения событий XEvent любой структуры и с любым составом полей. Также есть возможность использования общего буфера при сборе событий из множества источников. Это позволяет оптимизировать запись в базу ClickHouse и значительно уменьшить количество операций пакетных вставок данных. Выше в приложения как-раз используется буфер. Общий пример его использования такой:

```csharp
IConfiguration Configuration = Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();
XEventsExportSettings exportConfig = XEventsExportSettings.Create(Configuration);
ExtendedEventsExport export = new ExtendedEventsExport(
                exportConfig, 
                new ExtendedEventsOnClickHouseBuilder());

export.OnErrorEvent += OnErrorExportData;
export.OnSendLogEvent += OnExportData;
await export.StartExport(CancellationToken.None);
```

Обработчики событий отправки данных и обработки ошибок могут быть следующие.

```csharp
#region Events

private static void OnErrorExportData(OnErrorExportSharedBufferEventArgs e)
{
    // Информация об исключении
    // e.Exception
}

private void OnExportData(BeforeExportDataEventArgs e)
{
    // Если действие отменено, то ничего не делаем
    if(e.Cancel) return;

    // Коллекция событий для экспорта
    // e.Rows
}

#endregion
```

Использовать или нет буфер - все зависит от задачи. Но если экспорт идет массовый, на постоянной основе и источников очень много, то это лучшее решение для оптимальной работы с ClickHouse.

## Сценарии использования

При анализе больших объемов расширенных событий лучше их сохранять в какое-либо хранилище. В классическом варианте их сохраняют в базу SQL Server, но это не всегда удобно и возможно.

Плюс ко всему, эта библиотека выполняет некоторые операции предобработки SQL-запросов, чтобы их было удобнее группировать (обработка имет временных таблицы, параметров и др.).

В итоге, обрабатывать расширенные события в базе ClickHouse - это быстро и эффективно.

## TODO

Планы в части разработки:

* Сделать Wiki с примерами использования библиотек и приложений
* Добавить возможность экспорта логов PostgreSQL
* Добавить возможность экспорта XEvents онлайн с помощью запросов к SQL Server
* Добавить онлайн получение данных логов для PostgreSQL
* В базы ClickHouse добавить специализированные представления для упрощения анализа данных
* Расширение unit-тестов библиотек и приложений

## Лицензия

MIT - делайте все, что посчитаете нужным. Никакой гарантии и никаких ограничений по использованию.
