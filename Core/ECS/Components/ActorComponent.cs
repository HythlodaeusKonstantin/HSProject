using System;

namespace Engine.Core.ECS.Components
{
    /// <summary>
    /// Базовый компонент для игровых акторов с индивидуальной логикой
    /// </summary>
    public class ActorComponent : IComponent
    {
        /// <summary>
        /// Вызывается вручную или системой при инициализации
        /// </summary>
        public virtual void OnInitialize() { }

        /// <summary>
        /// Вызывается каждый кадр системой ActorSystem
        /// </summary>
        public virtual void OnUpdate(double deltaTime) { }
    }
} 