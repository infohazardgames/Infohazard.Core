// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core.Tests.PlayMode {
    /// <summary>
    /// A component implementing only <see cref="ITestInterfaceA"/>, for interface-validation tests.
    /// </summary>
    public class SingleInterfaceComponent : MonoBehaviour, ITestInterfaceA { }
}
