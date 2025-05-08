using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Engine.Core.Rendering;
using Engine.Core.Logging;

namespace Engine.Core.UI.Rendering
{
    /// <summary>
    /// Контекст рендеринга для UI элементов
    /// </summary>
    public class UIRenderContext : IRenderContext
    {
        private readonly IRenderContext _baseContext;
        private readonly ILogger _logger;
        private readonly Stack<Rectangle> _clipStack;
        private float _opacity;
        
        /// <summary>
        /// Текущая область отсечения
        /// </summary>
        public Rectangle CurrentClipRect => _clipStack.Count > 0 ? _clipStack.Peek() : new Rectangle(0, 0, (int)ViewportSize.X, (int)ViewportSize.Y);
        
        /// <summary>
        /// Размер области отображения
        /// </summary>
        public Vector2 ViewportSize => _baseContext.ViewportSize;
        
        /// <summary>
        /// Прозрачность
        /// </summary>
        public float Opacity
        {
            get => _opacity;
            set => _opacity = Math.Clamp(value, 0f, 1f);
        }
        
        /// <summary>
        /// Инициализация контекста рендеринга
        /// </summary>
        /// <param name="baseContext">Базовый контекст рендеринга</param>
        /// <param name="logger">Логгер</param>
        public UIRenderContext(IRenderContext baseContext, ILogger logger)
        {
            _baseContext = baseContext ?? throw new ArgumentNullException(nameof(baseContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clipStack = new Stack<Rectangle>();
            _opacity = 1.0f;
            
            _logger.Debug("UIRenderContext initialized");
        }
        
        /// <summary>
        /// Добавление области отсечения
        /// </summary>
        /// <param name="rect">Прямоугольник отсечения</param>
        public void PushClipRect(Rectangle rect)
        {
            if (_clipStack.Count > 0)
            {
                // Пересечение с текущей областью отсечения
                rect = Intersect(rect, _clipStack.Peek());
            }
            
            _clipStack.Push(rect);
            _logger.Debug($"Pushed clip rect: {rect}");
        }
        
        /// <summary>
        /// Удаление верхней области отсечения
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
        /// Отрисовка прямоугольника с учетом отсечения
        /// </summary>
        public void DrawRectangle(Rectangle bounds, Color color, float depth = 0)
        {
            if (!IsVisibleInClipRect(bounds))
                return;
                
            Color adjustedColor = AdjustColorOpacity(color);
            _baseContext.DrawRectangle(bounds, adjustedColor, depth);
        }
        
        /// <summary>
        /// Отрисовка текстуры с учетом отсечения
        /// </summary>
        public void DrawTexture(Rectangle bounds, int textureId, Color tint, float depth = 0)
        {
            if (!IsVisibleInClipRect(bounds))
                return;
                
            Color adjustedColor = AdjustColorOpacity(tint);
            _baseContext.DrawTexture(bounds, textureId, adjustedColor, depth);
        }
        
        /// <summary>
        /// Отрисовка текста с учетом отсечения
        /// </summary>
        public void DrawText(string text, Vector2 position, Color color, float scale = 1.0f, float depth = 0)
        {
            // Для текста используем аппроксимацию области
            // Точное отсечение текста требует специальных алгоритмов
            Rectangle bounds = new Rectangle((int)position.X, (int)position.Y, (int)(text.Length * 8 * scale), (int)(16 * scale));
            
            if (!IsVisibleInClipRect(bounds))
                return;
                
            Color adjustedColor = AdjustColorOpacity(color);
            _baseContext.DrawText(text, position, adjustedColor, scale, depth);
        }
        
        /// <summary>
        /// Проверка видимости прямоугольника в текущей области отсечения
        /// </summary>
        private bool IsVisibleInClipRect(Rectangle bounds)
        {
            if (_clipStack.Count == 0)
                return true;
                
            Rectangle clipRect = _clipStack.Peek();
            return bounds.IntersectsWith(clipRect);
        }
        
        /// <summary>
        /// Получение пересечения двух прямоугольников
        /// </summary>
        private Rectangle Intersect(Rectangle a, Rectangle b)
        {
            int x = Math.Max(a.X, b.X);
            int y = Math.Max(a.Y, b.Y);
            int width = Math.Min(a.Right, b.Right) - x;
            int height = Math.Min(a.Bottom, b.Bottom) - y;
            
            if (width <= 0 || height <= 0)
                return Rectangle.Empty;
                
            return new Rectangle(x, y, width, height);
        }
        
        /// <summary>
        /// Корректировка цвета с учетом прозрачности
        /// </summary>
        private Color AdjustColorOpacity(Color color)
        {
            return Color.FromArgb(
                (int)(color.A * _opacity),
                color.R,
                color.G,
                color.B
            );
        }

        public void SaveGLState()
        {
            throw new NotImplementedException();
        }

        public void RestoreGLState()
        {
            throw new NotImplementedException();
        }
    }
} 