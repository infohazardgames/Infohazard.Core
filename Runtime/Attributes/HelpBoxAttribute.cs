// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;

namespace Infohazard.Core {
    /// <summary>
    /// Used to add a detailed help box that can be toggled in the inspector.
    /// This can be used to provide more information than a simple tooltip.
    ///
    /// This attribute does not contain a custom drawer, in order to allow compatibility with other custom drawers.
    /// Instead, a custom editor script must be created to handle the display of the help box.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class HelpBoxAttribute : Attribute {
        public string Text { get; }
        
        public HelpBoxAttribute(string text) {
            Text = text;
        }
    }
}