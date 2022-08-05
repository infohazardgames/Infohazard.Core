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
using UnityEditor;
using System.Linq;
using System;
using Infohazard.Core;
using UnityEditor.IMGUI.Controls;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(TypeSelectAttribute))]
    public class TypeSelectAttributeDrawer : PropertyDrawer {
        private string[] _types;
        private SearchField _searchField;
        private string _search = "";

        private void OnSearch() {

        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            // First get the attribute since it contains the range for the slider
            TypeSelectAttribute attr = (TypeSelectAttribute) attribute;

            // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
            if (property.propertyType == SerializedPropertyType.String) {
                if (_searchField == null && attr.Search) {
                    _searchField = new SearchField();
                }

                if (_types == null) {
                    var tempTypes = TypeUtility.AllTypes.Where(p => !p.IsInterface &&
                                                             (attr.AllowGeneric || !p.IsGenericType) &&
                                                             (attr.AllowAbstract || !p.IsAbstract) &&
                                                             attr.BaseClass.IsAssignableFrom(p));

                    if (attr.Search) {
                        tempTypes = tempTypes.Where(t => t.Name.ToLower().Contains(_search));
                    }

                    var cur = TypeUtility.GetType(property.stringValue);
                    if (cur != null && !tempTypes.Contains(cur)) tempTypes.Append(cur);

                    _types = tempTypes.Select(t => t.FullName).Prepend("(none)").ToArray();
                }

                int index = Array.IndexOf(_types, property.stringValue);
                if (index < 0) {
                    index = Mathf.Max(Array.IndexOf(_types, attr.BaseClass.FullName), 0);
                }

                EditorGUI.BeginProperty(position, label, property);
                if (attr.Search) {
                    EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 3, position.height), label);
                    var newSearch = _searchField.OnGUI(new Rect(position.x + position.width / 3, position.y, position.width / 3 - 5, position.height), _search);
                    index = EditorGUI.Popup(new Rect(position.x + position.width * 2 / 3, position.y, position.width / 3, position.height), index, _types);
                    property.stringValue = _types[index];

                    if (newSearch != _search) {
                        _search = newSearch.ToLower();
                        _types = null;
                    }
                } else {
                    EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 2, position.height), label);
                    index = EditorGUI.Popup(new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height), index, _types);
                    property.stringValue = _types[index];
                }
                EditorGUI.EndProperty();
            } else {
                EditorGUI.LabelField(position, label.text, "Use TypeSelect with string.");
            }
        }
    }
}