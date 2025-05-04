using System.Collections.Generic;
using System.Numerics;
using Engine.Core.UI.Elements;

namespace Engine.Core.UI.Events
{
    /// <summary>
    /// Типы событий UI
    /// </summary>
    public enum UIEventType
    {
        // События мыши
        MouseMove,
        MouseDown,
        MouseUp,
        MouseEnter,
        MouseLeave,
        Click,
        DoubleClick,
        MouseWheel,
        Drag,
        
        // События клавиатуры
        KeyDown,
        KeyUp,
        TextInput,
        
        // События касания
        TouchStart,
        TouchMove,
        TouchEnd,
        
        // События жестов
        Tap,
        DoubleTap,
        LongPress,
        Swipe,
        Pinch,
        Rotate,
        
        // События фокуса
        FocusGained,
        FocusLost,
        
        // События UI
        ValueChanged,
        StateChanged,
        VisibilityChanged,
        Resize
    }
    
    /// <summary>
    /// Интерфейс системы событий UI, отвечающей за обработку и распространение событий
    /// </summary>
    public interface IUIEventSystem
    {
        /// <summary>
        /// Обрабатывает событие UI
        /// </summary>
        /// <param name="evt">Событие для обработки</param>
        void ProcessEvent(UIEvent evt);
        
        /// <summary>
        /// Добавляет обработчик для указанного типа события
        /// </summary>
        /// <param name="type">Тип события</param>
        /// <param name="handler">Обработчик события</param>
        void AddHandler(UIEventType type, IUIEventHandler handler);
        
        /// <summary>
        /// Удаляет обработчик для указанного типа события
        /// </summary>
        /// <param name="type">Тип события</param>
        /// <param name="handler">Обработчик события</param>
        void RemoveHandler(UIEventType type, IUIEventHandler handler);
        
        /// <summary>
        /// Проверяет, есть ли обработчики для указанного типа события
        /// </summary>
        /// <param name="type">Тип события</param>
        /// <returns>True, если есть хотя бы один обработчик</returns>
        bool HasHandler(UIEventType type);
        
        /// <summary>
        /// Регистрирует интерактивный элемент в системе событий
        /// </summary>
        /// <param name="element">Интерактивный элемент</param>
        void RegisterInteractiveElement(UIInteractiveElement element);
        
        /// <summary>
        /// Удаляет регистрацию интерактивного элемента из системы событий
        /// </summary>
        /// <param name="element">Интерактивный элемент</param>
        void UnregisterInteractiveElement(UIInteractiveElement element);
        
        /// <summary>
        /// Получает UI элемент в указанной позиции
        /// </summary>
        /// <param name="position">Позиция для проверки</param>
        /// <returns>Найденный элемент или null</returns>
        IUIElement? GetElementAt(Vector2 position);
    }
} 