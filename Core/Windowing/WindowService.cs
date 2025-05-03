using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Core;
using Silk.NET.OpenGL;

namespace Engine.Core.Windowing
{
    /// <summary>
    /// Implementation of window management service using Silk.NET
    /// </summary>
    public class WindowService : IWindowService
    {
        private IWindow _window;
        private bool _shouldClose;

        public WindowService(IWindow window)
        {
            _window = window;
        }

        public void Initialize()
        {
            _window.Load += OnLoad;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
        }

        public void ProcessEvents()
        {
            _window.DoEvents();
            _window.DoUpdate();
        }

        public void SwapBuffers()
        {
            _window.SwapBuffers();
        }

        public bool ShouldClose => _shouldClose;

        public void Shutdown()
        {
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }
        }

        public int Width => _window is not null ? _window.Size.X : 0;
        public int Height => _window is not null ? _window.Size.Y : 0;

        private void OnLoad()
        {
            if (_window != null)
            {
                GL.GetApi(_window).Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);
            }
        }

        private void OnResize(Vector2D<int> size)
        {
            if (_window != null)
            {
                GL.GetApi(_window).Viewport(0, 0, (uint)size.X, (uint)size.Y);
            }
        }

        private void OnClosing()
        {
            _shouldClose = true;
        }

        public void RequestClose()
        {
            _shouldClose = true;
            _window?.Close();
        }
    }
} 