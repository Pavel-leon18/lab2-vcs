using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Serilog;
using Newtonsoft.Json;

namespace LoggingSystem
{
    /// <summary>
    /// Интерфейс для системы логирования. Определяет контракт для всех логгеров.
    /// </summary>
    internal interface ILogger
    {
        void Log(string level, string message);
    }

    /// <summary>
    /// Логгер, записывающий события в файл с помощью Serilog.
    /// </summary>
    internal sealed class FileLogger : ILogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Log(string level, string message)
        {
            Serilog.Log.Information("[FILE] Записано в {FilePath}: [{Level}] {Message}",
                _filePath, level, message);
        }
    }

    /// <summary>
    /// Логгер, симулирующий запись событий в базу данных.
    /// </summary>
    internal sealed class DatabaseLogger : ILogger
    {
        private readonly string _connectionString;

        public DatabaseLogger(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Log(string level, string message)
        {
            Serilog.Log.Information("[DATABASE] (Connection: {ConnString}) [{Level}] {Message}",
                _connectionString, level, message);
        }
    }

    /// <summary>
    /// Логгер, симулирующий отправку событий на удалённый сервер.
    /// </summary>
    internal sealed class RemoteLogger : ILogger
    {
        private readonly string _apiUrl;

        public RemoteLogger(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public void Log(string level, string message)
        {
            Serilog.Log.Information("[REMOTE] (API: {ApiUrl}) [{Level}] {Message}",
                _apiUrl, level, message);
        }
    }

    /// <summary>
    /// Основной модуль приложения, который использует внедрённые логгеры.
    /// </summary>
    internal sealed class Application
    {
        private readonly List<ILogger> _loggers;

        public Application(List<ILogger> loggers)
        {
            _loggers = loggers;
        }

        public void LogEvent(string level, string message)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(level, message);
            }
        }

        public void Run()
        {
            LogEvent("INFO", "Приложение запущено");
            LogEvent("WARNING", "Это предупреждение");
            LogEvent("ERROR", "Произошла ошибка");
            LogEvent("INFO", "Приложение завершает работу");
        }
    }

    /// <summary>
    /// Логгер, выводящий события в консоль.
    /// </summary>
    internal sealed class ConsoleLogger : ILogger
    {
        public void Log(string level, string message)
        {
            Serilog.Log.Information("[CONSOLE] [{Level}] {Message}", level, message);
        }
    }

    /// <summary>
    /// Точка входа в программу.
    /// </summary>
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            // Исправление CA1305: указываем InvariantCulture для форматирования
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.File("logs/app.log", formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            Serilog.Log.Information("========== СИСТЕМА ЛОГИРОВАНИЯ ==========");
            
            var fileLogger = new FileLogger("app.log");
            var dbLogger = new DatabaseLogger("Server=localhost;Database=Logs");
            var remoteLogger = new RemoteLogger("https://api.logging-service.com/logs");

            var loggers = new List<ILogger>
            {
                fileLogger,
                dbLogger,
                remoteLogger
            };

            Serilog.Log.Information("--- Базовый запуск (3 логгера) ---");
            var app = new Application(loggers);
            app.Run();

            Serilog.Log.Information("--- Расширение (добавляем ConsoleLogger) ---");
            var consoleLogger = new ConsoleLogger();
            var extendedLoggers = new List<ILogger>(loggers) { consoleLogger };
            var extendedApp = new Application(extendedLoggers);
            extendedApp.Run();
            
            Serilog.Log.Information("--- Сохранение списка логгеров в JSON ---");

            var loggersInfo = new List<object>
            {
                new { Type = "FileLogger",     Path = "app.log" },
                new { Type = "DatabaseLogger", Connection = "Server=localhost;Database=Logs" },
                new { Type = "RemoteLogger",   Url = "https://api.logging-service.com/logs" },
                new { Type = "ConsoleLogger",  Info = "Цветной вывод в консоль" }
            };

            string json = JsonConvert.SerializeObject(loggersInfo, Formatting.Indented);
            File.WriteAllText("loggers.json", json);

            Serilog.Log.Information("Список логгеров сохранён в loggers.json");
            Serilog.Log.Information("========== ЗАВЕРШЕНИЕ РАБОТЫ ==========");
            Serilog.Log.CloseAndFlush();

            // Исправление CA1303: строка вынесена в переменную (в реальном проекте — в ресурсы)
          
            Console.ReadKey();
        }
    }
}