// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;
using System;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute that draws string fields as a dropdown where a Type can be selected.
    /// </summary>
    public class TypeSelectAttribute : PropertyAttribute {
        /// <summary>
        /// If set, dropdown will only show types assignable to this type.
        /// </summary>
        public Type BaseClass { get; }
        
        /// <summary>
        /// Whether to show abstract classes.
        /// </summary>
        public bool AllowAbstract { get; }
        
        /// <summary>
        /// Whether to show generic types.
        /// </summary>
        public bool AllowGeneric { get; }
        
        /// <summary>
        /// Whether to show a search bar.
        /// </summary>
        public bool Search { get; }

        /// <summary>
        /// Construct a new TypeSelectAttribute.
        /// </summary>
        /// <param name="baseClass">If set, dropdown will only show types assignable to this type.</param>
        /// <param name="allowAbstract">Whether to show abstract classes.</param>
        /// <param name="allowGeneric">Whether to show generic types.</param>
        /// <param name="search">Whether to show a search bar./</param>
        public TypeSelectAttribute(Type baseClass, bool allowAbstract = false, bool allowGeneric = false, bool search = false) {
            BaseClass = baseClass;
            AllowAbstract = allowAbstract;
            AllowGeneric = allowGeneric;
            Search = search;
        }
    }
}