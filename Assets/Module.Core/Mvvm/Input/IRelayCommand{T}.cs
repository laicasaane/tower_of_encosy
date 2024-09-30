namespace Module.Core.Mvvm.Input
{
    /// <summary>
    /// A generic interface representing a more specific version of <see cref="IRelayCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type used as argument for the interface methods.</typeparam>
    public interface IRelayCommand<in T> : ICommand
    {
        /// <summary>
        /// Notifies that the <see cref="ICommand.CanExecute"/> property has changed.
        /// </summary>
        void NotifyCanExecuteChanged();

        /// <summary>
        /// Provides a strongly-typed variant of <see cref="ICommand.CanExecute(in Unions.Union)"/>.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        /// <returns>Whether or not the current command can be executed.</returns>
        bool CanExecute(T parameter);

        /// <summary>
        /// Provides a strongly-typed variant of <see cref="ICommand.Execute(in Unions.Union)"/>.
        /// </summary>
        /// <param name="parameter">The input parameter.</param>
        void Execute(T parameter);
    }
}
