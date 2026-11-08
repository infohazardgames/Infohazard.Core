// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Tests.PlayMode {
    /// <summary>
    /// Base class for PlayMode tests. Destroys every tracked object after each test, and removes the
    /// <see cref="PoolManager"/> (and everything pooled under it) so tests don't share pools.
    /// </summary>
    public abstract class PlayModeTestFixture {
        private readonly List<Object> _tracked = new();

        [SetUp]
        public void SetUpFixture() {
            DestroyPoolManager();
        }

        [TearDown]
        public void TearDownFixture() {
            for (int i = _tracked.Count - 1; i >= 0; i--) {
                if (_tracked[i]) Object.DestroyImmediate(_tracked[i]);
            }

            _tracked.Clear();

            // Spawned instances are often moved to the scene root, outside anything tracked.
            DestroyAll<Spawnable>();
            DestroyAll<SpawnRecorder>();
            DestroyAll<TimeToLive>();
            DestroyPoolManager();
        }

        private static void DestroyAll<T>() where T : Component {
            foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Include,
                                                                FindObjectsSortMode.None)) {
                if (component) Object.DestroyImmediate(component.gameObject);
            }
        }

        private static void DestroyPoolManager() {
            foreach (PoolManager manager in Object.FindObjectsByType<PoolManager>(FindObjectsInactive.Include,
                                                                                  FindObjectsSortMode.None)) {
                Object.DestroyImmediate(manager.gameObject);
            }

            PoolManager.ResetStaticState();
        }

        /// <summary>
        /// Create a GameObject that is destroyed after the test. Pass active: false to add components
        /// and set their serialized fields before Awake/OnEnable run.
        /// </summary>
        protected GameObject Create(string name, bool active = true, Transform parent = null) {
            var obj = new GameObject(name);
            obj.SetActive(active);
            if (parent) obj.transform.SetParent(parent, false);
            _tracked.Add(obj);
            return obj;
        }

        /// <summary>
        /// Create an inactive "prefab" GameObject with a Spawnable component.
        /// </summary>
        protected Spawnable CreateSpawnablePrefab(string name = "Prefab", bool pooled = true) {
            GameObject obj = Create(name, active: false);
            Spawnable spawnable = obj.AddComponent<Spawnable>();
            if (!pooled) SetField(spawnable, "_pooled", false);
            return spawnable;
        }

        protected T Track<T>(T obj) where T : Object {
            _tracked.Add(obj);
            return obj;
        }

        /// <summary>
        /// Set a private (usually serialized) field, searching base classes.
        /// </summary>
        protected static void SetField(object target, string fieldName, object value) {
            for (Type type = target.GetType(); type != null; type = type.BaseType) {
                FieldInfo field = type.GetField(fieldName,
                                                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) continue;
                field.SetValue(target, value);
                return;
            }

            throw new ArgumentException($"Field {fieldName} not found on {target.GetType()}.");
        }

        /// <summary>
        /// Unity-aware null check (a destroyed object compares equal to null).
        /// </summary>
        protected static void AssertDestroyed(Object obj, string message = null) {
            Assert.That(obj == null, Is.True, message ?? $"Expected {obj} to be destroyed.");
        }
    }
}
