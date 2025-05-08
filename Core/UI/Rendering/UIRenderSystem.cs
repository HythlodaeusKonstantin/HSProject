using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Engine.Core.Logging;
using Engine.Core.Rendering;
using Engine.Core.ECS;

namespace Engine.Core.UI.Rendering
{
    /// <summary>
    /// Система рендеринга UI элементов
    /// </summary>
    public class UIRenderSystem : IUIRenderSystem, ISystem
    {
        private readonly ILogger _logger;
        private readonly IRenderContext _baseRenderContext;
        private readonly EntityManager _entityManager;
        private readonly Stack<Rectangle> _clipStack;
        private readonly Dictionary<int, List<IUIRenderable>> _layeredElements;
        
        private UIRenderContext _uiRenderContext;
        private Rectangle _viewport;
        private bool _isInitialized;
        
        /// <summary>
        /// Создание новой системы рендеринга UI
        /// </summary>
        /// <param name="renderContext">Базовый контекст рендеринга</param>
        /// <param name="logger">Логгер</param>
        /// <param name="entityManager">Менеджер сущностей</param>
        public UIRenderSystem(IRenderContext renderContext, ILogger logger, EntityManager entityManager)
        {
            _baseRenderContext = renderContext ?? throw new ArgumentNullException(nameof(renderContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _clipStack = new Stack<Rectangle>();
            _layeredElements = new Dictionary<int, List<IUIRenderable>>();
            _viewport = new Rectangle(0, 0, (int)renderContext.ViewportSize.X, (int)renderContext.ViewportSize.Y);
            _isInitialized = false;
            
            _logger.Debug("UIRenderSystem created");
        }
        
        /// <summary>
        /// Инициализация системы рендеринга (реализация ISystem)
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                _logger.Warning("UIRenderSystem already initialized");
                return;
            }
            _uiRenderContext = new UIRenderContext(_baseRenderContext, _logger);
            _isInitialized = true;
            _logger.Info("UIRenderSystem initialized");
        }
        
        /// <summary>
        /// Установка области отображения
        /// </summary>
        /// <param name="viewport">Прямоугольник области отображения</param>
        public void SetViewport(Rectangle viewport)
        {
            _viewport = viewport;
            _logger.Debug($"Viewport set to {viewport}");
        }
        
        /// <summary>
        /// Добавление региона отсечения в стек
        /// </summary>
        /// <param name="clipRect">Прямоугольник отсечения</param>
        public void PushClipRect(Rectangle clipRect)
        {
            if (_clipStack.Count > 0)
            {
                // Пересечение с текущей областью отсечения
                clipRect = IntersectRectangles(_clipStack.Peek(), clipRect);
            }
            
            _clipStack.Push(clipRect);
            _logger.Debug($"Pushed clip rect: {clipRect}");
        }
        
        /// <summary>
        /// Удаление верхнего региона отсечения из стека
        /// </summary>
        public void PopClipRect()
        {
            if (_clipStack.Count > 0)
            {
                _clipStack.Pop();
                _logger.Debug("Popped clip rect");
            }
            else
            {
                _logger.Warning("Attempted to pop clip rect from empty stack");
            }
        }
        
        /// <summary>
        /// Рендеринг UI элементов
        /// </summary>
        /// <param name="elements">Список элементов для рендеринга</param>
        public void Render(IEnumerable<IUIRenderable> elements)
        {
            if (!_isInitialized)
            {
                _logger.Error("UIRenderSystem not initialized");
                return;
            }
            
            // Явная инициализация UI-рендеринга
            if (!(_baseRenderContext is RenderSystem rs))
                throw new InvalidOperationException("UIRenderSystem: _baseRenderContext должен быть RenderSystem для корректного рендера UI");
            rs.InitUIRendering();
            rs.SaveGLState();
            
            if (elements == null)
            {
                _logger.Warning("Attempted to render null elements collection");
                return;
            }
            
            // Группировка элементов по z-индексу
            SortElementsByZIndex(elements);
            
            // Применение клиппинга из стека к UIRenderContext
            if (_clipStack.Count > 0)
            {
                _uiRenderContext.PushClipRect(_clipStack.Peek());
            }
            
            // Рендеринг элементов по слоям (z-index)
            RenderLayeredElements();
            
            // Сброс клиппинга
            if (_clipStack.Count > 0)
            {
                _uiRenderContext.PopClipRect();
            }
            
            // Очистка словаря слоев после рендеринга
            _layeredElements.Clear();
            rs.RestoreGLState();
        }
        
        /// <summary>
        /// Группировка элементов по z-индексу
        /// </summary>
        private void SortElementsByZIndex(IEnumerable<IUIRenderable> elements)
        {
            _layeredElements.Clear();
            
            foreach (var element in elements)
            {
                if (!IsElementVisible(element))
                    continue;
                
                int zIndex = element.ZIndex;
                
                if (!_layeredElements.ContainsKey(zIndex))
                {
                    _layeredElements[zIndex] = new List<IUIRenderable>();
                }
                
                _layeredElements[zIndex].Add(element);
            }
            
            _logger.Debug($"Sorted {elements.Count()} elements into {_layeredElements.Count} layers");
        }
        
        /// <summary>
        /// Рендеринг элементов по слоям
        /// </summary>
        private void RenderLayeredElements()
        {
            // Сортировка ключей по возрастанию для правильного порядка отрисовки (z-index)
            var sortedKeys = _layeredElements.Keys.OrderBy(k => k).ToList();
            
            foreach (var zIndex in sortedKeys)
            {
                var layerElements = _layeredElements[zIndex];
                
                foreach (var element in layerElements)
                {
                    try
                    {
                        element.Render(_uiRenderContext);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error rendering UI element: {ex.Message}");
                    }
                }
            }
            
            _logger.Debug($"Rendered {sortedKeys.Count} layers");
        }
        
        /// <summary>
        /// Проверка видимости элемента
        /// </summary>
        private bool IsElementVisible(IUIRenderable element)
        {
            _logger.Debug($"Проверка видимости: Bounds={element.Bounds}, Viewport={_viewport}");
            return element.Bounds.IntersectsWith(_viewport);
        }
        
        /// <summary>
        /// Пересечение двух прямоугольников
        /// </summary>
        private Rectangle IntersectRectangles(Rectangle a, Rectangle b)
        {
            int x = Math.Max(a.X, b.X);
            int y = Math.Max(a.Y, b.Y);
            int width = Math.Min(a.Right, b.Right) - x;
            int height = Math.Min(a.Bottom, b.Bottom) - y;
            
            if (width <= 0 || height <= 0)
                return Rectangle.Empty;
                
            return new Rectangle(x, y, width, height);
        }

        // Реализация ISystem
        public void Update(double deltaTime)
        {
            // Обычно рендер-система не обновляет состояние, только рендерит
        }

        public void Render()
        {
            // Собираем все UIComponent из ECS
            var uiEntities = _entityManager.QueryEntities(typeof(UIComponent));
            var renderables = new List<IUIRenderable>();
            foreach (var entity in uiEntities)
            {
                var uiComponent = _entityManager.GetComponent<UIComponent>(entity);
                if (uiComponent.RootElement is IUIRenderable renderable && uiComponent.IsVisible)
                {
                    renderables.Add(renderable);
                }
            }
            Render(renderables);
        }
    }
}