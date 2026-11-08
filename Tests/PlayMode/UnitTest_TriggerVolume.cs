// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.PlayMode {
    public class UnitTest_TriggerVolume : PlayModeTestFixture {
        private static readonly Vector3 Outside = new(20, 0, 0);

        private TriggerVolume _volume;
        private List<string> _events;

        [SetUp]
        public void SetUp() {
            GameObject volumeObject = Create("Volume");
            var trigger = volumeObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = Vector3.one * 2;
            _volume = volumeObject.AddComponent<TriggerVolume>();

            _events = new List<string>();
            // Occupants may already be destroyed when the volume is disabled during teardown.
            _volume.TriggerEntered += obj => _events.Add($"enter {NameOf(obj)}");
            _volume.TriggerExited += obj => _events.Add($"exit {NameOf(obj)}");
            _volume.AllExited += _ => _events.Add("all exited");
        }

        private static string NameOf(GameObject obj) => obj ? obj.name : "<destroyed>";

        [Test]
        public void DestroyingControlledColliderBeforeVolume_DoesNotThrow() {
            var controlled = Create("Controlled").AddComponent<BoxCollider>();
            SetField(_volume, "_controlledColliders", new Collider[] { controlled });

            Object.DestroyImmediate(controlled.gameObject);
            _volume.enabled = false;
            _volume.enabled = true;

            LogAssert.NoUnexpectedReceived();
        }

        private GameObject CreateOccupant(string name, string tag = Tag.Player) {
            GameObject obj = Create(name);
            obj.tag = tag;
            obj.transform.position = Outside;
            obj.AddComponent<BoxCollider>().size = Vector3.one * 0.5f;
            var rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            return obj;
        }

        private static async Task MoveAndSimulate(GameObject obj, Vector3 position) {
            obj.transform.position = position;
            Physics.SyncTransforms();
            await Awaitable.FixedUpdateAsync();
            await Awaitable.FixedUpdateAsync();
            await Awaitable.NextFrameAsync();
        }

        [Test]
        public async Task Enter_TaggedObject_RaisesEnteredAndTracksOccupant() {
            GameObject player = CreateOccupant("Player1");

            await MoveAndSimulate(player, Vector3.zero);

            Assert.That(_events, Is.EqualTo(new[] { "enter Player1" }));
            Assert.That(_volume.Occupants, Does.Contain(player));
        }

        [Test]
        public async Task Enter_ObjectNotMatchingTagFilter_IsIgnored() {
            GameObject other = CreateOccupant("Other", Tag.Untagged);

            await MoveAndSimulate(other, Vector3.zero);

            Assert.That(_events, Is.Empty);
            Assert.That(_volume.Occupants, Is.Empty);
        }

        [Test]
        public async Task Exit_LastOccupant_RaisesExitedAndAllExited() {
            GameObject player = CreateOccupant("Player1");
            await MoveAndSimulate(player, Vector3.zero);

            await MoveAndSimulate(player, Outside);

            Assert.That(_events, Is.EqualTo(new[] { "enter Player1", "exit Player1", "all exited" }));
            Assert.That(_volume.Occupants, Is.Empty);
        }

        [Test]
        public async Task Exit_WithOtherOccupantsRemaining_DoesNotRaiseAllExited() {
            GameObject a = CreateOccupant("A");
            GameObject b = CreateOccupant("B");
            await MoveAndSimulate(a, Vector3.zero);
            await MoveAndSimulate(b, new Vector3(0.5f, 0, 0));

            await MoveAndSimulate(a, Outside);

            Assert.That(_events, Is.EqualTo(new[] { "enter A", "enter B", "exit A" }));
            Assert.That(_volume.Occupants, Is.EquivalentTo(new[] { b }));
        }

        [Test]
        public async Task ObjectWithTwoColliders_IsCountedOnce() {
            GameObject player = CreateOccupant("Player1");
            player.AddComponent<BoxCollider>().center = new Vector3(0, 0.2f, 0);

            await MoveAndSimulate(player, Vector3.zero);
            Assert.That(_events, Is.EqualTo(new[] { "enter Player1" }));

            await MoveAndSimulate(player, Outside);
            Assert.That(_events.Count(e => e == "exit Player1"), Is.EqualTo(1));
        }

        [Test]
        public async Task DeactivatingOccupant_RaisesExit() {
            GameObject player = CreateOccupant("Player1");
            await MoveAndSimulate(player, Vector3.zero);

            player.SetActive(false);
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            Assert.That(_events, Is.EqualTo(new[] { "enter Player1", "exit Player1", "all exited" }));
            Assert.That(_volume.Occupants, Is.Empty);
        }

        [Test]
        public async Task DisablingVolume_ExitsOccupantsAndTogglesControlledColliders() {
            var controlled = Create("Controlled").AddComponent<BoxCollider>();
            SetField(_volume, "_controlledColliders", new Collider[] { controlled });
            GameObject player = CreateOccupant("Player1");
            await MoveAndSimulate(player, Vector3.zero);

            _volume.enabled = false;

            Assert.That(controlled.enabled, Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "enter Player1", "exit Player1", "all exited" }));
            Assert.That(_volume.Occupants, Is.Empty);

            _volume.enabled = true;
            Assert.That(controlled.enabled, Is.True);
            Assert.That(_volume.ControlledColliders, Is.EqualTo(new[] { controlled }));
        }

        [Test]
        public async Task UnityEvents_AreInvoked() {
            int enters = 0, exits = 0, allExits = 0;
            _volume.Events.OnTriggerEnter.AddListener(() => enters++);
            _volume.Events.OnTriggerExit.AddListener(() => exits++);
            _volume.Events.OnAllExit.AddListener(() => allExits++);
            GameObject player = CreateOccupant("Player1");

            await MoveAndSimulate(player, Vector3.zero);
            await MoveAndSimulate(player, Outside);

            Assert.That((enters, exits, allExits), Is.EqualTo((1, 1, 1)));
        }
    }

    public class UnitTest_ColliderCast : PlayModeTestFixture {
        private Collider _target;

        [SetUp]
        public void SetUp() {
            GameObject target = Create("Target");
            target.transform.position = new Vector3(0, 0, 5);
            _target = target.AddComponent<BoxCollider>();
        }

        private T CreateCaster<T>() where T : Collider {
            var caster = Create("Caster").AddComponent<T>();
            Physics.SyncTransforms();
            return caster;
        }

        [Test]
        public void BoxCollider_HitsTarget() {
            var caster = CreateCaster<BoxCollider>();
            Assert.That(caster.ColliderCast(0, Vector3.forward, out RaycastHit hit, 10), Is.True);
            Assert.That(hit.collider, Is.SameAs(_target));
            Assert.That(hit.distance, Is.EqualTo(4).Within(0.05f));
        }

        [Test]
        public void SphereCollider_HitsTarget() {
            var caster = CreateCaster<SphereCollider>();
            Assert.That(caster.ColliderCast(0, Vector3.forward, out RaycastHit hit, 10), Is.True);
            Assert.That(hit.collider, Is.SameAs(_target));
            Assert.That(hit.distance, Is.EqualTo(4).Within(0.05f));
        }

        [Test]
        public void CapsuleCollider_HitsTarget() {
            var caster = CreateCaster<CapsuleCollider>();
            Assert.That(caster.ColliderCast(0, Vector3.forward, out RaycastHit hit, 10), Is.True);
            Assert.That(hit.collider, Is.SameAs(_target));
            Assert.That(hit.distance, Is.EqualTo(4).Within(0.05f));
        }

        [Test]
        public void Padding_ShrinksCastShape() {
            var caster = CreateCaster<BoxCollider>();
            caster.ColliderCast(0.25f, Vector3.forward, out RaycastHit hit, 10);
            Assert.That(hit.distance, Is.EqualTo(4.25f).Within(0.05f));
        }

        [Test]
        public void MaxDistance_TooShort_Misses() {
            var caster = CreateCaster<SphereCollider>();
            Assert.That(caster.ColliderCast(0, Vector3.forward, out _, 2), Is.False);
        }

        [Test]
        public void NonAlloc_ReturnsHits() {
            var hits = new RaycastHit[4];
            foreach (Collider caster in new Collider[] {
                         CreateCaster<BoxCollider>(), CreateCaster<SphereCollider>(), CreateCaster<CapsuleCollider>(),
                     }) {
                int count = caster.ColliderCastNonAlloc(0, Vector3.forward, hits, 10);
                Assert.That(hits.Take(count).Select(h => h.collider), Does.Contain(_target), caster.GetType().Name);
            }
        }
    }
}
