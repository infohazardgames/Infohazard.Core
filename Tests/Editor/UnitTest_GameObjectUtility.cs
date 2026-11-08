// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Infohazard.Core.Tests.Editor.TestUtil;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_GameObjectUtility : UnityObjectTestFixture {
        #region Transform Paths

        [Test]
        public void RelativePath_DeepHierarchy_RoundTrips() {
            GameObject root = Create("Root");
            GameObject a = Create("A", root.transform);
            GameObject b = Create("B", a.transform);
            GameObject c = Create("C", b.transform);

            string path = root.transform.GetRelativeTransformPath(c.transform);

            Assert.That(path, Is.EqualTo("A/B/C"));
            Assert.That(root.transform.GetTransformAtRelativePath(path), Is.SameAs(c.transform));
        }

        [TestCase("with space")]
        [TestCase("with/slash")]
        [TestCase("with+plus")]
        [TestCase("ünïcödé")]
        [TestCase("mixed /+% chars")]
        public void RelativePath_SpecialCharacters_RoundTrip(string name) {
            GameObject root = Create("Root");
            GameObject parent = Create("Parent", root.transform);
            GameObject child = Create(name, parent.transform);

            string path = root.transform.GetRelativeTransformPath(child.transform);

            Assert.That(path.Split('/').Length, Is.EqualTo(2), "Names must be escaped so they add no separators.");
            Assert.That(root.transform.GetTransformAtRelativePath(path), Is.SameAs(child.transform));
        }

        [Test]
        public void RelativePath_TargetNotUnderRoot_LogsErrorAndReturnsNull() {
            GameObject root = Create("Root");
            GameObject other = Create("Other");

            LogAssert.Expect(LogType.Error, new Regex("GetRelativeTransformPath did not find parent Root"));
            Assert.That(root.transform.GetRelativeTransformPath(other.transform), Is.Null);
        }

        [Test]
        public void RelativePath_SameTransform_RoundTripsToRoot() {
            GameObject root = Create("Root");
            string path = root.transform.GetRelativeTransformPath(root.transform);

            Assert.That(path, Is.Empty);
            Assert.That(root.transform.GetTransformAtRelativePath(path), Is.SameAs(root.transform));
        }

        [Test]
        public void GetTransformAtRelativePath_MissingChild_ReturnsNull() {
            GameObject root = Create("Root");
            Create("A", root.transform);
            Assert.That(root.transform.GetTransformAtRelativePath("A/Missing"), Is.Null);
            Assert.That(root.transform.GetTransformAtRelativePath("Missing"), Is.Null);
        }

        [Test]
        public void GetTransformAtRelativePath_DuplicateNames_ReturnsFirst() {
            GameObject root = Create("Root");
            GameObject first = Create("Dup", root.transform);
            Create("Dup", root.transform);
            Assert.That(root.transform.GetTransformAtRelativePath("Dup"), Is.SameAs(first.transform));
        }

        #endregion

        #region Name Uniqueness

        [Test]
        public void IsNameUniqueInSiblings_ChildWithDuplicateSibling_ReturnsFalse() {
            GameObject root = Create("Root");
            GameObject a = Create("Same", root.transform);
            Create("Same", root.transform);
            GameObject unique = Create("Unique", root.transform);

            Assert.That(a.transform.IsNameUniqueInSiblings(), Is.False);
            Assert.That(unique.transform.IsNameUniqueInSiblings(), Is.True);
        }

        [Test]
        public void IsNameUniqueInSiblings_RootObjects_ChecksSceneRoots() {
            GameObject a = Create($"Root_{UniqueSuffix}");
            Assert.That(a.transform.IsNameUniqueInSiblings(), Is.True);

            Create($"Root_{UniqueSuffix}");
            Assert.That(a.transform.IsNameUniqueInSiblings(), Is.False);
        }

        [Test]
        public void IsPathUnique_ChecksEveryLevelBelowRoot() {
            GameObject root = Create("Root");
            GameObject a = Create("A", root.transform);
            GameObject b = Create("B", a.transform);
            Assert.That(root.transform.IsPathUnique(b.transform), Is.True);

            Create("A", root.transform);
            Assert.That(root.transform.IsPathUnique(b.transform), Is.False);
        }

        #endregion

        #region Transform Initialization

        [Test]
        public void SetParentAndReset_ParentsAndResetsLocalTransform() {
            GameObject parent = Create("Parent");
            parent.transform.position = new Vector3(5, 0, 0);
            GameObject child = Create("Child");
            child.transform.position = new Vector3(1, 2, 3);
            child.transform.localScale = Vector3.one * 2;
            child.transform.rotation = Quaternion.Euler(10, 20, 30);

            child.transform.SetParentAndReset(parent.transform);

            Assert.That(child.transform.parent, Is.SameAs(parent.transform));
            Assert.That(child.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(child.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(child.transform.localScale, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void Initialize_LocalSpace_SetsLocalValues() {
            GameObject parent = Create("Parent");
            parent.transform.position = new Vector3(10, 0, 0);
            GameObject child = Create("Child");

            child.transform.Initialize(parent.transform, new Vector3(1, 2, 3), Quaternion.Euler(0, 90, 0),
                                       Vector3.one * 2);

            Assert.That(child.transform.parent, Is.SameAs(parent.transform));
            AssertApprox(new Vector3(1, 2, 3), child.transform.localPosition);
            AssertApprox(new Vector3(11, 2, 3), child.transform.position);
            Assert.That(Quaternion.Angle(child.transform.localRotation, Quaternion.Euler(0, 90, 0)), Is.LessThan(0.01f));
            AssertApprox(Vector3.one * 2, child.transform.localScale);
        }

        [Test]
        public void Initialize_WorldSpace_SetsWorldPosition() {
            GameObject parent = Create("Parent");
            parent.transform.position = new Vector3(10, 0, 0);
            GameObject child = Create("Child");

            child.transform.Initialize(parent.transform, new Vector3(1, 2, 3), inWorldSpace: true);

            Assert.That(child.transform.parent, Is.SameAs(parent.transform));
            AssertApprox(new Vector3(1, 2, 3), child.transform.position);
            AssertApprox(new Vector3(-9, 2, 3), child.transform.localPosition);
        }

        [Test]
        public void Initialize_NullValues_KeepExistingValues() {
            GameObject obj = Create("Obj");
            obj.transform.localPosition = new Vector3(1, 2, 3);
            obj.transform.localScale = new Vector3(4, 5, 6);

            obj.transform.Initialize(rotation: Quaternion.Euler(0, 45, 0));

            AssertApprox(new Vector3(1, 2, 3), obj.transform.localPosition);
            AssertApprox(new Vector3(4, 5, 6), obj.transform.localScale);
            Assert.That(Quaternion.Angle(obj.transform.localRotation, Quaternion.Euler(0, 45, 0)), Is.LessThan(0.01f));
        }

        [Test]
        public void Initialize_SpawnParams_AppliesAllValues() {
            GameObject parent = Create("Parent");
            GameObject obj = Create("Obj");

            obj.transform.Initialize(new SpawnParams {
                Parent = parent.transform,
                Position = new Vector3(1, 0, 0),
                Scale = Vector3.one * 3,
            });

            Assert.That(obj.transform.parent, Is.SameAs(parent.transform));
            AssertApprox(new Vector3(1, 0, 0), obj.transform.localPosition);
            AssertApprox(Vector3.one * 3, obj.transform.localScale);
        }

        #endregion

        #region Hierarchy Helpers

        [Test]
        public void DestroyChildrenImmediate_RemovesAllChildren() {
            GameObject root = Create("Root");
            Create("A", root.transform);
            Create("B", root.transform);

            root.transform.DestroyChildrenImmediate();

            Assert.That(root.transform.childCount, Is.Zero);
        }

        [Test]
        public void SetLayerRecursively_SetsAllDescendants() {
            GameObject root = Create("Root");
            GameObject a = Create("A", root.transform);
            GameObject b = Create("B", a.transform);

            root.SetLayerRecursively(5);

            Assert.That(root.layer, Is.EqualTo(5));
            Assert.That(a.layer, Is.EqualTo(5));
            Assert.That(b.layer, Is.EqualTo(5));
        }

        [Test]
        public void TryGetComponentInParent_FindsAncestorComponent() {
            BoxCollider parentCollider = Create<BoxCollider>("Parent");
            GameObject child = Create("Child", parentCollider.transform);

            Assert.That(child.TryGetComponentInParent(out BoxCollider found), Is.True);
            Assert.That(found, Is.SameAs(parentCollider));
            Assert.That(child.transform.TryGetComponentInParent(out BoxCollider fromComponent), Is.True);
            Assert.That(fromComponent, Is.SameAs(parentCollider));
            Assert.That(child.TryGetComponentInParent(out SphereCollider missing), Is.False);
            Assert.That(missing, Is.Null);
        }

        [Test]
        public void TryGetComponentInChildren_RespectsIncludeInactive() {
            GameObject root = Create("Root");
            BoxCollider childCollider = Create<BoxCollider>("Child", root.transform);
            childCollider.gameObject.SetActive(false);

            Assert.That(root.TryGetComponentInChildren(out BoxCollider _), Is.False);
            Assert.That(root.TryGetComponentInChildren(out BoxCollider found, includeInactive: true), Is.True);
            Assert.That(found, Is.SameAs(childCollider));

            childCollider.gameObject.SetActive(true);
            Assert.That(root.transform.TryGetComponentInChildren(out BoxCollider fromComponent), Is.True);
            Assert.That(fromComponent, Is.SameAs(childCollider));
        }

        #endregion

        #region Capsules

        [TestCase(0, 1f, 0f, 0f)]
        [TestCase(1, 0f, 1f, 0f)]
        [TestCase(2, 0f, 0f, 1f)]
        public void GetCapsuleInfo_Direction_PlacesPointsAlongAxis(int direction, float x, float y, float z) {
            GameObjectUtility.GetCapsuleInfo(0.5f, 3, Vector3.zero, direction, Matrix4x4.identity,
                                             out Vector3 point1, out Vector3 point2, out float radius,
                                             out float height);
            var axis = new Vector3(x, y, z);
            AssertApprox(axis, point1);
            AssertApprox(-axis, point2);
            AssertApprox(0.5f, radius);
            AssertApprox(3, height);
        }

        [Test]
        public void GetCapsuleInfo_NonUniformScale_UsesAxisAndLargestOtherScale() {
            Matrix4x4 matrix = Matrix4x4.Scale(new Vector3(2, 3, 1));
            GameObjectUtility.GetCapsuleInfo(0.5f, 2, Vector3.zero, 1, matrix,
                                             out Vector3 point1, out Vector3 point2, out float radius,
                                             out float height);
            AssertApprox(1, radius);
            AssertApprox(6, height);
            AssertApprox(new Vector3(0, 2, 0), point1);
            AssertApprox(new Vector3(0, -2, 0), point2);
        }

        [Test]
        public void GetCapsuleInfo_HeightBelowDiameter_ClampsToSphere() {
            GameObjectUtility.GetCapsuleInfo(1, 1, new Vector3(0, 1, 0), 1, Matrix4x4.identity,
                                             out Vector3 point1, out Vector3 point2, out _, out float height);
            AssertApprox(2, height);
            AssertApprox(new Vector3(0, 1, 0), point1);
            AssertApprox(new Vector3(0, 1, 0), point2);
        }

        [Test]
        public void GetCapsuleInfo_TranslatedAndRotated_TransformsCenterAndAxis() {
            Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(10, 0, 0), Quaternion.Euler(0, 0, 90), Vector3.one);
            GameObjectUtility.GetCapsuleInfo(0.5f, 3, new Vector3(0, 1, 0), 1, matrix,
                                             out Vector3 point1, out Vector3 point2, out _, out _);
            // Local up maps to world -X, and the local center (0, 1, 0) maps to (9, 0, 0).
            AssertApprox(new Vector3(8, 0, 0), point1);
            AssertApprox(new Vector3(10, 0, 0), point2);
        }

        [Test]
        public void GetCapsuleInfo_CapsuleCollider_UsesColliderSettings() {
            CapsuleCollider capsule = Create<CapsuleCollider>("Capsule");
            capsule.transform.position = new Vector3(0, 5, 0);
            capsule.radius = 0.5f;
            capsule.height = 3;
            capsule.direction = 2;

            capsule.GetCapsuleInfo(out Vector3 point1, out Vector3 point2, out float radius, out float height);

            AssertApprox(new Vector3(0, 5, 1), point1);
            AssertApprox(new Vector3(0, 5, -1), point2);
            AssertApprox(0.5f, radius);
            AssertApprox(3, height);
        }

        [Test]
        public void GetCapsuleInfo_CharacterController_UsesVerticalAxis() {
            CharacterController controller = Create<CharacterController>("Controller");
            controller.radius = 0.5f;
            controller.height = 3;
            controller.center = Vector3.zero;

            controller.GetCapsuleInfo(out Vector3 point1, out Vector3 point2, out _, out _);

            AssertApprox(new Vector3(0, 1, 0), point1);
            AssertApprox(new Vector3(0, -1, 0), point2);
        }

        [Test]
        public void ColliderCast_UnsupportedCollider_Throws() {
            MeshCollider meshCollider = Create<MeshCollider>("Mesh");
            Assert.Throws<ArgumentException>(() => meshCollider.ColliderCast(0, Vector3.forward, out _));
            Assert.Throws<ArgumentException>(
                () => meshCollider.ColliderCastNonAlloc(0, Vector3.forward, new RaycastHit[1]));
        }

        #endregion

        #region Rigidbody

        [Test]
        public void LinearVelocityHelpers_ReadAndWriteVelocity() {
            Rigidbody rb = Create<Rigidbody>("Body");

            rb.SetLinearVelocity(new Vector3(1, 2, 3));
            AssertApprox(new Vector3(1, 2, 3), rb.GetLinearVelocity());

            rb.AddLinearVelocity(new Vector3(1, 0, 0));
            AssertApprox(new Vector3(2, 2, 3), rb.GetLinearVelocity());

            rb.MoveTowardsLinearVelocity(new Vector3(2, 2, 13), 4);
            AssertApprox(new Vector3(2, 2, 7), rb.GetLinearVelocity());
        }

        #endregion
    }
}
