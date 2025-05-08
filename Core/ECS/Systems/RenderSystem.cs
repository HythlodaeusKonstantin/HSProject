using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Numerics;
using System.Runtime.InteropServices;
using System.IO;
using System;
using Engine.Core.ECS;
using Engine.Core.Logging;
using System.Collections.Generic;
using Engine.Core.ECS.Components;
using Engine.Core.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace Engine.Core.Rendering
{
    public interface IRenderContext
    {
        System.Numerics.Vector2 ViewportSize { get; }
        void DrawRectangle(System.Drawing.Rectangle bounds, System.Drawing.Color color, float depth = 0);
        void DrawTexture(System.Drawing.Rectangle bounds, int textureId, System.Drawing.Color tint, float depth = 0);
        void DrawText(string text, System.Numerics.Vector2 position, System.Drawing.Color color, float scale = 1.0f, float depth = 0);
        
        /// <summary>
        /// Сохраняет текущее состояние OpenGL
        /// </summary>
        void SaveGLState();
        
        /// <summary>
        /// Восстанавливает сохраненное состояние OpenGL
        /// </summary>
        void RestoreGLState();
    }

    public class RenderSystem : ISystem, IRenderContext, IDisposable
    {
        private readonly GL gl;
        private readonly IShaderManager shaderManager;
        private readonly IEntityManager entityManager;
        private Matrix4x4 projectionMatrix;
        private readonly Matrix4x4 viewMatrix;
        private Matrix4x4 modelMatrix;

        // SimpleShader
        private int _programId;
        private string _vertSrc = string.Empty;
        private string _fragSrc = string.Empty;
        private bool _initialized = false;

        // --- FBO and Depth Buffer (Manual) ---
        private uint _fbo;
        private uint _colorTex;
        private uint _depthRbo;
        private int _fboWidth = 800;
        private int _fboHeight = 600;

        private readonly ILogger? _logger;

        // --- UI Rendering ---
        private int _uiShaderProgramId = -1;
        private uint _uiVao = 0;
        private uint _uiVbo = 0;
        private uint _uiEbo = 0;
        private bool _uiInitialized = false;

        private uint _whiteTextureId = 0;

        // Хранение состояния OpenGL
        private bool _savedDepthTestEnabled = false;
        private bool _savedBlendEnabled = false;

        // Кэш текстур текста
        private readonly Dictionary<string, int> _textTextureCache = new();

        public RenderSystem(GL gl, IEntityManager entityManager, IShaderManager shaderManager, ILogger? logger = null)
        {
            _logger = logger;
            try
            {
                _logger?.Log(LogType.Info, "RenderSystem", $"Log file will be created at: {System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "render.log")}");
                _logger?.Log(LogType.Info, "RenderSystem", $"Initializing at {DateTime.Now}");
                _logger?.Log(LogType.Info, "RenderSystem", $"Working directory: {Environment.CurrentDirectory}");
                _logger?.Log(LogType.Info, "RenderSystem", $"Base directory: {AppDomain.CurrentDomain.BaseDirectory}");
                _logger?.Log(LogType.Info, "RenderSystem", $"OpenGL Version: {gl.GetStringS(StringName.Version)}");
                _logger?.Log(LogType.Info, "RenderSystem", $"OpenGL Vendor: {gl.GetStringS(StringName.Vendor)}");
                _logger?.Log(LogType.Info, "RenderSystem", $"OpenGL Renderer: {gl.GetStringS(StringName.Renderer)}");

                this.gl = gl;
                this.entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
                this.shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));

                // Enable depth testing
                gl.Enable(EnableCap.DepthTest);
                gl.DepthFunc(DepthFunction.Less);
                _logger?.Log(LogType.Info, "RenderSystem", "Depth testing ENABLED");

                // Enable blending
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                CheckGLError("Enable blending");

                // Enable face culling
                gl.Enable(EnableCap.CullFace);
                gl.CullFace(GLEnum.Back);
                gl.FrontFace(FrontFaceDirection.Ccw);
                
                _logger?.Log(LogType.Info, "RenderSystem", "Face culling ENABLED (back, CCW)");
                CheckGLError("Enable face culling");

                _logger?.Log(LogType.Info, "RenderSystem", "OpenGL state initialized");
                _logger?.Log(LogType.Info, "RenderSystem", "RenderSystem: - Depth testing enabled");
                _logger?.Log(LogType.Info, "RenderSystem", "RenderSystem: - Blending enabled");
                _logger?.Log(LogType.Info, "RenderSystem", "RenderSystem: - Face culling enabled");

                // --- FBO and Depth Buffer Setup ---
                InitFbo(_fboWidth, _fboHeight);

                projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 4.0f,
                    800.0f / 600.0f,
                    0.1f,
                    100.0f
                );

                viewMatrix = Matrix4x4.CreateLookAt(
                    new Vector3(0.0f, 0.0f, 3.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f)
                );

                // Initialize model matrix
                modelMatrix = Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.0f);

                _logger?.Log(LogType.Info, "RenderSystem", "Matrices initialized");
                _logger?.Log(LogType.Info, "RenderSystem", $"View Matrix: {viewMatrix}");
                _logger?.Log(LogType.Info, "RenderSystem", $"Projection Matrix: {projectionMatrix}");
                _logger?.Log(LogType.Info, "RenderSystem", $"Model Matrix: {modelMatrix}");

                CheckGLError("Initialize");
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Fatal error during initialization - {ex}");
                throw;
            }
        }

        public void Initialize()
        {
            if (_initialized) return;
            try
            {
                _vertSrc = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders", "SimpleShader.vert"));
                _fragSrc = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders", "SimpleShader.frag"));
                _logger?.Log(LogType.Info, "RenderSystem", "Loaded SimpleShader.vert and SimpleShader.frag");
                _programId = shaderManager.GetOrCreateProgram(_vertSrc, _fragSrc);
                _logger?.Log(LogType.Info, "RenderSystem", $"SimpleShader programId: {_programId}");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error initializing shader program: {ex}");
                throw;
            }
        }

        private void CheckGLError(string context)
        {
            var error = gl.GetError();
            if (error != GLEnum.NoError)
            {
                var nonCritical = (context?.Contains("Clear FBO buffers") == true) ? " [NonCritical]" : string.Empty;
                _logger?.Log(LogType.Error, "OpenGL", $"OpenGL Error after {context}:{nonCritical} {error}");
            }
        }

        private unsafe void InitFbo(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"InitFbo called with invalid size: {width}x{height}");
                throw new ArgumentException($"FBO width and height must be > 0, got {width}x{height}");
            }
            _logger?.Log(LogType.Info, "RenderSystem", $"InitFbo: width={width}, height={height}");
            _fboWidth = width;
            _fboHeight = height;
            // 1. FBO
            _fbo = gl.GenFramebuffer();
            gl.BindFramebuffer(GLEnum.Framebuffer, _fbo);
            // 2. Color texture
            _colorTex = gl.GenTexture();
            gl.BindTexture(GLEnum.Texture2D, _colorTex);
            gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, _colorTex, 0);
            // 3. Depth renderbuffer
            _depthRbo = gl.GenRenderbuffer();
            gl.BindRenderbuffer(GLEnum.Renderbuffer, _depthRbo);
            gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.DepthComponent24, (uint)width, (uint)height);
            gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Renderbuffer, _depthRbo);
            // 4. Check status
            if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
                _logger?.Log(LogType.Error, "RenderSystem", "Framebuffer is not complete!");
            else
                _logger?.Log(LogType.Info, "RenderSystem", $"FBO created: {_fbo}, ColorTex: {_colorTex}, DepthRBO: {_depthRbo}");
            // 5. Unbind
            gl.BindFramebuffer(GLEnum.Framebuffer, 0);

            // Обновляем матрицу проекции с новым aspect ratio
            float aspectRatio = (float)width / height;
            UpdateProjectionMatrix(aspectRatio);
        }

        // Метод для изменения размера FBO при изменении размера окна
        public unsafe void ResizeFbo(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"ResizeFbo called with invalid size: {width}x{height}");
                throw new ArgumentException($"FBO width and height must be > 0, got {width}x{height}");
            }
            _logger?.Log(LogType.Info, "RenderSystem", $"ResizeFbo: width={width}, height={height}");
            try
            {
                _logger?.Log(LogType.Info, "RenderSystem", $"Resizing FBO from {_fboWidth}x{_fboHeight} to {width}x{height}");
                
                // Если размеры не изменились - ничего не делаем
                if (width == _fboWidth && height == _fboHeight) return;
                
                // Привязываем FBO для обновления
                gl.BindFramebuffer(GLEnum.Framebuffer, _fbo);
                
                // Фиксируем новые размеры
                _fboWidth = width;
                _fboHeight = height;
                
                // Удаляем старые ресурсы
                gl.DeleteTexture(_colorTex);
                gl.DeleteRenderbuffer(_depthRbo);
                
                // Создаем новую текстуру цвета
                _colorTex = gl.GenTexture();
                gl.BindTexture(GLEnum.Texture2D, _colorTex);
                gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
                gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
                gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
                gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, _colorTex, 0);
                
                // Создаем новый буфер глубины
                _depthRbo = gl.GenRenderbuffer();
                gl.BindRenderbuffer(GLEnum.Renderbuffer, _depthRbo);
                gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.DepthComponent24, (uint)width, (uint)height);
                gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Renderbuffer, _depthRbo);
                
                // Проверяем статус FBO
                if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
                    _logger?.Log(LogType.Error, "RenderSystem", "Resized Framebuffer is not complete!");
                else
                    _logger?.Log(LogType.Info, "RenderSystem", $"FBO resized successfully: {_fbo}, ColorTex: {_colorTex}, DepthRBO: {_depthRbo}");
                
                // Отвязываем FBO
                gl.BindFramebuffer(GLEnum.Framebuffer, 0);

                // Обновляем матрицу проекции с новым aspect ratio
                float aspectRatio = (float)width / height;
                UpdateProjectionMatrix(aspectRatio);
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error resizing FBO: {ex}");
                throw;
            }
        }

        public void BeginFrame()
        {
            try
            {
                gl.BindFramebuffer(GLEnum.Framebuffer, 0); // Используем дефолтный фреймбуфер
                gl.Viewport(0, 0, (uint)_fboWidth, (uint)_fboHeight);
                _logger?.Log(LogType.Info, "RenderSystem", $"[BeginFrame] Using default framebuffer, Viewport: 0,0,{_fboWidth},{_fboHeight}");
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                CheckGLError("Clear default framebuffer");
                shaderManager.UseProgram(_programId);
                CheckGLError("Use shader program");
                _logger?.Log(LogType.Info, "RenderSystem", "Frame setup complete (default framebuffer)");
                _logger?.Log(LogType.Info, "RenderSystem", $"Clear color: (0.0, 0.0, 1.0, 1.0)");
                _logger?.Log(LogType.Info, "RenderSystem", $"Shader program: {_programId}");
            }
            catch(Exception ex)
            {
                _logger?.Log(LogType.Warning, "RenderSystem", $"[BeginFrame] Failed to get OpenGL state: {ex}");
            }
            _logger?.Log(LogType.Info, "RenderSystem", "Beginning frame (default framebuffer)");
        }

        public unsafe void RenderMesh(MeshData mesh, Matrix4x4 modelMatrix, System.Numerics.Vector4 color, bool useTexture = false, int textureId = 0)
        {
            shaderManager.SetUniform(_programId, "u_Model", modelMatrix);
            shaderManager.SetUniform(_programId, "u_UseTexture", useTexture ? 1.0f : 0.0f);
            if (useTexture)
            {
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D, (uint)textureId);
                shaderManager.SetUniform(_programId, "u_Texture", 0.0f);
            }
            else
            {
                shaderManager.SetUniform(_programId, "u_Color", color);
            }
            _logger?.Log(LogType.Info, "RenderSystem", $"[RenderMesh] Drawing {mesh.VertexCount} vertices, VAO={mesh.Vao}, EBO={(mesh.Ebo.HasValue ? mesh.Ebo.Value.ToString() : "none")}");
            gl.BindVertexArray(mesh.Vao);
            if (mesh.Ebo.HasValue && mesh.IndexCount > 0)
            {
                gl.DrawElements(mesh.PrimitiveType, (uint)mesh.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
            }
            else
            {
                gl.DrawArrays(mesh.PrimitiveType, 0, (uint)mesh.VertexCount);
            }
            gl.BindVertexArray(0);
        }

        public unsafe void EndFrame()
        {
            try
            {
                // Принудительное завершение всех рендеринговых операций и отображение на экран
                gl.Flush();
                gl.Finish();
                
                // Получаем размер окна
                var viewport = stackalloc int[4];
                gl.GetInteger(GLEnum.Viewport, viewport);
                CheckGLError("GetInteger(Viewport)");
                var windowWidth = viewport[2];
                var windowHeight = viewport[3];
                
                _logger?.Log(LogType.Info, "RenderSystem", $"EndFrame: Using default framebuffer ({windowWidth}x{windowHeight})");
                _logger?.Log(LogType.Info, "RenderSystem", "Ending frame (default framebuffer)");
                
                CheckGLError("End Frame (default framebuffer)");
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error in EndFrame - {ex}");
                throw;
            }
        }

        public void UpdateModelMatrix(Matrix4x4 matrix)
        {
            try
            {
                modelMatrix = matrix;
                shaderManager.SetUniform(_programId, "u_Model", modelMatrix);
                _logger?.Log(LogType.Info, "RenderSystem", $"Model matrix updated to: {modelMatrix}");
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error updating model matrix - {ex}");
                throw;
            }
        }

        public void UpdateProjectionMatrix(float aspectRatio)
        {
            try
            {
                projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathF.PI / 4.0f,
                    aspectRatio,
                    0.1f,
                    100.0f
                );
                shaderManager.SetUniform(_programId, "u_Projection", projectionMatrix);
                _logger?.Log(LogType.Info, "RenderSystem", $"Projection matrix updated with aspect ratio {aspectRatio}");
                _logger?.Log(LogType.Info, "RenderSystem", $"New projection matrix: {projectionMatrix}");
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error updating projection matrix - {ex}");
                throw;
            }
        }

        public void Update(double deltaTime)
        {
            try
            {
                _logger?.Log(LogType.Info, "RenderSystem", $"Update called with deltaTime {deltaTime}");
                if (!_initialized) Initialize();
                // Обновляем только матрицы камеры и шейдеры, рендер теперь в Render
                var (viewMatrix, projectionMatrix) = GetCameraMatrices();
                shaderManager.SetUniform(_programId, "u_View", viewMatrix);
                shaderManager.SetUniform(_programId, "u_Projection", projectionMatrix);
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error in Update - {ex}");
                throw;
            }
        }

        // Method to convert matrix from Silk.NET.Maths to System.Numerics
        private Matrix4x4 ConvertMatrix(Matrix4X4<float> matrix)
        {
            return new Matrix4x4(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        // Новый метод для получения матриц камеры
        private (Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix) GetCameraMatrices()
        {
            _logger?.Log(LogType.Info, "RenderSystem", "Getting camera matrices");
            
            // Поиск активной камеры (сущность с TransformComponent и CameraComponent)
            var cameraEntities = entityManager.QueryEntities(
                typeof(TransformComponent), 
                typeof(CameraComponent));
            
            Entity? activeCameraEntity = null;
            TransformComponent cameraTransform = TransformComponent.Default;
            CameraComponent cameraComponent = 
                CameraComponent.Default;
            
            foreach (var camEntity in cameraEntities)
            {
                // Берем первую найденную камеру как активную
                activeCameraEntity = camEntity;
                cameraTransform = entityManager.GetComponent<TransformComponent>(camEntity);
                cameraComponent = entityManager.GetComponent<CameraComponent>(camEntity);
                break; // Пока используем только первую камеру
            }
            
            // Если есть камера, рассчитываем матрицы вида и проекции
            Matrix4x4 viewMatrix;
            Matrix4x4 projectionMatrix;
            
            if (activeCameraEntity.HasValue)
            {
                // Преобразуем матрицы из Silk.NET.Maths в System.Numerics
                viewMatrix = ConvertMatrix(cameraComponent.CreateViewMatrix(cameraTransform));
                projectionMatrix = ConvertMatrix(cameraComponent.ProjectionMatrix);
                _logger?.Log(LogType.Info, "RenderSystem", $"Using camera entity #{activeCameraEntity.Value.Id} for rendering");
            }
            else
            {
                // Если камеры нет, используем стандартные матрицы
                viewMatrix = this.viewMatrix;
                projectionMatrix = this.projectionMatrix;
                _logger?.Log(LogType.Warning, "RenderSystem", "No camera entity found, using default view/projection");
            }
            
            return (viewMatrix, projectionMatrix);
        }

        public void Render()
        {
            try
            {
                if (!_initialized) Initialize();
                var (viewMatrix, projectionMatrix) = GetCameraMatrices();
                BeginFrame();
                shaderManager.SetUniform(_programId, "u_View", viewMatrix);
                shaderManager.SetUniform(_programId, "u_Projection", projectionMatrix);
                IEnumerable<Entity> entities = entityManager.QueryEntities(
                    typeof(TransformComponent),
                    typeof(MeshRendererComponent));
                foreach (var entity in entities)
                {
                    var transform = entityManager.GetComponent<TransformComponent>(entity);
                    var meshComp = entityManager.GetComponent<MeshRendererComponent>(entity);
                    Matrix4x4 modelMatrix = ConvertMatrix(transform.ModelMatrix);
                    RenderMesh(meshComp.MeshData, modelMatrix, meshComp.Color, false, 0);
                }
                EndFrame();
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error in Render - {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _logger?.Log(LogType.Info, "RenderSystem", "Disposing");
                shaderManager.Shutdown();
                // --- UI ресурсы ---
                if (_uiVao != 0) gl.DeleteVertexArray(_uiVao);
                if (_uiVbo != 0) gl.DeleteBuffer(_uiVbo);
                if (_uiEbo != 0) gl.DeleteBuffer(_uiEbo);
                if (_whiteTextureId != 0) gl.DeleteTexture(_whiteTextureId);
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "RenderSystem", $"Error during disposal - {ex}");
                throw;
            }
        }

        // Реализация IRenderContext
        public System.Numerics.Vector2 ViewportSize => new System.Numerics.Vector2(_fboWidth, _fboHeight);

        public void SaveGLState()
        {
            _savedDepthTestEnabled = gl.IsEnabled(EnableCap.DepthTest);
            _savedBlendEnabled = gl.IsEnabled(EnableCap.Blend);
            _logger?.Log(LogType.Info, "RenderSystem", $"Сохранено состояние OpenGL: DepthTest={_savedDepthTestEnabled}, Blend={_savedBlendEnabled}");
        }
        
        public void RestoreGLState()
        {
            if (_savedDepthTestEnabled)
                gl.Enable(EnableCap.DepthTest);
            else
                gl.Disable(EnableCap.DepthTest);
                
            if (_savedBlendEnabled)
                gl.Enable(EnableCap.Blend);
            else
                gl.Disable(EnableCap.Blend);
                
            _logger?.Log(LogType.Info, "RenderSystem", $"Восстановлено состояние OpenGL: DepthTest={_savedDepthTestEnabled}, Blend={_savedBlendEnabled}");
        }

        public unsafe void DrawRectangle(System.Drawing.Rectangle bounds, System.Drawing.Color color, float depth = 0)
        {
   
            // Отключаем тест глубины для 2D рендеринга
            gl.Disable(EnableCap.DepthTest);
            gl.UseProgram((uint)_uiShaderProgramId);
            // Матрица ортографической проекции для UI
            var ortho = Matrix4x4.CreateOrthographicOffCenter(0, _fboWidth, _fboHeight, 0, -1, 1);
            int loc = gl.GetUniformLocation((uint)_uiShaderProgramId, "ProjectionView");
            if (loc >= 0)
            {
                float[] mat = new float[16] {
                    ortho.M11, ortho.M12, ortho.M13, ortho.M14,
                    ortho.M21, ortho.M22, ortho.M23, ortho.M24,
                    ortho.M31, ortho.M32, ortho.M33, ortho.M34,
                    ortho.M41, ortho.M42, ortho.M43, ortho.M44
                };
                fixed(float* m = mat) gl.UniformMatrix4(loc, 1, false, m);
            }
            
            // Устанавливаем флаг UseTexture в false для рендеринга цветом
            int useTexLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "UseTexture");
            if (useTexLoc >= 0) gl.Uniform1(useTexLoc, 0); // 0 = false
            
            // Цвет
            float r = color.R / 255f, g = color.G / 255f, b = color.B / 255f, a = color.A / 255f;
            // Вершины quad с нужным размером и цветом
            float x = bounds.X, y = bounds.Y, w = bounds.Width, h = bounds.Height;
            float[] quadVertices = new float[] {
                x,     y,     0,0, r,g,b,a,
                x+w,   y,     1,0, r,g,b,a,
                x+w,   y+h,   1,1, r,g,b,a,
                x,     y+h,   0,1, r,g,b,a
            };

            if (_whiteTextureId == 0)
            {
                byte[] whitePixel = { 255, 255, 255, 255 };
                _whiteTextureId = gl.GenTexture();
                gl.BindTexture(GLEnum.Texture2D, _whiteTextureId);
                fixed (byte* p = whitePixel)
                    gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, 1, 1, 0, GLEnum.Rgba, GLEnum.UnsignedByte, p);
                gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Nearest);
                gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Nearest);
                gl.BindTexture(GLEnum.Texture2D, 0);
            }
            int texLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "Texture");
            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, _whiteTextureId);
            if (texLoc >= 0) gl.Uniform1(texLoc, 0);

            gl.BindVertexArray(_uiVao);
            gl.BindBuffer(GLEnum.ArrayBuffer, _uiVbo);
            fixed(float* v = quadVertices) gl.BufferSubData(GLEnum.ArrayBuffer, IntPtr.Zero, (nuint)(quadVertices.Length * sizeof(float)), v);
            gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)IntPtr.Zero);
            gl.BindVertexArray(0);
 
            _logger?.Log(LogType.Info, "RenderSystem", $"Отрисован прямоугольник UI: {bounds}, цвет: {color}");
        }

        public unsafe void DrawTexture(System.Drawing.Rectangle bounds, int textureId, System.Drawing.Color tint, float depth = 0)
        {
      
            // Отключаем тест глубины для 2D рендеринга
            gl.Disable(EnableCap.DepthTest);
            gl.UseProgram((uint)_uiShaderProgramId);
            // Матрица ортографической проекции для UI
            var ortho = Matrix4x4.CreateOrthographicOffCenter(0, _fboWidth, _fboHeight, 0, -1, 1);
            int loc = gl.GetUniformLocation((uint)_uiShaderProgramId, "ProjectionView");
            if (loc >= 0)
            {
                float[] mat = new float[16] {
                    ortho.M11, ortho.M12, ortho.M13, ortho.M14,
                    ortho.M21, ortho.M22, ortho.M23, ortho.M24,
                    ortho.M31, ortho.M32, ortho.M33, ortho.M34,
                    ortho.M41, ortho.M42, ortho.M43, ortho.M44
                };
                fixed(float* m = mat) gl.UniformMatrix4(loc, 1, false, m);
            }
            
            // Устанавливаем флаг UseTexture в true для рендеринга текстуры
            int useTexLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "UseTexture");
            if (useTexLoc >= 0) gl.Uniform1(useTexLoc, 1); // 1 = true
            
            // Цвет
            float r = tint.R / 255f, g = tint.G / 255f, b = tint.B / 255f, a = tint.A / 255f;
            float x = bounds.X, y = bounds.Y, w = bounds.Width, h = bounds.Height;
            float[] quadVertices = new float[] {
                x,     y,     0,0, r,g,b,a,
                x+w,   y,     1,0, r,g,b,a,
                x+w,   y+h,   1,1, r,g,b,a,
                x,     y+h,   0,1, r,g,b,a
            };
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, (uint)textureId);
            int texLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "Texture");
            if (texLoc >= 0) gl.Uniform1(texLoc, 0);
            gl.BindVertexArray(_uiVao);
            gl.BindBuffer(GLEnum.ArrayBuffer, _uiVbo);
            fixed(float* v = quadVertices) gl.BufferSubData(GLEnum.ArrayBuffer, IntPtr.Zero, (nuint)(quadVertices.Length * sizeof(float)), v);
            gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)IntPtr.Zero);
            gl.BindVertexArray(0);
            gl.BindTexture(TextureTarget.Texture2D, 0);
 
            _logger?.Log(LogType.Info, "RenderSystem", $"Отрисована текстура UI: {bounds}, текстура: {textureId}, тон: {tint}");
        }

        public void DrawText(string text, System.Numerics.Vector2 position, System.Drawing.Color color, float scale = 1.0f, float depth = 0)
        {
            // Ключ для кэша текстур
            string cacheKey = $"{text}_{color.ToArgb()}_{scale}";
            if (!_textTextureCache.TryGetValue(cacheKey, out int textureId))
            {
                // Генерируем bitmap с текстом через ImageSharp
                int width = (int)(text.Length * 16 * scale);
                int height = (int)(32 * scale);
                using (var img = new Image<Rgba32>(width, height))
                {
                    img.Mutate(ctx =>
                    {
                        var font = SystemFonts.CreateFont("Arial", 24 * scale);
                        var options = new DrawingOptions
                        {
                            GraphicsOptions = new GraphicsOptions { Antialias = true }
                        };
                        ctx.DrawText(options, text, font, SixLabors.ImageSharp.Color.FromRgba(color.R, color.G, color.B, color.A), new SixLabors.ImageSharp.PointF(0, 0));
                    });
                    // Копируем bitmap в OpenGL-текстуру
                    textureId = CreateGLTextureFromImage(img);
                    _textTextureCache[cacheKey] = textureId;
                }
            }
            // Рендерим quad с текстурой
            var bounds = new System.Drawing.Rectangle((int)position.X, (int)position.Y, (int)(text.Length * 16 * scale), (int)(32 * scale));
            DrawTexture(bounds, textureId, color, depth);
        }

        // Создание OpenGL-текстуры из ImageSharp Image
        private int CreateGLTextureFromImage(Image<Rgba32> img)
        {
            var pixels = new byte[img.Width * img.Height * 4];
            img.CopyPixelDataTo(pixels);
            int texId = (int)gl.GenTexture();
            gl.BindTexture(GLEnum.Texture2D, (uint)texId);
            unsafe
            {
                fixed (byte* p = pixels)
                {
                    gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)img.Width, (uint)img.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, p);
                }
            }
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            gl.BindTexture(GLEnum.Texture2D, 0);
            return texId;
        }

        public unsafe void InitUIRendering()
        {
            if (_uiInitialized) return;
            // Отключаем тест глубины для 2D рендеринга
            gl.Disable(EnableCap.DepthTest);
            // Отключаем отсечение полигонов
            gl.Disable(EnableCap.CullFace);
            // Загружаем UI-шейдеры
            string vertPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders", "ui_base.vert");
            string fragPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders", "ui_base.frag");
            if (!File.Exists(vertPath) || !File.Exists(fragPath))
                throw new FileNotFoundException($"UI shader files not found: {vertPath} или {fragPath}");
            _logger?.Log(LogType.Info, "RenderSystem", $"Загрузка UI шейдеров из: {vertPath} и {fragPath}");
            string vertSrc = File.ReadAllText(vertPath);
            string fragSrc = File.ReadAllText(fragPath);
            _uiShaderProgramId = shaderManager.GetOrCreateProgram(vertSrc, fragSrc);
            _logger?.Log(LogType.Info, "RenderSystem", $"UI шейдеры загружены, program ID: {_uiShaderProgramId}");

            // Создаём VAO/VBO для quad (2 треугольника)
            float[] quadVertices = new float[] {
                // pos      // uv    // color (rgba)
                0, 0,       0, 0,    1,1,1,1,
                1, 0,       1, 0,    1,1,1,1,
                1, 1,       1, 1,    1,1,1,1,
                0, 1,       0, 1,    1,1,1,1
            };
            uint[] indices = new uint[] { 0, 1, 2, 2, 3, 0 };
            _uiVao = gl.GenVertexArray();
            _uiVbo = gl.GenBuffer();
            _uiEbo = gl.GenBuffer();
            gl.BindVertexArray(_uiVao);
            gl.BindBuffer(GLEnum.ArrayBuffer, _uiVbo);
            fixed(float* v = quadVertices) gl.BufferData(GLEnum.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), v, GLEnum.StaticDraw);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, _uiEbo);
            fixed(uint* i = indices) gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, GLEnum.StaticDraw);
            int stride = 2 * sizeof(float) + 2 * sizeof(float) + 4 * sizeof(float); // pos+uv+color
            gl.EnableVertexAttribArray(0); gl.VertexAttribPointer((uint)0, 2, GLEnum.Float, false, (uint)stride, (void*)IntPtr.Zero);
            gl.EnableVertexAttribArray(1); gl.VertexAttribPointer((uint)1, 2, GLEnum.Float, false, (uint)stride, (void*)new IntPtr(2 * sizeof(float)));
            gl.EnableVertexAttribArray(2); gl.VertexAttribPointer((uint)2, 4, GLEnum.Float, false, (uint)stride, (void*)new IntPtr(4 * sizeof(float)));
            gl.BindVertexArray(0);
            
            // Проверка наличия uniform переменных
            gl.UseProgram((uint)_uiShaderProgramId);
            int projectionViewLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "ProjectionView");
            int textureLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "Texture");
            int useTextureLoc = gl.GetUniformLocation((uint)_uiShaderProgramId, "UseTexture");
            
            _logger?.Log(LogType.Info, "RenderSystem", $"Uniform locations: ProjectionView={projectionViewLoc}, Texture={textureLoc}, UseTexture={useTextureLoc}");
            
            _uiInitialized = true;
            _logger?.Log(LogType.Info, "RenderSystem", "UI рендеринг успешно инициализирован");
        }

        public void ResizeUI(int width, int height)
        {
            // Удаляем старые буферы
            if (_uiVao != 0) gl.DeleteVertexArray(_uiVao);
            if (_uiVbo != 0) gl.DeleteBuffer(_uiVbo);
            if (_uiEbo != 0) gl.DeleteBuffer(_uiEbo);
            _uiVao = 0;
            _uiVbo = 0;
            _uiEbo = 0;
            _uiInitialized = false;
        }
    }
}