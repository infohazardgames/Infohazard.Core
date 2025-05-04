// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Provides string constants for builtin Unity tags.
    /// </summary>
    /// <remarks>
    /// To extend with custom tags, see <c>GameTag</c>,
    /// which you can generate using the command Infohazard > Generate > Update GameTag.cs.
    /// </remarks>
    public static class Tag {
        /// <summary>
        /// The string <c>"Untagged"</c>.
        /// </summary>
        public const string Untagged = "Untagged";

        /// <summary>
        /// The string <c>"Respawn"</c>.
        /// </summary>
        public const string Respawn = "Respawn";

        /// <summary>
        /// The string <c>"Finish"</c>.
        /// </summary>
        public const string Finish = "Finish";

        /// <summary>
        /// The string <c>"EditorOnly"</c>.
        /// </summary>
        public const string EditorOnly = "EditorOnly";

        /// <summary>
        /// The string <c>"MainCamera"</c>.
        /// </summary>
        public const string MainCamera = "MainCamera";

        /// <summary>
        /// The string <c>"Player"</c>.
        /// </summary>
        public const string Player = "Player";

        /// <summary>
        /// The string <c>"GameController"</c>.
        /// </summary>
        public const string GameController = "GameController";

        /// <summary>
        /// Array of default tags provided by Unity.
        /// </summary>
        public static readonly string[] DefaultTags = {
            "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController",
        };

        /// <summary>
        /// Set by the generated <c>GameTag</c> script.
        /// </summary>
        public static string[] GameOverrideTags = null;

        /// <summary>
        /// Array of all default and custom tags in the project.
        /// </summary>
        public static string[] Tags => GameOverrideTags ?? DefaultTags;
    }

    /// <summary>
    /// Used to select tags in the inspector, including the ability to select multiple tags.
    /// </summary>
    /// <remarks>
    /// Works similar to LayerMask. If you have a custom <c>GameTag</c> script generated,
    /// your custom tags will be available here too.
    /// You can find code constants for those tags in <c>GameTagMask</c>.
    /// Like LayerMask, TagMask is implicitly convertable to and from an integer value (long in this case).
    /// </remarks>
    [Serializable]
    public struct TagMask : IEquatable<TagMask> {
        /// <summary>
        /// Mask value for the Untagged tag.
        /// </summary>
        public const long UntaggedMask = 1 << 0;

        /// <summary>
        /// Mask value for the Respawn tag.
        /// </summary>
        public const long RespawnMask = 1 << 1;

        /// <summary>
        /// Mask value for the Finish tag.
        /// </summary>
        public const long FinishMask = 1 << 2;

        /// <summary>
        /// Mask value for the EditorOnly tag.
        /// </summary>
        public const long EditorOnlyMask = 1 << 3;

        /// <summary>
        /// Mask value for the MainCamera tag.
        /// </summary>
        public const long MainCameraMask = 1 << 4;

        /// <summary>
        /// Mask value for the Player tag.
        /// </summary>
        public const long PlayerMask = 1 << 5;

        /// <summary>
        /// Mask value for the GameController tag.
        /// </summary>
        public const long GameControllerMask = 1 << 6;

        [SerializeField] private long _value;

        /// <summary>
        /// The value of the mask as a 64-bit integer.
        /// </summary>
        public long Value {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Initialize a new TagMask with the given value.
        /// </summary>
        /// <param name="value">The value to initialize with, representing which tags are "on".</param>
        public TagMask(long value) {
            _value = value;
        }

        #region Conversions

        /// <summary>
        /// Convert a TagMask to a long.
        /// </summary>
        /// <param name="mask">TagMask to convert.</param>
        /// <returns>The mask's value.</returns>
        public static implicit operator long(TagMask mask) {
            return mask._value;
        }

        /// <summary>
        /// Convert a long to a TagMask.
        /// </summary>
        /// <param name="mask">The mask value.</param>
        /// <returns>The created TagMask.</returns>
        public static implicit operator TagMask(long mask) {
            return new TagMask(mask);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Apply bitwise AND operator to two TagMasks.
        /// </summary>
        public static TagMask operator &(in TagMask lhs, in TagMask rhs) {
            return new TagMask(lhs._value & rhs._value);
        }

        /// <summary>
        /// Apply bitwise AND operator to a TagMask and a long.
        /// </summary>
        public static TagMask operator &(TagMask lhs, long rhs) {
            return new TagMask(lhs._value & rhs);
        }

        /// <summary>
        /// Apply bitwise AND operator to a long and a TagMask.
        /// </summary>
        public static long operator &(long lhs, TagMask rhs) {
            return lhs & rhs._value;
        }

        /// <summary>
        /// Apply bitwise OR operator to a TagMask and a TagMask.
        /// </summary>
        public static TagMask operator |(TagMask lhs, TagMask rhs) {
            return new TagMask(lhs._value | rhs._value);
        }

        /// <summary>
        /// Apply bitwise OR operator to a TagMask and a long.
        /// </summary>
        public static TagMask operator |(TagMask lhs, long rhs) {
            return new TagMask(lhs._value | rhs);
        }

        /// <summary>
        /// Apply bitwise OR operator to a long and a TagMask.
        /// </summary>
        public static long operator |(long lhs, TagMask rhs) {
            return lhs | rhs._value;
        }

        /// <summary>
        /// Apply bitwise XOR operator to a TagMask and a TagMask.
        /// </summary>
        public static TagMask operator ^(TagMask lhs, TagMask rhs) {
            return new TagMask(lhs._value ^ rhs._value);
        }

        /// <summary>
        /// Apply bitwise XOR operator to a TagMask and a long.
        /// </summary>
        public static TagMask operator ^(TagMask lhs, long rhs) {
            return new TagMask(lhs._value ^ rhs);
        }

        /// <summary>
        /// Apply bitwise XOR operator to a long and a TagMask.
        /// </summary>
        public static long operator ^(long lhs, TagMask rhs) {
            return lhs ^ rhs._value;
        }

        /// <summary>
        /// Apply bitwise NOT operator to a TagMask.
        /// </summary>
        public static TagMask operator ~(TagMask mask) {
            return new TagMask(~mask._value);
        }

        #endregion

        private static StringBuilder _toStringBuilder = new StringBuilder();

        /// <inheritdoc/>
        public override string ToString() {
            bool first = true;
            for (int i = 0; i < Tag.Tags.Length; i++) {
                if ((_value & (1 << i)) != 0) {
                    if (first) _toStringBuilder.Append(", ");
                    _toStringBuilder.Append(Tag.Tags[i]);
                    first = false;
                }
            }

            string result = _toStringBuilder.ToString();
            _toStringBuilder.Clear();
            return result;
        }

        public string[] ToStringArray() {
            List<string> result = new();
            for (int i = 0; i < Tag.Tags.Length; i++) {
                if ((_value & (1 << i)) != 0) {
                    result.Add(Tag.Tags[i]);
                }
            }

            return result.ToArray();
        }

        /// <inheritdoc cref="System.IEquatable{T}.Equals(T)"/>
        public bool Equals(TagMask other) {
            return _value == other._value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            return obj is TagMask other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Gets the index of a given tag in the <see cref="Tag.Tags"/> array.
        /// </summary>
        /// <param name="name">Tag name.</param>
        /// <returns>The index of the tag or -1 if it doesn't exist.</returns>
        public static int NameToTag(string name) => Array.IndexOf(Tag.Tags, name);

        /// <summary>
        /// Gets the tag name at the given index in the <see cref="Tag.Tags"/> array.
        /// </summary>
        /// <param name="tag">Tag index. Must be in range [0, TAG COUNT - 1].</param>
        /// <returns>The tag's name.</returns>
        public static string TagToName(int tag) => Tag.Tags[tag];

        /// <summary>
        /// Get a mask value that contains all the given tag names.
        /// </summary>
        /// <param name="names">Names of tags to include in the mask.</param>
        /// <returns>The created mask.</returns>
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
        /// <summary>
        /// Get a mask value that contains the given tag name.
        /// </summary>
        /// <param name="name">Name of tag to include in the mask.</param>
        /// <returns>The created mask.</returns>
        public static long GetMask(string name) {
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

        public static bool CompareTagArray(this Component obj, string[] tags) {
            for (int i = 0; i < tags.Length; i++) {
                if (obj.CompareTag(tags[i])) return true;
            }

            return false;
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
        public static long GetTagMask(this GameObject obj) {
            return TagMask.GetMask(obj.tag);
        }

        /// <summary>
        /// Get the tag mask of a Component.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The Component's tag as a mask.</returns>
        public static long GetTagMask(this Component obj) {
            return TagMask.GetMask(obj.tag);
        }
    }
}
