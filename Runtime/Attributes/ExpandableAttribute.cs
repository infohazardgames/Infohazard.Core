// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute that enables editing properties of a referenced object.
    /// </summary>
    /// <remarks>
    /// Normally, to edit a referenced object, you'd have to change the inspector context.
    /// With an ExpandableAttribute, you can just expand the property and edit the referenced object inline.
    /// If the type of the reference field is a ScriptableObject,
    /// the ExpandableAttribute also allows creating new instances in the inspector.
    /// </remarks>
    public class ExpandableAttribute : PropertyAttribute {
        /// <summary>
        /// Whether the attribute is always expanded.
        /// </summary>
        /// <remarks>
        /// If false, will show the expander.
        /// </remarks>
        public bool AlwaysExpanded { get; }

        /// <summary>
        /// Whether to show a "New" button to create new instances.
        /// </summary>
        /// <remarks>
        /// This only works if the type of the field is a ScriptableObject.
        /// </remarks>
        public bool ShowNewButton { get; }

        /// <summary>
        /// The default path to save newly created ScriptableObjects at.
        /// </summary>
        /// <remarks>
        /// If unset, this will use the same path as the containing object if it is an asset.
        /// </remarks>
        public string SavePath { get; }

        /// <summary>
        /// Whether to show a dropdown of all types extending the type of the variable.
        /// </summary>
        public bool ShowChildTypes { get; }

        /// <summary>
        /// Interfaces that must be implemented by assigned objects.
        /// </summary>
        public IReadOnlyList<Type> RequiredInterfaces { get; }

        /// <summary>
        /// If true, don't show the child properties even if the property is expanded.
        /// You can use this to implement your own inspector logic for the child properties.
        /// </summary>
        public bool OnlyShowMainLine { get; set; }

        /// <summary>
        /// Construct a new ExpandableAttribute.
        /// </summary>
        /// <param name="alwaysExpanded">Whether the attribute is always expanded.</param>
        /// <param name="savePath">Whether to show a "New" button to create new instances.</param>
        /// <param name="showNewButton">The default path to save newly created ScriptableObjects at.</param>
        /// <param name="showChildTypes">Whether to show a dropdown of all types extending the type of the variable.</param>
        /// <param name="requiredInterfaces">Interfaces that must be implemented by assigned objects.</param>
        public ExpandableAttribute(bool alwaysExpanded = false, string savePath = null, bool showNewButton = true,
                                   bool showChildTypes = false, params Type[] requiredInterfaces) {
            AlwaysExpanded = alwaysExpanded;
            SavePath = savePath;
            ShowNewButton = showNewButton;
            ShowChildTypes = showChildTypes;
            RequiredInterfaces = requiredInterfaces;
        }
    }
}
