namespace EncosyTower.Modules.Mvvm.Input
{
    public interface ICommandListener
    {
        bool TryGetCommand<TCommand>(string commandName, out TCommand command) where TCommand : ICommand;
    }
}
