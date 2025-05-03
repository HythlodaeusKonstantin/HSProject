namespace Engine.Core.Logging
{
    /// <summary>
    /// Интерфейс для логирования
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Записать простое сообщение в лог
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Записать сообщение с уровнем и тегом
        /// </summary>
        void Log(LogType type, string tag, string message);

        /// <summary>
        /// Логировать отладочное сообщение
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Логировать информационное сообщение
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Логировать предупреждение
        /// </summary>
        void Warning(string message);

        /// <summary>
        /// Логировать ошибку
        /// </summary>
        void Error(string message);
    }
} 