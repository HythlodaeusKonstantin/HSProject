using Engine.Core.ECS.Components;
using Engine.Core.Input;
using Engine.Core.Logging;
using Silk.NET.Input;
using Silk.NET.Maths;
using System;
using Engine.Core.Windowing;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Система управления камерой: перемещение (WSAD/стрелки) и вращение мышью (yaw/pitch)
    /// </summary>
    public class CameraControllerSystem : ISystem
    {
        private readonly EntityManager _entityManager;
        private readonly IInputService _inputService;
        private readonly ILogger? _logger;
        private float _moveSpeed;
        private float _mouseSensitivity;
        private float _yaw;
        private float _pitch;
        private Entity? _activeCameraEntity;
        private bool _isCameraControlActive = false;
        private readonly IWindowService _windowService;

        public CameraControllerSystem(EntityManager entityManager, IInputService inputService, ILogger? logger = null, float moveSpeed = 5.0f, float mouseSensitivity = 0.1f, IWindowService? windowService = null)
        {
            _entityManager = entityManager;
            _inputService = inputService;
            _logger = logger;
            _moveSpeed = moveSpeed;
            _mouseSensitivity = mouseSensitivity;
            _yaw = 0f;
            _pitch = 0f;
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        }

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set => _mouseSensitivity = value;
        }

        public void Update(double deltaTime)
        {
            try
            {
                // --- Переключение режима по правой кнопке мыши ---
                bool rightMouseDown = false;
                if (_inputService is InputService concreteInput)
                {
                    var mouse = concreteInput.GetMouse();
                    if (mouse != null)
                    {
                        rightMouseDown = mouse.IsButtonPressed(MouseButton.Right);
                        if (rightMouseDown != _isCameraControlActive)
                        {
                            _isCameraControlActive = rightMouseDown;
                            mouse.Cursor.CursorMode = _isCameraControlActive ? CursorMode.Disabled : CursorMode.Normal;
                        }
                    }
                }

                // 1. Найти активную камеру (первую с CameraComponent и TransformComponent)
                if (_activeCameraEntity == null ||
                    !_entityManager.HasComponent<CameraComponent>(_activeCameraEntity.Value) ||
                    !_entityManager.HasComponent<TransformComponent>(_activeCameraEntity.Value))
                {
                    var cameraEntities = _entityManager.QueryEntities(typeof(CameraComponent), typeof(TransformComponent));
                    foreach (var camEntity in cameraEntities)
                    {
                        _activeCameraEntity = camEntity;
                        break;
                    }
                    // При первом запуске инициализируем yaw/pitch из TransformComponent
                    if (_activeCameraEntity != null)
                    {
                        var t = _entityManager.GetComponent<TransformComponent>(_activeCameraEntity.Value);
                        // Извлекаем yaw/pitch из кватерниона (Euler angles)
                        // Yaw (вокруг Y), Pitch (вокруг X)
                        var q = t.Rotation;
                        // Формула для извлечения yaw/pitch из кватерниона
                        _yaw = MathF.Atan2(2f * (q.W * q.Y + q.X * q.Z), 1f - 2f * (q.Y * q.Y + q.X * q.X)) * 180f / MathF.PI;
                        _pitch = MathF.Asin(2f * (q.W * q.X - q.Z * q.Y)) * 180f / MathF.PI;
                    }
                }
                if (_activeCameraEntity == null)
                {
                    _logger?.Log(LogType.Warning, "CameraControllerSystem", "Нет активной камеры для управления");
                    return;
                }

                var entity = _activeCameraEntity.Value;
                var transform = _entityManager.GetComponent<TransformComponent>(entity);

                // 2. Перемещение (WASD/стрелки)
                Vector3D<float> move = Vector3D<float>.Zero;
                // W — вперёд, S — назад, A — влево, D — вправо
                if (_inputService.IsKeyDown(Key.W) || _inputService.IsKeyDown(Key.Up))
                    move += new Vector3D<float>(0, 0, -1); // вперёд
                if (_inputService.IsKeyDown(Key.S) || _inputService.IsKeyDown(Key.Down))
                    move += new Vector3D<float>(0, 0, 1);  // назад
                if (_inputService.IsKeyDown(Key.A) || _inputService.IsKeyDown(Key.Left))
                    move += new Vector3D<float>(1, 0, 0); // влево
                if (_inputService.IsKeyDown(Key.D) || _inputService.IsKeyDown(Key.Right))
                    move += new Vector3D<float>(-1, 0, 0);  // вправо
                // Q — вверх, E — вниз
                if (_inputService.IsKeyDown(Key.Q))
                    move += new Vector3D<float>(0, -1, 0); // вверх
                if (_inputService.IsKeyDown(Key.E))
                    move += new Vector3D<float>(0, 1, 0); // вниз

                // 3. Вращение мышью (yaw/pitch)
                float dx = 0, dy = 0;
                if (_isCameraControlActive)
                {
                    (dx, dy) = _inputService.GetMouseDelta();
                    float oldYaw = _yaw;
                    float oldPitch = _pitch;
                    _yaw -= dx * _mouseSensitivity;
                    _pitch -= dy * _mouseSensitivity;
                    _pitch = Math.Clamp(_pitch, -89f, 89f); // Ограничение pitch
                    _logger?.Log(LogType.Info, "CameraControllerSystem", $"MouseDelta: dx={dx}, dy={dy}, yaw: {oldYaw}->{_yaw}, pitch: {oldPitch}->{_pitch}");
                }

                // 4. Вычислить новое направление взгляда (OpenGL-style: вперёд — +Z)
                float yawRad = MathF.PI / 180f * _yaw;
                float pitchRad = MathF.PI / 180f * _pitch;
                var forward = new Vector3D<float>(
                    MathF.Cos(pitchRad) * MathF.Sin(yawRad),
                    MathF.Sin(pitchRad),
                    MathF.Cos(pitchRad) * MathF.Cos(yawRad)
                );
                forward = Vector3D.Normalize(forward);

                // --- Классическое FPS-движение: forwardXZ и right только по горизонтали ---
                var forwardXZ = new Vector3D<float>(forward.X, 0, forward.Z);
                if (forwardXZ.LengthSquared > 0)
                    forwardXZ = Vector3D.Normalize(forwardXZ);
                var right = Vector3D.Normalize(Vector3D.Cross(forwardXZ, new Vector3D<float>(0, 1, 0)));
                var up = new Vector3D<float>(0, 1, 0);

                // 5. Перемещение относительно направления камеры (WASD — по горизонтали, Q/E — вверх/вниз)
                Vector3D<float> moveWorld = move.Z * forwardXZ + move.X * right + move.Y * up;
                if (moveWorld.LengthSquared > 0)
                    moveWorld = Vector3D.Normalize(moveWorld);
                transform.Position += moveWorld * _moveSpeed * (float)deltaTime;

                // 6. Обновить кватернион поворота (pitch — X, потом yaw — Y)
                var pitchQuat = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(1, 0, 0), pitchRad);
                var yawQuat = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0, 1, 0), yawRad);
                var finalQuat = yawQuat * pitchQuat;
                transform.Rotation = finalQuat;

                _logger?.Log(LogType.Info, "CameraControllerSystem", $"Quaternion: {finalQuat}, Forward: {forward}");

                _entityManager.AddComponent(entity, transform);
            }
            catch (Exception ex)
            {
                _logger?.Log(LogType.Error, "CameraControllerSystem", $"Ошибка в Update: {ex}");
            }
        }

        public void Render() { }
    }
} 