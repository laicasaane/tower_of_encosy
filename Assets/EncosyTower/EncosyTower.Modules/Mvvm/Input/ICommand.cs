using EncosyTower.Modules.Mvvm.Event;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.Input
{
    public interface ICommand
    {
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        void CanExecuteChanged<TInstance>(EventListener<TInstance> listener) where TInstance : class;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        bool CanExecute(in Union parameter);

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        void Execute(in Union parameter);
    }
}
