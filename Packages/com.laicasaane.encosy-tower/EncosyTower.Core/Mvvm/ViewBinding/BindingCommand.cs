using System;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class BindingCommand
    {
        /// <summary>
        /// The <see cref="EncosyTower.Mvvm.Input.ICommand"/> whose container class
        /// is an <see cref="EncosyTower.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        [field: SerializeField]
        public string TargetCommandName { get; set; }
    }
}
