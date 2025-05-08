# Система событий UI

Система событий UI реализует обработку и распространение событий для всех UI-элементов.

## Основные классы и интерфейсы
- **UIEvent** — базовый класс для всех событий UI
- **UIMouseEvent, UIMouseEventArgs** — события мыши (движение, клик, скролл, двойной клик и др.)
- **IUIEventSystem** — интерфейс системы событий (регистрация, обработка, распространение)
- **IUIEventHandler** — интерфейс обработчика событий

## Поддерживаемые типы событий
- Мышь: MouseMove, MouseDown, MouseUp, MouseEnter, MouseLeave, Click, DoubleClick, MouseWheel, Drag
- Клавиатура: KeyDown, KeyUp, TextInput
- Фокус: FocusGained, FocusLost
- Жесты: Tap, DoubleTap, LongPress, Swipe, Pinch, Rotate
- UI: ValueChanged, StateChanged, VisibilityChanged, Resize

## Обработка событий
- События распространяются по иерархии элементов (bubbling/capturing)
- Каждый элемент может зарегистрировать обработчик для нужных типов событий

## Подробнее о компонентах: [UI-компоненты](ui_components.md)

[Назад к UI](ui.md) 