using Silk.NET.Input;
using Engine.Core.Logging;
using Engine.Core.Windowing;

namespace Engine.Core.Input
{
    public class InputService : IInputService
    {
        private readonly ILogger _logger;
        private readonly IWindowService _windowService;
        public IMouse? PrimaryMouse => null;

        public InputService(IWindowService windowService, ILogger logger)
        {
            _windowService = windowService;
            _logger = logger;
            _logger.Log(LogType.Info, "InputService", "Конструктор InputService вызван");
            Initialize();
        }

        private void Initialize()
        {
            // TODO: Реализовать
        }

        public bool IsKeyDown(Key key)
        {
            // TODO: Реализовать
            return false;
        }

        public (float dx, float dy) GetMouseDelta()
        {
            // TODO: Реализовать
            return (0, 0);
        }

        public void Update()
        {
            // TODO: Реализовать
        }
    }
}
