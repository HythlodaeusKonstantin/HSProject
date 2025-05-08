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
using Engine.Core.Input;

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
        private readonly IInputService _inputService;
        private IUIElement? _lastHoveredElement = null;
        private bool _lastMouseDown = false;

        public UISystem(ILogger logger, UICoordinateSystem coordinateSystem, IEntityManager entityManager, IWindowService windowService, IUIRenderSystem uiRenderSystem, IInputService inputService)
        {
            _logger = logger;
            _coordinateSystem = coordinateSystem;
            _entityManager = entityManager;
            _windowService = windowService;
            _uiRenderSystem = uiRenderSystem;
            _inputService = inputService;
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
                element.Anchor = comp.Anchor;
                // Синхронизация стиля
                if (comp.Style != null)
                    element.Style = comp.Style;
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
            // --- Обработка событий мыши для UI ---
            var mouse = _inputService.mainMouse;
            if (mouse != null)
            {
                var mousePos = mouse.Position;
                bool mouseDown = mouse.IsButtonPressed(Silk.NET.Input.MouseButton.Left);
                IUIElement? hovered = null;
                foreach (var entity in _entityManager.QueryEntities(typeof(UIComponent)))
                {
                    var comp = _entityManager.GetComponent<UIComponent>(entity);
                    if (comp.RootElement is UIElementBase element && element.IsVisible && element is UIInteractiveElement interactive)
                    {
                        if (((UIRenderableElement)element).Bounds.Contains((int)mousePos.X, (int)mousePos.Y))
                        {
                            hovered = element;
                            // Наведение
                            if (_lastHoveredElement != element)
                                interactive.HandleMouseEvent(new UIMouseEventArgs(UIMouseEventType.Enter, mousePos, mousePos));
                            // Нажатие
                            if (mouseDown && !_lastMouseDown)
                                interactive.HandleMouseEvent(new UIMouseEventArgs(UIMouseEventType.Down, mousePos, mousePos));
                            // Отпускание
                            if (!mouseDown && _lastMouseDown)
                                interactive.HandleMouseEvent(new UIMouseEventArgs(UIMouseEventType.Up, mousePos, mousePos));
                        }
                        else
                        {
                            // Уход мыши
                            if (_lastHoveredElement == element)
                                interactive.HandleMouseEvent(new UIMouseEventArgs(UIMouseEventType.Leave, mousePos, mousePos));
                        }
                    }
                }
                _lastHoveredElement = hovered;
                _lastMouseDown = mouseDown;
            }
            // --- Старая логика обновления ---
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