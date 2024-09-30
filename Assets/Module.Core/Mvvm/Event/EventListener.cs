// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Module.Core.Mvvm.Event
{
    /// <summary>
    /// Implements a weak event listener that allows the owner to be garbage
    /// collected if its only remaining link is an event handler.
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class EventListener<TInstance> : IEventListener
        where TInstance : class
    {
        /// <summary>
        /// WeakReference to the instance listening for the event.
        /// </summary>
        private readonly WeakReference<TInstance> _weakInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventListener{TInstance}"/> class.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public EventListener(TInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _weakInstance = new WeakReference<TInstance>(instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTarget(out TInstance target)
            => _weakInstance.TryGetTarget(out target);

        /// <summary>
        /// Gets or sets the method to call when the event fires.
        /// </summary>
        public Action<TInstance, MvvmEventArgs> OnEventAction { get; set; }

        /// <summary>
        /// Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<EventListener<TInstance>> OnDetachAction { get; set; }

        /// <summary>
        /// Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="eventArgs">Event arguments.</param>
        public void OnEvent(in MvvmEventArgs eventArgs)
        {
            if (_weakInstance.TryGetTarget(out var target))
            {
                // Call registered action
                OnEventAction?.Invoke(target, eventArgs);
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            OnDetachAction?.Invoke(this);
            OnDetachAction = null;
        }
    }
}
