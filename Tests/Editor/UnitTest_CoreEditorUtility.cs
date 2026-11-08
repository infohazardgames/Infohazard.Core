// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Infohazard.Core.Editor;
using Infohazard.Core.Tests.PlayMode;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_SerializedPropertyHelpers {
        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown() {
            foreach (Object obj in _created) {
                if (obj) Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        private TestPropertyHost CreateHost() {
            var host = ScriptableObject.CreateInstance<TestPropertyHost>();
            _created.Add(host);
            return host;
        }

        private static SerializedProperty Prop(TestPropertyHost host, string path) {
            SerializedProperty property = new SerializedObject(host).FindProperty(path);
            Assert.That(property, Is.Not.Null, $"Property {path} not found.");
            return property;
        }

        #region GetValue

        private static readonly object[] GetValueCases = {
            new object[] { nameof(TestPropertyHost._int), 42 },
            new object[] { nameof(TestPropertyHost._bool), true },
            new object[] { nameof(TestPropertyHost._float), 1.5f },
            new object[] { nameof(TestPropertyHost._string), "hello" },
            new object[] { nameof(TestPropertyHost._color), Color.red },
            new object[] { nameof(TestPropertyHost._vector2), new Vector2(1, 2) },
            new object[] { nameof(TestPropertyHost._vector3), new Vector3(1, 2, 3) },
            new object[] { nameof(TestPropertyHost._vector4), new Vector4(1, 2, 3, 4) },
            new object[] { nameof(TestPropertyHost._rect), new Rect(1, 2, 3, 4) },
            new object[] { nameof(TestPropertyHost._bounds), new Bounds(Vector3.one, Vector3.one * 2) },
            new object[] { nameof(TestPropertyHost._quaternion), Quaternion.Euler(0, 90, 0) },
            new object[] { nameof(TestPropertyHost._vector2Int), new Vector2Int(1, 2) },
            new object[] { nameof(TestPropertyHost._vector3Int), new Vector3Int(1, 2, 3) },
            new object[] { nameof(TestPropertyHost._rectInt), new RectInt(1, 2, 3, 4) },
            new object[] { nameof(TestPropertyHost._boundsInt), new BoundsInt(1, 2, 3, 4, 5, 6) },
        };

        [TestCaseSource(nameof(GetValueCases))]
        public void GetValue_ReturnsFieldValue(string fieldName, object value) {
            TestPropertyHost host = CreateHost();
            typeof(TestPropertyHost).GetField(fieldName).SetValue(host, value);

            Assert.That(Prop(host, fieldName).GetValue(), Is.EqualTo(value));
        }

        [Test]
        public void GetValue_SpecialTypes() {
            TestPropertyHost host = CreateHost();
            host._object = host;
            host._layerMask = 1 << 3;
            host._enum = TestEnum.Third;
            host._array = new[] { 1, 2, 3 };
            host._curve = AnimationCurve.Linear(0, 0, 1, 1);

            Assert.That(Prop(host, "_object").GetValue(), Is.SameAs(host));
            Assert.That(Prop(host, "_layerMask").GetValue(), Is.EqualTo((LayerMask) (1 << 3)));
            Assert.That(Prop(host, "_enum").GetValue(), Is.EqualTo((int) TestEnum.Third));
            Assert.That(Prop(host, "_array.Array.size").GetValue(), Is.EqualTo(3));
            Assert.That(((AnimationCurve) Prop(host, "_curve").GetValue()).keys.Length, Is.EqualTo(2));
        }

        [Test]
        public void GetValue_UnsupportedTypes_ReturnNull() {
            TestPropertyHost host = CreateHost();
            Assert.That(Prop(host, "_nested").GetValue(), Is.Null);
            Assert.That(Prop(host, "_gradient").GetValue(), Is.Null);
        }

        #endregion

        #region FindValue and FindFieldInfo

        [Test]
        public void FindValue_TopLevelNestedAndInherited() {
            TestPropertyHost host = CreateHost();
            host._int = 5;
            host._nested._string = "nested";

            Assert.That(Prop(host, "_int").FindValue<int>(), Is.EqualTo(5));
            Assert.That(Prop(host, "_nested._string").FindValue<string>(), Is.EqualTo("nested"));
            Assert.That(Prop(host, "_nested").FindValue(), Is.SameAs(host._nested));
            Assert.That(Prop(host, "_baseInt").FindValue<int>(), Is.EqualTo(7), "Inherited private field.");
        }

        [Test]
        public void FindValue_ArrayAndListElements() {
            TestPropertyHost host = CreateHost();
            host._array = new[] { 10, 20, 30 };
            host._list = new List<TestNested> { new() { _int = 1 }, new() { _int = 2 } };

            Assert.That(Prop(host, "_array.Array.data[1]").FindValue<int>(), Is.EqualTo(20));
            Assert.That(Prop(host, "_list.Array.data[1]").FindValue(), Is.SameAs(host._list[1]));
            Assert.That(Prop(host, "_list.Array.data[1]._int").FindValue<int>(), Is.EqualTo(2));
        }

        [Test]
        public void FindFieldInfo_ReturnsDeclaringField() {
            TestPropertyHost host = CreateHost();
            host._list = new List<TestNested> { new() };

            Assert.That(Prop(host, "_int").FindFieldInfo(), Is.EqualTo(typeof(TestPropertyHost).GetField("_int")));
            Assert.That(Prop(host, "_nested._string").FindFieldInfo(),
                        Is.EqualTo(typeof(TestNested).GetField("_string")));
            Assert.That(Prop(host, "_list.Array.data[0]._int").FindFieldInfo(),
                        Is.EqualTo(typeof(TestNested).GetField("_int")));
            Assert.That(Prop(host, "_baseInt").FindFieldInfo(),
                        Is.EqualTo(typeof(TestPropertyHostBase).GetField(
                                       "_baseInt", BindingFlags.Instance | BindingFlags.NonPublic)));
        }

        [Test]
        public void FindFieldInfo_ArrayElement_ReturnsNull() {
            TestPropertyHost host = CreateHost();
            host._array = new[] { 1 };
            Assert.That(Prop(host, "_array.Array.data[0]").FindFieldInfo(), Is.Null);
        }

        #endregion

        #region GetTypeName

        [Test]
        public void GetTypeName_StripsObjectReferenceWrapper() {
            TestPropertyHost host = CreateHost();
            Assert.That(Prop(host, "_object").GetTypeName(), Is.EqualTo("Object"));
            Assert.That(Prop(host, "_int").GetTypeName(), Is.EqualTo("int"));
            Assert.That(Prop(host, "_nested").GetTypeName(), Is.EqualTo(nameof(TestNested)));
        }

        #endregion

        #region CopyFrom

        [Test]
        public void CopyFrom_SimpleProperties() {
            TestPropertyHost source = CreateHost();
            source._int = 3;
            source._string = "copied";
            source._vector3 = Vector3.one;
            source._enum = TestEnum.Second;
            source._object = source;
            TestPropertyHost destination = CreateHost();

            var destObject = new SerializedObject(destination);
            var sourceObject = new SerializedObject(source);
            foreach (string field in new[] { "_int", "_string", "_vector3", "_enum", "_object" }) {
                destObject.FindProperty(field).CopyFrom(sourceObject.FindProperty(field));
            }

            destObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(destination._int, Is.EqualTo(3));
            Assert.That(destination._string, Is.EqualTo("copied"));
            Assert.That(destination._vector3, Is.EqualTo(Vector3.one));
            Assert.That(destination._enum, Is.EqualTo(TestEnum.Second));
            Assert.That(destination._object, Is.SameAs(source));
        }

        [Test]
        public void CopyFrom_CurveAndGradient_AreDeepCopies() {
            TestPropertyHost source = CreateHost();
            source._curve = AnimationCurve.Linear(0, 0, 1, 2);
            source._gradient.SetKeys(new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.blue, 1) },
                                     new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0.5f, 1) });
            TestPropertyHost destination = CreateHost();

            var destObject = new SerializedObject(destination);
            var sourceObject = new SerializedObject(source);
            destObject.FindProperty("_curve").CopyFrom(sourceObject.FindProperty("_curve"));
            destObject.FindProperty("_gradient").CopyFrom(sourceObject.FindProperty("_gradient"));
            destObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(destination._curve.Evaluate(1), Is.EqualTo(2).Within(1e-4f));
            Assert.That(destination._curve, Is.Not.SameAs(source._curve));
            Assert.That(destination._gradient.Evaluate(0), Is.EqualTo(Color.red));
            Assert.That(destination._gradient.alphaKeys[1].alpha, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void CopyFrom_Array_CopiesElementsAndLeavesSourceUnchanged() {
            TestPropertyHost source = CreateHost();
            source._array = new[] { 1, 2, 3 };
            TestPropertyHost destination = CreateHost();

            var destObject = new SerializedObject(destination);
            var sourceObject = new SerializedObject(source);
            destObject.FindProperty("_array").CopyFrom(sourceObject.FindProperty("_array"));
            destObject.ApplyModifiedPropertiesWithoutUndo();
            sourceObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(destination._array, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(source._array, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void CopyFrom_NestedObject_CopiesOnlyItsChildren() {
            TestPropertyHost source = CreateHost();
            source._nested._int = 5;
            source._nested._string = "nested";
            TestPropertyHost destination = CreateHost();
            destination._int = 100;
            destination._string = "top-level";

            var destObject = new SerializedObject(destination);
            destObject.FindProperty("_nested").CopyFrom(new SerializedObject(source).FindProperty("_nested"));
            destObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(destination._nested._int, Is.EqualTo(5));
            Assert.That(destination._nested._string, Is.EqualTo("nested"));
            Assert.That(destination._int, Is.EqualTo(100), "Top-level fields after the copied property must not change.");
            Assert.That(destination._string, Is.EqualTo("top-level"));
        }

        [Test]
        public void CopyFrom_SerializedObject_CopiesMatchingFieldsIntoProperty() {
            TestPropertyHost source = CreateHost();
            source._int = 9;
            source._string = "from object";
            TestPropertyHost destination = CreateHost();
            destination._int = 100;
            destination._string = "top-level";

            var destObject = new SerializedObject(destination);
            destObject.FindProperty("_nested").CopyFrom(new SerializedObject(source));
            destObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(destination._nested._int, Is.EqualTo(9));
            Assert.That(destination._nested._string, Is.EqualTo("from object"));
            Assert.That(destination._int, Is.EqualTo(100), "Top-level fields after the copied property must not change.");
        }

        [Test]
        public void CopyFrom_MismatchedTypes_LogsError() {
            TestPropertyHost host = CreateHost();
            var serializedObject = new SerializedObject(host);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "SerializedProperty types do not match.");
            serializedObject.FindProperty("_int").CopyFrom(serializedObject.FindProperty("_string"));
        }

        #endregion
    }

    public class UnitTest_EditorPathHelpers {
        [TestCase("Assets/Resources/Foo/Bar.asset", "Foo/Bar")]
        [TestCase("Assets/Resources/Bar.prefab", "Bar")]
        [TestCase("Assets/A/Resources/B/Resources/C.prefab", "C")]
        [TestCase("Packages/com.example/Resources/Data.asset", "Data")]
        [TestCase("Assets/Data/Bar.asset", null)]
        public void GetResourcePath_ReturnsPathInsideResourcesFolder(string path, string expected) {
            Assert.That(CoreEditorUtility.GetResourcePath(path), Is.EqualTo(expected));
        }

        [TestCase("Assets/MyResources/Bar.asset")]
        [TestCase("Assets/NotResources/Foo/Bar.asset")]
        public void GetResourcePath_FolderEndingInResources_ReturnsNull(string path) {
            Assert.That(CoreEditorUtility.GetResourcePath(path), Is.Null);
        }

        [TestCase("Assets/Foo/Bar.asset", "Foo/Bar.asset")]
        [TestCase("/Foo/Bar.asset", "Foo/Bar.asset")]
        [TestCase("Foo/Bar.asset", "Foo/Bar.asset")]
        [TestCase("Assets\\Foo\\Bar.asset", "Foo/Bar.asset")]
        [TestCase("Assets", "")]
        public void GetPathRelativeToAssetsFolder(string path, string expected) {
            Assert.That(CoreEditorUtility.GetPathRelativeToAssetsFolder(path), Is.EqualTo(expected));
        }

        [Test]
        public void GetPathRelativeToAssetsFolder_AbsolutePath() {
            string absolute = Application.dataPath + "/Foo/Bar.asset";
            Assert.That(CoreEditorUtility.GetPathRelativeToAssetsFolder(absolute), Is.EqualTo("Foo/Bar.asset"));
            Assert.That(CoreEditorUtility.GetPathRelativeToAssetsFolder(absolute.Replace('/', '\\')),
                        Is.EqualTo("Foo/Bar.asset"));
        }

        [TestCase("AssetsExtra/Foo.asset", "AssetsExtra/Foo.asset")]
        public void GetPathRelativeToAssetsFolder_FolderStartingWithAssets_IsUnchanged(string path, string expected) {
            Assert.That(CoreEditorUtility.GetPathRelativeToAssetsFolder(path), Is.EqualTo(expected));
        }
    }

    public class UnitTest_ObjectValidators : UnityObjectTestFixture {
        private readonly List<Object> _assets = new();

        [TearDown]
        public void TearDown() {
            foreach (Object obj in _assets) {
                if (obj) Object.DestroyImmediate(obj);
            }

            _assets.Clear();
        }

        private static Func<Object, Object> Validator(params Type[] interfaces) =>
            ObjectValidators.MustImplement(interfaces);

        [Test]
        public void ObjectImplementingInterfaces_IsReturned() {
            var asset = ScriptableObject.CreateInstance<TestPropertyHost>();
            _assets.Add(asset);

            Assert.That(Validator(typeof(ITestInterfaceA))(asset), Is.SameAs(asset));
            Assert.That(Validator(typeof(ITestInterfaceA), typeof(ITestInterfaceB))(asset), Is.SameAs(asset));
        }

        [Test]
        public void ObjectNotImplementingInterface_ReturnsNull() {
            var asset = ScriptableObject.CreateInstance<UniqueNameListEntry>();
            _assets.Add(asset);
            Assert.That(Validator(typeof(ITestInterfaceA))(asset), Is.Null);
        }

        [Test]
        public void GameObject_ReturnsImplementingComponent() {
            GameObject obj = Create("Obj");
            var component = obj.AddComponent<SingleInterfaceComponent>();

            Assert.That(Validator(typeof(ITestInterfaceA))(obj), Is.SameAs(component));
            Assert.That(Validator(typeof(IEnumerable))(obj), Is.SameAs(obj.transform));
        }

        [Test]
        public void GameObject_WithoutImplementingComponent_ReturnsNull() {
            GameObject obj = Create("Obj");
            Assert.That(Validator(typeof(ITestInterfaceA))(obj), Is.Null);
        }

        [Test]
        public void GameObject_MultipleInterfacesOnOneComponent_ReturnsComponent() {
            GameObject obj = Create("Obj");
            var component = obj.AddComponent<MultiInterfaceComponent>();

            Assert.That(Validator(typeof(ITestInterfaceA), typeof(ITestInterfaceB))(obj), Is.SameAs(component));
        }

        [Test]
        public void GameObject_InterfacesSplitAcrossComponents_ReturnsComponentImplementingAll() {
            GameObject obj = Create("Obj");
            obj.AddComponent<SingleInterfaceComponent>();
            var both = obj.AddComponent<MultiInterfaceComponent>();

            Assert.That(Validator(typeof(ITestInterfaceA), typeof(ITestInterfaceB))(obj), Is.SameAs(both));
        }

        [Test]
        public void GameObject_NoSingleComponentImplementsAll_ReturnsNull() {
            GameObject obj = Create("Obj");
            obj.AddComponent<SingleInterfaceComponent>();

            Assert.That(Validator(typeof(ITestInterfaceA), typeof(ITestInterfaceB))(obj), Is.Null);
        }
    }
}
