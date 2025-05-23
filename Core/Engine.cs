using System;
using Engine.Core.Graphics;
using Engine.Core.Windowing;
using Engine.Core.ECS;
using Engine.Core.Input;
using Engine.Core.Logging;
using Engine.Core.ECS.Components;
using Engine.Core.Rendering;
using Engine.Core.UI.Systems;

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
        private EntityManager _entityManager;
        private readonly IInputService _inputService;
        private readonly ILogger? _logger;
        private bool _isRunning;
        private RenderSystem _renderSystem;
        private UISystem _uiSystem;

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
            _windowService.Initialize();
            _graphicsContext.Initialize();
            _isRunning = true;
        }

        public void InitializeSystems()
        {
            var gl = (_graphicsContext as GraphicsContext)?.GL;
            if (gl == null)
                throw new InvalidOperationException("GL context is not initialized");

            _entityManager = new EntityManager();
            var shaderManager = new ShaderManager(gl, _logger);

            // 1. Система сцены (создаёт начальные сущности)
            var sceneSystem = new SceneSystem(gl, _entityManager, shaderManager);
            _systemManager.RegisterSystem(sceneSystem);
            sceneSystem.Initialize();

            // 2. Система акторов
            var actorSystem = new ActorSystem(_entityManager);
            _systemManager.RegisterSystem(actorSystem);
            actorSystem.Initialize();

            // 3. Система управления камерой
            var cameraControllerSystem = new CameraControllerSystem(_entityManager, _inputService, _logger, 5.0f, 0.1f, _windowService);
            _systemManager.RegisterSystem(cameraControllerSystem);

            // 4. Система рендера
            _renderSystem = new RenderSystem(gl, _entityManager, shaderManager);
            _systemManager.RegisterSystem(_renderSystem);

            // 5. Система трансформаций
            var transformSystem = new TransformSystem(_entityManager);
            _systemManager.RegisterSystem(transformSystem);

            // 6. UI: координатная система
            var uiCoordinateSystem = new UI.UICoordinateSystem(_logger!);

            // 8. UI Render System
            var uiRenderSystem = new UI.Rendering.UIRenderSystem(_renderSystem, _logger!, _entityManager);
            _systemManager.RegisterSystem(uiRenderSystem);
            uiRenderSystem.Initialize();

            // 7. UI System
            _uiSystem = new UI.Systems.UISystem(_logger!, uiCoordinateSystem, _entityManager, _windowService, uiRenderSystem, _inputService);
            _systemManager.RegisterSystem(_uiSystem);
            _uiSystem.Initialize();


        }

        public void UpdateAndRender(double deltaTime)
        {
            // 1. Обработка событий ввода в начале кадра
            _inputService.Update();

            // 2. Обновление логики ECS
            _systemManager.UpdateAll(deltaTime);

            // 3. Подготовка рендера: очистка экрана
            _graphicsContext.Clear(new Color(0.2f, 0.2f, 0.2f, 1f));

            // 4. Рендеринг всех систем (включая RenderSystem)
            _systemManager.RenderAll();
        }

        public void OnResize(Silk.NET.Maths.Vector2D<int> size)
        {
            // 1. Обновить viewport OpenGL
            var gl = (_graphicsContext as GraphicsContext)?.GL;
            gl?.Viewport(0, 0, (uint)size.X, (uint)size.Y);

            // 2. Обновить FBO и проекционную матрицу
            _renderSystem?.ResizeFbo(size.X, size.Y);
            _renderSystem?.ResizeUI(size.X, size.Y);
            _renderSystem?.UpdateProjectionMatrix((float)size.X / size.Y);
            _uiSystem?.SyncAll(new System.Numerics.Vector2(size.X, size.Y));

            // 3. Обновить aspect ratio у всех камер
            if (_entityManager != null)
            {
                float aspectRatio = (float)size.X / size.Y;
                var cameraEntities = _entityManager.QueryEntities(typeof(CameraComponent));
                foreach (var entity in cameraEntities)
                {
                    var camera = _entityManager.GetComponent<CameraComponent>(entity);
                    camera.AspectRatio = aspectRatio;
                    _entityManager.AddComponent(entity, camera);
                }
            }
        }
    }
}
