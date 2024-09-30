namespace Module.Core.Mvvm.Input
{
    /// <summary>
    /// An interface expanding <see cref="ICommand"/> with the ability to raise
    /// the <see cref="ICommand.CanExecuteChanged"/> event externally.
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        /// <summary>
        /// Notifies that the <see cref="ICommand.CanExecute"/> property has changed.
        /// </summary>
        void NotifyCanExecuteChanged();

        /// <summary>
        /// Provides a parameterless variant of <see cref="ICommand.CanExecute(in Unions.Union)"/>.
        /// </summary>
        bool CanExecute();

        /// <summary>
        /// Provides a parameterless variant of <see cref="ICommand.Execute(in Unions.Union)"/>.
        /// </summary>
        void Execute();
    }
}
