using Silk.NET.Maths;
using Engine.Core.ECS;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Компонент трансформации: позиция, поворот, масштаб.
    /// </summary>
    public class TransformComponent : IComponent
    {
        /// <summary>Положение в мировых координатах.</summary>
        public Vector3D<float> Position;

        /// <summary>Ориентация в виде кватерниона.</summary>
        public Quaternion<float> Rotation;

        /// <summary>Масштаб по осям X, Y, Z.</summary>
        public Vector3D<float> Scale;

        /// <summary>
        /// Матрица модели: Scale → Rotation → Translation.
        /// </summary>
        public Matrix4X4<float> ModelMatrix
        {
            get
            {
                var scaleM = Matrix4X4.CreateScale(Scale);
                var rotM   = Matrix4X4.CreateFromQuaternion(Rotation);
                var transM = Matrix4X4.CreateTranslation(Position);
                return scaleM * rotM * transM;
            }
        }

        /// <summary>
        /// Создаёт компонент с заданными значениями.
        /// </summary>
        public TransformComponent(
            Vector3D<float> position,
            Quaternion<float> rotation,
            Vector3D<float> scale)
        {
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }

        /// <summary>
        /// Создаёт компонент в начальном состоянии (0,0,0), Identity, (1,1,1).
        /// </summary>
        public static TransformComponent Default =>
            new TransformComponent(
                new Vector3D<float>(0, 0, 0),
                Quaternion<float>.Identity,
                new Vector3D<float>(1, 1, 1));
    }
} 