using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Module.Core.Mvvm.Event;
using Module.Core.Unions;
using Module.Core.Unions.Converters;
using UnityEngine;

namespace Module.Core.Mvvm.Input
{
    /// <summary>
    /// A generic command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the CanExecute
    /// method is <see langword="true"/>. This class allows you to accept command parameters
    /// in the <see cref="Execute(T)"/> and <see cref="CanExecute(T)"/> callback methods.
    /// </summary>
    /// <typeparam name="T">The type of parameter being passed as input to the callbacks.</typeparam>
    public sealed class RelayCommand<T> : IRelayCommand<T>
    {
        /// <summary>
        /// The <see cref="Action"/> to invoke when <see cref="Execute(T)"/> is used.
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// The optional action to invoke when <see cref="CanExecute(T)"/> is used.
        /// </summary>
        private readonly Predicate<T> _canExecute;

        private event MvvmEventHandler _canExecuteChanged;

        private readonly CachedUnionConverter<T> _converter = new CachedUnionConverter<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <remarks>
        /// Due to the fact that the <see cref="System.Windows.Input.ICommand"/> interface exposes methods that accept a
        /// nullable <see cref="object"/> parameter, it is recommended that if <typeparamref name="T"/> is a reference type,
        /// you should always declare it as nullable, and to always perform checks within <paramref name="execute"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public RelayCommand(Action<T> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <remarks>See notes in <see cref="RelayCommand{T}(Action{T})"/>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        /// <inheritdoc/>
        public void CanExecuteChanged<TInstance>(EventListener<TInstance> listener)
            where TInstance : class
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            _canExecuteChanged += listener.OnEvent;
            listener.OnDetachAction = (listener) => _canExecuteChanged -= listener.OnEvent;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NotifyCanExecuteChanged()
            => _canExecuteChanged?.Invoke(new MvvmEventArgs(this, Union.Undefined));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(T parameter)
            => _canExecute?.Invoke(parameter) != false;

        /// <inheritdoc/>
        public bool CanExecute(in Union parameter)
        {
            if (_converter.TryGetValue(parameter, out T result) == false)
            {
                ThrowArgumentException();
            }

            return CanExecute(result);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(T parameter) => _execute(parameter);

        /// <inheritdoc/>
        public void Execute(in Union parameter)
        {
            if (_converter.TryGetValue(parameter, out T result) == false)
            {
                ThrowArgumentException();
            }

            Execute(result);
        }

        [HideInCallstack, DoesNotReturn, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException()
            => throw new ArgumentException($"The command type requires an argument of type {typeof(T)}.");
    }
}
