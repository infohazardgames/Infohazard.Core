// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// A unique name asset, usable by a <see cref="UniqueNamedObject"/>.
    /// </summary>
    /// <remarks>
    /// The asset's name is the unique name that will be referenced.
    /// UniqueNameListEntries should be created via a UniqueNameList.
    /// </remarks>
    public class UniqueNameListEntry : ScriptableObject { }
}