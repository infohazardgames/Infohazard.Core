// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using NUnit.Framework;
using UnityEngine;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_Pause {
        private float _savedTimeScale;
        private int _pausedCount;
        private int _resumedCount;

        [SetUp]
        public void SetUp() {
            _savedTimeScale = Time.timeScale;
            Pause.ResetStaticState();
            Time.timeScale = 1;
            _pausedCount = 0;
            _resumedCount = 0;
            Pause.GamePaused += () => _pausedCount++;
            Pause.GameResumed += () => _resumedCount++;
        }

        [TearDown]
        public void TearDown() {
            Pause.ResetStaticState();
            Time.timeScale = _savedTimeScale;
        }

        [Test]
        public void Paused_StopsTimeAndRaisesEvent() {
            Pause.Paused = true;
            Assert.That(Pause.Paused, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(_pausedCount, Is.EqualTo(1));
        }

        [Test]
        public void Unpause_RestoresPreviousTimeScaleAndRaisesEvent() {
            Time.timeScale = 0.5f;
            Pause.Paused = true;
            Pause.Paused = false;

            Assert.That(Time.timeScale, Is.EqualTo(0.5f));
            Assert.That(_resumedCount, Is.EqualTo(1));
        }

        [Test]
        public void SettingSameValue_RaisesNoEvent() {
            Pause.Paused = false;
            Pause.Paused = true;
            Pause.Paused = true;
            Assert.That(_pausedCount, Is.EqualTo(1));
            Assert.That(_resumedCount, Is.Zero);
        }

        [Test]
        public void TimeScale_WhileNotPaused_AppliesImmediately() {
            Pause.TimeScale = 0.25f;
            Assert.That(Time.timeScale, Is.EqualTo(0.25f));
            Assert.That(Pause.TimeScale, Is.EqualTo(0.25f));
        }

        [Test]
        public void TimeScale_WhilePaused_AppliesOnResume() {
            Pause.Paused = true;
            Pause.TimeScale = 0.25f;
            Assert.That(Time.timeScale, Is.Zero);

            Pause.Paused = false;
            Assert.That(Time.timeScale, Is.EqualTo(0.25f));
        }

        [Test]
        public void ResetStaticState_UnpausesAndClearsSubscribers() {
            Pause.Paused = true;
            Pause.ResetStaticState();

            Assert.That(Pause.Paused, Is.False);
            Assert.That(Pause.TimeScale, Is.EqualTo(1));
            Pause.Paused = true;
            Assert.That(_pausedCount, Is.EqualTo(1), "Subscribers from before the reset should be removed.");
        }
    }
}
