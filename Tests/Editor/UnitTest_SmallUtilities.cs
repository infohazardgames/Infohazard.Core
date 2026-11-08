// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_StringUtility {
        [TestCase("camelCase", false, "camel Case")]
        [TestCase("camelCase", true, "Camel Case")]
        [TestCase("PascalCase", false, "Pascal Case")]
        [TestCase("HTMLParser", false, "HTML Parser")]
        [TestCase("parseHTML", false, "parse HTML")]
        [TestCase("lower", false, "lower")]
        [TestCase("", true, "")]
        public void SplitCamelCase(string input, bool capitalizeFirst, string expected) {
            Assert.That(input.SplitCamelCase(capitalizeFirst), Is.EqualTo(expected));
        }

        [Test]
        public void SplitCamelCase_AlreadySplit_IsUnchanged() {
            Assert.That("Already Split".SplitCamelCase(), Is.EqualTo("Already Split"));
        }
    }

    public class UnitTest_TypeUtility {
        private class Outer<T> {
            public class Inner { }
        }

        private class Plain { }

        [TearDown]
        public void TearDown() {
            TypeUtility.ResetStaticState();
        }

        [TestCase(typeof(int), false, "int")]
        [TestCase(typeof(int), true, "Int")]
        [TestCase(typeof(string), false, "string")]
        [TestCase(typeof(object), false, "object")]
        public void GetDisplayName_BuiltInTypes(Type type, bool capitalizeFirst, string expected) {
            Assert.That(type.GetDisplayName(capitalizeFirst: capitalizeFirst), Is.EqualTo(expected));
        }

        [Test]
        public void GetDisplayName_BuiltInWithNamespace_UsesFullName() {
            Assert.That(typeof(int).GetDisplayName(includeNamespace: true), Is.EqualTo("System.Int32"));
        }

        [Test]
        public void GetDisplayName_SimpleType() {
            Assert.That(typeof(Vector3).GetDisplayName(), Is.EqualTo("Vector3"));
            Assert.That(typeof(Vector3).GetDisplayName(includeNamespace: true), Is.EqualTo("UnityEngine.Vector3"));
        }

        [Test]
        public void GetDisplayName_NestedGenericArguments() {
            Assert.That(typeof(Dictionary<string, List<int>>).GetDisplayName(),
                        Is.EqualTo("Dictionary<string, List<int>>"));
            Assert.That(typeof(Dictionary<string, List<int>>).GetDisplayName(includeNamespace: true),
                        Is.EqualTo("System.Collections.Generic.Dictionary<System.String, " +
                                   "System.Collections.Generic.List<System.Int32>>"));
        }

        [Test]
        public void GetDisplayName_OpenGeneric() {
            Assert.That(typeof(List<>).GetDisplayName(), Is.EqualTo("List<T>"));
        }

        [Test]
        public void GetDisplayName_NestedTypeInsideGeneric_DoesNotThrow() {
            Assert.That(typeof(Outer<int>.Inner).GetDisplayName(), Is.EqualTo("Inner"));
        }

        [Test]
        public void GetDisplayName_Null_ReturnsEmpty() {
            Assert.That(((Type) null).GetDisplayName(), Is.Empty);
        }

        [Test]
        public void GetType_FindsTypesAcrossAssemblies() {
            Assert.That(TypeUtility.GetType(typeof(Plain).FullName), Is.EqualTo(typeof(Plain)));
            Assert.That(TypeUtility.GetType("UnityEngine.Vector3"), Is.EqualTo(typeof(Vector3)));
            Assert.That(TypeUtility.GetType("Not.A.Real.Type"), Is.Null);
            Assert.That(TypeUtility.GetType(null), Is.Null);
            Assert.That(TypeUtility.GetType(""), Is.Null);
        }

        [Test]
        public void AllTypes_ContainsKnownTypes() {
            Assert.That(TypeUtility.AllTypes.Contains(typeof(ListQueue<>)), Is.True);
            Assert.That(TypeUtility.AllAssemblies, Does.Contain(typeof(ListQueue<>).Assembly));
        }

        [Test]
        public void ResetStaticState_ClearsAssemblyCache() {
            System.Reflection.Assembly[] first = TypeUtility.AllAssemblies;
            TypeUtility.ResetStaticState();
            Assert.That(TypeUtility.AllAssemblies, Is.Not.SameAs(first));
        }
    }

    public class UnitTest_EnumerableUtility {
        private static bool TryParse(string input, out int value) => int.TryParse(input, out value);

        [Test]
        public void SelectWhere_KeepsSuccessfulSelections() {
            string[] input = { "1", "x", "3" };
            Assert.That(input.SelectWhere<string, int>(TryParse).ToList(), Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void FirstOrDefaultWhere_ReturnsFirstSuccessOrDefault() {
            Assert.That(new[] { "x", "2", "3" }.FirstOrDefaultWhere<string, int>(TryParse), Is.EqualTo(2));
            Assert.That(new[] { "x", "y" }.FirstOrDefaultWhere<string, int>(TryParse), Is.EqualTo(0));
        }

        [Test]
        public void IndexOf_ReadOnlyList() {
            IReadOnlyList<string> list = new[] { "a", null, "b" };
            Assert.That(list.IndexOf("b"), Is.EqualTo(2));
            Assert.That(list.IndexOf(null), Is.EqualTo(1));
            Assert.That(list.IndexOf("z"), Is.EqualTo(-1));
        }
    }

    public class UnitTest_SerializableRange {
        [Test]
        public void End_GetAndSet_KeepStart() {
            var range = new SerializableRange(3, 4);
            Assert.That(range.End, Is.EqualTo(7));

            range.End = 10;
            Assert.That(range.Start, Is.EqualTo(3));
            Assert.That(range.Length, Is.EqualTo(7));

            range.Start = 5;
            Assert.That(range.End, Is.EqualTo(12));
        }
    }

    public class UnitTest_RandomUtility {
        [TestCase(0L, 10L)]
        [TestCase(-50L, -10L)]
        [TestCase(long.MinValue / 2, long.MaxValue / 2)]
        public void NextLong_StaysInRange(long min, long max) {
            var random = new System.Random(1234);
            for (int i = 0; i < 10000; i++) {
                long value = random.NextLong(min, max);
                Assert.That(value, Is.GreaterThanOrEqualTo(min).And.LessThan(max));
            }
        }

        [Test]
        public void NextLong_CoversSmallRange() {
            var random = new System.Random(42);
            var seen = new HashSet<long>();
            for (int i = 0; i < 1000; i++) seen.Add(random.NextLong(0, 5));
            Assert.That(seen, Is.EquivalentTo(new long[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void NextLong_EmptyRange_ReturnsMin() {
            Assert.That(new System.Random(1).NextLong(5, 5), Is.EqualTo(5));
        }

        [Test]
        public void NextLong_MaxLessThanMin_Throws() {
            Assert.Throws<ArgumentOutOfRangeException>(() => new System.Random(1).NextLong(5, 4));
        }

        [Test]
        public void NextUlong_IsDeterministicForSeed() {
            ulong a = new System.Random(99).NextUlong();
            ulong b = new System.Random(99).NextUlong();
            Assert.That(a, Is.EqualTo(b));
        }
    }

    public class UnitTest_ComponentIDEquality {
        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown() {
            foreach (Object obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        private ComponentID CreateID(string childName) {
            var root = new GameObject("Root");
            var child = new GameObject(childName);
            _created.Add(root);
            child.transform.SetParent(root.transform);
            return new ComponentID(root.transform, child.transform);
        }

        [Test]
        public void Constructor_StoresPathAndTypeName() {
            ComponentID id = CreateID("Child");
            Assert.That(id.Path, Is.EqualTo("Child"));
            Assert.That(id.Type, Is.EqualTo(typeof(Transform).FullName));
            Assert.That(id.ToString(), Is.EqualTo("Child:UnityEngine.Transform"));
        }

        [Test]
        public void Equality_ComparesPathAndType() {
            ComponentID a = CreateID("Child");
            ComponentID b = CreateID("Child");
            ComponentID c = CreateID("Other");

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.Equals((object) b), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a.Equals(c), Is.False);
            Assert.That(a.Equals("not an id"), Is.False);
        }

        [Test]
        public void Default_HasNullFieldsAndIsEqualToDefault() {
            ComponentID a = default;
            Assert.That(a.Equals(default(ComponentID)), Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(default(ComponentID).GetHashCode()));
            Assert.That(a.ToString(), Is.EqualTo(":"));
        }
    }
}
