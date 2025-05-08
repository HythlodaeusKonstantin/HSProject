using System.Numerics;
using Engine.Core.Logging;
using Engine.Core.UI.Elements;

namespace Engine.Core.UI
{
    /// <summary>
    /// Единицы измерения для UI координат
    /// </summary>
    public enum CoordinateUnit
    {
        /// <summary>
        /// Пиксели
        /// </summary>
        Pixels,

        /// <summary>
        /// Проценты от размера родителя
        /// </summary>
        Percentage,

        /// <summary>
        /// Единицы вьюпорта (0-1)
        /// </summary>
        ViewportUnits
    }

    /// <summary>
    /// Система координат для UI элементов
    /// </summary>
    public class UICoordinateSystem
    {
        private readonly ILogger _logger;
        private Vector2 _viewportSize;

        public UICoordinateSystem(ILogger logger)
        {
            _logger = logger;
            _viewportSize = Vector2.One;
        }

        /// <summary>
        /// Установить размер вьюпорта
        /// </summary>
        public void SetViewportSize(Vector2 size)
        {
            _viewportSize = size;
            _logger.Debug($"UI viewport size updated: {size}");
        }

        /// <summary>
        /// Преобразовать координаты в экранные с учетом якоря (Anchor)
        /// </summary>
        public Vector2 ToScreenCoordinates(Vector2 position, CoordinateUnit unit, Vector2 size, UIAnchor anchor)
        {
            // Сначала переводим позицию в экранные координаты
            var screenPos = ToScreenCoordinates(position, unit);
            var screenSize = ToScreenCoordinates(size, unit);
            // Смещение по якорю
            var offset = GetAnchorOffset(anchor, screenSize);
            return screenPos + offset;
        }

        /// <summary>
        /// Преобразовать координаты в экранные
        /// </summary>
        public Vector2 ToScreenCoordinates(Vector2 position, CoordinateUnit unit)
        {
            return unit switch
            {
                CoordinateUnit.Pixels => position,
                CoordinateUnit.Percentage => new Vector2(
                    position.X * _viewportSize.X / 100f,
                    position.Y * _viewportSize.Y / 100f
                ),
                CoordinateUnit.ViewportUnits => new Vector2(
                    position.X * _viewportSize.X,
                    position.Y * _viewportSize.Y
                ),
                _ => throw new System.ArgumentException($"Unsupported coordinate unit: {unit}")
            };
        }

        /// <summary>
        /// Преобразовать экранные координаты в нормализованные (0-1)
        /// </summary>
        public Vector2 ToNormalizedCoordinates(Vector2 screenPosition)
        {
            return new Vector2(
                screenPosition.X / _viewportSize.X,
                screenPosition.Y / _viewportSize.Y
            );
        }

        /// <summary>
        /// Конвертировать значение из одних единиц измерения в другие
        /// </summary>
        public Vector2 ConvertUnits(Vector2 value, CoordinateUnit fromUnit, CoordinateUnit toUnit)
        {
            if (fromUnit == toUnit) return value;

            // Сначала преобразуем в пиксели
            var pixels = ToScreenCoordinates(value, fromUnit);

            // Затем из пикселей в целевые единицы
            return toUnit switch
            {
                CoordinateUnit.Pixels => pixels,
                CoordinateUnit.Percentage => new Vector2(
                    pixels.X * 100f / _viewportSize.X,
                    pixels.Y * 100f / _viewportSize.Y
                ),
                CoordinateUnit.ViewportUnits => ToNormalizedCoordinates(pixels),
                _ => throw new System.ArgumentException($"Unsupported coordinate unit: {toUnit}")
            };
        }

        /// <summary>
        /// Применить пересчёт координат и размера к UI-элементу
        /// </summary>
        public void ApplyToElement(UIElementBase element)
        {
            if (element == null) return;
            element.Position = ToScreenCoordinates(element.Position, element.PositionUnit, element.Size, element.Anchor);
            element.Size = ToScreenCoordinates(element.Size, element.SizeUnit);
        }

        /// <summary>
        /// Вспомогательный метод для вычисления смещения по якорю
        /// </summary>
        private Vector2 GetAnchorOffset(UIAnchor anchor, Vector2 size)
        {
            return anchor switch
            {
                UIAnchor.TopLeft => Vector2.Zero,
                UIAnchor.TopCenter => new Vector2(-size.X / 2f, 0),
                UIAnchor.TopRight => new Vector2(-size.X, 0),
                UIAnchor.CenterLeft => new Vector2(0, -size.Y / 2f),
                UIAnchor.Center => new Vector2(-size.X / 2f, -size.Y / 2f),
                UIAnchor.CenterRight => new Vector2(-size.X, -size.Y / 2f),
                UIAnchor.BottomLeft => new Vector2(0, -size.Y),
                UIAnchor.BottomCenter => new Vector2(-size.X / 2f, -size.Y),
                UIAnchor.BottomRight => new Vector2(-size.X, -size.Y),
                _ => Vector2.Zero
            };
        }
    }
} 