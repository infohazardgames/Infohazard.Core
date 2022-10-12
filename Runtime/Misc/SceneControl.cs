// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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
