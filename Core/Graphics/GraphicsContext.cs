using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Engine.Core.Graphics;

namespace Engine.Core.Graphics
{
    /// <summary>
    /// Реализация графического контекста на основе OpenGL
    /// </summary>
    public class GraphicsContext : IGraphicsContext
    {
        private GL _gl = null!;
        private IWindow _window = null!;

        public GraphicsContext(IWindow window)
        {
            _window = window;
        }

        public void Initialize()
        {
            _gl = GL.GetApi(_window);
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        public void Clear(Color color)
        {
            _gl.ClearColor(color.R, color.G, color.B, color.A);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Shutdown()
        {
            _gl.Dispose();
        }

        public GL GL => _gl;
    }
} 