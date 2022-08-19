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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    public static class Tag {
        public const string Untagged = "Untagged";
        public const string Respawn = "Respawn";
        public const string Finish = "Finish";
        public const string EditorOnly = "EditorOnly";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
        public const string GameController = "GameController";
        
        public static readonly string[] DefaultTags = {
            "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController",
        };

        public static string[] GameOverrideTags = null;
        public static string[] Tags => GameOverrideTags ?? Tag.DefaultTags;
    }
    
    [Serializable]
    public struct TagMask {
        public const long UntaggedMask = 1 << 0;
        public const long RespawnMask = 1 << 1;
        public const long FinishMask = 1 << 2;
        public const long EditorOnlyMask = 1 << 3;
        public const long MainCameraMask = 1 << 4;
        public const long PlayerMask = 1 << 5;
        public const long GameControllerMask = 1 << 6;

        [SerializeField] private long _value;
        public long Value => _value;

        public TagMask(long value) {
            _value = value;
        }
        
        public static implicit operator long(TagMask mask) {
            return mask._value;
        }

        public static implicit operator TagMask(long mask) {
            return new TagMask(mask);
        }

        public static int NameToTag(string name) => Array.IndexOf(Tag.Tags, name);
        public static string TagToName(int tag) => Tag.Tags[tag];

        public static long GetMask(params string[] names) {
            long mask = 0;
            for (var i = 0; i < names.Length; i++) {
                string name = names[i];
                int index = NameToTag(name);
                if (index < 0) {
                    Debug.LogError($"Tag name {name} not found.");
                    continue;
                }

                mask |= (uint)(1 << index);
            }

            return mask;
        }
        
        public static int GetMask(string name) {
            int index = NameToTag(name);
            if (index < 0) {
                Debug.LogError($"Tag name {name} not found.");
                return 0;
            }

            return 1 << index;
        }
    }

    /// <summary>
    /// Static operations on Tag enum values.
    /// </summary>
    public static class TagMaskUtility {
        /// <summary>
        /// Return true if GameObject's tag matches given any tag in given value.
        /// </summary>
        /// <param name="obj">The GameObject to check.</param>
        /// <param name="tag">The tag to compare, which may be multiple tags.</param>
        /// <returns>Whether GameObject matches given tag.</returns>
        public static bool CompareTagMask(this GameObject obj, long tag) {
            int objTag = TagMask.NameToTag(obj.tag);
            return ((1 << objTag) & tag) != 0;
        }
        
        /// <summary>
        /// Return true if Component's tag matches given any tag in given value.
        /// </summary>
        /// <param name="obj">The Component to check.</param>
        /// <param name="tag">The tag to compare, which may be multiple tags.</param>
        /// <returns>Whether Component matches given tag.</returns>
        public static bool CompareTagMask(this Component obj, long tag) {
            int objTag = TagMask.NameToTag(obj.tag);
            return ((1 << objTag) & tag) != 0;
        }

        /// <summary>
        /// Set the tag index of a GameObject.
        /// </summary>
        /// <param name="obj">Object to modify.</param>
        /// <param name="tagIndex">Tag to set.</param>
        public static void SetTagIndex(this GameObject obj, int tagIndex) {
            obj.tag = TagMask.TagToName(tagIndex);
        }
        
        /// <summary>
        /// Get the tag index of a GameObject.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The GameObject's tag index.</returns>
        public static int GetTagIndex(this GameObject obj) {
            return TagMask.NameToTag(obj.tag);
        }
        
        /// <summary>
        /// Get the tag index of a Component.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The Component's tag index.</returns>
        public static int GetTagIndex(this Component obj) {
            return TagMask.NameToTag(obj.tag);
        }
        
        /// <summary>
        /// Get the tag mask of a GameObject.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The GameObject's tag as a mask.</returns>
        public static int GetTagMask(this GameObject obj) {
            return TagMask.GetMask(obj.tag);
        }
        
        /// <summary>
        /// Get the tag mask of a Component.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The Component's tag as a mask.</returns>
        public static int GetTagMask(this Component obj) {
            return TagMask.GetMask(obj.tag);
        }
    }
}
