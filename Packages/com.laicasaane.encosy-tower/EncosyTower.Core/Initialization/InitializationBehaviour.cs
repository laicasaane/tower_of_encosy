namespace EncosyTower.Initialization
{
    public enum InitializationBehaviour
    {
        /// <summary>
        /// Does not perform any initialization.
        /// </summary>
        None,

        /// <summary>
        /// If the system is already initialized, it will not be re-initialized.
        /// </summary>
        Respect,

        /// <summary>
        /// If the system is already initialized, it will be re-initialized.
        /// </summary>
        Forced,
    }
}
