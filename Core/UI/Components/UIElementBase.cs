using System.Numerics;
using Engine.Core.Logging;

namespace Engine.Core.UI.Elements
{
    /// <summary>
    /// Базовый класс для всех UI элементов
    /// </summary>
    public abstract class UIElementBase : IUIElement
    {
        protected readonly ILogger _logger;

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public IUIElement? Parent { get; set; }
        public virtual CoordinateUnit PositionUnit { get; set; } = CoordinateUnit.Pixels;
        public virtual CoordinateUnit SizeUnit { get; set; } = CoordinateUnit.Pixels;
        public virtual UIAnchor Anchor { get; set; } = UIAnchor.TopLeft;
        public virtual UIStyle? Style { get; set; }

        protected UIElementBase(ILogger logger)
        {
            _logger = logger;
            Position = Vector2.Zero;
            Size = Vector2.One;

            _logger.Debug($"Created UI element: {GetType().Name}");
        }

        public virtual void Update(float deltaTime)
        {
            // Базовая реализация обновления
            if (!IsEnabled) return;

            // Дополнительная логика обновления в производных классах
        }

        protected virtual void OnPositionChanged()
        {
            _logger.Debug($"UI element position changed: {Position}");
        }

        protected virtual void OnSizeChanged()
        {
            _logger.Debug($"UI element size changed: {Size}");
        }

        protected virtual void OnVisibilityChanged()
        {
            _logger.Debug($"UI element visibility changed: {IsVisible}");
        }

        protected virtual void OnEnabledChanged()
        {
            _logger.Debug($"UI element enabled state changed: {IsEnabled}");
        }

        /// <summary>
        /// Возвращает эффективный стиль для элемента (ищет вверх по иерархии, если у текущего элемента стиль не задан)
        /// </summary>
        public UIStyle? GetEffectiveStyle()
        {
            UIElementBase? current = this;
            while (current != null)
            {
                if (current.Style != null)
                    return current.Style;
                current = current.Parent as UIElementBase;
            }
            return null;
        }
    }
} 