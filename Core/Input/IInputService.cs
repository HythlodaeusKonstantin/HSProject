using Silk.NET.Input;

namespace Engine.Core.Input
{
    /// <summary>
    /// Интерфейс сервиса ввода (клавиатура и мышь)
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Основное устройство мыши
        /// </summary>
        IMouse? PrimaryMouse { get; }

        /// <summary>
        /// Проверить, нажата ли клавиша
        /// </summary>
        /// <param name="key">Код клавиши</param>
        /// <returns>true, если клавиша нажата</returns>
        bool IsKeyDown(Key key);

        /// <summary>
        /// Получить смещение мыши с прошлого кадра
        /// </summary>
        /// <returns>Двумерный вектор смещения</returns>
        (float dx, float dy) GetMouseDelta();

        /// <summary>
        /// Обновить состояние ввода (вызывать каждый кадр)
        /// </summary>
        void Update();
    }
} 