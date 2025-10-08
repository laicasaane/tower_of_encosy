namespace EncosyTower.Pooling
{
    /// <summary>
    /// Represents an operation after GameObjects are rented from the pool.
    /// </summary>
    internal readonly record struct RentOperation
    {
        public RentOperation(
              RentingStrategy defaultStrategy
            , RentingStrategy overriddenStrategy
        )
        {
            DefaultStrategy = defaultStrategy;
            OverriddenStrategy = overriddenStrategy;
        }

        /// <summary>
        /// The default strategy defined by the pool.
        /// </summary>
        public RentingStrategy DefaultStrategy { get; init; }

        /// <summary>
        /// The strategy that overrides the default one for a specific rent operation.
        /// </summary>
        public RentingStrategy OverriddenStrategy { get; init; }

        /// <summary>
        /// Determines if the rented GameObjects should be activate.
        /// </summary>
        /// <returns>
        /// True if both <see cref="OverriddenStrategy"/> and <see cref="DefaultStrategy"/> are either
        /// <see cref="RentingStrategy.Activate"/> or <see cref="RentingStrategy.Default"/>.
        /// <br/>
        /// Otherwise false.
        /// </returns>
        public bool ShouldActivate()
        {
            if (OverriddenStrategy == RentingStrategy.DoNothing)
            {
                return false;
            }

            if (OverriddenStrategy == RentingStrategy.Activate)
            {
                return true;
            }

            if (DefaultStrategy == RentingStrategy.DoNothing)
            {
                return false;
            }

            // At this point the default strategy must always be Activate.
            return true;
        }
    }
}
