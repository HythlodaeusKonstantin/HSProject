using Engine.Core.ECS;
using Engine.Core.ECS.Components;
using Engine.Core.Rendering;
using Engine.Core.Graphics;
using Silk.NET.Maths;
using System.Numerics;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Система, отвечающая за инициализацию и глобальную логику сцены (мира)
    /// </summary>
    public class SceneSystem : ISystem
    {
        private readonly EntityManager _entityManager;
        private readonly ShaderManager _shaderManager;
        private readonly Silk.NET.OpenGL.GL _gl;
        private bool _initialized = false;

        public SceneSystem(Silk.NET.OpenGL.GL gl, EntityManager entityManager, ShaderManager shaderManager)
        {
            _gl = gl;
            _entityManager = entityManager;
            _shaderManager = shaderManager;
        }

        public void Initialize()
        {
            if (_initialized) return;
            // Создание начальных сущностей
            CreateInitialEntities();
            _initialized = true;
        }

        public void Update(double deltaTime)
        {
            // Глобальная логика сцены (например, спавн, обработка событий и т.д.)
        }

        public void Render()
        {
            // Обычно SceneSystem не рендерит, но можно добавить визуализацию gizmo или debug info
        }

        private void CreateInitialEntities()
        {
            // Пример: создаём треугольник, квадрат, куб, камеру
            // --- Треугольник ---
            // var triangleMesh = MeshFactory.CreatePyramid(_gl);
            // var triangleColor = RandomColor();
            // var triangleTransform = TransformComponent.Default;
            // var triangleEntity = _entityManager.CreateEntity();
            // _entityManager.AddComponent(triangleEntity, new MeshRendererComponent(triangleMesh, triangleColor));
            // _entityManager.AddComponent(triangleEntity, triangleTransform);
            // _entityManager.AddComponent(triangleEntity, new PrimitiveMeshActorComponent("Triangle", triangleTransform));

            // // --- Квадрат ---
            // var quadMesh = MeshFactory.CreateCylinder(_gl);
            // var quadColor = RandomColor();
            // var quadTransform = ComponentFactory.CreateTransform(new Vector3D<float>(1, 1, 1), new Vector3D<float>(1, 1, 1), new Vector3D<float>(1.1f, 1.1f, 1.1f));
            // var quadEntity = _entityManager.CreateEntity();
            // _entityManager.AddComponent(quadEntity, new MeshRendererComponent(quadMesh, quadColor));
            // _entityManager.AddComponent(quadEntity, quadTransform);
            // _entityManager.AddComponent(quadEntity, new PrimitiveMeshActorComponent("Quad", quadTransform));

            // // --- Куб ---
            // var cubeMesh = MeshFactory.CreateCube(_gl);
            // var cubeColor = RandomColor();
            // var cubeTransform = ComponentFactory.CreateTransform(new Vector3D<float>(-1, -1, -1), new Vector3D<float>(2, 2, 2), new Vector3D<float>(1.2f, 1.2f, 1.2f));
            // var cubeEntity = _entityManager.CreateEntity();
            // _entityManager.AddComponent(cubeEntity, new MeshRendererComponent(cubeMesh, cubeColor));
            // _entityManager.AddComponent(cubeEntity, cubeTransform);
            // _entityManager.AddComponent(cubeEntity, new PrimitiveMeshActorComponent("Cube", cubeTransform));

            // // --- Камера ---
            // var cameraEntity = _entityManager.CreateEntity();
            // _entityManager.AddComponent(cameraEntity, new TransformComponent(
            //     new Vector3D<float>(0, 2, 5),
            //     Quaternion<float>.Identity,
            //     new Vector3D<float>(1, 1, 1)
            // ));
            // _entityManager.AddComponent(cameraEntity, new CameraComponent(
            //     60f, 800f / 600f, 0.1f, 100f
            // ));

            // --- UI: демо-кнопка ---
            // var logger = new Engine.Core.Logging.LoggerAdapter();
            // Engine.Core.UI.Animation.UIAnimationManager.Initialize(logger);
            // var button = Engine.Core.UI.Animation.UIAnimationExamples.CreateAnimatedButton(
            //     logger,
            //     new System.Numerics.Vector2(400, 300),
            //     new System.Numerics.Vector2(150, 50),
            //     "Нажми меня");
            // var uiEntity = _entityManager.CreateEntity();
            // var uiComponent = new Engine.Core.UI.UIComponent(button, button.Position, button.Size, Engine.Core.UI.CoordinateUnit.Pixels, Engine.Core.UI.CoordinateUnit.Pixels);
            // _entityManager.AddComponent(uiEntity, uiComponent);
        }

        private static Vector4 RandomColor()
        {
            var rand = new Random(Guid.NewGuid().GetHashCode());
            return new Vector4(
                (float)rand.NextDouble(),
                (float)rand.NextDouble(),
                (float)rand.NextDouble(),
                1.0f);
        }
    }
} 