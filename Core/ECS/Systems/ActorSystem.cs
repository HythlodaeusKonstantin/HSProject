using Engine.Core.ECS.Components;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Система, вызывающая OnUpdate у всех ActorComponent
    /// </summary>
    public class ActorSystem : ISystem
    {
        private readonly EntityManager _entityManager;

        public ActorSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public void Initialize()
        {
            foreach (var entity in _entityManager.GetAllEntities())
            {
                foreach (var component in _entityManager.GetAllComponents(entity))
                {
                    if (component is ActorComponent actor)
                    {
                        actor.OnInitialize();
                    }
                }
            }
        }

        public void Update(double deltaTime)
        {
            foreach (var entity in _entityManager.GetAllEntities())
            {
                foreach (var component in _entityManager.GetAllComponents(entity))
                {
                    if (component is ActorComponent actor)
                    {
                        actor.OnUpdate(deltaTime);
                    }
                }
            }
        }

        public void Render() { }
    }
} 