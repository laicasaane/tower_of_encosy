using System;
using UnityEngine;

namespace Module.Core.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class BindingCommand
    {
        /// <summary>
        /// The <see cref="Module.Core.Mvvm.Input.ICommand"/> whose container class
        /// is an <see cref="Module.Core.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        [field: SerializeField]
        public string TargetCommandName { get; set; }
    }
}
