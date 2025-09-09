namespace EncosyTower.Mvvm.Input
{
    public interface ICommandListener
    {
        public bool TryGetCommand<TCommand>(string commandName, out TCommand command)
            where TCommand : ICommand
        {
            command = default;
            return false;
        }
    }
}
