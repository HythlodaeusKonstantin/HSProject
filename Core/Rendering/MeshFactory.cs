using Silk.NET.OpenGL;

namespace Engine.Core.Rendering
{
    /// <summary>
    /// Утилита для создания примитивных мешей.
    /// </summary>
    public static class MeshFactory
    {
        /// <summary>
        /// Создаёт меш в форме треугольника и возвращает данные VAO и числа вершин.
        /// </summary>
        /// <param name="gl">Экземпляр OpenGL-контекста из Silk.NET.</param>
        /// <returns>Структура MeshData (VAO, VertexCount).</returns>
        public static unsafe MeshData CreateTriangle(GL gl)
        {
            Console.WriteLine("[MeshFactory] Creating triangle mesh...");

            // Define triangle vertices with positions and UVs
            float[] vertices = new float[]
            {
                 0.0f,  0.5f, 0.0f,    0.5f, 1.0f, // Top vertex (позиция + UV)
                -0.5f, -0.5f, 0.0f,    0.0f, 0.0f, // Bottom left vertex
                 0.5f, -0.5f, 0.0f,    1.0f, 0.0f  // Bottom right vertex
            };

            Console.WriteLine("[MeshFactory] Vertex data created with {0} vertices", vertices.Length / 5);

            // Create and bind Vertex Array Object
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            Console.WriteLine("[MeshFactory] Created VAO: {0}", vao);

            // Create and bind Vertex Buffer Object
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            
            Console.WriteLine("[MeshFactory] Created VBO: {0}", vbo);

            // Upload vertex data to GPU
            fixed (void* v = &vertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }

            Console.WriteLine("[MeshFactory] Uploaded vertex data to GPU");

            // Set up vertex attributes
            // Position attribute
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
            gl.EnableVertexAttribArray(0);

            // UV attribute
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(1);

            Console.WriteLine("[MeshFactory] Set up vertex attributes");

            // Return mesh data
            var meshData = new MeshData(vao, vbo, 3);
            Console.WriteLine("[MeshFactory] Triangle mesh created successfully");
            return meshData;
        }

        public enum VertexAttributeType
        {
            Position,
            Normal,
            UV,
            Color,
            // Можно добавить другие типы
        }

        public struct VertexAttribute
        {
            public VertexAttributeType Type;
            public int Size;    // Количество float-ов
            public int Offset;  // Смещение в байтах
            public int Location; // Индекс атрибута в шейдере
        }

        public struct MeshDescription
        {
            public float[] Vertices;                // Вершинные данные
            public uint[]? Indices;                 // Индексы (если есть)
            public VertexAttribute[] Attributes;    // Описание структуры вершины
            public GLEnum PrimitiveType;            // Тип примитива (Triangles, Lines и т.д.)
        }

        public static unsafe MeshData CreateMesh(GL gl, MeshDescription desc)
        {
            // 1. Создаём VAO
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // 2. Создаём VBO
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* v = &desc.Vertices[0])
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(desc.Vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }

            uint? ebo = null;
            int indexCount = 0;
            if (desc.Indices != null && desc.Indices.Length > 0)
            {
                // 3. Создаём EBO (если есть индексы)
                uint eboVal = gl.GenBuffer();
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, eboVal);
                fixed (void* i = &desc.Indices[0])
                {
                    gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(desc.Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
                }
                ebo = eboVal;
                indexCount = desc.Indices.Length;
            }

            // 4. Настраиваем атрибуты
            int stride = 0;
            foreach (var attr in desc.Attributes)
                stride += attr.Size * sizeof(float);
            foreach (var attr in desc.Attributes)
            {
                gl.VertexAttribPointer((uint)attr.Location, attr.Size, VertexAttribPointerType.Float, false, (uint)stride, (void*)attr.Offset);
                gl.EnableVertexAttribArray((uint)attr.Location);
            }

            // 5. Возвращаем MeshData
            int vertexCount = desc.Indices != null && desc.Indices.Length > 0
                ? desc.Indices.Length
                : desc.Vertices.Length / (stride / sizeof(float));
            return new MeshData(vao, vbo, ebo, vertexCount, indexCount, desc.PrimitiveType);
        }

