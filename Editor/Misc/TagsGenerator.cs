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
using System.IO;
using System.Linq;
using System.Text;
using Infohazard.Core.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [InitializeOnLoad]
    public static class TagsGenerator {
        public const string CheckTagsPref = "CheckTags";

        public const string Newline = @"
";
        private const string TagTemplate = "        public const int {0} = {1};" + Newline;
        private const string TagMaskTemplate = "        public const int {0}Mask = 1 << {0};" + Newline;
        private const string TagArrayTemplate = "\"{0}\", ";

        private const string Template = @"using System;
using UnityEngine;
using UnityEditor;

namespace Infohazard.Core.Runtime {{
    public static class GameTagMask {{
{0}
{1}
        public static readonly string[] GameTags = {{
            {2}
        }};

        [InitializeOnLoadMethod, RuntimeInitializeOnLoadMethod]
        private static void Initialize() {{
            TagMask.GameOverrideTags = GameTags;
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
            var tags = InternalEditorUtility.tags;

            bool needsRegen = !Enumerable.SequenceEqual(tags.Where(ValidEnumValue), TagMask.Tags);

            if (needsRegen) {
                if (EditorUtility.DisplayDialog("Generate Tags", "Do you want to generate a Tag.cs file?", "OK", "No")) {
                    DoGenerate();
                } else {
                    EditorPrefs.SetBool(CheckTagsPref, false);
                }
            }
        }

        private static bool ValidEnumValue(string tag) => !tag.Contains(" ");

        [MenuItem("Assets/Update Tag Enum")]
        public static void Generate() {
            if (EditorUtility.DisplayDialog("Update Tag Enum", "This will create or overwrite the file SBR_Data/Tag.cs. This may produce some errors in the console. Don't worry about it.", "OK", "Cancel")) {
                DoGenerate();
            }
        }

        [MenuItem("Assets/Remove Tag Enum")]
        public static void Remove() {
            if (EditorUtility.DisplayDialog("Remove Tag Enum", "This will delete the generated Tags.cs file, and revert to using only the builtin tags.", "OK", "Cancel")) {
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

                tagDecls += string.Format(TagTemplate, tag, i);
                tagMasks += string.Format(TagMaskTemplate, tag);
                tagArray += string.Format(TagArrayTemplate, tag);
            }
            
            string generated = string.Format(Template, tagDecls, tagMasks, tagArray);

            CoreEditorUtility.EnsureDataFolderExists();
            string defPath = Path.Combine(CoreEditorUtility.DataFolder, "GameTagMask.cs");

            StreamWriter outStream = new StreamWriter(defPath);
            outStream.Write(generated);
            outStream.Close();
            AssetDatabase.Refresh();
        }

        private static void DoRemove() {
            EditorPrefs.SetBool(CheckTagsPref, false);
            string defPath = Path.Combine(CoreEditorUtility.DataFolder, "GameTagMask.cs");
            if (File.Exists(defPath)) {
                AssetDatabase.DeleteAsset(defPath);
                AssetDatabase.Refresh();
            }
        }
    }
}