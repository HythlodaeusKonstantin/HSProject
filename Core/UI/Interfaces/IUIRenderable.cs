using Engine.Core.Rendering;
using System.Drawing;

namespace Engine.Core.UI
{
    /// <summary>
    /// Интерфейс для отрисовки UI элементов
    /// </summary>
    public interface IUIRenderable
    {
        /// <summary>
        /// Отрисовка элемента
        /// </summary>
        /// <param name="context">Контекст рендеринга</param>
        void Render(IRenderContext context);

        /// <summary>
        /// Границы элемента в экранных координатах
        /// </summary>
        Rectangle Bounds { get; }

        /// <summary>
        /// Z-индекс для определения порядка отрисовки
        /// </summary>
        int ZIndex { get; set; }
    }
} 