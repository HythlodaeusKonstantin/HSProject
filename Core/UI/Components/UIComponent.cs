using System.Numerics;
using Engine.Core.ECS.Components;
using Engine.Core.Logging;

namespace Engine.Core.UI
{
    /// <summary>
    /// Компонент для UI элементов в ECS
    /// </summary>
    public class UIComponent : IComponent
    {
        /// <summary>
        /// Корневой UI элемент
        /// </summary>
        public IUIElement RootElement { get; }
        
        /// <summary>
        /// Позиция компонента
        /// </summary>
        public Vector2 Position { get; set; }
        
        /// <summary>
        /// Размер компонента
        /// </summary>
        public Vector2 Size { get; set; }
        
        /// <summary>
        /// Единицы измерения для позиции
        /// </summary>
        public CoordinateUnit PositionUnit { get; set; }
        
        /// <summary>
        /// Единицы измерения для размера
        /// </summary>
        public CoordinateUnit SizeUnit { get; set; }
        
        /// <summary>
        /// Видимость компонента
        /// </summary>
        public bool IsVisible { get; set; }
        
        /// <summary>
        /// Привязка UI элемента (якорь)
        /// </summary>
        public UIAnchor Anchor { get; set; }
        
        /// <summary>
        /// Стиль компонента (UIStyle)
        /// </summary>
        public UIStyle? Style { get; set; }
        
        /// <summary>
        /// Создание нового UI компонента
        /// </summary>
        /// <param name="rootElement">Корневой элемент UI</param>
        /// <param name="style">Стиль компонента</param>
        public UIComponent(IUIElement rootElement, UIStyle? style = null)
        {
            RootElement = rootElement;
            Position = Vector2.Zero;
            Size = Vector2.One;
            PositionUnit = CoordinateUnit.Pixels;
            SizeUnit = CoordinateUnit.Percentage;
            IsVisible = true;
            Anchor = UIAnchor.TopLeft;
            Style = style;
        }
        
        /// <summary>
        /// Создание нового UI компонента с заданными параметрами
        /// </summary>
        /// <param name="rootElement">Корневой элемент UI</param>
        /// <param name="position">Позиция</param>
        /// <param name="size">Размер</param>
        /// <param name="positionUnit">Единицы измерения для позиции</param>
        /// <param name="sizeUnit">Единицы измерения для размера</param>
        /// <param name="anchor">Привязка</param>
        /// <param name="style">Стиль компонента</param>
        public UIComponent(IUIElement rootElement, Vector2 position, Vector2 size, 
                          CoordinateUnit positionUnit = CoordinateUnit.Pixels, 
                          CoordinateUnit sizeUnit = CoordinateUnit.Percentage,
                          UIAnchor anchor = UIAnchor.TopLeft,
                          UIStyle? style = null)
        {
            RootElement = rootElement;
            Position = position;
            Size = size;
            PositionUnit = positionUnit;
            SizeUnit = sizeUnit;
            IsVisible = true;
            Anchor = anchor;
            Style = style;
        }

        public static UIComponent CreateButton(
            string text,
            Vector2 position,
            Vector2 size,
            CoordinateUnit positionUnit = CoordinateUnit.Pixels,
            CoordinateUnit sizeUnit = CoordinateUnit.Percentage,
            UIAnchor anchor = UIAnchor.TopLeft,
            UIStyle? style = null,
            ILogger? logger = null)
        {
            logger ??= new Logging.LoggerAdapter();
            var button = new Components.UIButton(logger, text)
            {
                Position = position,
                Size = size,
                PositionUnit = positionUnit,
                SizeUnit = sizeUnit,
                Anchor = anchor,
                Style = style ?? new UIStyle()
            };
            return new UIComponent(button, position, size, positionUnit, sizeUnit, anchor, style);
        }
    }

    /// <summary>
    /// Типы привязки UI элементов
    /// </summary>
    public enum UIAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
} 