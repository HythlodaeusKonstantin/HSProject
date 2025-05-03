namespace Engine.Core
{
    /// <summary>
    /// Интерфейс основного движка приложения
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Запустить главный цикл
        /// </summary>
        void Run();

        /// <summary>
        /// Признак того, что движок запущен
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Инициализация всех систем движка
        /// </summary>
        void InitializeSystems();

        /// <summary>
        /// Обновить и отрисовать кадр
        /// </summary>
        void UpdateAndRender(double deltaTime);

        /// <summary>
        /// Обработка изменения размера окна
        /// </summary>
        void OnResize(Silk.NET.Maths.Vector2D<int> size);
    }
} 