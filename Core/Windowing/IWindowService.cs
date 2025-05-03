using System;

namespace Engine.Core.Windowing
{
    /// <summary>
    /// Interface for window management service
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Initialize and create the window
        /// </summary>
        void Initialize();

        /// <summary>
        /// Process all pending window events
        /// </summary>
        void ProcessEvents();

        /// <summary>
        /// Check if window close has been requested
        /// </summary>
        bool ShouldClose { get; }

        /// <summary>
        /// Properly shutdown the window and release resources
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Current window width in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Current window height in pixels
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Swap the front and back buffers
        /// </summary>
        void SwapBuffers();

        /// <summary>
        /// Запросить закрытие окна (установить ShouldClose в true)
        /// </summary>
        void RequestClose();
    }
} 