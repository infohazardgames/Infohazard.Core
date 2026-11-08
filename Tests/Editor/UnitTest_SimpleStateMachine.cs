// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using Infohazard.Core.StateMachine;
using NUnit.Framework;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_SimpleStateMachine {
        private enum TestState {
            None,
            A,
            B,
            C,
        }

        private List<string> _log;

        [SetUp]
        public void SetUp() {
            _log = new List<string>();
        }

        private StateMachineState LoggingState(string name) {
            return new StateMachineState {
                OnEnter = () => _log.Add($"enter {name}"),
                OnExit = () => _log.Add($"exit {name}"),
                OnUpdate = () => _log.Add($"update {name}"),
            };
        }

        private SimpleStateMachine<TestState> CreateMachine() {
            var machine = new SimpleStateMachine<TestState>();
            machine.AddState(TestState.A, LoggingState("A"));
            machine.AddState(TestState.B, LoggingState("B"));
            machine.AddState(TestState.C, LoggingState("C"));
            return machine;
        }

        private StateMachineTransition Transition(Func<bool> condition, string name = null) {
            return new StateMachineTransition {
                Condition = condition,
                OnTransition = name != null ? () => _log.Add($"transition {name}") : null,
            };
        }

        [Test]
        public void SetCurrentStateKey_ExitsOldThenEntersNew() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.CurrentStateKey = TestState.A;
            machine.CurrentStateKey = TestState.B;

            Assert.That(_log, Is.EqualTo(new[] { "enter A", "exit A", "enter B" }));
            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.B));
        }

        [Test]
        public void SetCurrentStateKey_SameKey_DoesNothing() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.CurrentStateKey = TestState.A;
            machine.CurrentStateKey = TestState.A;
            Assert.That(_log, Is.EqualTo(new[] { "enter A" }));
        }

        [Test]
        public void SetCurrentStateKey_UnknownKey_ClearsStateObject() {
            var machine = new SimpleStateMachine<TestState>();
            machine.AddState(TestState.A, LoggingState("A"));
            machine.CurrentStateKey = TestState.A;

            machine.CurrentStateKey = TestState.B;

            Assert.That(machine.CurrentStateObject, Is.Null);
            Assert.That(_log, Is.EqualTo(new[] { "enter A", "exit A" }));
        }

        [Test]
        public void Update_CallsCurrentStateUpdate() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.CurrentStateKey = TestState.A;
            _log.Clear();
            machine.Update();
            Assert.That(_log, Is.EqualTo(new[] { "update A" }));
        }

        [Test]
        public void Update_FirstTrueTransitionWins() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.AddTransition(TestState.A, TestState.B, Transition(() => false, "AB"));
            machine.AddTransition(TestState.A, TestState.C, Transition(() => true, "AC1"));
            machine.AddTransition(TestState.A, TestState.B, Transition(() => true, "AB2"));
            machine.CurrentStateKey = TestState.A;
            _log.Clear();

            machine.Update();

            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.C));
            Assert.That(_log, Is.EqualTo(new[] { "update A", "transition AC1", "exit A", "enter C" }));
        }

        [Test]
        public void Update_NoTrueCondition_StaysInState() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.AddTransition(TestState.A, TestState.B, Transition(() => false));
            machine.CurrentStateKey = TestState.A;
            machine.Update();
            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.A));
        }

        [Test]
        public void Update_StateChangedDuringUpdate_SkipsTransitions() {
            var machine = new SimpleStateMachine<TestState>();
            var stateA = new StateMachineState();
            stateA.OnUpdate = () => machine.CurrentStateKey = TestState.B;
            machine.AddState(TestState.A, stateA);
            machine.AddState(TestState.B, new StateMachineState());
            machine.AddState(TestState.C, new StateMachineState());
            machine.AddTransition(TestState.B, TestState.C, Transition(() => true));
            machine.CurrentStateKey = TestState.A;

            machine.Update();

            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.B));
        }

        [Test]
        public void Update_NoCurrentState_DoesNothing() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            Assert.DoesNotThrow(() => machine.Update());
            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void AddTransition_UnknownState_Throws() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            Assert.Throws<ArgumentException>(
                () => machine.AddTransition(TestState.A, TestState.None, Transition(() => true)));
            Assert.Throws<ArgumentException>(
                () => machine.AddTransition(TestState.None, TestState.A, Transition(() => true)));
        }

        [Test]
        public void AddTransition_Bidirectional_AddsReverse() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            machine.AddTransition(TestState.A, TestState.B, Transition(() => true), isBidirectional: true);
            machine.CurrentStateKey = TestState.A;

            machine.Update();
            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.B));
            machine.Update();
            Assert.That(machine.CurrentStateKey, Is.EqualTo(TestState.A));
        }

        [Test]
        public void AddState_DuplicateKey_Throws() {
            SimpleStateMachine<TestState> machine = CreateMachine();
            Assert.Throws<ArgumentException>(() => machine.AddState(TestState.A, new StateMachineState()));
        }

        [Test]
        [Ignore("Bug #9: Update with a reference-type key before any state is set throws ArgumentNullException.")]
        public void Update_ReferenceKeyBeforeStateSet_DoesNotThrow() {
            var machine = new SimpleStateMachine<string>();
            machine.AddState("a", new StateMachineState());
            Assert.DoesNotThrow(() => machine.Update());
        }

        [Test]
        [Ignore("Bug #9: The default(T) state can never be entered because setting it is a no-op.")]
        public void SetCurrentStateKey_DefaultKey_EntersState() {
            var machine = new SimpleStateMachine<TestState>();
            machine.AddState(TestState.None, LoggingState("None"));
            machine.CurrentStateKey = TestState.None;
            Assert.That(_log, Is.EqualTo(new[] { "enter None" }));
        }
    }
}
