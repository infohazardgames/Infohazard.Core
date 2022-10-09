// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    /// <summary>
    /// Provides some methods to navigate to scenes.
    /// </summary>
    /// <remarks>
    /// Also provides a static method to quit the game that works in a standalone build as well as in the editor.
    /// This script is useful if you’re building a super quick main menu
    /// (such as in the last half hour of a game jam) and need to hook up your buttons as fast as possible.
    /// </remarks>
    public class SceneControl : MonoBehaviour {
        /// <summary>
        /// If in the editor, exit play mode. Otherwise, close the application.
        /// </summary>
        public static void Quit() {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }

        /// <summary>
        /// Non-static equivalent of <see cref="Quit"/>.
        /// </summary>
        public void QuitButton() {
            Quit();
        }

        /// <summary>
        /// Reload the current scene.
        /// </summary>
        /// <remarks>
        /// Current scene is determined by <c>SceneManager.GetActiveScene()</c>, and is loaded as a single scene.
        /// This method is not very helpful if your game has multiple scenes open at a time.
        /// </remarks>
        public void ReloadScene() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Load a scene with a given name.
        /// </summary>
        /// <remarks>
        /// Scene will be loaded as a single (not additively).
        /// </remarks>
        /// <param name="sceneName">The scene name to load.</param>
        public void LoadScene(string sceneName) {
            SceneManager.LoadScene(sceneName);
        }
    }
}
