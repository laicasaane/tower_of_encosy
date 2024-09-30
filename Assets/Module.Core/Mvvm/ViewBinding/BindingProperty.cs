using System;
using UnityEngine;

namespace Module.Core.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class BindingProperty
    {
        /// <summary>
        /// The property whose container class is an <see cref="Module.Core.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        /// <remarks>
        /// This property must be applicable for
        /// either <see cref="Module.Core.Mvvm.ComponentModel.INotifyPropertyChanging"/>
        /// or <see cref="Module.Core.Mvvm.ComponentModel.INotifyPropertyChanged"/>
        /// or both.
        /// </remarks>
        [field: SerializeField]
        public string TargetPropertyName { get; set; }
    }
}
