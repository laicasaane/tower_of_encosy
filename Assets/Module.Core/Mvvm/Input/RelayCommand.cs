using System;
using System.Runtime.CompilerServices;
using Module.Core.Mvvm.Event;
using Module.Core.Unions;

namespace Module.Core.Mvvm.Input
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates. The default return value for the <see cref="CanExecute"/>
    /// method is <see langword="true"/>. This type does not allow you to accept command parameters
    /// in the <see cref="Execute"/> and <see cref="CanExecute"/> callback methods.
    /// </summary>
    public sealed class RelayCommand : IRelayCommand
    {
        /// <summary>
        /// The <see cref="Action"/> to invoke when <see cref="Execute"/> is used.
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// The optional action to invoke when <see cref="CanExecute"/> is used.
        /// </summary>
        private readonly Func<bool> _canExecute;

        private event MvvmEventHandler _canExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> is <see langword="null"/>.</exception>
        public RelayCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="execute"/> or <paramref name="canExecute"/> are <see langword="null"/>.</exception>
        public RelayCommand(Action execute, Func<bool> canExecute)
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
        public bool CanExecute()
            => _canExecute?.Invoke() != false;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
            => _execute();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(in Union parameter)
            => _canExecute?.Invoke() != false;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(in Union parameter)
            => _execute();
    }
}
