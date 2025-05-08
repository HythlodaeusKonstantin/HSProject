using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Engine.Core.UI
{
    /// <summary>
    /// Состояния элемента UI для стилизации
    /// </summary>
    public enum UIState
    {
        Normal,
        Hover,
        Pressed,
        Focused,
        Disabled
    }

    /// <summary>
    /// Стиль UI элемента для конкретного состояния
    /// </summary>
    public class UIStyleState
    {
        /// <summary>
        /// Цвет фона
        /// </summary>
        public Color? BackgroundColor { get; set; }
        
        /// <summary>
        /// Цвет текста
        /// </summary>
        public Color? TextColor { get; set; }
        
        /// <summary>
        /// Масштаб элемента
        /// </summary>
        public float? Scale { get; set; }
        
        /// <summary>
        /// Прозрачность элемента
        /// </summary>
        public float? Opacity { get; set; }
        
        /// <summary>
        /// Масштаб текста
        /// </summary>
        public float? TextScale { get; set; }
        
        /// <summary>
        /// Прозрачность текста
        /// </summary>
        public float? TextOpacity { get; set; }
        
        /// <summary>
        /// Цвет границы
        /// </summary>
        public Color? BorderColor { get; set; }
        
        /// <summary>
        /// Толщина границы
        /// </summary>
        public float? BorderWidth { get; set; }
    }

    /// <summary>
    /// Отступы элемента
    /// </summary>
    public struct Padding
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Padding(float all)
        {
            Left = Right = Top = Bottom = all;
        }

        public Padding(float horizontal, float vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }

        public Padding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    /// <summary>
    /// Внешний отступ элемента
    /// </summary>
    public struct Margin
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Margin(float all)
        {
            Left = Right = Top = Bottom = all;
        }

        public Margin(float horizontal, float vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }

        public Margin(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    /// <summary>
    /// Стиль UI элемента
    /// </summary>
    public class UIStyle
    {
        /// <summary>
        /// Цвет фона
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.White;
        
        /// <summary>
        /// Цвет текста
        /// </summary>
        public Color TextColor { get; set; } = Color.Black;
        
        /// <summary>
        /// Цвет границы
        /// </summary>
        public Color BorderColor { get; set; } = Color.Black;
        
        /// <summary>
        /// Толщина границы
        /// </summary>
        public float BorderWidth { get; set; }
        
        /// <summary>
        /// Радиус скругления углов
        /// </summary>
        public float CornerRadius { get; set; }
        
        /// <summary>
        /// Внутренний отступ
        /// </summary>
        public Padding Padding { get; set; } = new Padding(0);
        
        /// <summary>
        /// Внешний отступ
        /// </summary>
        public Margin Margin { get; set; } = new Margin(0);
        
        /// <summary>
        /// Масштаб элемента
        /// </summary>
        public float Scale { get; set; } = 1.0f;
        
        /// <summary>
        /// Прозрачность элемента (0.0 - полностью прозрачный, 1.0 - полностью непрозрачный)
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Масштаб текста
        /// </summary>
        public float TextScale { get; set; } = 1.0f;

        /// <summary>
        /// Прозрачность текста
        /// </summary>
        public float TextOpacity { get; set; } = 1.0f;

        /// <summary>
        /// Стили для различных состояний элемента
        /// </summary>
        public Dictionary<UIState, UIStyleState> States { get; }

        public UIStyle()
        {
            States = new Dictionary<UIState, UIStyleState>
            {
                { UIState.Normal, new UIStyleState() },
                { UIState.Hover, new UIStyleState() },
                { UIState.Pressed, new UIStyleState() },
                { UIState.Focused, new UIStyleState() },
                { UIState.Disabled, new UIStyleState 
                    { 
                        Opacity = 0.5f,
                        TextColor = Color.Gray
                    } 
                }
            };
        }

        /// <summary>
        /// Создает копию стиля
        /// </summary>
        public UIStyle Clone()
        {
            var style = new UIStyle
            {
                BackgroundColor = BackgroundColor,
                TextColor = TextColor,
                BorderColor = BorderColor,
                BorderWidth = BorderWidth,
                CornerRadius = CornerRadius,
                Padding = Padding,
                Margin = Margin,
                Scale = Scale,
                Opacity = Opacity,
                TextScale = TextScale,
                TextOpacity = TextOpacity
            };

            foreach (var state in States)
            {
                style.States[state.Key] = new UIStyleState
                {
                    BackgroundColor = state.Value.BackgroundColor,
                    TextColor = state.Value.TextColor,
                    Scale = state.Value.Scale,
                    Opacity = state.Value.Opacity,
                    TextScale = state.Value.TextScale,
                    TextOpacity = state.Value.TextOpacity,
                    BorderColor = state.Value.BorderColor,
                    BorderWidth = state.Value.BorderWidth
                };
            }

            return style;
        }
    }
} 