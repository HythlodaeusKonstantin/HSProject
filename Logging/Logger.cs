using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Engine.Core.Logging
{
    public enum LogType { Error, Warning, Info }

    public class LoggerConfig
    {
        public bool consoleOutput { get; set; } = true;
        public bool fileOutput { get; set; } = true;
        public List<string> excludeTypes { get; set; } = new();
        public List<string> excludeTags { get; set; } = new();
        public bool antispamEnabled { get; set; } = true;
        public int antispamIntervalMs { get; set; } = 1000;
    }

    public class Logger
    {
        private static readonly object _lock = new();
        private static LoggerConfig _config;
        private static readonly string ConfigPath = "LogStorage/logconfig.json";
        private static readonly string LogDir = "LogStorage";
        private static readonly ConcurrentDictionary<string, (string msg, DateTime time)> _lastLogs = new();

        static Logger()
        {
            LoadConfig();
            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);
            // Удаляем старые логи при запуске
            foreach (var file in Directory.GetFiles(LogDir, "*.log"))
            {
                if (!file.EndsWith("logconfig.json", StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        public static void ReloadConfig() => LoadConfig();

        private static void LoadConfig()
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _config = JsonSerializer.Deserialize<LoggerConfig>(json) ?? new LoggerConfig();
            }
            else
            {
                _config = new LoggerConfig();
            }
        }

        public static void Log(LogType type, string tag, string message)
        {
            if (_config.excludeTypes.Contains(type.ToString(), StringComparer.OrdinalIgnoreCase)) return;
            if (_config.excludeTags.Contains(tag, StringComparer.OrdinalIgnoreCase)) return;

            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{type}] [{tag}] {message}";
            string key = $"{type}|{tag}|{message}";

            if (_config.antispamEnabled)
            {
                if (_lastLogs.TryGetValue(key, out var last))
                {
                    if ((DateTime.Now - last.time).TotalMilliseconds < _config.antispamIntervalMs)
                        return;
                }
                _lastLogs[key] = (message, DateTime.Now);
            }

            if (_config.consoleOutput)
            {
                switch (type)
                {
                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                }
                Console.WriteLine(logLine);
                Console.ResetColor();
            }

            if (_config.fileOutput)
            {
                string fileName = $"{LogDir}/{type}_{tag}.log";
                lock (_lock)
                {
                    // Проверка размера файла: если больше 10 МБ — очищаем
                    const long maxLogSize = 10 * 1024 * 1024;
                    try
                    {
                        if (File.Exists(fileName) && new FileInfo(fileName).Length > maxLogSize)
                        {
                            File.WriteAllText(fileName, "");
                        }
                    }
                    catch { }
                    File.AppendAllText(fileName, logLine + Environment.NewLine);
                }
            }
        }
    }

    public class LoggerAdapter : ILogger
    {
        public void Log(string message)
        {
            Logger.Log(LogType.Info, "Default", message);
        }

        public void Log(LogType type, string tag, string message)
        {
            Logger.Log(type, tag, message);
        }

        public void Debug(string message)
        {
            Logger.Log(LogType.Info, "Debug", message);
        }

        public void Info(string message)
        {
            Logger.Log(LogType.Info, "Info", message);
        }

        public void Warning(string message)
        {
            Logger.Log(LogType.Warning, "Warning", message);
        }

        public void Error(string message)
        {
            Logger.Log(LogType.Error, "Error", message);
        }
    }
}
