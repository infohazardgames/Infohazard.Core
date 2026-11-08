// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;

namespace Infohazard.Core.StateMachine {
    public class SimpleStateMachine<T> {
        private readonly Dictionary<T, StateMachineState> _states = new();
        private readonly Dictionary<T, List<Transition>> _transitions = new();
        private T _currentStateKey;
        private bool _hasCurrentState;

        /// <summary>
        /// Get the current state object.
        /// </summary>
        public StateMachineState CurrentStateObject { get; private set; }

        /// <summary>
        /// Get the key of the current state.
        /// Set the current state by a key and invoke appropriate enter/exit actions.
        /// Note that when setting this directly,
        /// a transition action will NOT be invoked even if a transition exists to the new state.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the value is not a valid state key.</exception>
        public T CurrentStateKey {
            get => _currentStateKey;
            set {
                if (value is null) throw new ArgumentNullException(nameof(value));
                if (_hasCurrentState && EqualityComparer<T>.Default.Equals(_currentStateKey, value)) return;

                if (!_states.TryGetValue(value, out StateMachineState state)) {
                    throw new ArgumentException("State key is not a valid state.", nameof(value));
                }

                CurrentStateObject?.OnExit?.Invoke();
                _currentStateKey = value;
                _hasCurrentState = true;
                CurrentStateObject = state;
                CurrentStateObject?.OnEnter?.Invoke();
            }
        }

        /// <summary>
        /// Add a new state to the state machine.
        /// </summary>
        /// <param name="key">Key to identify the state. Must be unique.</param>
        /// <param name="state">The new state.</param>
        /// <exception cref="ArgumentNullException">Thrown if either argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the key is already in use.</exception>
        public void AddState(T key, StateMachineState state) {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (state is null) throw new ArgumentNullException(nameof(state));
            _states.Add(key, state);
        }

        /// <summary>
        /// Add a new transition to the state machine.
        /// The target state of the transition does not need to be unique in the 'from' state,
        /// but both must be valid state keys.
        /// </summary>
        /// <param name="from">The key of the state to transition from.</param>
        /// <param name="to">The key of the state to transition to.</param>
        /// <param name="transition">The new transition.</param>
        /// <param name="isBidirectional">Whether the transition is bidirectional.
        /// If true, an inverse transition will be added with the same callbacks.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if either the 'from' or 'to' state is not a valid state key.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments are null.</exception>
        public void AddTransition(T from, T to, StateMachineTransition transition, bool isBidirectional = false) {
            if (from is null) throw new ArgumentNullException(nameof(from));
            if (to is null) throw new ArgumentNullException(nameof(to));
            if (transition is null) throw new ArgumentNullException(nameof(transition));

            if (!_states.ContainsKey(from) ||
                !_states.ContainsKey(to)) {
                throw new ArgumentException("Transition target state must be a valid state key.");
            }

            if (!_transitions.TryGetValue(from, out List<Transition> transitions)) {
                transitions = new List<Transition>();
                _transitions.Add(from, transitions);
            }

            transitions.Add(new Transition {
                Callbacks = transition,
                TargetState = to,
            });

            if (isBidirectional) {
                AddTransition(to, from, transition, false);
            }
        }

        /// <summary>
        /// Invoke the update event of the current state.
        /// If a transition condition is met, the transition will be invoked.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the state machine has not been initialized with a current state.
        /// </exception>
        public void Update() {
            if (!_hasCurrentState) {
                throw new InvalidOperationException("Trying to update state machine without setting an initial state.");
            }

            T currentStateKey = _currentStateKey;
            CurrentStateObject?.OnUpdate?.Invoke();
            T newStateKey = _currentStateKey;

            // If the state has changed during the update, do not check for transitions.
            if (!EqualityComparer<T>.Default.Equals(currentStateKey, newStateKey)) {
                return;
            }

            if (_transitions.TryGetValue(_currentStateKey, out List<Transition> transitions)) {
                foreach (Transition transition in transitions) {
                    if (!transition.Callbacks.Condition()) continue;

                    transition.Callbacks.OnTransition?.Invoke();
                    CurrentStateKey = transition.TargetState;
                    break;
                }
            }
        }

        private class Transition {
            public StateMachineTransition Callbacks { get; set; }
            public T TargetState { get; set; }
        }
    }

    /// <summary>
    /// A state in a <see cref="SimpleStateMachine{T}"/>.
    /// </summary>
    public class StateMachineState {
        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        public Action OnEnter { get; set; }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public Action OnExit { get; set; }

        /// <summary>
        /// Invoked when the state is updated.
        /// </summary>
        public Action OnUpdate { get; set; }
    }

    /// <summary>
    /// A transition in a <see cref="SimpleStateMachine{T}"/>.
    /// </summary>
    public class StateMachineTransition {
        /// <summary>
        /// Invoked to determine whether the transition should occur.
        /// </summary>
        public Func<bool> Condition { get; set; }

        /// <summary>
        /// Invoked when the transition occurs.
        /// </summary>
        public Action OnTransition { get; set; }
    }
}
