using System.Collections.Generic;
using System.Drawing;

namespace Engine.Core.UI.Rendering
{
    /// <summary>
    /// Интерфейс системы рендеринга UI элементов
    /// </summary>
    public interface IUIRenderSystem
    {
        /// <summary>
        /// Инициализация системы рендеринга
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Рендеринг UI элементов
        /// </summary>
        /// <param name="elements">Список элементов для рендеринга</param>
        void Render(IEnumerable<IUIRenderable> elements);
        
        /// <summary>
        /// Установка области отображения
        /// </summary>
        /// <param name="viewport">Прямоугольник области отображения</param>
        void SetViewport(Rectangle viewport);
        
        /// <summary>
        /// Добавление региона отсечения в стек
        /// </summary>
        /// <param name="clipRect">Прямоугольник отсечения</param>
        void PushClipRect(Rectangle clipRect);
        
        /// <summary>
        /// Удаление верхнего региона отсечения из стека
        /// </summary>
        void PopClipRect();
    }
} 