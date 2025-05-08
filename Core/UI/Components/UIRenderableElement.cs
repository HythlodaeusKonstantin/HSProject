using System.Numerics;
using System.Drawing;
using Engine.Core.Logging;
using Engine.Core.Rendering;

namespace Engine.Core.UI.Elements
{
    /// <summary>
    /// Базовый класс для рендерируемых UI элементов
    /// </summary>
    public abstract class UIRenderableElement : UIElementBase, IUIRenderable
    {
        private int _zIndex;
        
        /// <summary>
        /// Z-индекс для определения порядка отрисовки
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex != value)
                {
                    _zIndex = value;
                    OnZIndexChanged();
                }
            }
        }

        /// <summary>
        /// Границы элемента в экранных координатах
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                var x = (int)Position.X;
                var y = (int)Position.Y;
                var width = (int)Size.X;
                var height = (int)Size.Y;
                return new Rectangle(x, y, width, height);
            }
        }
        
        /// <summary>
        /// Стиль элемента
        /// </summary>
        public UIStyle? Style { get; set; }
        
        /// <summary>
        /// Текущее состояние элемента для стилизации
        /// </summary>
        public UIState CurrentState { get; protected set; } = UIState.Normal;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger">Логгер</param>
        protected UIRenderableElement(ILogger logger) : base(logger)
        {
            Style = new UIStyle();
        }

        /// <summary>
        /// Отрисовка элемента
        /// </summary>
        /// <param name="context">Контекст рендеринга</param>
        public abstract void Render(IRenderContext context);
        
        /// <summary>
        /// Обработчик изменения z-индекса
        /// </summary>
        protected virtual void OnZIndexChanged()
        {
            _logger.Debug($"UI element z-index changed: {ZIndex}");
        }
        
        /// <summary>
        /// Обновление состояния элемента для стилизации
        /// </summary>
        protected virtual void UpdateState()
        {
            if (!IsEnabled)
            {
                CurrentState = UIState.Disabled;
                return;
            }
            
            // Базовая реализация для всех элементов
            CurrentState = UIState.Normal;
        }
        
        /// <summary>
        /// Получает стиль для текущего состояния
        /// </summary>
        protected UIStyleState GetCurrentStateStyle()
        {
            if (Style?.States.TryGetValue(CurrentState, out var stateStyle) == true)
            {
                return stateStyle;
            }
            return Style?.States[UIState.Normal] ?? new UIStyleState();
        }
    }
} 