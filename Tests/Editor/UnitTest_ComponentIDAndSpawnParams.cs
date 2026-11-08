// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Infohazard.Core.Tests.Editor.TestUtil;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_ComponentIDGet : UnityObjectTestFixture {
        private static ComponentID FromFields(string path, string type) {
            return JsonUtility.FromJson<ComponentID>($"{{\"_path\":\"{path}\",\"_type\":\"{type}\"}}");
        }

        [Test]
        public void Get_RoundTripsComponentOnDescendant() {
            GameObject root = Create("Root");
            GameObject child = Create("Child", root.transform);
            BoxCollider target = Create<BoxCollider>("Target", child.transform);

            var id = new ComponentID(root.transform, target);

            Assert.That(id.Get<BoxCollider>(root.transform), Is.SameAs(target));
            Assert.That(id.Get<Collider>(root.transform), Is.SameAs(target), "Base types are allowed.");
        }

        [Test]
        public void Get_ComponentOnRoot_RoundTrips() {
            BoxCollider target = Create<BoxCollider>("Root");
            var id = new ComponentID(target.transform, target);
            Assert.That(id.Get<BoxCollider>(target.transform), Is.SameAs(target));
        }

        [Test]
        public void Get_WorksOnACopyOfTheHierarchy() {
            GameObject root = Create("Root");
            BoxCollider target = Create<BoxCollider>("Target", root.transform);
            var id = new ComponentID(root.transform, target);

            GameObject copy = Track(Object.Instantiate(root));

            BoxCollider copied = id.Get<BoxCollider>(copy.transform);
            Assert.That(copied, Is.Not.Null);
            Assert.That(copied, Is.Not.SameAs(target));
            Assert.That(copied.transform.parent, Is.SameAs(copy.transform));
        }

        [Test]
        public void Get_NotAssignableType_LogsErrorAndReturnsNull() {
            GameObject root = Create("Root");
            BoxCollider target = Create<BoxCollider>("Target", root.transform);
            var id = new ComponentID(root.transform, target);

            LogAssert.Expect(LogType.Error, new Regex("is not assignable to given type"));
            Assert.That(id.Get<Rigidbody>(root.transform), Is.Null);
        }

        [Test]
        public void Get_UnknownTypeName_LogsErrorAndReturnsNull() {
            GameObject root = Create("Root");
            ComponentID id = FromFields("", "Not.A.Real.Type");

            LogAssert.Expect(LogType.Error, new Regex("has invalid type Not.A.Real.Type"));
            Assert.That(id.Get<Component>(root.transform), Is.Null);
        }

        [Test]
        public void Get_MissingPathOrComponent_ReturnsNull() {
            GameObject root = Create("Root");
            Create("Child", root.transform);

            Assert.That(FromFields("Missing", typeof(BoxCollider).FullName).Get<BoxCollider>(root.transform),
                        Is.Null);
            Assert.That(FromFields("Child", typeof(BoxCollider).FullName).Get<BoxCollider>(root.transform),
                        Is.Null);
        }
    }

    public class UnitTest_SpawnParams : UnityObjectTestFixture {
        [Test]
        public void At_Unparented_CopiesWorldPose() {
            GameObject target = Create("Target");
            target.transform.SetPositionAndRotation(new Vector3(1, 2, 3), Quaternion.Euler(0, 90, 0));

            SpawnParams spawnParams = SpawnParams.At(target.transform);

            AssertApprox(new Vector3(1, 2, 3), spawnParams.Position.Value);
            Assert.That(Quaternion.Angle(spawnParams.Rotation.Value, target.transform.rotation), Is.LessThan(0.01f));
            Assert.That(spawnParams.Parent, Is.Null);
            Assert.That(spawnParams.Scene, Is.Null);
        }

        [Test]
        public void At_Parented_UsesIdentityLocalPose() {
            GameObject target = Create("Target");
            target.transform.position = new Vector3(1, 2, 3);

            SpawnParams spawnParams = SpawnParams.At(target.transform, parented: true);

            Assert.That(spawnParams.Position, Is.EqualTo(Vector3.zero));
            Assert.That(spawnParams.Rotation, Is.EqualTo(Quaternion.identity));
            Assert.That(spawnParams.Parent, Is.SameAs(target.transform));
        }

        [Test]
        public void At_IncludeScene_UsesTargetScene() {
            GameObject target = Create("Target");
            SpawnParams spawnParams = SpawnParams.At(target.transform, includeScene: true);
            Assert.That(spawnParams.Scene, Is.EqualTo(target.scene));
        }
    }
}
