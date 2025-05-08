using System;
using Silk.NET.Maths;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Компонент-актор для примитивных мешей (треугольник, квадрат, куб)
    /// </summary>
    public class PrimitiveMeshActorComponent : ActorComponent
    {
        public string MeshType { get; set; }
        public TransformComponent Transform { get; set; }
        private Random _random = new Random(Guid.NewGuid().GetHashCode());
        private Vector3D<float> _currentAxis = Vector3D<float>.UnitY;
        private float _currentSpeed = 1.0f; // рад/сек
        private float _timeToNextChange = 0.0f;

        public PrimitiveMeshActorComponent(string meshType, TransformComponent transform)
        {
            MeshType = meshType;
            Transform = transform;
        }

        public override void OnInitialize()
        {
            // Можно добавить начальное логирование или настройку
        }

        public override void OnUpdate(double deltaTime)
        {
            // Плавное вращение вокруг текущей оси
            float angleDelta = _currentSpeed * (float)deltaTime;
            var deltaRot = Quaternion<float>.CreateFromAxisAngle(_currentAxis, angleDelta);
            Transform.Rotation = Quaternion<float>.Normalize(Transform.Rotation * deltaRot);

            // Таймер смены оси и скорости
            _timeToNextChange -= (float)deltaTime;
            if (_timeToNextChange <= 0f)
            {
                // Новая случайная ось (нормализованный вектор)
                _currentAxis = new Vector3D<float>(
                    (float)(_random.NextDouble() - 0.5),
                    (float)(_random.NextDouble() - 0.5),
                    (float)(_random.NextDouble() - 0.5)
                );
                if (_currentAxis.LengthSquared < 0.01f) _currentAxis = Vector3D<float>.UnitY;
                _currentAxis = Vector3D.Normalize(_currentAxis);
                // Новая скорость
                _currentSpeed = 0.5f + (float)_random.NextDouble() * 2.5f; // [0.5, 3.0] рад/сек
                // Новый таймер (1.5 - 4 сек)
                _timeToNextChange = 1.5f + (float)_random.NextDouble() * 2.5f;
            }
        }
    }
}