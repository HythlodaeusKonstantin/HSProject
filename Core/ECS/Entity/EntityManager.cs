using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Core.ECS.Components;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Реализация менеджера сущностей, управляющего созданием, уничтожением сущностей и их компонентами
    /// </summary>
    public class EntityManager : IEntityManager
    {
        // Набор активных сущностей
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();

        // Для каждой сущности: словарь Тип→IComponent
        private readonly Dictionary<Entity, Dictionary<Type, IComponent>> _components
            = new Dictionary<Entity, Dictionary<Type, IComponent>>();

        /// <summary>
        /// Создает новую сущность и возвращает её идентификатор
        /// </summary>
        /// <returns>Новая сущность с уникальным идентификатором</returns>
        public Entity CreateEntity()
        {
            var entity = Entity.Create();
            _entities.Add(entity);
            _components[entity] = new Dictionary<Type, IComponent>();
            return entity;
        }

        /// <summary>
        /// Уничтожает сущность и удаляет все её компоненты
        /// </summary>
        /// <param name="entity">Сущность для уничтожения</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует</exception>
        public void DestroyEntity(Entity entity)
        {
            if (!_entities.Contains(entity))
            {
                throw new ArgumentException($"Entity {entity} does not exist.", nameof(entity));
            }

            _components.Remove(entity);
            _entities.Remove(entity);
        }

        /// <summary>
        /// Добавляет компонент заданного типа к сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <param name="entity">Сущность, к которой добавляется компонент</param>
        /// <param name="component">Компонент для добавления</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует</exception>
        public void AddComponent<T>(Entity entity, T component) where T : IComponent
        {
            if (!_entities.Contains(entity))
            {
                throw new ArgumentException($"Entity {entity} does not exist.", nameof(entity));
            }

            _components[entity][typeof(T)] = component;
        }

        /// <summary>
        /// Удаляет компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для удаления</typeparam>
        /// <param name="entity">Сущность, у которой удаляется компонент</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует или не содержит компонент</exception>
        public void RemoveComponent<T>(Entity entity) where T : IComponent
        {
            if (!_entities.Contains(entity))
            {
                throw new ArgumentException($"Entity {entity} does not exist.", nameof(entity));
            }

            if (!_components[entity].ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Entity {entity} does not have component of type {typeof(T)}.", nameof(entity));
            }

            _components[entity].Remove(typeof(T));
        }

        /// <summary>
        /// Проверяет наличие компонента заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для проверки</typeparam>
        /// <param name="entity">Сущность для проверки</param>
        /// <returns>True, если сущность содержит компонент указанного типа</returns>
        public bool HasComponent<T>(Entity entity) where T : IComponent
        {
            return _components.TryGetValue(entity, out var dict) && dict.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Получает компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <param name="entity">Сущность, у которой запрашивается компонент</param>
        /// <returns>Компонент указанного типа</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует или не содержит компонент</exception>
        public T GetComponent<T>(Entity entity) where T : IComponent
        {
            if (!_entities.Contains(entity))
            {
                throw new ArgumentException($"Entity {entity} does not exist.", nameof(entity));
            }

            if (!_components[entity].TryGetValue(typeof(T), out var component))
            {
                throw new ArgumentException($"Entity {entity} does not have component of type {typeof(T)}.", nameof(entity));
            }

            return (T)component;
        }

        /// <summary>
        /// Возвращает все существующие сущности
        /// </summary>
        /// <returns>Перечисление всех активных сущностей</returns>
        public IEnumerable<Entity> GetAllEntities()
        {
            return _entities;
        }

        /// <summary>
        /// Находит все сущности, у которых есть все указанные типы компонентов
        /// </summary>
        /// <param name="componentTypes">Типы компонентов, которые должны быть у сущности</param>
        /// <returns>Перечисление сущностей, содержащих все указанные типы компонентов</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если componentTypes равен null</exception>
        public IEnumerable<Entity> QueryEntities(params Type[] componentTypes)
        {
            if (componentTypes == null)
            {
                throw new ArgumentNullException(nameof(componentTypes));
            }

            return _entities.Where(entity =>
            {
                if (!_components.TryGetValue(entity, out var entityComponents))
                {
                    return false;
                }

                return componentTypes.All(type => entityComponents.ContainsKey(type));
            });
        }

        /// <summary>
        /// Возвращает все компоненты, прикреплённые к сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Перечисление всех компонентов</returns>
        public IEnumerable<IComponent> GetAllComponents(Entity entity)
        {
            if (!_entities.Contains(entity))
                throw new ArgumentException($"Entity {entity} does not exist.", nameof(entity));
            return _components[entity].Values;
        }

        /// <summary>
        /// Пытается получить компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <param name="entity">Сущность, у которой запрашивается компонент</param>
        /// <param name="component">Найденный компонент или null, если компонент не найден</param>
        /// <returns>True, если компонент найден, иначе false</returns>
        public bool TryGetComponent<T>(Entity entity, out T? component) where T : IComponent
        {
            component = default;

            if (!_components.TryGetValue(entity, out var entityComponents))
            {
                return false;
            }

            if (!entityComponents.TryGetValue(typeof(T), out var foundComponent))
            {
                return false;
            }

            component = (T)foundComponent;
            return true;
        }

        /// <summary>
        /// Получает все компоненты заданного типа из всех сущностей
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <returns>Перечисление всех компонентов указанного типа</returns>
        public IEnumerable<T> GetAllComponentsOfType<T>() where T : IComponent
        {
            var result = new List<T>();
            
            foreach (var entity in _entities)
            {
                if (_components.TryGetValue(entity, out var entityComponents) &&
                    entityComponents.TryGetValue(typeof(T), out var component))
                {
                    result.Add((T)component);
                }
            }
            
            return result;
        }
    }
}