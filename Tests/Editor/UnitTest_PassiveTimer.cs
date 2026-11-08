// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using NUnit.Framework;
using UnityEngine;
using static Infohazard.Core.Tests.Editor.TestUtil;
using TimeMode = Infohazard.Core.PassiveTimer.TimeMode;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_PassiveTimer {
        private ManualTimeSource _time;

        [SetUp]
        public void SetUp() {
            _time = new ManualTimeSource();
        }

        #region Construction

        [Test]
        public void Constructor_WithTimeSource_InitializesInCustomMode() {
            var timer = new PassiveTimer(5, _time);

            Assert.That(timer.Mode, Is.EqualTo(TimeMode.Custom));
            Assert.That(timer.TimeSource, Is.SameAs(_time));
            Assert.That(timer.IsInitialized, Is.True);
            Assert.That(timer.HasIntervalStarted, Is.True);
            Assert.That(timer.IsIntervalEnded, Is.False);
            Assert.That(timer.Interval, Is.EqualTo(5));
            Assert.That(timer.InitialInterval, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_InitializeFalse_IsNotInitialized() {
            var timer = new PassiveTimer(5, _time, initialize: false);
            Assert.That(timer.IsInitialized, Is.False);
            Assert.That(timer.HasIntervalStarted, Is.False);
        }

        [Test]
        public void Constructor_ZeroInitialInterval_IsEndedImmediately() {
            var timer = new PassiveTimer(0, 5, _time);
            Assert.That(timer.IsIntervalEnded, Is.True);
            Assert.That(timer.HasIntervalStarted, Is.False);
        }

        [Test]
        public void Constructor_InitialInterval_EndsAfterInitialInterval() {
            var timer = new PassiveTimer(1, 5, _time);

            _time.Advance(0.9f);
            Assert.That(timer.IsIntervalEnded, Is.False);
            _time.Advance(0.2f);
            Assert.That(timer.IsIntervalEnded, Is.True);

            timer.StartInterval();
            _time.Advance(4.9f);
            Assert.That(timer.IsIntervalEnded, Is.False, "The repeat interval is used after the first.");
        }

        [Test]
        public void Constructor_CustomWithNullSource_Throws() {
            Assert.Throws<InvalidOperationException>(() => _ = new PassiveTimer(5, (ITimeSource) null));
        }

        #endregion

        #region Interval Control

        [Test]
        public void TryConsume_SucceedsOncePerInterval() {
            var timer = new PassiveTimer(5, _time);
            Assert.That(timer.TryConsume(), Is.False);

            _time.Advance(5);
            Assert.That(timer.TryConsume(), Is.True);
            Assert.That(timer.TryConsume(), Is.False);

            _time.Advance(5);
            Assert.That(timer.TryConsume(), Is.True);
        }

        [Test]
        public void StartInterval_RestartsCountdown() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(4);
            timer.StartInterval();
            _time.Advance(4);
            Assert.That(timer.IsIntervalEnded, Is.False);
            AssertApprox(1, timer.TimeUntilIntervalEnd);
        }

        [Test]
        public void EndInterval_EndsImmediately() {
            var timer = new PassiveTimer(5, _time);
            timer.EndInterval();
            Assert.That(timer.IsIntervalEnded, Is.True);
            AssertApprox(0, timer.TimeUntilIntervalEnd);
        }

        #endregion

        #region Time Queries

        [Test]
        public void TimeQueries_PartwayThroughInterval() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(2);

            AssertApprox(3, timer.TimeUntilIntervalEnd);
            AssertApprox(0.6f, timer.RatioUntilIntervalEnd);
            AssertApprox(2, timer.TimeSinceIntervalStart);
            AssertApprox(0.4f, timer.RatioSinceIntervalStart);
            AssertApprox(-3, timer.TimeSinceIntervalEnded);
            AssertApprox(_time.Time - 2, timer.IntervalStartTime);
            AssertApprox(_time.Time + 3, timer.IntervalEndTime);
        }

        [Test]
        public void TimeQueries_AfterIntervalEnded_AreClamped() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(7);

            AssertApprox(0, timer.TimeUntilIntervalEnd);
            AssertApprox(0, timer.RatioUntilIntervalEnd);
            AssertApprox(1, timer.RatioSinceIntervalStart);
            AssertApprox(7, timer.TimeSinceIntervalStart);
            AssertApprox(2, timer.TimeSinceIntervalEnded);
        }

        [Test]
        public void TimeSetters_MoveIntervalStart() {
            var timer = new PassiveTimer(5, _time);

            timer.TimeUntilIntervalEnd = 1;
            AssertApprox(4, timer.TimeSinceIntervalStart);

            timer.TimeSinceIntervalStart = 1;
            AssertApprox(4, timer.TimeUntilIntervalEnd);
        }

        [Test]
        public void DidIntervalEndThisFrame_TrueOnlyOnCrossingFrame() {
            var timer = new PassiveTimer(5, _time);

            _time.Advance(4);
            Assert.That(timer.DidIntervalEndThisFrame, Is.False);

            _time.Advance(1.5f);
            Assert.That(timer.DidIntervalEndThisFrame, Is.True);

            _time.Advance(1);
            Assert.That(timer.DidIntervalEndThisFrame, Is.False);
        }

        [Test]
        public void DidIntervalEndThisFrame_FalseBeforeIntervalStarted() {
            var timer = new PassiveTimer(0, 5, _time);
            _time.Advance(0.1f);
            Assert.That(timer.DidIntervalEndThisFrame, Is.False);
        }

        [Test]
        [Ignore("Bug #10: With a zero interval, the ratio properties divide by zero and return NaN.")]
        public void Ratios_ZeroInterval_AreNotNaN() {
            var timer = new PassiveTimer(0, _time);
            Assert.That(timer.RatioUntilIntervalEnd, Is.EqualTo(0));
            Assert.That(timer.RatioSinceIntervalStart, Is.EqualTo(1));
        }

        #endregion

        #region Pausing

        [Test]
        public void Pause_FreezesTimer() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(2);
            float pausedTime = timer.CurrentTime;

            timer.IsPaused = true;
            _time.Advance(10);

            Assert.That(timer.IsPaused, Is.True);
            AssertApprox(pausedTime, timer.CurrentTime);
            AssertApprox(2, timer.TimeSinceIntervalStart);
            Assert.That(timer.IsIntervalEnded, Is.False);
            Assert.That(timer.DeltaTime, Is.Zero);
        }

        [Test]
        public void Resume_AccumulatesPausedTime() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(2);
            timer.IsPaused = true;
            _time.Advance(10);
            timer.IsPaused = false;

            AssertApprox(10, timer.PausedTime);
            AssertApprox(3, timer.TimeUntilIntervalEnd);

            _time.Advance(3);
            Assert.That(timer.IsIntervalEnded, Is.True);
        }

        [Test]
        public void Pause_SettingTwice_CountsPausedTimeOnce() {
            var timer = new PassiveTimer(5, _time);
            timer.IsPaused = true;
            _time.Advance(1);
            timer.IsPaused = true;
            _time.Advance(1);
            timer.IsPaused = false;
            timer.IsPaused = false;

            AssertApprox(2, timer.PausedTime);
        }

        #endregion

        #region Time Modes

        [Test]
        public void DeltaTime_CustomMode_ReadsSource() {
            var timer = new PassiveTimer(5, _time);
            _time.Advance(0.25f);
            AssertApprox(0.25f, timer.DeltaTime);
        }

        [Test]
        public void TimeSource_SwappedMidInterval_TakesEffect() {
            var timer = new PassiveTimer(5, _time);
            timer.TimeSource = new ManualTimeSource(_time.Time + 10);
            Assert.That(timer.IsIntervalEnded, Is.True);
        }

        [Test]
        public void CustomMode_NullSource_Throws() {
            var timer = new PassiveTimer(5, TimeMode.Custom, initialize: false);
            Assert.Throws<InvalidOperationException>(() => _ = timer.CurrentTimeWithoutPause);
            Assert.Throws<InvalidOperationException>(() => _ = timer.DeltaTime);
        }

        [Test]
        public void BuiltInModes_ReadUnityClocks() {
            var scaled = new PassiveTimer(5, TimeMode.Scaled);
            var unscaled = new PassiveTimer(5, TimeMode.Unscaled);
            var realtime = new PassiveTimer(5, TimeMode.Realtime);

            Assert.That(scaled.CurrentTimeWithoutPause, Is.EqualTo(Time.time));
            Assert.That(unscaled.CurrentTimeWithoutPause, Is.EqualTo(Time.unscaledTime));
            Assert.That(realtime.CurrentTimeWithoutPause, Is.EqualTo(Time.realtimeSinceStartup).Within(0.5f));
            Assert.That(scaled.DeltaTime, Is.EqualTo(Time.deltaTime));
            Assert.That(unscaled.DeltaTime, Is.EqualTo(Time.unscaledDeltaTime));
        }

        [Test]
        public void RealtimeMode_HasNoDeltaTimeOrFrameEnd() {
            var timer = new PassiveTimer(0, 5, TimeMode.Realtime);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = timer.DeltaTime);
            Assert.That(timer.DidIntervalEndThisFrame, Is.False);
        }

        #endregion
    }
}
