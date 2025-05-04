using System.Numerics;
using Engine.Core.ECS.Components;

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
        /// Создание нового UI компонента
        /// </summary>
        /// <param name="rootElement">Корневой элемент UI</param>
        public UIComponent(IUIElement rootElement)
        {
            RootElement = rootElement;
            Position = Vector2.Zero;
            Size = Vector2.One;
            PositionUnit = CoordinateUnit.Pixels;
            SizeUnit = CoordinateUnit.Percentage;
            IsVisible = true;
        }
        
        /// <summary>
        /// Создание нового UI компонента с заданными параметрами
        /// </summary>
        /// <param name="rootElement">Корневой элемент UI</param>
        /// <param name="position">Позиция</param>
        /// <param name="size">Размер</param>
        /// <param name="positionUnit">Единицы измерения для позиции</param>
        /// <param name="sizeUnit">Единицы измерения для размера</param>
        public UIComponent(IUIElement rootElement, Vector2 position, Vector2 size, 
                          CoordinateUnit positionUnit = CoordinateUnit.Pixels, 
                          CoordinateUnit sizeUnit = CoordinateUnit.Percentage)
        {
            RootElement = rootElement;
            Position = position;
            Size = size;
            PositionUnit = positionUnit;
            SizeUnit = sizeUnit;
            IsVisible = true;
        }
    }
} 