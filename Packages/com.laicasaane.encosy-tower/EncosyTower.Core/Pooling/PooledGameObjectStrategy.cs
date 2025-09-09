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
    public enum PooledGameObjectStrategy
    {
        /// <summary>
        /// This strategy will try to deactivate the pooled GameObjects if possible.
        /// </summary>
        /// <remarks>
        /// Refers the implementation of <see cref="PooledGameObjectOperation.ShouldDeactivate"/>.
        /// </remarks>
        Default,

        /// <summary>
        /// This strategy will always deactivate the pooled GameObjects.
        /// </summary>
        Deactivate,

        /// <summary>
        /// This strategy will do nothing to the pooled GameObjects.
        /// </summary>
        DoNothing,
    }
}
