using System;
using UnityEngine;

namespace Infohazard.Core.Runtime {
    /// <summary>
    /// Attribute that tells Unity to draw an object reference as a dropdown that searches through the project.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetDropdownAttribute : PropertyAttribute { }
}