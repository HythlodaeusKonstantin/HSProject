using Engine.Core.Graphics;
using Engine.Core.Windowing;
using Engine.Core.ECS;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Engine.Core.Logging;
using Engine.Core.Input;
using System.IO;

namespace Engine.Core
{
    class Program
    {
        // Статические поля для доступа из обработчиков
        static IEngine? engine = null;
        static ILogger? logger = null;
        static IWindow? window = null;

        static void Main(string[] args)
        {
            // Создаем окно
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "History Project";
            options.VSync = true;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
            options.PreferredDepthBufferBits = 24;
            options.PreferredStencilBufferBits = 8;
            
            // Логирование параметров окна
            logger = new LoggerAdapter();
            logger.Log(LogType.Info, "Program", $"WindowOptions: Size={options.Size}, API={options.API}, DepthBits={options.PreferredDepthBufferBits}, StencilBits={options.PreferredStencilBufferBits}");
            logger.Log(LogType.Info, "Program", $"WindowOptions type: {options.GetType().FullName}");

            // Копируем UI шейдеры в директорию сборки, если это необходимо
            CopyUIShadersToOutputDirectory();

            window = Window.Create(options);
            engine = null;

            window.Load += OnWindowLoad;
            window.Render += OnWindowRender;
            window.Resize += OnWindowResize;

            try
            {
                logger.Log(LogType.Info, "Program", "Starting engine main loop");
                window.Run();
                logger.Log(LogType.Info, "Program", "Engine main loop ended");
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Program", $"Fatal error: {ex}");
                throw;
            }
            finally
            {
                logger.Log(LogType.Info, "Program", "Cleaning up");
                window.Dispose();
            }
        }

        /// <summary>
        /// Копирует UI шейдеры из исходной директории в директорию сборки
        /// </summary>
        private static void CopyUIShadersToOutputDirectory()
        {
            try
            {
                // TODO: при необходимости добавить фильтрацию или обработку конкретных шейдеров
                string sourceDir = Path.Combine("Shaders");
                string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    logger?.Log(LogType.Info, "Program", $"Создана директория для шейдеров: {destDir}");
                }

                // Копируем все файлы из папки Shaders
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(destDir, fileName);
                    File.Copy(file, destPath, true);
                    logger?.Log(LogType.Info, "Program", $"Скопирован шейдер: {fileName}");
                }

                // TODO: если потребуется, добавить обработку поддиректорий
                logger?.Log(LogType.Info, "Program", "Все шейдеры успешно скопированы");
            }
            catch (Exception ex)
            {
                logger?.Log(LogType.Error, "Program", $"Ошибка при копировании шейдеров: {ex.Message}");
            }
        }

        private static void OnWindowLoad()
        {
            logger?.Log(LogType.Info, "Program", "Window loaded, creating engine context");
            var graphicsContext = new GraphicsContext(window!);
            graphicsContext.Initialize();
            var windowService = new WindowService(window!);
            var systemManager = new SystemManager();
            var inputService = new InputService(windowService, logger!);
            engine = new Engine(windowService, graphicsContext, systemManager, inputService, logger!);
            
            engine.InitializeSystems();
            engine.Run();
        }

        private static void OnWindowRender(double deltaTime)
        {
            engine?.UpdateAndRender(deltaTime);
        }

        private static void OnWindowResize(Vector2D<int> size)
        {
            logger?.Log(LogType.Info, "Program", $"Window resized to {size.X}x{size.Y}");
            engine?.OnResize(size);
        }
    }
}