        /// <summary>
        /// Создаёт квадрат (плоскость 2D, XY, с UV).
        /// </summary>
        public static MeshData CreateQuad(GL gl)
        {
            // Вершины: позиция (x, y, z), UV (u, v)
            float[] vertices = new float[]
            {
                //  x     y    z     u    v
                -0.5f, -0.5f, 0f,   0f, 0f, // левый нижний
                 0.5f, -0.5f, 0f,   1f, 0f, // правый нижний
                 0.5f,  0.5f, 0f,   1f, 1f, // правый верхний
                -0.5f,  0.5f, 0f,   0f, 1f  // левый верхний
            };
            uint[] indices = new uint[]
            {
                0, 1, 2,
                2, 3, 0
            };
            var attributes = new[]
            {
                new VertexAttribute { Type = VertexAttributeType.Position, Size = 3, Offset = 0, Location = 0 },
                new VertexAttribute { Type = VertexAttributeType.UV, Size = 2, Offset = 3 * sizeof(float), Location = 1 }
            };
            var desc = new MeshDescription
            {
                Vertices = vertices,
                Indices = indices,
                Attributes = attributes,
                PrimitiveType = GLEnum.Triangles
            };
            return CreateMesh(gl, desc);
        }

        /// <summary>
        /// Создаёт куб (позиция + цвет + UV, 8 вершин, 12 треугольников).
        /// </summary>
        public static MeshData CreateCube(GL gl)
        {
            // Вершины: позиция (x, y, z), цвет (r, g, b), UV (u, v)
            float[] vertices = new float[]
            {
                // x     y     z     r   g   b    u   v
                -0.5f, -0.5f, -0.5f, 1, 0, 0,   0, 0, // 0
                 0.5f, -0.5f, -0.5f, 0, 1, 0,   1, 0, // 1
                 0.5f,  0.5f, -0.5f, 0, 0, 1,   1, 1, // 2
                -0.5f,  0.5f, -0.5f, 1, 1, 0,   0, 1, // 3
                -0.5f, -0.5f,  0.5f, 1, 0, 1,   0, 0, // 4
                 0.5f, -0.5f,  0.5f, 0, 1, 1,   1, 0, // 5
                 0.5f,  0.5f,  0.5f, 1, 1, 1,   1, 1, // 6
                -0.5f,  0.5f,  0.5f, 0, 0, 0,   0, 1  // 7
            };
            uint[] indices = new uint[]
            {
                // Задняя грань (z = -0.5)
                0, 2, 1, 0, 3, 2,
                // Передняя грань (z = +0.5)
                4, 5, 6, 4, 6, 7,
                // Левая грань (x = -0.5)
                0, 7, 3, 0, 4, 7,
                // Правая грань (x = +0.5)
                1, 2, 6, 1, 6, 5,
                // Нижняя грань (y = -0.5)
                0, 1, 5, 0, 5, 4,
                // Верхняя грань (y = +0.5)
                3, 7, 6, 3, 6, 2
            };
            var attributes = new[]
            {
                new VertexAttribute { Type = VertexAttributeType.Position, Size = 3, Offset = 0, Location = 0 },
                new VertexAttribute { Type = VertexAttributeType.Color, Size = 3, Offset = 3 * sizeof(float), Location = 1 },
                new VertexAttribute { Type = VertexAttributeType.UV, Size = 2, Offset = 6 * sizeof(float), Location = 2 }
            };
            var desc = new MeshDescription
            {
                Vertices = vertices,
                Indices = indices,
                Attributes = attributes,
                PrimitiveType = GLEnum.Triangles
            };
            return CreateMesh(gl, desc);
        }

