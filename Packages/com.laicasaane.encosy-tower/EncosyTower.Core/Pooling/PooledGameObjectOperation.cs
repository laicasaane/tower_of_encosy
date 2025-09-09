namespace EncosyTower.Pooling
{
    /// <summary>
    /// Represents an operation after GameObjects are returned to the pool.
    /// </summary>
    internal readonly record struct PooledGameObjectOperation
    {
        public PooledGameObjectOperation(
              PooledGameObjectStrategy defaultStrategy
            , PooledGameObjectStrategy overriddenStrategy
        )
        {
            DefaultStrategy = defaultStrategy;
            OverriddenStrategy = overriddenStrategy;
        }

        /// <summary>
        /// The default strategy defined by the pool.
        /// </summary>
        public PooledGameObjectStrategy DefaultStrategy { get; init; }

        /// <summary>
        /// The strategy that overrides the default one for a specific return operation.
        /// </summary>
        public PooledGameObjectStrategy OverriddenStrategy { get; init; }

        /// <summary>
        /// Determines if the pooled GameObjects should be deactivate.
        /// </summary>
        /// <returns>
        /// True if both <see cref="OverriddenStrategy"/> and <see cref="DefaultStrategy"/> are either
        /// <see cref="PooledGameObjectStrategy.Deactivate"/> or <see cref="PooledGameObjectStrategy.Default"/>.
        /// <br/>
        /// Otherwise false.
        /// </returns>
        public bool ShouldDeactivate()
        {
            if (OverriddenStrategy == PooledGameObjectStrategy.DoNothing)
            {
                return false;
            }

            if (OverriddenStrategy == PooledGameObjectStrategy.Deactivate)
            {
                return true;
            }

            if (DefaultStrategy == PooledGameObjectStrategy.DoNothing)
            {
                return false;
            }

            // At this point the default strategy must always be Deactivate.
            return true;
        }
    }
}
