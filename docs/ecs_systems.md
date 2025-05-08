# Системы ECS

Системы реализуют логику обработки сущностей с определёнными компонентами. Все системы реализуют интерфейс `ISystem`.

## Основные системы

### RenderSystem
- Отвечает за рендеринг всех объектов сцены
- Управляет OpenGL состоянием, FBO, шейдерами
- Взаимодействует с компонентами MeshRendererComponent, CameraComponent, TransformComponent

### SceneSystem
- Инициализация и глобальная логика сцены (создание начальных сущностей)

### ActorSystem
- Вызывает методы `OnInitialize` и `OnUpdate` у всех ActorComponent

### CameraControllerSystem
- Управление камерой: перемещение (WASD/стрелки), вращение мышью
- Обновляет TransformComponent и CameraComponent активной камеры

### TransformSystem
- Универсальное управление трансформациями (позиция, масштаб, ротация)

## Подробнее о компонентах: [Компоненты ECS](ecs_components.md)

[Назад к ECS](ecs.md) 