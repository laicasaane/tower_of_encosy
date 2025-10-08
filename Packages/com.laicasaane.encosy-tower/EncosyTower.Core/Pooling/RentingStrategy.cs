namespace EncosyTower.Pooling
{
    /// <summary>
    /// Defines the possible strategies to deal with a GameObject when it is rented from the pool.
    /// </summary>
    /// <remarks>
    /// A pool itself should specify the default strategy to deal with GameObjects when they are rented.
    /// <br/>
    /// However, each rent operation should allow the caller to override this default behavior.
    /// </remarks>
    public enum RentingStrategy : byte
    {
        /// <summary>
        /// This strategy will try to activate the rented GameObjects if possible.
        /// </summary>
        /// <remarks>
        /// Refers the implementation of <see cref="RentOperation.ShouldActivate"/>.
        /// </remarks>
        Default   = 0,

        /// <summary>
        /// This strategy will always activate the rented GameObjects.
        /// </summary>
        Activate  = 1,

        /// <summary>
        /// This strategy will do nothing to the rented GameObjects.
        /// </summary>
        DoNothing = 2,
    }
}
