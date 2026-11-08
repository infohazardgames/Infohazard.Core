// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Tests.Editor {
    /// <summary>
    /// Base class for EditMode tests that create GameObjects. Everything created with <see cref="Create"/>
    /// is destroyed with DestroyImmediate after each test.
    /// </summary>
    public abstract class UnityObjectTestFixture {
        private readonly List<Object> _createdObjects = new();

        /// <summary>
        /// A suffix that makes object names unique, so root-level name checks don't collide with
        /// objects already in the open scene.
        /// </summary>
        protected string UniqueSuffix { get; private set; }

        [SetUp]
        public void SetUpObjects() {
            UniqueSuffix = Guid.NewGuid().ToString("N");
        }

        [TearDown]
        public void TearDownObjects() {
            for (int i = _createdObjects.Count - 1; i >= 0; i--) {
                if (_createdObjects[i]) Object.DestroyImmediate(_createdObjects[i]);
            }

            _createdObjects.Clear();
        }

        protected GameObject Create(string name, Transform parent = null) {
            var obj = new GameObject(name);
            if (parent) obj.transform.SetParent(parent, false);
            _createdObjects.Add(obj);
            return obj;
        }

        protected T Create<T>(string name, Transform parent = null) where T : Component {
            return Create(name, parent).AddComponent<T>();
        }

        /// <summary>
        /// Track an object created some other way (e.g. Object.Instantiate) so it is destroyed after the test.
        /// </summary>
        protected T Track<T>(T obj) where T : Object {
            _createdObjects.Add(obj);
            return obj;
        }
    }
}
