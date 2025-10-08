namespace EncosyTower.Pooling
{
    /// <summary>
    /// Defines the possible strategies to deal with a GameObject when it is returned to the pool.
    /// </summary>
    /// <remarks>
    /// A pool itself should specify the default strategy to deal with GameObjects when they are returned.
    /// <br/>
    /// However, each return operation should allow the caller to override this default behavior.
    /// </remarks>
    public enum ReturningStrategy : byte
    {
        /// <summary>
        /// This strategy will try to deactivate the returned GameObjects if possible.
        /// </summary>
        /// <remarks>
        /// Refers the implementation of <see cref="ReturnOperation.ShouldDeactivate"/>.
        /// </remarks>
        Default    = 0,

        /// <summary>
        /// This strategy will always deactivate the returned GameObjects.
        /// </summary>
        Deactivate = 1,

        /// <summary>
        /// This strategy will do nothing to the returned GameObjects.
        /// </summary>
        DoNothing  = 2,
    }
}
