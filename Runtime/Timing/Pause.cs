// MIT License
// 
// Copyright (c) 2020 Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Manages pausing and unpausing of the game.
    /// </summary>
    /// <remarks>
    /// Any actions that should only happen when the game is not paused should check Pause.paused.
    /// Controllers and motors will not have DoInput, DoOutput, and PostOutput called if paused.
    /// Can be used statically if pause is controlled elsewhere,
    /// or placed as a component to automatically pause when a button is pressed.
    /// For creating a pause menu, see PauseMenu.cs.
    /// </remarks>
    public static class Pause {
        private static bool _paused;
        private static float _timeScale;

        public static event Action GamePaused;
        public static event Action GameResumed;

        /// <summary>
        /// Controls paused state of the game. 
        /// This cannot completely prevent game actions from happening, but it does the following:
        /// - Sets Time.timeScale to 0 so that Physics and animation will stop.
        /// - Disables all Motors and Controllers from updating.
        /// </summary>
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