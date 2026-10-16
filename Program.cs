using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using Newtonsoft.Json;

namespace LoggingSystem
{

    public interface ILogger
    {
        void Log(string level, string message);
    }

    public class FileLogger : ILogger
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
    public class DatabaseLogger : ILogger
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

    public class RemoteLogger : ILogger
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

   
    public class Application
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

    public class ConsoleLogger : ILogger
    {
        public void Log(string level, string message)
        {
            Serilog.Log.Information("[CONSOLE] [{Level}] {Message}", level, message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/app.log")
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

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}