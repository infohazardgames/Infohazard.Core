using UnityEngine;

namespace Infohazard.Core.Runtime {
    public class DrawSingleChildPropertyAttribute : PropertyAttribute {
        public string PropertyName { get; }
        
        public DrawSingleChildPropertyAttribute(string propertyName) {
            PropertyName = propertyName;
        }
    }
}