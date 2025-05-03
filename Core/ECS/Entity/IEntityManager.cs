using System;
using System.Collections.Generic;
using Engine.Core.ECS.Components;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Интерфейс менеджера сущностей, отвечающий за создание, уничтожение сущностей
    /// и управление их компонентами
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// Создает новую сущность и возвращает её идентификатор
        /// </summary>
        /// <returns>Новая сущность с уникальным идентификатором</returns>
        Entity CreateEntity();

        /// <summary>
        /// Уничтожает сущность и удаляет все её компоненты
        /// </summary>
        /// <param name="entity">Сущность для уничтожения</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует</exception>
        void DestroyEntity(Entity entity);

        /// <summary>
        /// Добавляет компонент заданного типа к сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <param name="entity">Сущность, к которой добавляется компонент</param>
        /// <param name="component">Компонент для добавления</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует</exception>
        void AddComponent<T>(Entity entity, T component) where T : IComponent;

        /// <summary>
        /// Удаляет компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для удаления</typeparam>
        /// <param name="entity">Сущность, у которой удаляется компонент</param>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует или не содержит компонент</exception>
        void RemoveComponent<T>(Entity entity) where T : IComponent;

        /// <summary>
        /// Проверяет наличие компонента заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для проверки</typeparam>
        /// <param name="entity">Сущность для проверки</param>
        /// <returns>True, если сущность содержит компонент указанного типа</returns>
        bool HasComponent<T>(Entity entity) where T : IComponent;

        /// <summary>
        /// Получает компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <param name="entity">Сущность, у которой запрашивается компонент</param>
        /// <returns>Компонент указанного типа</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если сущность не существует или не содержит компонент</exception>
        T GetComponent<T>(Entity entity) where T : IComponent;

        /// <summary>
        /// Возвращает все существующие сущности
        /// </summary>
        /// <returns>Перечисление всех активных сущностей</returns>
        IEnumerable<Entity> GetAllEntities();

        /// <summary>
        /// Находит все сущности, у которых есть все указанные типы компонентов
        /// </summary>
        /// <param name="componentTypes">Типы компонентов, которые должны быть у сущности</param>
        /// <returns>Перечисление сущностей, содержащих все указанные типы компонентов</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если componentTypes равен null</exception>
        IEnumerable<Entity> QueryEntities(params Type[] componentTypes);

        /// <summary>
        /// Пытается получить компонент заданного типа у сущности
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <param name="entity">Сущность, у которой запрашивается компонент</param>
        /// <param name="component">Найденный компонент или null, если компонент не найден</param>
        /// <returns>True, если компонент найден, иначе false</returns>
        bool TryGetComponent<T>(Entity entity, out T? component) where T : IComponent;

        /// <summary>
        /// Получает все компоненты заданного типа из всех сущностей
        /// </summary>
        /// <typeparam name="T">Тип компонента для получения</typeparam>
        /// <returns>Перечисление всех компонентов указанного типа</returns>
        IEnumerable<T> GetAllComponentsOfType<T>() where T : IComponent;
    }
}