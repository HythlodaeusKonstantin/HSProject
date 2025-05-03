using System;
using System.Collections.Generic;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Менеджер систем ECS, отвечающий за регистрацию и обновление систем.
    /// </summary>
    public class SystemManager
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        /// <summary>
        /// Регистрирует систему в менеджере.
        /// </summary>
        /// <param name="system">Система для регистрации</param>
        /// <exception cref="ArgumentNullException">Выбрасывается, если system равен null</exception>
        public void RegisterSystem(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            if (!_systems.Contains(system))
            {
                _systems.Add(system);
            }
        }

        /// <summary>
        /// Удаляет систему из менеджера.
        /// </summary>
        /// <param name="system">Система для удаления</param>
        public void UnregisterSystem(ISystem system)
        {
            _systems.Remove(system);
        }

        /// <summary>
        /// Проверяет, зарегистрирована ли система в менеджере.
        /// </summary>
        /// <param name="system">Система для проверки</param>
        /// <returns>True, если система зарегистрирована</returns>
        public bool HasSystem(ISystem system)
        {
            return _systems.Contains(system);
        }

        /// <summary>
        /// Возвращает все зарегистрированные системы.
        /// </summary>
        /// <returns>Список всех систем</returns>
        public IReadOnlyList<ISystem> GetAllSystems()
        {
            return _systems.AsReadOnly();
        }

        /// <summary>
        /// Очищает все зарегистрированные системы.
        /// </summary>
        public void ClearSystems()
        {
            _systems.Clear();
        }

        /// <summary>
        /// Обновляет все зарегистрированные системы с заданным дельта-временем.
        /// </summary>
        /// <param name="deltaTime">Время (в секундах) с момента предыдущего вызова</param>
        public void UpdateAll(double deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }

        /// <summary>
        /// Вызывает отрисовку всех систем, которые реализуют рендеринг.
        /// </summary>
        public void RenderAll()
        {
            foreach (var system in _systems)
            {
                system.Render();
            }
        }
    }
} 