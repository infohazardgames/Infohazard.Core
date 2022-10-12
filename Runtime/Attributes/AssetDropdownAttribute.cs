// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute that draws an object reference as a dropdown that searches through the project.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetDropdownAttribute : PropertyAttribute { }
}