        /// <summary>
        /// Создаёт меш четырёхгранной пирамиды (основание + 4 боковые грани).
        /// </summary>
        public static MeshData CreatePyramid(GL gl)
        {
            // Вершины: позиция (x, y, z), цвет (r, g, b), UV (u, v)
            float[] vertices = new float[]
            {
                // Основание (квадрат)
                -0.5f, 0f, -0.5f, 1, 0, 0, 0, 0, // 0
                 0.5f, 0f, -0.5f, 0, 1, 0, 1, 0, // 1
                 0.5f, 0f,  0.5f, 0, 0, 1, 1, 1, // 2
                -0.5f, 0f,  0.5f, 1, 1, 0, 0, 1, // 3
                // Вершина пирамиды
                 0f,  1f,  0f,  1, 0, 1, 0.5f, 0.5f // 4
            };
            uint[] indices = new uint[]
            {
                // Основание (обход против часовой стрелки при взгляде снизу)
                0, 1, 2, 0, 2, 3,
                // Боковые грани (каждая - треугольник, обход против часовой стрелки при взгляде снаружи)
                0, 4, 1,
                1, 4, 2,
                2, 4, 3,
                3, 4, 0
            };
            var attributes = new[]
            {
                new VertexAttribute { Type = VertexAttributeType.Position, Size = 3, Offset = 0, Location = 0 },
                new VertexAttribute { Type = VertexAttributeType.Color, Size = 3, Offset = 3 * sizeof(float), Location = 1 },
                new VertexAttribute { Type = VertexAttributeType.UV, Size = 2, Offset = 6 * sizeof(float), Location = 2 }
            };
            var desc = new MeshDescription
            {
                Vertices = vertices,
                Indices = indices,
                Attributes = attributes,
                PrimitiveType = GLEnum.Triangles
            };
            return CreateMesh(gl, desc);
        }

