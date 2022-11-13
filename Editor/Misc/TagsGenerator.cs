// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private const string TagTemplate = "        public const string {0} = @\"{1}\";" + Newline;
        private const string TagMaskTemplate = "        public const long {0}Mask = 1 << {1};" + Newline;
        private const string TagArrayTemplate = "@\"{0}\", ";

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

            bool needsRegen = false;
            for (int i = 0; i < 64; i++) {
                if (i == tags.Length && i == Tag.Tags.Length) break;

                if (i >= tags.Length || i >= Tag.Tags.Length || tags[i] != Tag.Tags[i]) {
                    needsRegen = true;
                    break;
                }
            }

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
        [MenuItem("Tools/Infohazard/Generate/Update GameTag.cs", priority = 0)]
        public static void Generate() {
            if (EditorUtility.DisplayDialog("Update GameTag.cs", "This will create or overwrite the file Infohazard.Core.Data/GameTag.cs.", "OK", "Cancel")) {
                DoGenerate();
            }
        }

        /// <summary>
        /// Remove the GameTag file.
        /// </summary>
        [MenuItem("Tools/Infohazard/Generate/Remove GameTag.cs", priority = 0)]
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

            int maskTags = 0;

            HashSet<string> tagVars = new HashSet<string>();
            for (int i = 0; i < tags.Length; i++) {
                string tag = tags[i];
                
                // Remove all characters except letters, numbers, and underscore.
                string varName = Regex.Replace(tag, "\\W", "");
                
                // Strings are generated as verbatim, so replace single quotes with double quotes.
                string tagString = tag.Replace("\"", "\"\"");

                bool generateMask = maskTags < 64;

                // Dont create empty variables or duplicate variables.
                if (!string.IsNullOrEmpty(varName) && tagVars.Add(varName)) {
                    // Ensure first character is not a digit.
                    if (char.IsDigit(varName[0])) {
                        varName = '_' + varName;
                    }

                    // Add @ character in case var name is a keyword.
                    varName = '@' + varName;
                    
                    tagDecls += string.Format(TagTemplate, varName, tagString);

                    if (generateMask) {
                        tagMasks += string.Format(TagMaskTemplate, varName, maskTags);
                    }
                }

                if (generateMask) {
                    tagArray += string.Format(TagArrayTemplate, tagString);
                    maskTags++;
                }
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