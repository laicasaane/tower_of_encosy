using UnityEngine.Scripting;

namespace EncosyTower.VisualDebugging.Commands
{
    [RequireImplementors]
    public interface IVisualCommand
    {
        void Execute();
    }
}
