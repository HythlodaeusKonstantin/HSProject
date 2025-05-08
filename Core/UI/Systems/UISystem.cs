using Engine.Core.ECS;
using Engine.Core.UI;
using Engine.Core.Logging;
using Engine.Core.ECS.Components;
using System;
using System.Collections.Generic;
using Engine.Core.UI.Elements;
using Engine.Core.Windowing;
using Engine.Core.UI.Rendering;
using System.Drawing;

namespace Engine.Core.UI.Systems
{
    /// <summary>
    /// Система для обработки UI элементов
    /// </summary>
    public class UISystem : ISystem
    {
        private readonly ILogger _logger;
        private readonly UICoordinateSystem _coordinateSystem;
        private readonly IEntityManager _entityManager;
        private readonly IWindowService _windowService;
        private readonly IUIRenderSystem _uiRenderSystem;

        public UISystem(ILogger logger, UICoordinateSystem coordinateSystem, IEntityManager entityManager, IWindowService windowService, IUIRenderSystem uiRenderSystem)
        {
            _logger = logger;
            _coordinateSystem = coordinateSystem;
            _entityManager = entityManager;
            _windowService = windowService;
            _uiRenderSystem = uiRenderSystem;
            _logger.Debug("UI System initialized");
        }

        public void Initialize()
        {
            _logger.Debug("UI System starting initialization");
            // Установить размер вьюпорта при инициализации
            _coordinateSystem.SetViewportSize(new System.Numerics.Vector2(_windowService.Width, _windowService.Height));
            // Дополнительная инициализация, если потребуется
        }

        private void SyncComponentToElement(UIComponent comp)
        {
            if (comp.RootElement is UIElementBase element)
            {
                element.Position = comp.Position;
                element.Size = comp.Size;
                element.PositionUnit = comp.PositionUnit;
                element.SizeUnit = comp.SizeUnit;
                _coordinateSystem.ApplyToElement(element);
            }
        }

        public void SyncAll(System.Numerics.Vector2 viewportSize)
        {
            _coordinateSystem.SetViewportSize(viewportSize);

            _uiRenderSystem.SetViewport(
                new Rectangle(0, 0, (int)viewportSize.X, (int)viewportSize.Y)
            );

            foreach (var entity in _entityManager.GetAllEntities())
            {
                if (_entityManager.TryGetComponent<UIComponent>(entity, out var uiComponent))
                {
                    SyncComponentToElement(uiComponent);
                }
            }
        }

        public void Update(double deltaTime)
        {
            foreach (var entity in _entityManager.GetAllEntities())
            {
                if (_entityManager.TryGetComponent<UIComponent>(entity, out var uiComponent))
                {
                    try
                    {
                        SyncComponentToElement(uiComponent);
                        uiComponent.RootElement.Update((float)deltaTime);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Error($"Error updating UI element: {ex.Message}");
                    }
                }
            }
        }

        public void Render()
        {

        }

    }
} 