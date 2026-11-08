// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.PlayMode {
    /// <remarks>
    /// These live in the PlayMode assembly because MonoBehaviours defined in an Editor-only assembly
    /// can't be added to GameObjects.
    /// </remarks>
    public class UnitTest_Singleton {
        private readonly List<GameObject> _created = new();

        [SetUp]
        public void SetUp() {
            TestSingleton.ResetStaticState();
        }

        [TearDown]
        public void TearDown() {
            foreach (GameObject obj in _created) {
                if (obj) Object.DestroyImmediate(obj);
            }

            _created.Clear();
            TestSingleton.ResetStaticState();
        }

        private TestSingleton CreateSingleton(string name) {
            var obj = new GameObject(name);
            _created.Add(obj);
            return obj.AddComponent<TestSingleton>();
        }

        [Test]
        public void Instance_FindsSceneInstanceAndInitializesOnce() {
            TestSingleton singleton = CreateSingleton("Singleton");

            Assert.That(TestSingleton.Instance, Is.SameAs(singleton));
            Assert.That(TestSingleton.Instance, Is.SameAs(singleton));
            Assert.That(singleton.InitializeCount, Is.EqualTo(1));
        }

        [Test]
        public void Instance_NoInstance_LogsErrorAndReturnsNull() {
            LogAssert.Expect(LogType.Error, new Regex("TestSingleton instance not found"));
            Assert.That(TestSingleton.Instance, Is.Null);
        }

        [Test]
        public void Instance_AfterDestroy_ReturnsDestroyedInstance() {
            TestSingleton singleton = CreateSingleton("Singleton");
            Assert.That(TestSingleton.Instance, Is.SameAs(singleton));

            Object.DestroyImmediate(singleton.gameObject);

            Assert.That(ReferenceEquals(TestSingleton.Instance, singleton), Is.True,
                        "The destroyed instance is returned so callers can still unsubscribe from events.");
        }

        [Test]
        public void ResetStaticState_ForgetsCachedInstance() {
            TestSingleton first = CreateSingleton("First");
            Assert.That(TestSingleton.Instance, Is.SameAs(first));
            Object.DestroyImmediate(first.gameObject);
            TestSingleton second = CreateSingleton("Second");

            TestSingleton.ResetStaticState();

            Assert.That(TestSingleton.Instance, Is.SameAs(second));
        }
    }
}
