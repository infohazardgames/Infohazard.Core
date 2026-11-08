// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core.Tests.PlayMode {
    public interface ITestInterfaceA { }

    public interface ITestInterfaceB { }

    /// <summary>
    /// A component implementing two test interfaces, for interface-validation tests.
    /// </summary>
    /// <remarks>
    /// It lives in the PlayMode assembly because components from the Editor-only test assembly
    /// can't be added to GameObjects.
    /// </remarks>
    public class MultiInterfaceComponent : MonoBehaviour, ITestInterfaceA, ITestInterfaceB { }
}
