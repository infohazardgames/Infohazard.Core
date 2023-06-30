// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(MustImplementAttribute))]
    public class MustImplementAttributeDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            CoreDrawers.ObjectFieldWithInterfaces(position, property, this.GetFieldType(), label,
                                                  EditorStyles.objectField,
                                                  ((MustImplementAttribute) attribute).RequiredTypes);
        }
    }
}