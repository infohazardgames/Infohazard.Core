// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

namespace Infohazard.Core.Tests.PlayMode {
    /// <summary>
    /// A singleton used by <see cref="UnitTest_Singleton"/>.
    /// </summary>
    public class TestSingleton : Singleton<TestSingleton> {
        public int InitializeCount { get; private set; }

        public override void Initialize() {
            InitializeCount++;
        }
    }
}
