namespace EncosyTower.Pooling
{
    /// <summary>
    /// Represents an operation after GameObjects are returned to the pool.
    /// </summary>
    internal readonly record struct ReturnOperation
    {
        public ReturnOperation(
              ReturningStrategy defaultStrategy
            , ReturningStrategy overriddenStrategy
        )
        {
            DefaultStrategy = defaultStrategy;
            OverriddenStrategy = overriddenStrategy;
        }

        /// <summary>
        /// The default strategy defined by the pool.
        /// </summary>
        public ReturningStrategy DefaultStrategy { get; init; }

        /// <summary>
        /// The strategy that overrides the default one for a specific return operation.
        /// </summary>
        public ReturningStrategy OverriddenStrategy { get; init; }

        /// <summary>
        /// Determines if the returned GameObjects should be deactivate.
        /// </summary>
        /// <returns>
        /// True if both <see cref="OverriddenStrategy"/> and <see cref="DefaultStrategy"/> are either
        /// <see cref="ReturningStrategy.Deactivate"/> or <see cref="ReturningStrategy.Default"/>.
        /// <br/>
        /// Otherwise false.
        /// </returns>
        public bool ShouldDeactivate()
        {
            if (OverriddenStrategy == ReturningStrategy.DoNothing)
            {
                return false;
            }

            if (OverriddenStrategy == ReturningStrategy.Deactivate)
            {
                return true;
            }

            if (DefaultStrategy == ReturningStrategy.DoNothing)
            {
                return false;
            }

            // At this point the default strategy must always be Deactivate.
            return true;
        }
    }
}
