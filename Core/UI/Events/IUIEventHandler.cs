namespace Engine.Core.UI.Events
{
    /// <summary>
    /// Интерфейс обработчика событий UI
    /// </summary>
    public interface IUIEventHandler
    {
        /// <summary>
        /// Обрабатывает событие UI
        /// </summary>
        /// <param name="evt">Событие для обработки</param>
        /// <returns>True, если событие было обработано</returns>
        bool HandleEvent(UIEvent evt);
        
        /// <summary>
        /// Массив типов событий, которые обрабатываются этим обработчиком
        /// </summary>
        UIEventType[] HandledEventTypes { get; }
    }
} 