        /// <summary>
        /// Создаёт меш сферы (UV-сфера, face наружу).
        /// </summary>
        public static MeshData CreateSphere(GL gl, int sectorCount = 32, int stackCount = 16)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            float radius = 0.5f;
            for (int i = 0; i <= stackCount; ++i)
            {
                float stackAngle = MathF.PI / 2 - i * MathF.PI / stackCount; // от pi/2 до -pi/2
                float xy = radius * MathF.Cos(stackAngle);
                float z = radius * MathF.Sin(stackAngle);
                for (int j = 0; j <= sectorCount; ++j)
                {
                    float sectorAngle = j * 2 * MathF.PI / sectorCount; // от 0 до 2pi
                    float x = xy * MathF.Cos(sectorAngle);
                    float y = xy * MathF.Sin(sectorAngle);
                    float u = (float)j / sectorCount;
                    float v = (float)i / stackCount;
                    // Цвет задаём по UV для наглядности
                    vertices.AddRange(new float[] { x, y, z, u, v, u, 1-v });
                }
            }
            // Индексы
            for (int i = 0; i < stackCount; ++i)
            {
                for (int j = 0; j < sectorCount; ++j)
                {
                    uint first = (uint)(i * (sectorCount + 1) + j);
                    uint second = (uint)(first + sectorCount + 1);
                    // Два треугольника на каждый квад
                    indices.Add(first);
                    indices.Add(second);
                    indices.Add(first + 1);
                    indices.Add(second);
                    indices.Add(second + 1);
                    indices.Add(first + 1);
                }
            }
            var attributes = new[]
            {
                new VertexAttribute { Type = VertexAttributeType.Position, Size = 3, Offset = 0, Location = 0 },
                new VertexAttribute { Type = VertexAttributeType.UV, Size = 2, Offset = 3 * sizeof(float), Location = 1 },
                new VertexAttribute { Type = VertexAttributeType.Color, Size = 2, Offset = 5 * sizeof(float), Location = 2 }
            };
            var desc = new MeshDescription
            {
                Vertices = vertices.ToArray(),
                Indices = indices.ToArray(),
                Attributes = attributes,
                PrimitiveType = GLEnum.Triangles
            };
            return CreateMesh(gl, desc);
        }

        /// <summary>
        /// Создаёт меш цилиндра (face наружу).
        /// </summary>
        public static MeshData CreateCylinder(GL gl, int sectorCount = 32, int stackCount = 1, float radius = 0.5f, float height = 1.0f)
        {
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();
            float halfHeight = height / 2f;
            // --- Вершины боковой поверхности ---
            for (int i = 0; i <= stackCount; ++i)
            {
                float h = -halfHeight + i * height / stackCount;
                float v = (float)i / stackCount;
                for (int j = 0; j <= sectorCount; ++j)
                {
                    float sectorAngle = j * 2 * MathF.PI / sectorCount;
                    float x = radius * MathF.Cos(sectorAngle);
                    float y = radius * MathF.Sin(sectorAngle);
                    float u = (float)j / sectorCount;
                    // Цвет задаём по UV для наглядности
                    vertices.AddRange(new float[] { x, y, h, u, v, u, 1-v });
                }
            }
            // --- Индексы боковой поверхности ---
            for (int i = 0; i < stackCount; ++i)
            {
                for (int j = 0; j < sectorCount; ++j)
                {
                    uint current = (uint)(i * (sectorCount + 1) + j);
                    uint next = (uint)((i + 1) * (sectorCount + 1) + j);
                    // Инвертированный порядок обхода
                    indices.Add(current);
                    indices.Add(current + 1);
                    indices.Add(next);

                    indices.Add(current + 1);
                    indices.Add(next + 1);
                    indices.Add(next);
                }
            }
            // --- Вершины и индексы верхней крышки ---
            uint baseIndex = (uint)vertices.Count / 7;
            vertices.AddRange(new float[] { 0, 0, halfHeight, 0.5f, 0.5f, 1, 1 }); // центр
            for (int j = 0; j <= sectorCount; ++j)
            {
                float sectorAngle = j * 2 * MathF.PI / sectorCount;
                float x = radius * MathF.Cos(sectorAngle);
                float y = radius * MathF.Sin(sectorAngle);
                float u = 0.5f + 0.5f * MathF.Cos(sectorAngle);
                float v = 0.5f + 0.5f * MathF.Sin(sectorAngle);
                vertices.AddRange(new float[] { x, y, halfHeight, u, v, u, v });
            }
            for (int j = 0; j < sectorCount; ++j)
            {
                indices.Add(baseIndex);
                indices.Add(baseIndex + (uint)j + 1);
                indices.Add(baseIndex + (uint)j + 2);
            }
            // --- Вершины и индексы нижней крышки ---
            baseIndex = (uint)vertices.Count / 7;
            vertices.AddRange(new float[] { 0, 0, -halfHeight, 0.5f, 0.5f, 1, 0 }); // центр
            for (int j = 0; j <= sectorCount; ++j)
            {
                float sectorAngle = j * 2 * MathF.PI / sectorCount;
                float x = radius * MathF.Cos(sectorAngle);
                float y = radius * MathF.Sin(sectorAngle);
                float u = 0.5f + 0.5f * MathF.Cos(sectorAngle);
                float v = 0.5f + 0.5f * MathF.Sin(sectorAngle);
                vertices.AddRange(new float[] { x, y, -halfHeight, u, v, u, v });
            }
            for (int j = 0; j < sectorCount; ++j)
            {
                indices.Add(baseIndex);
                indices.Add(baseIndex + (uint)j + 2);
                indices.Add(baseIndex + (uint)j + 1);
            }
            var attributes = new[]
            {
                new VertexAttribute { Type = VertexAttributeType.Position, Size = 3, Offset = 0, Location = 0 },
                new VertexAttribute { Type = VertexAttributeType.UV, Size = 2, Offset = 3 * sizeof(float), Location = 1 },
                new VertexAttribute { Type = VertexAttributeType.Color, Size = 2, Offset = 5 * sizeof(float), Location = 2 }
            };
            var desc = new MeshDescription
            {
                Vertices = vertices.ToArray(),
                Indices = indices.ToArray(),
                Attributes = attributes,
                PrimitiveType = GLEnum.Triangles
            };
            return CreateMesh(gl, desc);
        }
    }
} 