using System;
using UnityEngine;

namespace Infohazard.Core.Runtime {
    [Serializable]
    public struct PassiveTimer {
        [SerializeField] private float _initialInterval;
        [SerializeField] private float _interval;
        [SerializeField] private PassiveTimerMode _mode;

        public float InitialInterval {
            get => _initialInterval;
            set => _initialInterval = value;
        }

        public float Interval {
            get => _interval;
            set => _interval = value;
        }

        public PassiveTimerMode Mode {
            get => _mode;
            set => _mode = value;
        }

        public bool IntervalPassed => Time.time - IntervalStartTime >= _interval;
        public float IntervalStartTime { get; set; }
        public float IntervalEndTime => IntervalStartTime + _interval;
        public bool IsInitialized { get; private set; }
        public bool HasIntervalStarted { get; private set; }

        public float TimeUntilIntervalEnd => Mathf.Max(IntervalEndTime - CurrentTime, 0);
        public float RatioUntilIntervalEnd => Math.Min(TimeUntilIntervalEnd / _interval, 1);

        public float TimeSinceIntervalStart => Mathf.Max(CurrentTime - IntervalStartTime, 0);
        public float RatioSinceIntervalStart => Mathf.Min(TimeSinceIntervalStart / _interval, 0);

        public bool IntervalPassedThisFrame {
            get {
                if (_mode == PassiveTimerMode.Realtime) return false;
                return HasIntervalStarted && IntervalPassed && CurrentTime - DeltaTime <= IntervalStartTime + _interval;
            }
        }

        public float CurrentTimeWithoutPause {
            get {
                return _mode switch {
                    PassiveTimerMode.Realtime => Time.realtimeSinceStartup,
                    PassiveTimerMode.Scaled => Time.time,
                    PassiveTimerMode.Unscaled => Time.unscaledTime,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public float CurrentTime {
            get {
                if (IsPaused) return _pauseStartTime - PausedTime;
                return CurrentTimeWithoutPause - PausedTime;
            }
        }

        public float DeltaTime {
            get {
                if (_isPaused) return 0;
                
                return _mode switch {
                    PassiveTimerMode.Scaled => Time.deltaTime,
                    PassiveTimerMode.Unscaled => Time.unscaledDeltaTime,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        
        public float PausedTime { get; private set; }
        private bool _isPaused;
        private float _pauseStartTime;

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
        

        public PassiveTimer(float interval, PassiveTimerMode mode = PassiveTimerMode.Scaled, bool initialize = false) :
            this(interval, interval, mode, initialize) { }

        public PassiveTimer(float initialInterval, float interval, PassiveTimerMode mode = PassiveTimerMode.Scaled, bool initialize = false) {
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

        public void Initialize() {
            IsInitialized = true;
            IntervalStartTime = CurrentTime - (_interval - _initialInterval);
            HasIntervalStarted = _initialInterval > 0;
        }

        public bool TryConsume() {
            if (!IntervalPassed) return false;
            StartInterval();
            return true;
        }

        public void StartInterval() {
            IntervalStartTime = CurrentTime;
            HasIntervalStarted = true;
        }

        public void EndInterval() {
            IntervalStartTime = CurrentTime - _interval;
        }
    }

    public enum PassiveTimerMode {
        Scaled,
        Unscaled,
        Realtime,
    }
}
