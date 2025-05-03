using Silk.NET.OpenGL;

namespace Engine.Core.Rendering
{
    /// <summary>
    /// Данные о меше: VAO и количество вершин.
    /// </summary>
    public struct MeshData
    {
        /// <summary>Идентификатор VAO.</summary>
        public uint Vao { get; }

        /// <summary>Идентификатор VBO.</summary>
        public uint Vbo { get; }

        /// <summary>Идентификатор EBO (если есть).</summary>
        public uint? Ebo { get; }

        /// <summary>Число вершин в меше.</summary>
        public int VertexCount { get; }

        /// <summary>Число индексов (если есть).</summary>
        public int IndexCount { get; }

        /// <summary>Тип примитива (треугольники, линии и т.д.).</summary>
        public GLEnum PrimitiveType { get; }

        public MeshData(uint vao, uint vbo, uint? ebo, int vertexCount, int indexCount, GLEnum primitiveType)
        {
            Vao = vao;
            Vbo = vbo;
            Ebo = ebo;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            PrimitiveType = primitiveType;
        }

        public MeshData(uint vao, uint vbo, int vertexCount)
            : this(vao, vbo, null, vertexCount, 0, GLEnum.Triangles) { }

        /// <summary>
        /// Освобождает ресурсы OpenGL.
        /// </summary>
        public void Dispose(GL gl)
        {
            gl.DeleteBuffer(Vbo);
            if (Ebo.HasValue)
                gl.DeleteBuffer(Ebo.Value);
            gl.DeleteVertexArray(Vao);
        }
    }
} 