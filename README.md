# Набор библиотек для работы с различными СУБД

Набор небольших библиотек и приложений на .NET для упрощения работы с некоторыми СУБД в части задач разработки и администрирования.

| Проект                                          | Актуальная версия                                                   | Описание |
|-------------------------------------------------|---------------------------------------------------------------------| -------- |
| YPermitin.DBTools.Core                          | -                                                                   | Базовый пакет |
| YPermitin.DBTools.SQLServer.XEvents                    | -                                                                   | Пакет для чтения файлов расширенных событий SQL Server |
| YPermitin.DBTools.SQLServer.XEvents.ToClickHouse       | -                                                                   | Пакет для экспорта расширенных событий SQL Server в базу ClickHouse |
| YPermitin.DBTools.SQLServer.ExtendedEventsToClickHouse | [последний релиз](https://github.com/YPermitin/YY.DBTools/releases) | Консольное приложение для экспорта расширенных событий SQL Server в ClickHouse |
| [YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper](Libs/YPermitin.DBTools.SQLServer.BulkCopyProgramWrapper/README.md) | - | Библиотека-оболочка для работы с утилитой Bulk Copy Program (BCP) |

Общими задачами проекта являются:

* Упрощение обработки большого объема логов и выделения важных событий.
* Упрощенная диагностика некоторых нештатных ситуаций.
* Помощь в мониторинге и обработке логов.
* Создание единого похода в анализе работы различных СУБД, по крайней мере там, где это возможно.

Подробную информацию по каждой разработкам и примеры использования можно найти в **[Wiki проекта](https://github.com/YPermitin/YY.DBTools/wiki)**.

## Состояние сборки

| Windows | Linux |
|:-------:|:-----:|
| - |   -   |

## TODO

Планы в части разработки:

* Добавить возможность экспорта логов PostgreSQL
* Добавить возможность экспорта XEvents онлайн с помощью запросов к SQL Server
* Добавить онлайн получение данных логов для PostgreSQL
* В базы ClickHouse добавить специализированные представления для упрощения анализа данных
* Расширение unit-тестов библиотек и приложений

## Лицензия

MIT - делайте все, что посчитаете нужным. Никакой гарантии и никаких ограничений по использованию.
