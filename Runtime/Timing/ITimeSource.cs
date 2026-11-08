// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

namespace Infohazard.Core {
    /// <summary>
    /// A source of time values, used by <see cref="PassiveTimer"/> in
    /// <see cref="PassiveTimer.TimeMode.Custom"/> mode.
    /// </summary>
    /// <remarks>
    /// Implement this to drive timers from a clock other than Unity's, such as a game clock that pauses
    /// separately, a per-entity time scale, or a manually advanced clock in tests.
    /// </remarks>
    public interface ITimeSource {
        /// <summary>
        /// The current time in seconds.
        /// </summary>
        float Time { get; }

        /// <summary>
        /// The time in seconds that elapsed during the current frame.
        /// </summary>
        float DeltaTime { get; }
    }
}
