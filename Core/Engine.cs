using System;
using Engine.Core.Graphics;
using Engine.Core.Windowing;
using Engine.Core.ECS;
using Engine.Core.Input;
using Engine.Core.Logging;

namespace Engine.Core
{
    /// <summary>
    /// Основная реализация движка приложения
    /// </summary>
    public class Engine : IEngine
    {
        private readonly IWindowService _windowService;
        private readonly IGraphicsContext _graphicsContext;
        private readonly SystemManager _systemManager;
        private readonly IInputService _inputService;
        private readonly ILogger? _logger;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

     public Engine(
            IWindowService windowService,
            IGraphicsContext graphicsContext,
            SystemManager systemManager,
            IInputService inputService,
            ILogger? logger = null)
        {
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _graphicsContext = graphicsContext ?? throw new ArgumentNullException(nameof(graphicsContext));
            _systemManager = systemManager ?? throw new ArgumentNullException(nameof(systemManager));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _logger = logger;
        }

        /// <summary>
        /// Запустить главный цикл
        /// </summary>
        public void Run()
        {
            _isRunning = true;
            // Пример простого главного цикла
            while (_isRunning)
            {
                // Здесь должна быть логика обновления и рендеринга
                // Для примера просто завершаем цикл
                _isRunning = false;
            }
        }

        public void InitializeSystems()
        {
            // TODO: Реализовать инициализацию систем
        }

        public void UpdateAndRender(double deltaTime)
        {
            // TODO: Реализовать обновление и рендеринг
        }

        public void OnResize(Silk.NET.Maths.Vector2D<int> size)
        {
            // TODO: Реализовать обработку изменения размера окна
        }
    }
}
