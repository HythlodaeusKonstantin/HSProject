using Silk.NET.Input;
using Engine.Core.Logging;
using Engine.Core.Windowing;

namespace Engine.Core.Input
{
    public class InputService : IInputService
    {
        private readonly ILogger _logger;
        private readonly IWindowService _windowService;
        private IInputContext? _inputContext;
        private static IKeyboard? _mainKeyboard;
        private static IMouse? _mainMouse;
        private readonly HashSet<Key> _pressedKeys = new();
        private float _mouseDeltaX = 0f;
        private float _mouseDeltaY = 0f;
        private float _lastMouseX = 0f;
        private float _lastMouseY = 0f;
        private bool _firstMouseMove = true;
        private static bool isInputActive = false;
        private static string inputText = "";

        public InputService(IWindowService windowService, ILogger logger)
        {
            _windowService = windowService;
            _logger = logger;
            _logger.Log(LogType.Info, "InputService", "Конструктор InputService вызван");
            Initialize();
        }

        private void Initialize()
        {
            var windowField = _windowService.GetType().GetField("_window", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var window = windowField?.GetValue(_windowService) as Silk.NET.Windowing.IWindow;
            if (window == null)
            {
                _logger.Log(LogType.Error, "InputService", "Не удалось получить IWindow из IWindowService");
                return;
            }

            _inputContext = window.CreateInput();

            _mainKeyboard = _inputContext.Keyboards.Count > 0 ? _inputContext.Keyboards[0] : null;
            _mainMouse = _inputContext.Mice.Count > 0 ? _inputContext.Mice[0] : null;

            if (_mainKeyboard != null)
            {
                _mainKeyboard.KeyDown += OnKeyDown;
                _mainKeyboard.KeyUp += OnKeyUp;
                _mainKeyboard.KeyChar += OnKeyChar;
            }
            if (_mainMouse != null)
            {
                _mainMouse.MouseMove += OnMouseMove;
                _mainMouse.MouseDown += OnMouseDown;
                _mainMouse.MouseUp += OnMouseUp;
                _mainMouse.Click += OnMouseClick;
                _mainMouse.DoubleClick += OnMouseDoubleClick;
                _mainMouse.Scroll += OnMouseScroll;
            }
        }

        private void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            _pressedKeys.Add(key);
            _logger.Log(LogType.Info, "InputService", $"KeyDown: {key}");
        }

        private void OnKeyUp(IKeyboard keyboard, Key key, int arg3)
        {
            _pressedKeys.Remove(key);
            _logger.Log(LogType.Info, "InputService", $"KeyUp: {key}");
        }

        private static void OnKeyChar(IKeyboard keyboard, char c)
        {
            if (isInputActive && !char.IsControl(c))
            {
                inputText += c;
            }
        }

        private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 pos)
        {
            if (_firstMouseMove)
            {
                _lastMouseX = pos.X;
                _lastMouseY = pos.Y;
                _firstMouseMove = false;
                _mouseDeltaX = 0f;
                _mouseDeltaY = 0f;
                return;
            }
            // Просто обновляем дельты, не накапливаем их
            _mouseDeltaX = pos.X - _lastMouseX;
            _mouseDeltaY = pos.Y - _lastMouseY;
            _lastMouseX = pos.X;
            _lastMouseY = pos.Y;
            _logger.Log(LogType.Info, "InputService", $"MouseMove: X={pos.X}, Y={pos.Y}, dX={_mouseDeltaX}, dY={_mouseDeltaY}");
        }

        private void OnMouseDown(IMouse mouse, MouseButton button)
        {
            _logger.Log(LogType.Info, "InputService", $"MouseDown: {button}");
        }

        private void OnMouseUp(IMouse mouse, MouseButton button)
        {
            _logger.Log(LogType.Info, "InputService", $"MouseUp: {button}");
        }

        private void OnMouseClick(IMouse mouse, MouseButton button, System.Numerics.Vector2 position)
        {
            _logger.Log(LogType.Info, "InputService", $"MouseClick: {button} at {position}");
        }

        private void OnMouseDoubleClick(IMouse mouse, MouseButton button, System.Numerics.Vector2 position)
        {
            _logger.Log(LogType.Info, "InputService", $"MouseDoubleClick: {button} at {position}");
        }

        private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
        {
            _logger.Log(LogType.Info, "InputService", $"MouseScroll: {scroll.Y}");
        }

        public bool IsKeyDown(Key key)
        {
            return _pressedKeys.Contains(key);
        }

        public (float dx, float dy) GetMouseDelta()
        {
            var delta = (_mouseDeltaX, _mouseDeltaY);
            // Сбрасываем дельты сразу после получения
            _mouseDeltaX = 0f;
            _mouseDeltaY = 0f;
            return delta;
        }

        public void Update()
        {
            // TODO: Реализовать
        }

        public IMouse? GetMouse()
        {
            return _mainMouse;
        }

        public IMouse? mainMouse => _mainMouse;
        public IKeyboard? mainKeyboard => _mainKeyboard;
    }
}
