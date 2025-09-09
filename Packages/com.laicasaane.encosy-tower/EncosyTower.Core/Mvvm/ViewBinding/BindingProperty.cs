using System;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    public sealed class BindingProperty
    {
        /// <summary>
        /// The property whose container class is an <see cref="EncosyTower.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        /// <remarks>
        /// This property must be applicable for
        /// either <see cref="EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanging"/>
        /// or <see cref="EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanged"/>
        /// or both.
        /// </remarks>
        [field: SerializeField]
        public string TargetPropertyName { get; set; }
    }
}
