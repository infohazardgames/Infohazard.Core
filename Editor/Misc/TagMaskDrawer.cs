using Infohazard.Core;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(TagMask))]
    public class TagMaskDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUIContent realLabel = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty prop = property.FindPropertyRelative("_mask");
            prop.longValue = EditorGUI.MaskField(position, realLabel, (int)prop.longValue, TagMask.Tags);
            EditorGUI.EndProperty();
        }
    }
}