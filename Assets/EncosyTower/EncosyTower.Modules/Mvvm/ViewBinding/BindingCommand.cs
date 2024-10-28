using System;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class BindingCommand
    {
        /// <summary>
        /// The <see cref="EncosyTower.Modules.Mvvm.Input.ICommand"/> whose container class
        /// is an <see cref="EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        [field: SerializeField]
        public string TargetCommandName { get; set; }
    }
}
