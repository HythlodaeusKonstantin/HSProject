using Engine.Core.ECS;
using Engine.Core.UI;
using Engine.Core.Logging;
using Engine.Core.ECS.Components;
using System;
using System.Collections.Generic;

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

        public UISystem(ILogger logger, UICoordinateSystem coordinateSystem, IEntityManager entityManager)
        {
            _logger = logger;
            _coordinateSystem = coordinateSystem;
            _entityManager = entityManager;

            _logger.Debug("UI System initialized");
        }

        public void Initialize()
        {
            _logger.Debug("UI System starting initialization");
            // Дополнительная инициализация, если потребуется
        }

        public void Update(double deltaTime)
        {
               foreach (var entity in _entityManager.GetAllEntities())
            {
                if (_entityManager.TryGetComponent<UIComponent>(entity, out var uiComponent))
                {
                    try
                    {
                        // Обновляем позицию в экранных координатах
                        var screenPosition = _coordinateSystem.ToScreenCoordinates(
                            uiComponent.Position,
                            uiComponent.PositionUnit
                        );

                        // Обновляем размер в экранных координатах
                        var screenSize = _coordinateSystem.ToScreenCoordinates(
                            uiComponent.Size,
                            uiComponent.SizeUnit
                        );

                        // Обновляем UI элемент
                        uiComponent.RootElement.Position = screenPosition;
                        uiComponent.RootElement.Size = screenSize;
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