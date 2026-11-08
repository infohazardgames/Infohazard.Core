// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.PlayMode {
    public class UnitTest_UniqueNamedObject : PlayModeTestFixture {
        private UniqueNameListEntry _entry;
        private List<string> _events;

        [SetUp]
        public void SetUp() {
            UniqueNamedObject.ResetStaticState();
            _entry = Track(ScriptableObject.CreateInstance<UniqueNameListEntry>());
            _entry.name = "TestUniqueName";
            _events = new List<string>();
            UniqueNamedObject.ObjectAdded += obj => _events.Add($"added {obj.name}");
            UniqueNamedObject.ObjectRemoved += obj => _events.Add($"removed {obj.name}");
        }

        [TearDown]
        public void TearDown() {
            UniqueNamedObject.ResetStaticState();
        }

        private UniqueNamedObject CreateNamed(string objectName, bool removeOnDisable = true,
                                              UniqueNameListEntry entry = null) {
            GameObject obj = Create(objectName, active: false);
            var named = obj.AddComponent<UniqueNamedObject>();
            SetField(named, "_uniqueName", entry ? entry : _entry);
            SetField(named, "_removeOnDisable", removeOnDisable);
            obj.SetActive(true);
            return named;
        }

        [Test]
        public void Enable_RegistersObject() {
            UniqueNamedObject named = CreateNamed("A");

            Assert.That(named.UniqueName, Is.EqualTo("TestUniqueName"));
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out GameObject found), Is.True);
            Assert.That(found, Is.SameAs(named.gameObject));
            Assert.That(UniqueNamedObject.TryGetObject(_entry, out GameObject byEntry), Is.True);
            Assert.That(byEntry, Is.SameAs(named.gameObject));
            Assert.That(UniqueNamedObject.Objects["TestUniqueName"], Is.SameAs(named));
            Assert.That(_events, Is.EqualTo(new[] { "added A" }));
        }

        [Test]
        public void Disable_UnregistersObject() {
            UniqueNamedObject named = CreateNamed("A");
            named.gameObject.SetActive(false);

            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.False);
            Assert.That(_events, Is.EqualTo(new[] { "added A", "removed A" }));
        }

        [Test]
        public void RemoveOnDisableFalse_StaysRegisteredUntilDestroyed() {
            UniqueNamedObject named = CreateNamed("A", removeOnDisable: false);
            named.gameObject.SetActive(false);
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.True);

            Object.DestroyImmediate(named.gameObject);
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.False);
        }

        [Test]
        public void DuplicateName_LogsErrorAndKeepsFirst() {
            UniqueNamedObject first = CreateNamed("A");

            LogAssert.Expect(LogType.Error, new Regex("Object B trying to use unique name TestUniqueName"));
            CreateNamed("B");

            UniqueNamedObject.TryGetObject("TestUniqueName", out GameObject found);
            Assert.That(found, Is.SameAs(first.gameObject));
        }

        [Test]
        public void DuplicateName_DisablingRejectedObject_LeavesFirstRegistered() {
            UniqueNamedObject first = CreateNamed("A");
            LogAssert.Expect(LogType.Error, new Regex("already in use"));
            UniqueNamedObject second = CreateNamed("B");

            second.gameObject.SetActive(false);

            UniqueNamedObject.TryGetObject("TestUniqueName", out GameObject found);
            Assert.That(found, Is.SameAs(first.gameObject));
        }

        [Test]
        public void DuplicateName_DisablingBothObjects_DoesNotThrow() {
            UniqueNamedObject first = CreateNamed("A");
            LogAssert.Expect(LogType.Error, new Regex("already in use"));
            UniqueNamedObject second = CreateNamed("B");

            first.gameObject.SetActive(false);
            second.gameObject.SetActive(false);

            LogAssert.NoUnexpectedReceived();
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.False);
        }

        [Test]
        public void TryGetObject_UnknownOrNullEntry_ReturnsFalse() {
            Assert.That(UniqueNamedObject.TryGetObject("Unknown", out GameObject byName), Is.False);
            Assert.That(byName, Is.Null);
            Assert.That(UniqueNamedObject.TryGetObject((UniqueNameListEntry) null, out GameObject byEntry), Is.False);
            Assert.That(byEntry, Is.Null);
        }

        [Test]
        public void NoEntryAssigned_DoesNotRegister() {
            GameObject obj = Create("NoName", active: false);
            var named = obj.AddComponent<UniqueNamedObject>();
            obj.SetActive(true);

            Assert.That(named.UniqueName, Is.Null);
            Assert.That(UniqueNamedObject.Objects, Is.Empty);
        }

        [Test]
        public void RemoveOnDisable_ChangedWhileEnabled_KeepsRegistration() {
            UniqueNamedObject named = CreateNamed("A");
            named.RemoveOnDisable = false;
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.True);

            named.gameObject.SetActive(false);
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.True,
                        "With RemoveOnDisable off, disabling should not unregister.");
        }

        [Test]
        public void RemoveOnDisable_ChangedWhileDisabled_UpdatesRegistration() {
            UniqueNamedObject named = CreateNamed("A");
            named.gameObject.SetActive(false);

            named.RemoveOnDisable = false;
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.True,
                        "An object that no longer removes itself on disable should be registered while disabled.");

            named.RemoveOnDisable = true;
            Assert.That(UniqueNamedObject.TryGetObject("TestUniqueName", out _), Is.False,
                        "An object that removes itself on disable should not be registered while disabled.");
        }
    }
}
