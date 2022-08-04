using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core.Runtime {
    public class ExpandableAttribute : PropertyAttribute {
        public bool AlwaysExpanded { get; }
        public bool ShowNewButton { get; }
        public string SavePath { get; }

        public ExpandableAttribute(bool alwaysExpanded = false, string savePath = null, bool showNewButton = true) {
            AlwaysExpanded = alwaysExpanded;
            SavePath = savePath;
            ShowNewButton = showNewButton;
        }
    }
}