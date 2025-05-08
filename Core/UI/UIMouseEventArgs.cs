using System;
using System.Numerics;

namespace Engine.Core.UI
{
    /// <summary>
    /// Типы событий мыши
    /// </summary>
    public enum UIMouseEventType
    {
        Enter,
        Leave,
        Down,
        Up,
        Move
    }

    /// <summary>
    /// Аргументы событий мыши для UI элементов
    /// </summary>
    public class UIMouseEventArgs : EventArgs
    {
        /// <summary>
        /// Тип события мыши
        /// </summary>
        public UIMouseEventType EventType { get; }

        /// <summary>
        /// Позиция мыши относительно элемента
        /// </summary>
        public Vector2 LocalPosition { get; }

        /// <summary>
        /// Позиция мыши в экранных координатах
        /// </summary>
        public Vector2 ScreenPosition { get; }

        /// <summary>
        /// Использованная кнопка мыши
        /// </summary>
        public int Button { get; }

        /// <summary>
        /// Создает новый экземпляр аргументов событий мыши
        /// </summary>
        /// <param name="eventType">Тип события</param>
        /// <param name="localPosition">Локальная позиция относительно элемента</param>
        /// <param name="screenPosition">Позиция в экранных координатах</param>
        /// <param name="button">Номер кнопки мыши (0 - левая, 1 - правая, 2 - средняя)</param>
        public UIMouseEventArgs(UIMouseEventType eventType, Vector2 localPosition, Vector2 screenPosition, int button = 0)
        {
            EventType = eventType;
            LocalPosition = localPosition;
            ScreenPosition = screenPosition;
            Button = button;
        }
    }
} 