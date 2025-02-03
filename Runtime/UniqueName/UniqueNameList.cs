// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// A list used to organize unique names used by objects.
    /// </summary>
    /// <remarks>
    /// You can have one or many UniqueNameLists in your project, it is totally up to you.
    /// When selecting a unique name for an object,
    /// you will have the option to create a new one in any UniqueNameList.
    /// </remarks>
    [CreateAssetMenu(menuName = "Infohazard/Unique Name List")]
    public class UniqueNameList : ScriptableObject {
        [SerializeField]
        [EditNameOnly]
        [Tooltip("All unique name assets in this list.")]
        private UniqueNameListEntry[] _entries;

        /// <summary>
        /// All unique name assets in this list.
        /// </summary>
        public IReadOnlyList<UniqueNameListEntry> Entries => _entries;
    }
}
