using System;

namespace Engine.Core.ECS
{
    /// <summary>
    /// Represents a unique entity in the ECS architecture.
    /// </summary>
    public readonly struct Entity : IEquatable<Entity>
    {
        private static int _nextId = 1;
        private readonly int _id;

        /// <summary>
        /// Gets the unique identifier of this entity.
        /// </summary>
        public int Id => _id;

        private Entity(int id)
        {
            _id = id;
        }

        /// <summary>
        /// Creates a new entity with a unique identifier.
        /// </summary>
        /// <returns>A new entity instance.</returns>
        public static Entity Create()
        {
            return new Entity(_nextId++);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Entity entity && Equals(entity);
        }

        /// <summary>
        /// Determines whether the specified entity is equal to the current entity.
        /// </summary>
        public bool Equals(Entity other)
        {
            return _id == other._id;
        }

        /// <summary>
        /// Returns the hash code for this entity.
        /// </summary>
        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary>
        /// Determines whether two entities are equal.
        /// </summary>
        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two entities are not equal.
        /// </summary>
        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
} 