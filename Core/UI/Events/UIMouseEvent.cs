using System.Numerics;
using Engine.Core.UI.Elements;
using Silk.NET.Input;

namespace Engine.Core.UI.Events
{
    /// <summary>
    /// Класс событий мыши для UI
    /// </summary>
    public class UIMouseEvent : UIEvent
    {
        /// <summary>
        /// Позиция курсора относительно элемента
        /// </summary>
        public Vector2 Position { get; }
        
        /// <summary>
        /// Изменение позиции курсора с прошлого события
        /// </summary>
        public Vector2 Delta { get; }
        
        /// <summary>
        /// Кнопка мыши, связанная с событием
        /// </summary>
        public MouseButton Button { get; }
        
        /// <summary>
        /// Флаг, указывающий на двойной клик
        /// </summary>
        public bool IsDoubleClick { get; }
        
        /// <summary>
        /// Уровень давления для устройств с поддержкой давления
        /// </summary>
        public float PressureLevel { get; }
        
        /// <summary>
        /// Создает новое событие мыши
        /// </summary>
        /// <param name="type">Тип события</param>
        /// <param name="target">Целевой элемент</param>
        /// <param name="position">Позиция курсора</param>
        /// <param name="delta">Изменение позиции</param>
        /// <param name="button">Кнопка мыши</param>
        /// <param name="isDoubleClick">Флаг двойного клика</param>
        /// <param name="pressureLevel">Уровень давления</param>
        public UIMouseEvent(
            UIEventType type, 
            IUIElement target, 
            Vector2 position, 
            Vector2 delta = default, 
            MouseButton button = MouseButton.Left, 
            bool isDoubleClick = false, 
            float pressureLevel = 1.0f) 
            : base(type, target)
        {
            Position = position;
            Delta = delta;
            Button = button;
            IsDoubleClick = isDoubleClick;
            PressureLevel = pressureLevel;
        }
        
        /// <summary>
        /// Конвертирует в существующий формат UIMouseEventArgs
        /// </summary>
        /// <returns>Аргументы события мыши в старом формате</returns>
        public UIMouseEventArgs ToMouseEventArgs()
        {
            var eventType = Type switch
            {
                UIEventType.MouseEnter => UIMouseEventType.Enter,
                UIEventType.MouseLeave => UIMouseEventType.Leave,
                UIEventType.MouseDown => UIMouseEventType.Down,
                UIEventType.MouseUp => UIMouseEventType.Up,
                UIEventType.MouseMove => UIMouseEventType.Move,
                _ => UIMouseEventType.Move
            };
            
            int button = Button switch
            {
                MouseButton.Left => 0,
                MouseButton.Right => 1,
                MouseButton.Middle => 2,
                _ => 0
            };
            
            return new UIMouseEventArgs(eventType, Position, Position, button);
        }
        
        /// <summary>
        /// Создает UIMouseEvent из существующего UIMouseEventArgs
        /// </summary>
        /// <param name="args">Аргументы события мыши в старом формате</param>
        /// <param name="type">Тип события UI</param>
        /// <param name="target">Целевой элемент</param>
        /// <returns>Событие мыши в новом формате</returns>
        public static UIMouseEvent FromMouseEventArgs(UIMouseEventArgs args, UIEventType type, IUIElement target)
        {
            MouseButton button = args.Button switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Right,
                2 => MouseButton.Middle,
                _ => MouseButton.Left
            };
            
            return new UIMouseEvent(type, target, args.LocalPosition, default, button);
        }
    }
    
    /// <summary>
    /// Класс событий колеса мыши для UI
    /// </summary>
    public class UIMouseWheelEvent : UIMouseEvent
    {
        /// <summary>
        /// Величина прокрутки колеса
        /// </summary>
        public float WheelDelta { get; }
        
        /// <summary>
        /// Флаг инверсии прокрутки
        /// </summary>
        public bool IsInverted { get; }
        
        /// <summary>
        /// Создает новое событие колеса мыши
        /// </summary>
        /// <param name="target">Целевой элемент</param>
        /// <param name="position">Позиция курсора</param>
        /// <param name="wheelDelta">Величина прокрутки</param>
        /// <param name="isInverted">Флаг инверсии прокрутки</param>
        public UIMouseWheelEvent(
            IUIElement target, 
            Vector2 position, 
            float wheelDelta, 
            bool isInverted = false) 
            : base(UIEventType.MouseWheel, target, position)
        {
            WheelDelta = wheelDelta;
            IsInverted = isInverted;
        }
    }
    
    /// <summary>
    /// Перечисление кнопок мыши
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        Button4,
        Button5
    }
} 