// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core.Editor {
    public static class OpenSceneUtility {
        public static bool UsePaths {
            get => EditorPrefs.GetBool("Infohazard.OpenSceneUtility.UsePaths", false);
            set => EditorPrefs.SetBool("Infohazard.OpenSceneUtility.UsePaths", value);
        }

        public static int PathLength {
            get => EditorPrefs.GetInt("Infohazard.OpenSceneUtility.PathLength", 2);
            set => EditorPrefs.SetInt("Infohazard.OpenSceneUtility.PathLength", value);
        }

        [MenuItem("Tools/Infohazard/Open Scene Context %#o")]
        public static void OpenSceneContextMenu() {
            OpenSceneContextMenu(false);
        }

        [MenuItem("Tools/Infohazard/Open Scene Context (Additive) %&o")]
        public static void OpenSceneContextMenuAdditive() {
            OpenSceneContextMenu(true);
        }

        private static void OpenSceneContextMenu(bool additive) {
            string[] guids = AssetDatabase.FindAssets("t:Scene");

            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Use paths"), UsePaths, () => UsePaths = !UsePaths);
            menu.AddSeparator("");

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);

                string partialPath = path;
                int index = -1;
                for (int i = 0; i < PathLength; i++) {
                    int temp = partialPath.IndexOf('/', index + 1);
                    if (temp >= 0) {
                        index = temp;
                    } else {
                        break;
                    }
                }

                if (index > 0) {
                    partialPath = partialPath[..(index + 1)] + partialPath[(index + 1)..].Replace("/", " \u2215 ");
                }

                Scene scene = SceneManager.GetSceneByPath(path);
                bool isOpen = scene.isLoaded;
                GUIContent guiContent = new(UsePaths ? partialPath : name);

                if (!isOpen) {
                    OpenSceneMode mode = additive ? OpenSceneMode.Additive : OpenSceneMode.Single;
                    menu.AddItem(guiContent, false, () => EditorSceneManager.OpenScene(path, mode));
                } else if (additive && SceneManager.sceneCount > 1) {
                    menu.AddItem(guiContent, true, () => EditorSceneManager.CloseScene(scene, true));
                } else {
                    menu.AddDisabledItem(guiContent, true);
                }
            }

            menu.ShowAsContext();
        }
    }
}
