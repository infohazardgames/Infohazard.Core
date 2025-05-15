// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// A lightweight timer that does not need to be updated each frame.
    /// </summary>
    /// <remarks>
    /// Can be serialized directly in the inspector or created in code.
    /// If it is assigned in the inspector,
    /// you must call <see cref="Initialize"/> in Start/Awake/OnEnable/OnSpawned.
    /// <para/>
    /// A PassiveTimer can be in one of four states:
    /// <list type="bullet">
    /// <item>It has not yet been initialized (uninitialized state).</item>
    /// <item>An interval is active and counting down (counting state).</item>
    /// <item>The timer is paused (paused state).</item>
    /// <item>An interval has expired (expired state).</item>
    /// </list>
    /// </remarks>
    [Serializable]
    public struct PassiveTimer {
        [SerializeField]
        [Tooltip("Initial interval to set the timer for in seconds.")]
        private float _initialInterval;

        [SerializeField]
        [Tooltip("The repeat interval for the timer in seconds.")]
        private float _interval;

        [SerializeField]
        [Tooltip("What value for time that the timer uses (scaled, unscaled, or realtime).")]
        private TimeMode _mode;

        /// <summary>
        /// Initial interval to set the timer for in seconds.
        /// </summary>
        /// <remarks>
        /// This interval begins when <see cref="Initialize"/> is called,
        /// or when the timer is created from a non-default constructor.
        /// </remarks>
        public float InitialInterval {
            get => _initialInterval;
            set => _initialInterval = value;
        }

        /// <summary>
        /// The repeat interval for the in seconds.
        /// </summary>
        /// <remarks>
        /// This interval begins when <see cref="StartInterval"/> or <see cref="TryConsume"/> is used.
        /// </remarks>
        public float Interval {
            get => _interval;
            set => _interval = value;
        }

        /// <summary>
        /// What value for time that the timer uses (scaled, unscaled, or realtime).
        /// </summary>
        public TimeMode Mode {
            get => _mode;
            set => _mode = value;
        }

        /// <summary>
        /// Whether the timer is in the expired state, meaning the current interval has elapsed.
        /// </summary>
        public bool IsIntervalEnded => CurrentTime - IntervalStartTime >= _interval;

        /// <summary>
        /// The start time for the current interval.
        /// </summary>
        public float IntervalStartTime { get; set; }

        /// <summary>
        /// The time at which the current interval will end (or has ended).
        /// </summary>
        public float IntervalEndTime => IntervalStartTime + _interval;

        /// <summary>
        /// Time that has passed since the current interval ended (or, if not ended, a negative value).
        /// </summary>
        public float TimeSinceIntervalEnded => CurrentTime - IntervalEndTime;

        /// <summary>
        /// Whether the timer is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Whether an interval has started yet.
        /// </summary>
        public bool HasIntervalStarted { get; private set; }

        /// <summary>
        /// The time in seconds until the current interval ends.
        /// </summary>
        public float TimeUntilIntervalEnd {
            get => Mathf.Max(IntervalEndTime - CurrentTime, 0);
            set => IntervalStartTime = CurrentTime + value - _interval;
        }

        /// <summary>
        /// A ratio going from one at interval start to zero at interval end.
        /// </summary>
        public float RatioUntilIntervalEnd => Mathf.Clamp01(TimeUntilIntervalEnd / _interval);

        /// <summary>
        /// The time in seconds since the current interval started.
        /// </summary>
        public float TimeSinceIntervalStart {
            get => Mathf.Max(CurrentTime - IntervalStartTime, 0);
            set => IntervalStartTime = CurrentTime - value;
        }

        /// <summary>
        /// A ratio going from zero at interval start to one at interval end.
        /// </summary>
        public float RatioSinceIntervalStart => Mathf.Clamp01(TimeSinceIntervalStart / _interval);

        /// <summary>
        /// Whether the current interval ended during the current frame.
        /// </summary>
        /// <remarks>
        /// This can be used to create actions that happen only once,
        /// the moment a timer expires.
        /// </remarks>
        public bool DidIntervalEndThisFrame {
            get {
                if (_mode == TimeMode.Realtime) return false;
                return HasIntervalStarted && IsIntervalEnded &&
                       CurrentTime - DeltaTime <= IntervalStartTime + _interval;
            }
        }

        /// <summary>
        /// The current time read from Unity, based on the <see cref="Mode"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If <see cref="Mode"/> is invalid.</exception>
        public float CurrentTimeWithoutPause {
            get {
                return _mode switch {
                    TimeMode.Realtime => Time.realtimeSinceStartup,
                    TimeMode.Scaled => Time.time,
                    TimeMode.Unscaled => Time.unscaledTime,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// The current time read from Unity, taking into account time that the PassiveTimer has spent paused.
        /// </summary>
        public float CurrentTime {
            get {
                if (IsPaused) return _pauseStartTime - PausedTime;
                return CurrentTimeWithoutPause - PausedTime;
            }
        }

        /// <summary>
        /// The delta time read from Unity, based on the <see cref="Mode"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If <see cref="Mode"/> is invalid or realtime.</exception>
        public float DeltaTime {
            get {
                if (_isPaused) return 0;

                return _mode switch {
                    TimeMode.Scaled => Time.deltaTime,
                    TimeMode.Unscaled => Time.unscaledDeltaTime,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// The time that the PassiveTimer has spent in a paused state.
        /// </summary>
        public float PausedTime { get; private set; }

        private bool _isPaused;
        private float _pauseStartTime;

        /// <summary>
        /// Get or set whether the PassiveTimer is paused.
        /// </summary>
        /// <remarks>
        /// It is not necessary to pause timers to account for the game pausing,
        /// as long as they are using realtime.
        /// This allows an individual timer to be paused separately from the rest of the game.
        /// </remarks>
        public bool IsPaused {
            get => _isPaused;
            set {
                if (_isPaused == value) return;
                _isPaused = value;
                if (value) {
                    _pauseStartTime = CurrentTimeWithoutPause;
                } else {
                    PausedTime += CurrentTimeWithoutPause - _pauseStartTime;
                }
            }
        }

        /// <summary>
        /// Construct a PassiveTimer with the given interval, which will be both the initial and repeat interval.
        /// </summary>
        /// <param name="interval">The initial and repeat interval.</param>
        /// <param name="mode">Time mode to use.</param>
        /// <param name="initialize">Whether to initialize the timer and start counting immediately.</param>
        public PassiveTimer(float interval, TimeMode mode = TimeMode.Scaled, bool initialize = true) :
            this(interval, interval, mode, initialize) { }

        /// <summary>
        /// Construct a PassiveTimer with the given interval.
        /// </summary>
        /// <param name="initialInterval">The initial interval.</param>
        /// <param name="interval">The repeat interval.</param>
        /// <param name="mode">Time mode to use.</param>
        /// <param name="initialize">Whether to initialize the timer and start counting immediately.</param>
        public PassiveTimer(float initialInterval, float interval, TimeMode mode = TimeMode.Scaled,
                            bool initialize = true) {
            _initialInterval = initialInterval;
            _interval = interval;
            _mode = mode;

            IsInitialized = false;
            HasIntervalStarted = false;
            IntervalStartTime = 0;
            PausedTime = 0;
            _isPaused = false;
            _pauseStartTime = 0;
            if (initialize) Initialize();
        }

        /// <summary>
        /// Initialize the timer.
        /// </summary>
        /// <remarks>
        /// You must call this when your script initializes if the timer was assigned in the inspector.
        /// </remarks>
        public void Initialize() {
            IsInitialized = true;
            IntervalStartTime = CurrentTime - (_interval - _initialInterval);
            HasIntervalStarted = _initialInterval > 0;
        }

        /// <summary>
        /// If the current interval has ended, reset the interval and return true, else return false without reset.
        /// </summary>
        /// <remarks>
        /// This is useful to create ability cooldowns or weapon fire rates. See the following example:
        /// <example>
        /// <code>
        /// if (AbilityButtonPressed() && AbilityTimer.TryConsume()) {
        ///     UseAbility();
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <returns>Whether the interval was ended and has been reset.</returns>
        public bool TryConsume() {
            if (!IsIntervalEnded) return false;
            StartInterval();
            return true;
        }

        /// <summary>
        /// Restart the interval, so that the timer starts counting down from its repeat interval.
        /// </summary>
        public void StartInterval() {
            IntervalStartTime = CurrentTime;
            HasIntervalStarted = true;
        }

        /// <summary>
        /// End the interval, so the timer is in the expired state.
        /// </summary>
        public void EndInterval() {
            IntervalStartTime = CurrentTime - _interval;
        }

        /// <summary>
        /// The various modes available for timers.
        /// </summary>
        public enum TimeMode {
            /// <summary>
            /// Use Unity scaled time (Time.time).
            /// </summary>
            Scaled,

            /// <summary>
            /// Use Unity unscaled time (Time.unscaledTime).
            /// </summary>
            Unscaled,

            /// <summary>
            /// Use Unity realtime (Time.realtimeSinceStartup).
            /// </summary>
            Realtime,
        }
    }
}
