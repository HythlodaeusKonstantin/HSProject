using System;
using Engine.Core.UI.Elements;

namespace Engine.Core.UI.Events
{
    /// <summary>
    /// Базовый класс для всех событий UI
    /// </summary>
    public abstract class UIEvent
    {
        /// <summary>
        /// Тип события
        /// </summary>
        public UIEventType Type { get; }
        
        /// <summary>
        /// Целевой элемент, на котором произошло событие
        /// </summary>
        public IUIElement Target { get; }
        
        /// <summary>
        /// Текущий элемент, обрабатывающий событие в процессе распространения
        /// </summary>
        public IUIElement? CurrentTarget { get; set; }
        
        /// <summary>
        /// Флаг, указывающий, продолжает ли событие распространяться
        /// </summary>
        public bool IsPropagating { get; private set; } = true;
        
        /// <summary>
        /// Флаг, указывающий, было ли событие обработано
        /// </summary>
        public bool IsHandled { get; set; }
        
        /// <summary>
        /// Временная метка события (время создания)
        /// </summary>
        public float Timestamp { get; }
        
        /// <summary>
        /// Создает новое событие UI
        /// </summary>
        /// <param name="type">Тип события</param>
        /// <param name="target">Целевой элемент</param>
        protected UIEvent(UIEventType type, IUIElement target)
        {
            Type = type;
            Target = target;
            CurrentTarget = target;
            Timestamp = (float)DateTime.Now.Subtract(DateTime.Today).TotalSeconds;
        }
        
        /// <summary>
        /// Останавливает распространение события по иерархии
        /// </summary>
        public void StopPropagation()
        {
            IsPropagating = false;
        }
        
        /// <summary>
        /// Предотвращает действие события по умолчанию
        /// </summary>
        public void PreventDefault()
        {
            IsHandled = true;
        }
    }
} 