using System.Numerics;
using Engine.Core.Logging;

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
    }
} 