using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Core.Graphics;
using Engine.Core.Logging;

namespace Engine.Core.Rendering
{
    /// <summary>
    /// Реализация менеджера шейдеров для загрузки, компиляции и управления GLSL программами.
    /// </summary>
    public class ShaderManager : IShaderManager
    {
        private readonly GL _gl;
        private readonly ILogger? _logger;
        private readonly Dictionary<string, int> _programCache = new();
        private readonly Dictionary<int, int[]> _shaderIds = new(); // programId -> [vertexId, fragmentId]

        public ShaderManager(GL gl, ILogger? logger = null)
        {
            _gl = gl;
            _logger = logger;
        }

        public int GetOrCreateProgram(string vertexSrc, string fragmentSrc)
        {
            string key = ComputeHash(vertexSrc, fragmentSrc);
            if (_programCache.TryGetValue(key, out int cachedProgram))
                return cachedProgram;

            int vertexShader = CompileShader(GLEnum.VertexShader, vertexSrc);
            int fragmentShader = CompileShader(GLEnum.FragmentShader, fragmentSrc);

            int program = (int)_gl.CreateProgram();
            _gl.AttachShader((uint)program, (uint)vertexShader);
            _gl.AttachShader((uint)program, (uint)fragmentShader);
            _gl.LinkProgram((uint)program);
            CheckLinkStatus(program);

            _programCache[key] = program;
            _shaderIds[program] = new[] { vertexShader, fragmentShader };
            return program;
        }

        public void UseProgram(int programId)
        {
            _gl.UseProgram((uint)programId);
        }

        public void SetUniform(int programId, string name, float value)
        {
            int location = _gl.GetUniformLocation((uint)programId, name);
            if (location == -1)
            {
                _logger?.Log(LogType.Warning, "ShaderManager", $"Uniform '{name}' not found in program {programId}");
                return;
            }
            _gl.Uniform1(location, value);
        }

        public void SetUniform(int programId, string name, Vector3 value)
        {
            int location = _gl.GetUniformLocation((uint)programId, name);
            if (location == -1)
            {
                _logger?.Log(LogType.Warning, "ShaderManager", $"Uniform '{name}' not found in program {programId}");
                return;
            }
            _gl.Uniform3(location, value.X, value.Y, value.Z);
        }

        public void SetUniform(int programId, string name, Vector4 value)
        {
            int location = _gl.GetUniformLocation((uint)programId, name);
            if (location == -1)
            {
                _logger?.Log(LogType.Warning, "ShaderManager", $"Uniform '{name}' not found in program {programId}");
                return;
            }
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }

        public unsafe void SetUniform(int programId, string name, Matrix4x4 value)
        {
            int location = _gl.GetUniformLocation((uint)programId, name);
            if (location == -1)
            {
                _logger?.Log(LogType.Warning, "ShaderManager", $"Uniform '{name}' not found in program {programId}");
                return;
            }
            float[] mat = new float[16]
            {
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44
            };
            fixed (float* ptr = mat)
            {
                _gl.UniformMatrix4(location, 1, false, ptr);
            }
        }

        public void Shutdown()
        {
            foreach (var kvp in _programCache)
            {
                int program = kvp.Value;
                if (_shaderIds.TryGetValue(program, out var shaders))
                {
                    foreach (var shader in shaders)
                        _gl.DeleteShader((uint)shader);
                }
                _gl.DeleteProgram((uint)program);
            }
            _programCache.Clear();
            _shaderIds.Clear();
        }

        private int CompileShader(GLEnum type, string source)
        {
            int shader = (int)_gl.CreateShader(type);
            _gl.ShaderSource((uint)shader, source);
            _gl.CompileShader((uint)shader);
            CheckCompileStatus(shader, type.ToString());
            return shader;
        }

        private void CheckCompileStatus(int shaderId, string type)
        {
            _gl.GetShader((uint)shaderId, GLEnum.CompileStatus, out int status);
            if (status == 0)
            {
                string info = _gl.GetShaderInfoLog((uint)shaderId);
                _logger?.Log(LogType.Error, "ShaderManager", $"{type} shader compilation failed: {info}");
                throw new Exception($"{type} shader compilation failed: {info}");
            }
        }

        private void CheckLinkStatus(int program)
        {
            _gl.GetProgram((uint)program, GLEnum.LinkStatus, out int status);
            if (status == 0)
            {
                string info = _gl.GetProgramInfoLog((uint)program);
                _logger?.Log(LogType.Error, "ShaderManager", $"Program link failed: {info}");
                throw new Exception($"Program link failed: {info}");
            }
        }

        private string ComputeHash(string vertex, string fragment)
        {
            return $"{vertex.GetHashCode()}_{fragment.GetHashCode()}";
        }
    }
}
