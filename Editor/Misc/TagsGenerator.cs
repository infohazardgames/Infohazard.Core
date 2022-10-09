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
using System.IO;
using System.Linq;
using System.Text;
using Infohazard.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Infohazard.Core.Editor {
    /// <summary>
    /// Class used to generate the GameTag.cs file to use your custom tags in code.
    /// </summary>
    /// <remarks>
    /// To generate this file, use the menu item Infohazard > Generate > Update GameTag.cs.
    /// </remarks>
    [InitializeOnLoad]
    public static class TagsGenerator {
        private const string CheckTagsPref = "CheckTags";

        private const string Newline = @"
";
        private const string TagTemplate = "        public const string {0} = \"{1}\";" + Newline;
        private const string TagMaskTemplate = "        public const long {0}Mask = 1 << {1};" + Newline;
        private const string TagArrayTemplate = "\"{0}\", ";

        private const string Template = @"using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infohazard.Core {{
    public static class GameTag {{
{0}
{1}
        public static readonly string[] Tags = {{
            {2}
        }};

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() {{
            Tag.GameOverrideTags = Tags;
        }}
    }}
}}
";
        private static bool _didGenerate = false;

        static TagsGenerator() {
            EditorApplication.update += CheckTags;
        }

        private static void CheckTags() {
            if (_didGenerate || !EditorPrefs.GetBool(CheckTagsPref, true)) {
                return;
            }
            string[] tags = InternalEditorUtility.tags;

            bool needsRegen = !Enumerable.SequenceEqual(tags, Tag.Tags);

            if (needsRegen) {
                if (EditorUtility.DisplayDialog("Generate Tags", "Do you want to generate a GameTag.cs file?", "OK", "No")) {
                    DoGenerate();
                } else {
                    EditorPrefs.SetBool(CheckTagsPref, false);
                }
            }
        }

        /// <summary>
        /// Generate the GameTag file.
        /// </summary>
        [MenuItem("Infohazard/Generate/Update GameTag.cs")]
        public static void Generate() {
            if (EditorUtility.DisplayDialog("Update GameTag.cs", "This will create or overwrite the file Infohazard.Core.Data/GameTag.cs. This may produce some errors in the console. Don't worry about it.", "OK", "Cancel")) {
                DoGenerate();
            }
        }

        /// <summary>
        /// Remove the GameTag file.
        /// </summary>
        [MenuItem("Infohazard/Generate/Remove GameTag.cs")]
        public static void Remove() {
            if (EditorUtility.DisplayDialog("Remove GameTag.cs", "This will delete the generated GameTag.cs file, and revert to using only the builtin tags.", "OK", "Cancel")) {
                DoRemove();
            }
        }

        private static void DoGenerate() {
            EditorPrefs.SetBool(CheckTagsPref, true);
            _didGenerate = true;

            string tagDecls = string.Empty;
            string tagMasks = string.Empty;
            string tagArray = string.Empty;
            
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++) {
                string tag = tags[i];

                string varName = tag.Replace(" ", "");
                tagDecls += string.Format(TagTemplate, varName, tag);
                tagMasks += string.Format(TagMaskTemplate, varName, i);
                tagArray += string.Format(TagArrayTemplate, tag);
            }
            
            string generated = string.Format(Template, tagDecls, tagMasks, tagArray);

            CoreEditorUtility.EnsureDataFolderExists();
            string defPath = Path.Combine(CoreEditorUtility.DataFolder, "GameTag.cs");

            StreamWriter outStream = new StreamWriter(defPath);
            outStream.Write(generated);
            outStream.Close();
            AssetDatabase.Refresh();
        }

        private static void DoRemove() {
            EditorPrefs.SetBool(CheckTagsPref, false);
            string defPath = Path.Combine(CoreEditorUtility.DataFolder, "GameTag.cs");
            if (File.Exists(defPath)) {
                AssetDatabase.DeleteAsset(defPath);
                AssetDatabase.Refresh();
            }
        }
    }
}