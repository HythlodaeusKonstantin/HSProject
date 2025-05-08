using System;
using System.Drawing;
using Engine.Core.Logging;
using Engine.Core.Rendering;
using Engine.Core.UI.Elements;

namespace Engine.Core.UI.Components
{
    /// <summary>
    /// Кнопка - интерактивный элемент UI
    /// </summary>
    public class UIButton : UIInteractiveElement
    {
        private UIText? _label;
        private int _zIndex;
        private Color _normalColor = Color.LightGray;
        private Color _hoverColor = Color.Silver;
        private Color _pressedColor = Color.Gray;
        private Color _disabledColor = Color.DarkGray;

        /// <summary>
        /// Текстовая метка кнопки
        /// </summary>
        public UIText? Label
        {
            get => _label;
            set
            {
                _label = value;
                if (_label != null)
                {
                    _label.Parent = this;
                }
            }
        }

        /// <summary>
        /// Цвет кнопки в нормальном состоянии
        /// </summary>
        public Color NormalColor
        {
            get => _normalColor;
            set => _normalColor = value;
        }

        /// <summary>
        /// Цвет кнопки при наведении
        /// </summary>
        public Color HoverColor
        {
            get => _hoverColor;
            set => _hoverColor = value;
        }

        /// <summary>
        /// Цвет кнопки при нажатии
        /// </summary>
        public Color PressedColor
        {
            get => _pressedColor;
            set => _pressedColor = value;
        }

        /// <summary>
        /// Цвет неактивной кнопки
        /// </summary>
        public Color DisabledColor
        {
            get => _disabledColor;
            set => _disabledColor = value;
        }

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
                    if (_label != null)
                    {
                        _label.ZIndex = value + 1;
                    }
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
        /// Создает новую кнопку
        /// </summary>
        /// <param name="logger">Логгер</param>
        public UIButton(ILogger logger) : base(logger)
        {
            _logger.Debug("Created UIButton");
        }

        /// <summary>
        /// Создает новую кнопку с текстом
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="text">Текст кнопки</param>
        public UIButton(ILogger logger, string text) : base(logger)
        {
            _label = new UIText(logger, text);
            _label.Parent = this;
            _label.ZIndex = ZIndex + 1;
            _label.Alignment = TextAlignment.Center;
            _label.Style = this.Style;
            _logger.Debug($"Created UIButton with text: '{text}'");
        }

        /// <summary>
        /// Обновление состояния кнопки
        /// </summary>
        /// <param name="deltaTime">Время с последнего обновления</param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_label != null)
            {
                // Обновляем позицию текста к центру кнопки
                _label.Position = new System.Numerics.Vector2(
                    Position.X + (Size.X - _label.Size.X) / 2,
                    Position.Y + (Size.Y - _label.Size.Y) / 2
                );
                _label.Update(deltaTime);
            }
        }

        /// <summary>
        /// Обработка событий мыши
        /// </summary>
        /// <param name="args">Аргументы события</param>
        public override void HandleMouseEvent(UIMouseEventArgs args)
        {
            base.HandleMouseEvent(args);
        }

        /// <summary>
        /// Отрисовка кнопки
        /// </summary>
        /// <param name="context">Контекст рендеринга</param>
        public override void Render(IRenderContext context)
        {
            if (!IsVisible)
                return;

            // Получаем стиль для текущего состояния
            var stateStyle = GetCurrentStateStyle();

            // Цвет кнопки: сначала из стиля, если нет — fallback на внутренние поля
            Color buttonColor = stateStyle.BackgroundColor ?? (
                !IsEnabled ? _disabledColor :
                (IsPressed && IsHovered) ? _pressedColor :
                IsHovered ? _hoverColor :
                _normalColor
            );

            // Прозрачность
            float opacity = stateStyle.Opacity ?? 1.0f;
            if (opacity < 1.0f)
                buttonColor = Color.FromArgb((int)(buttonColor.A * opacity), buttonColor.R, buttonColor.G, buttonColor.B);

            // Масштаб (scale) — можно использовать для анимации, если потребуется
            float scale = stateStyle.Scale ?? 1.0f;
            var bounds = Bounds;
            if (scale != 1.0f)
            {
                int newWidth = (int)(bounds.Width * scale);
                int newHeight = (int)(bounds.Height * scale);
                bounds = new Rectangle(
                    bounds.X + (bounds.Width - newWidth) / 2,
                    bounds.Y + (bounds.Height - newHeight) / 2,
                    newWidth,
                    newHeight
                );
            }

            // Отрисовка фона кнопки
            context.DrawRectangle(bounds, buttonColor, ZIndex * 0.01f);

            // Отрисовка текста
            _label?.Render(context);

            _logger.Debug($"Rendered UIButton at {bounds}");
        }

        /// <summary>
        /// Обработчик изменения z-индекса
        /// </summary>
        protected override void OnZIndexChanged()
        {
            base.OnZIndexChanged();
            if (_label != null)
            {
                _label.ZIndex = ZIndex + 1;
            }
            _logger.Debug($"UIButton z-index changed: {ZIndex}");
        }
    }

    /// <summary>
    /// Аргументы события UI
    /// </summary>
    public class UIEventArgs : EventArgs
    {
    }
} 