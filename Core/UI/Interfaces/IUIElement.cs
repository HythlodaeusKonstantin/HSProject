using System.Numerics;

namespace Engine.Core.UI
{
    /// <summary>
    /// Базовый интерфейс для всех UI элементов
    /// </summary>
    public interface IUIElement
    {
        /// <summary>
        /// Позиция элемента в системе координат родителя
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Размер элемента
        /// </summary>
        Vector2 Size { get; set; }

        /// <summary>
        /// Видимость элемента
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Активность элемента
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Родительский элемент
        /// </summary>
        IUIElement? Parent { get; set; }

        /// <summary>
        /// Обновление состояния элемента
        /// </summary>
        /// <param name="deltaTime">Время с последнего обновления</param>
        void Update(float deltaTime);
    }
} 