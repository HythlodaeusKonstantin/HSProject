using Silk.NET.OpenGL;
using System.Numerics;

namespace Engine.Core.Rendering
{
    /// <summary>
    /// Интерфейс менеджера шейдеров для загрузки, кэширования и управления GLSL программами.
    /// </summary>
    public interface IShaderManager
    {
        /// <summary>Создать или получить программу из исходников.</summary>
        /// <param name="vertexSrc">GLSL-код вершинного шейдера.</param>
        /// <param name="fragmentSrc">GLSL-код фрагментного шейдера.</param>
        /// <returns>Идентификатор OpenGL-программы.</returns>
        int GetOrCreateProgram(string vertexSrc, string fragmentSrc);

        /// <summary>Активировать программу.</summary>
        void UseProgram(int programId);

        /// <summary>Установить uniform-переменную float.</summary>
        void SetUniform(int programId, string name, float value);

        /// <summary>Установить uniform-переменную vec3.</summary>
        void SetUniform(int programId, string name, Vector3 value);

        /// <summary>Установить uniform-переменную vec4.</summary>
        void SetUniform(int programId, string name, Vector4 value);

        /// <summary>Установить uniform-переменную mat4.</summary>
        void SetUniform(int programId, string name, Matrix4x4 value);

        /// <summary>Удалить все закэшированные программы и шейдеры.</summary>
        void Shutdown();
    }
}
