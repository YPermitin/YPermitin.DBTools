# Библиотека-оболочка для использования утилиты BCP (Bulk Copy Program)

Библиотека-оболочка для работы с утилитой [Bulk Copy Program (BCP)](https://learn.microsoft.com/ru-ru/sql/tools/bcp-utility?view=sql-server-ver16).

## Функциональность

Основные возможности библиотеки:

* Поддержка всех возможных операций утилиты [BCP](https://learn.microsoft.com/ru-ru/sql/tools/bcp-utility?view=sql-server-ver16) и их настроек:
	* Выгрузка данных в файл из таблицы / представления.
	* Выгрузка данных в файл по произвольному запросу.
	* Загрузка данных из файла в таблицу / представление.
	* Создание файла форматирования для описания источника данных.
* Надежный запуск утилиты BCP с обработкой ошибок и защиты от зависания.

## Окружение для разработки

Для окружение разработчика необходимы:

* [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [Visual Studio 2022](https://visualstudio.microsoft.com/ru/vs/)
* [SQL Server 2012+](https://www.microsoft.com/ru-ru/sql-server/sql-server-downloads)

## Примеры использования

Несколько сценариев использования.

### Выгруза данных таблицы в файл данных

Выгрузка данных таблицы в файл либо с помощью собственных типов SQL Server, либо в текстовом формате. 

При этом указываем аутентификацию через учетную запись SQL Server, а также включаем переадресацию вывода утилиты в текстовый файл.

```csharp
BCP bcpExport = new BCP();

// Настройка соединения со SQL Server
bcpExport.Config.ConnectionSettings
    .WithServerName("localhost")
    .WithDatabaseName("MyDatabase")
    .WithSQLServerAuthentication(
        loginId: "sa",
        password: "Passsword");

// Настройка объектов для импорта / экспорта
bcpExport.Config.ImportExportSettings
    // Указываем операцию экспорта данных таблицы в файл
    .ExportFromTableToFile(
        table: "TableName",
        dataFile: "C:\\Temp\\TableName.bak",
        schema: "dbo");

// Настройка BULK-операций
bool useNativeDataType = true;
if (useNativeDataType) {
    // Использовать встроенные типы данных SQL Server для выгрузки в файл.
    // Один из самых производительных вариантов выгрузки.
    bcpExport.Config.BulkSettings
        .WithNativeDataTypes(
            useNativeDataTypes: true,
            withCharactersSupport: true);
}
else {
    // Использовать символьный тип данных SQL Server для выгрузки в файл.
    // В результатирующем файле данные будут храниться в виде текста.
    bcpExport.Config.BulkSettings
        .WithCharacterType(
            useCharacterType: true,
            withUnicode: true);
}

// Дополнительные настройки
bcpExport.Config.AdditionalSettings
    // Указываем файл для сохранения вывода утилиты BCP
    .WithOutputFile("C:\\Temp\\TableName.log");

// Запускаем утилиту
bcpExport.Execute();

// Результат можно узнать через свойство
// bcpExport.LastExecutionResult
//  * Success - операция успешно выполнена
//  * Message - сообщение по результатам запуска
//  * ErrorMessage - сообщение об ошибках
//  * ExitCode - код возврата при завершении работы утилиты BCP
// Примечание: при указании опции OutputFile (как выше), сообщения будут пустыми, т.к. переадресованы в файл.

// Вызвать исключение, если были ошибки при выполнении операции.
bcpExport.ThrowExceptionIfError();
```

### Загрузка данных из файла в таблицу

Загрузим выгруженные данные из примера выше в новую таблицу.

```csharp
BCP bcpExport = new BCP();

// Настройка соединения со SQL Server
bcpExport.Config.ConnectionSettings
    .WithServerName("localhost")
    .WithDatabaseName("MyDatabase")
    .WithSQLServerAuthentication(
        loginId: "sa",
        password: "Passsword");

// Настройка объектов для импорта / экспорта
bcpExport.Config.ImportExportSettings
    // Указываем загрузку данных файла в таблицу
    .ImportFromFileToTable(
        table: "TableName",
        dataFile: "C:\\Temp\\TableName.bak",
        schema: "dbo",
        // Задаем размер пакета в 10000 элементов на одну транзакцию
        batchSize: 10000);

// Настройка BULK-операций. Настройка должна совпадать с той, которая использовалась при выгрузке.
bool useNativeDataType = true;
if (useNativeDataType) {
    // Использовать встроенные типы данных SQL Server для выгрузки в файл.
    // Один из самых производительных вариантов выгрузки.
    bcpExport.Config.BulkSettings
        .WithNativeDataTypes(
            useNativeDataTypes: true,
            withCharactersSupport: true);
}
else {
    // Использовать символьный тип данных SQL Server для выгрузки в файл.
    // В результатирующем файле данные будут храниться в виде текста.
    bcpExport.Config.BulkSettings
        .WithCharacterType(
            useCharacterType: true,
            withUnicode: true);
}

// Дополнительные настройки
bcpExport.Config.AdditionalSettings
    // Указываем файл для сохранения вывода утилиты BCP
    .WithOutputFile("C:\\Temp\\TableName.log");

// Запускаем утилиту
bcpExport.Execute();

// Результат можно узнать через свойство
// bcpExport.LastExecutionResult
//  * Success - операция успешно выполнена
//  * Message - сообщение по результатам запуска
//  * ErrorMessage - сообщение об ошибках
//  * ExitCode - код возврата при завершении работы утилиты BCP
// Примечание: при указании опции OutputFile (как выше), сообщения будут пустыми, т.к. переадресованы в файл.

// Вызвать исключение, если были ошибки при выполнении операции.
bcpExport.ThrowExceptionIfError();
```

### Проверка доступности BCP и ее версии

Перед выполнением каких-либо действий можно проверить доступность утилиты BCP и ее версию.

```csharp
BCP bcp = new BCP();

// Проверяем доступна ли утилита BCP
bool available = bcp.Available();
if(available)
{
    // Если доступна, то узнаем версию
    string version = bcp.Version();
    // Например, версия 15.0.1300.359
}
```

## Другие параметры и возможности

Класс настроек подробно документирован и в целом работает в рамках логики [официальной документации](https://learn.microsoft.com/ru-ru/sql/tools/bcp-utility?view=sql-server-ver16).

Настройки можно устанавливать как напрямую в поля, что позволяет делать настройки полностью под контролем.

```csharp
BCP bcpExport = new BCP();

// Настройка соединения со SQL Server
bcpImport.Config.ConnectionSettings.ServerName = "localhost";
bcpImport.Config.ConnectionSettings.DatabaseName = "MyDatabase";
bcpImport.Config.ConnectionSettings.LoginId = "sa";
bcpImport.Config.ConnectionSettings.Password = "Password";
bcpImport.Config.ConnectionSettings.UseTrustedConnection = false;
```

Или можно использовать некоторые вспомогательные методы, например вот так.

```csharp
BCP bcpExport = new BCP();

// Настройка соединения со SQL Server
bcpExport.Config.ConnectionSettings
    .WithServerName("localhost")
    .WithDatabaseName("MyDatabase")
    .WithSQLServerAuthentication(
        loginId: "sa",
        password: "Passsword");
```

В любом случае, нужно читать официальную документацию для корректной и эффективной работы с инструментом.