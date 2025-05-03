using Silk.NET.Maths;
using Engine.Core.ECS;
using System;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Компонент камеры: параметры проекции.
    /// </summary>
    public struct CameraComponent : IComponent
    {
        /// <summary>Угол обзора по вертикали в градусах</summary>
        public float FieldOfViewDeg { get; set; }

        /// <summary>Отношение ширины к высоте экрана</summary>
        public float AspectRatio { get; set; }

        /// <summary>Ближняя clip-плоскость</summary>
        public float NearPlane { get; set; }

        /// <summary>Дальняя clip-плоскость</summary>
        public float FarPlane { get; set; }

        /// <summary>
        /// Проекционная матрица (рассчитывается на основе параметров).
        /// </summary>
        public Matrix4X4<float> ProjectionMatrix 
        { 
            get
            {
                // Преобразуем градусы в радианы
                float fovRadians = FieldOfViewDeg * (MathF.PI / 180.0f);
                
                // Создаем матрицу проекции
                return Matrix4X4.CreatePerspectiveFieldOfView(
                    fovRadians,
                    AspectRatio,
                    NearPlane,
                    FarPlane);
            }
        }

        /// <summary>
        /// Создаёт матрицу вида на основе переданного компонента трансформации.
        /// </summary>
        /// <param name="transform">Компонент трансформации, содержащий позицию и ориентацию камеры</param>
        /// <returns>Матрица вида (View matrix)</returns>
        public Matrix4X4<float> CreateViewMatrix(TransformComponent transform)
        {
            // Вычисляем направление "вперёд" на основе поворота
            var rotation = transform.Rotation;
            
            // Начальное направление вперёд (по оси -Z)
            var forwardVec = new Vector3D<float>(0, 0, -1);
            
            // Создаем матрицу вращения из кватерниона
            var rotMatrix = Matrix4X4.CreateFromQuaternion(rotation);
            
            // Применяем матрицу вращения к вектору направления "вперёд"
            var forward = Vector3D.Transform(forwardVec, rotMatrix);

            // Начальное направление вверх (по оси Y)
            var upVec = new Vector3D<float>(0, 1, 0);
            
            // Применяем матрицу вращения к вектору направления "вверх"
            var up = Vector3D.Transform(upVec, rotMatrix);

            // Создаем матрицу вида с позицией камеры, точкой, на которую смотрит камера,
            // и вектором "вверх"
            return Matrix4X4.CreateLookAt(
                transform.Position,                       // Позиция камеры
                transform.Position + forward,             // Точка, на которую смотрит камера
                up);                                     // Вектор "вверх"
        }

        /// <summary>
        /// Создает компонент камеры с заданными параметрами.
        /// </summary>
        public CameraComponent(float fieldOfViewDeg, float aspectRatio, float nearPlane, float farPlane)
        {
            FieldOfViewDeg = fieldOfViewDeg;
            AspectRatio = aspectRatio;
            NearPlane = nearPlane;
            FarPlane = farPlane;
        }

        /// <summary>
        /// Создает компонент камеры со стандартными параметрами.
        /// </summary>
        public static CameraComponent Default =>
            new CameraComponent(60f, 16f/9f, 0.1f, 100f);
    }
}
