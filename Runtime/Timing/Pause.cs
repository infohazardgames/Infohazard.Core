// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Manages pausing and unpausing of the game.
    /// </summary>
    /// <remarks>
    /// Any actions that should only happen when the game is not paused should check Pause.paused.
    /// Can be used statically if pause is controlled elsewhere,
    /// or placed as a component to pause the game from a UnityEvent.
    /// The game will automatically unpause when a new scene is loaded.
    /// </remarks>
    public static class Pause {
        private static bool _paused;
        private static float _timeScale = 1;

        /// <summary>
        /// Get or set the non-paused timescale. Only affects current Time.timeScale if not paused.
        /// </summary>
        public static float TimeScale {
            get => _timeScale;
            set {
                _timeScale = value;
                if (!_paused) {
                    Time.timeScale = value;
                }
            }
        }

        /// <summary>
        /// Invoked when the game pauses.
        /// </summary>
        public static event Action GamePaused;

        /// <summary>
        /// Invoked when the game un-pauses.
        /// </summary>
        public static event Action GameResumed;

        /// <summary>
        /// Reset all static state of <see cref="Pause"/>, clearing event subscribers. Does not modify <see cref="Time.timeScale"/> to its initial values.
        /// </summary>
        /// <remarks>
        /// This exists for unit tests and editor tooling (such as entering play mode without a domain reload).
        /// It typically should not be called from game code.
        /// </remarks>
        public static void ResetStaticState() {
            _paused = false;
            _timeScale = 1;
            GamePaused = null;
            GameResumed = null;
        }

        /// <summary>
        /// Controls paused state of the game.
        /// </summary>
        /// <remarks>
        /// This cannot completely prevent game actions from happening,
        /// but it does sets Time.timeScale to 0 so that Physics and animation will stop.
        /// </remarks>
        public static bool Paused {
            get => _paused;
            set {
                if (_paused == value) return;
                _paused = value;
                if (value) {
                    _timeScale = Time.timeScale;
                    Time.timeScale = 0;
                    GamePaused?.Invoke();
                } else {
                    Time.timeScale = _timeScale;
                    GameResumed?.Invoke();
                }
            }
        }
    }
}
