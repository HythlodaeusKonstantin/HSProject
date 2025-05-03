using System;
using Engine.Core.ECS;
using Engine.Core.ECS.Components;
using Silk.NET.Maths;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Система для универсального управления трансформациями (позиция, масштаб, ротация)
    /// </summary>
    public class TransformSystem : ISystem
    {
        private readonly IEntityManager _entityManager;
        private float _rotation = 0.0f;

        public TransformSystem(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public void Update(double deltaTime)
        {
            // Пока ничего не делаем по умолчанию
        }

        /// <summary>
        /// Переместить сущность на заданный вектор.
        /// </summary>
        public void Move(Entity entity, Silk.NET.Maths.Vector3D<float> delta)
        {
            var transform = _entityManager.GetComponent<TransformComponent>(entity);
            transform.Position += delta;
            _entityManager.AddComponent(entity, transform);
        }

        /// <summary>
        /// Повернуть сущность на заданный угол (радианы) вокруг оси.
        /// </summary>
        public void Rotate(Entity entity, Silk.NET.Maths.Vector3D<float> axis, float angleRad)
        {
            var transform = _entityManager.GetComponent<TransformComponent>(entity);
            var q = Silk.NET.Maths.Quaternion<float>.CreateFromAxisAngle(axis, angleRad);
            transform.Rotation = q * transform.Rotation;
            _entityManager.AddComponent(entity, transform);
        }

        /// <summary>
        /// Изменить масштаб сущности (мультипликативно).
        /// </summary>
        public void Scale(Entity entity, Silk.NET.Maths.Vector3D<float> scaleFactor)
        {
            var transform = _entityManager.GetComponent<TransformComponent>(entity);
            transform.Scale *= scaleFactor;
            _entityManager.AddComponent(entity, transform);
        }

        public void Render()
        {
            // Обычно ничего не делает, трансформации не рендерятся напрямую
        }
    }
} 