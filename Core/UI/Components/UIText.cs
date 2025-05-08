using System.Drawing;
using System.Numerics;
using Engine.Core.Logging;
using Engine.Core.Rendering;
using Engine.Core.UI.Elements;

namespace Engine.Core.UI.Components
{
    /// <summary>
    /// Выравнивание текста
    /// </summary>
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Текстовый элемент UI
    /// </summary>
    public class UIText : UIRenderableElement
    {
        private string _text = string.Empty;
        private float _fontSize = 12.0f;
        private Color _textColor = Color.Black;
        private TextAlignment _alignment = TextAlignment.Left;
        private bool _wordWrap = false;
        private float _lineSpacing = 1.0f;

        /// <summary>
        /// Текст для отображения
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    _logger.Debug($"UIText content changed: '{_text}'");
                }
            }
        }

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    _logger.Debug($"UIText font size changed: {_fontSize}");
                }
            }
        }

        /// <summary>
        /// Цвет текста
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    _logger.Debug($"UIText color changed: {_textColor}");
                }
            }
        }

        /// <summary>
        /// Выравнивание текста
        /// </summary>
        public TextAlignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment != value)
                {
                    _alignment = value;
                    _logger.Debug($"UIText alignment changed: {_alignment}");
                }
            }
        }

        /// <summary>
        /// Включить перенос слов
        /// </summary>
        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    _logger.Debug($"UIText word wrap changed: {_wordWrap}");
                }
            }
        }

        /// <summary>
        /// Межстрочный интервал
        /// </summary>
        public float LineSpacing
        {
            get => _lineSpacing;
            set
            {
                if (_lineSpacing != value)
                {
                    _lineSpacing = value;
                    _logger.Debug($"UIText line spacing changed: {_lineSpacing}");
                }
            }
        }

        /// <summary>
        /// Создает новый текстовый элемент
        /// </summary>
        /// <param name="logger">Логгер</param>
        public UIText(ILogger logger) : base(logger)
        {
            _logger.Debug("Created UIText");
        }

        /// <summary>
        /// Создает новый текстовый элемент
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="text">Текст</param>
        public UIText(ILogger logger, string text) : base(logger)
        {
            _text = text;
            _logger.Debug($"Created UIText with content: '{_text}'");
        }

        /// <summary>
        /// Отрисовка текста
        /// </summary>
        /// <param name="context">Контекст рендеринга</param>
        public override void Render(IRenderContext context)
        {
            if (!IsVisible || string.IsNullOrEmpty(_text))
                return;

            UpdateState();
            var stateStyle = GetCurrentStateStyle();
            var style = Style;
            var textScale = stateStyle.TextScale ?? style?.TextScale ?? 1.0f;
            var textOpacity = stateStyle.TextOpacity ?? style?.TextOpacity ?? 1.0f;
            var color = stateStyle.TextColor ?? style?.TextColor ?? _textColor;
            var scale = stateStyle.Scale ?? style?.Scale ?? 1.0f;
            var position = Position;

            // Простая реализация выравнивания (без учета переноса слов)
            if (_alignment == TextAlignment.Center)
            {
                position.X += Size.X / 2 - (_text.Length * _fontSize * 0.5f * scale * textScale);
            }
            else if (_alignment == TextAlignment.Right)
            {
                position.X += Size.X - (_text.Length * _fontSize * scale * textScale);
            }

            // Применяем прозрачность к цвету текста
            var finalColor = System.Drawing.Color.FromArgb(
                (int)(color.A * textOpacity),
                color.R, color.G, color.B
            );

            context.DrawText(_text, position, finalColor, _fontSize * scale * textScale / 12.0f, ZIndex * 0.01f);
            _logger.Debug($"Rendered UIText at {Bounds}: '{_text}'");
        }
    }
} 