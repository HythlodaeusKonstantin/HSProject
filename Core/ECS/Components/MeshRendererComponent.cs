using Engine.Core.Rendering;
using Engine.Core.ECS;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Компонент для отрисовки меша.
    /// </summary>
    public class MeshRendererComponent : IComponent
    {
        /// <summary>Данные меша: VAO и количество вершин.</summary>
        public MeshData MeshData { get; }

        /// <summary>Цвет для рендера (по умолчанию белый).</summary>
        public System.Numerics.Vector4 Color { get; }

        /// <summary>
        /// Инициализирует компонент заданным мешем и цветом.
        /// </summary>
        public MeshRendererComponent(MeshData meshData, System.Numerics.Vector4 color)
        {
            MeshData = meshData;
            Color = color;
        }

        /// <summary>
        /// Инициализирует компонент заданным мешем (цвет по умолчанию — белый).
        /// </summary>
        public MeshRendererComponent(MeshData meshData)
            : this(meshData, new System.Numerics.Vector4(1, 1, 1, 1)) { }
    }
} 