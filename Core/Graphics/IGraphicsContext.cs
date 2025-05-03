using Engine.Core.Graphics;

namespace Engine.Core.Graphics
{
    /// <summary>
    /// Интерфейс для работы с графическим контекстом
    /// </summary>
    public interface IGraphicsContext
    {
        /// <summary>
        /// Инициализация графического контекста
        /// </summary>
        void Initialize();

        /// <summary>
        /// Очистка экрана указанным цветом
        /// </summary>
        /// <param name="color">Цвет очистки</param>
        void Clear(Color color);

        /// <summary>
        /// Завершение работы с графическим контекстом
        /// </summary>
        void Shutdown();
    }
} 