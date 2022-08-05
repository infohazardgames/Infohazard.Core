using UnityEngine;

namespace Infohazard.Core {
    public class DrawSingleChildPropertyAttribute : PropertyAttribute {
        public string PropertyName { get; }
        
        public DrawSingleChildPropertyAttribute(string propertyName) {
            PropertyName = propertyName;
        }
    }
}