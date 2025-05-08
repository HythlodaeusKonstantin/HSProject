namespace Engine.Core.UI
{
    /// <summary>
    /// Интерфейс для интерактивных UI элементов
    /// </summary>
    public interface IUIInteractable : IUIElement
    {
        /// <summary>
        /// Может ли элемент принимать взаимодействие
        /// </summary>
        bool IsInteractable { get; set; }

        /// <summary>
        /// Находится ли элемент в фокусе
        /// </summary>
        bool IsFocused { get; set; }

        /// <summary>
        /// Находится ли курсор над элементом
        /// </summary>
        bool IsHovered { get; set; }
    }
} 