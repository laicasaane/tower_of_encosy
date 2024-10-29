using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.Modules
{
    public readonly record struct EventData(
          EventType Type
        , KeyCode Key
        , EventModifiers Mods
        , int Button
        , Vector2 MousePos
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator EventData(Event ev)
            => new(ev.type, ev.keyCode, ev.modifiers, ev.button, ev.mousePosition);
    }
}
