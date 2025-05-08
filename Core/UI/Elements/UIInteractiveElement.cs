using System;
using System.Numerics;
using Engine.Core.Logging;

namespace Engine.Core.UI.Elements
{
    /// <summary>
    /// Базовый класс для интерактивных UI элементов
    /// </summary>
    public abstract class UIInteractiveElement : UIRenderableElement, IUIInteractable
    {
        public bool IsInteractable { get; set; } = true;
        public bool IsFocused { get; set; }
        public bool IsHovered { get; set; }
        public bool IsPressed { get; set; }

        /// <summary>
        /// Делегат для метода обновления состояния.
        /// Используется для перехвата и модификации логики обновления состояния.
        /// </summary>
        public Action UpdateStateMethod { get; set; }

        public event EventHandler<UIMouseEventArgs>? OnMouseEnter;
        public event EventHandler<UIMouseEventArgs>? OnMouseLeave;
        public event EventHandler<UIMouseEventArgs>? OnMouseDown;
        public event EventHandler<UIMouseEventArgs>? OnMouseUp;
        public event EventHandler<UIMouseEventArgs>? OnClick;

        protected UIInteractiveElement(ILogger logger) : base(logger)
        {
            // Инициализируем делегат стандартным методом
            UpdateStateMethod = () => UpdateState();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!IsEnabled || !IsVisible || !IsInteractable)
                return;

            // Дополнительная логика обновления в производных классах
        }

        /// <summary>
        /// Обработка события мыши
        /// </summary>
        /// <param name="args">Аргументы события мыши</param>
        public virtual void HandleMouseEvent(UIMouseEventArgs args)
        {
            if (!IsEnabled || !IsVisible || !IsInteractable)
                return;

            switch (args.EventType)
            {
                case UIMouseEventType.Enter:
                    IsHovered = true;
                    OnMouseEnter?.Invoke(this, args);
                    _logger.Debug($"Mouse enter: {GetType().Name}");
                    break;
                case UIMouseEventType.Leave:
                    IsHovered = false;
                    IsPressed = false;
                    OnMouseLeave?.Invoke(this, args);
                    _logger.Debug($"Mouse leave: {GetType().Name}");
                    break;
                case UIMouseEventType.Down:
                    IsPressed = true;
                    OnMouseDown?.Invoke(this, args);
                    _logger.Debug($"Mouse down: {GetType().Name}");
                    break;
                case UIMouseEventType.Up:
                    if (IsPressed && IsHovered)
                    {
                        OnClick?.Invoke(this, args);
                        _logger.Debug($"Click: {GetType().Name}");
                    }
                    IsPressed = false;
                    OnMouseUp?.Invoke(this, args);
                    _logger.Debug($"Mouse up: {GetType().Name}");
                    break;
            }

            // Вызываем метод обновления состояния при изменении состояния мыши
            UpdateStateMethod?.Invoke();
        }

        protected virtual void OnInteractableChanged()
        {
            _logger.Debug($"UI element interactable state changed: {IsInteractable}");
            UpdateStateMethod?.Invoke();
        }

        protected virtual void OnFocusChanged()
        {
            _logger.Debug($"UI element focus state changed: {IsFocused}");
            UpdateStateMethod?.Invoke();
        }
        
        /// <summary>
        /// Обновление состояния элемента для стилизации
        /// </summary>
        protected override void UpdateState()
        {
            if (!IsEnabled)
            {
                CurrentState = UIState.Disabled;
                return;
            }
            
            if (IsPressed && IsHovered)
            {
                CurrentState = UIState.Pressed;
            }
            else if (IsHovered)
            {
                CurrentState = UIState.Hover;
            }
            else if (IsFocused)
            {
                CurrentState = UIState.Focused;
            }
            else
            {
                CurrentState = UIState.Normal;
            }
        }
    }
} 