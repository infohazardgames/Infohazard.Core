// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

namespace Infohazard.Core.Tests.Editor {
    /// <summary>
    /// An <see cref="ITimeSource"/> whose time is advanced manually by tests.
    /// </summary>
    public class ManualTimeSource : ITimeSource {
        public float Time { get; set; }
        public float DeltaTime { get; set; }

        public ManualTimeSource(float time = 100) {
            Time = time;
        }

        /// <summary>
        /// Advance time by one frame of the given length.
        /// </summary>
        public void Advance(float deltaTime) {
            Time += deltaTime;
            DeltaTime = deltaTime;
        }
    }
}
