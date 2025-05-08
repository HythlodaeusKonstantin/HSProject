using Engine.Core.ECS;
using Engine.Core.ECS.Components;
using Engine.Core.Rendering;
using Engine.Core.Graphics;
using Engine.Core.UI.Elements;
using Engine.Core.UI.Components;
using Engine.Core.UI;
using Silk.NET.Maths;
using System.Numerics;
using Silk.NET.Core.Loader;

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
            var triangleMesh = MeshFactory.CreatePyramid(_gl);
            var triangleColor = RandomColor();
            var triangleTransform = TransformComponent.Default;
            var triangleEntity = _entityManager.CreateEntity();
            _entityManager.AddComponent(triangleEntity, new MeshRendererComponent(triangleMesh, triangleColor));
            _entityManager.AddComponent(triangleEntity, triangleTransform);
            _entityManager.AddComponent(triangleEntity, new PrimitiveMeshActorComponent("Triangle", triangleTransform));

            // // --- Квадрат ---
            var quadMesh = MeshFactory.CreateCylinder(_gl);
            var quadColor = RandomColor();
            var quadTransform = TransformComponent.CreateTransform(new Vector3D<float>(1, 1, 1), new Vector3D<float>(1, 1, 1), new Vector3D<float>(1.1f, 1.1f, 1.1f));
            var quadEntity = _entityManager.CreateEntity();
            _entityManager.AddComponent(quadEntity, new MeshRendererComponent(quadMesh, quadColor));
            _entityManager.AddComponent(quadEntity, quadTransform);
            _entityManager.AddComponent(quadEntity, new PrimitiveMeshActorComponent("Quad", quadTransform));

            // // --- Куб ---
            var cubeMesh = MeshFactory.CreateCube(_gl);
            var cubeColor = RandomColor();
            var cubeTransform = TransformComponent.CreateTransform(new Vector3D<float>(-1, -1, -1), new Vector3D<float>(2, 2, 2), new Vector3D<float>(1.2f, 1.2f, 1.2f));
            var cubeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponent(cubeEntity, new MeshRendererComponent(cubeMesh, cubeColor));
            _entityManager.AddComponent(cubeEntity, cubeTransform);
            _entityManager.AddComponent(cubeEntity, new PrimitiveMeshActorComponent("Cube", cubeTransform));

            // // --- Камера ---
            var cameraEntity = _entityManager.CreateEntity();
            _entityManager.AddComponent(cameraEntity, new TransformComponent(
                new Vector3D<float>(0, 2, 5),
                Quaternion<float>.Identity,
                new Vector3D<float>(1, 1, 1)
            ));
            _entityManager.AddComponent(cameraEntity, new CameraComponent(
                60f, 800f / 600f, 0.1f, 100f
            ));

            // --- UI: демо-кнопка ---
            var text = "Я кнопка!";
            var style = new UIStyle();
            style.States[UIState.Normal].BackgroundColor = System.Drawing.Color.DarkBlue;
            style.States[UIState.Normal].TextColor = System.Drawing.Color.White;
            style.States[UIState.Hover].BackgroundColor = System.Drawing.Color.Blue;
            style.States[UIState.Hover].TextColor = System.Drawing.Color.White;
            style.States[UIState.Hover].Scale = 1.05f;
            style.States[UIState.Hover].TextScale = 1.05f;
            style.States[UIState.Pressed].BackgroundColor = System.Drawing.Color.DarkBlue;
            style.States[UIState.Pressed].TextColor = System.Drawing.Color.LightGray;
            style.States[UIState.Pressed].Scale = 0.95f;
            style.States[UIState.Pressed].TextScale = 0.95f;
            style.States[UIState.Pressed].Opacity = 0.8f;
            style.States[UIState.Disabled].BackgroundColor = System.Drawing.Color.Gray;
            style.States[UIState.Disabled].TextColor = System.Drawing.Color.DarkGray;
            style.States[UIState.Disabled].Opacity = 0.7f;

            var uiEntity = _entityManager.CreateEntity();
            var uiComponent = UIComponent.CreateButton(
                text: text,
                position: new System.Numerics.Vector2(0.7f, 0.7f),
                size: new System.Numerics.Vector2(0.2f, 0.1f),
                positionUnit: CoordinateUnit.ViewportUnits,
                sizeUnit: CoordinateUnit.ViewportUnits,
                anchor: UIAnchor.Center,
                style: style
            );
            _entityManager.AddComponent(uiEntity, uiComponent);
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