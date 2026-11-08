// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using Infohazard.Core.Tests.PlayMode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Tests.Editor {
    public enum TestEnum {
        First,
        Second,
        Third,
    }

    [Serializable]
    public class TestNested {
        public int _int;
        public string _string;
    }

    public class TestPropertyHostBase : ScriptableObject {
        [SerializeField]
        private int _baseInt = 7;
    }

    /// <summary>
    /// A ScriptableObject with one serialized field of each property type, for SerializedProperty helper tests.
    /// </summary>
    /// <remarks>
    /// _nested comes first and shares field names with top-level fields, so copying it can't accidentally
    /// overwrite the top-level fields that follow it.
    /// </remarks>
    public class TestPropertyHost : TestPropertyHostBase, ITestInterfaceA, ITestInterfaceB {
        public TestNested _nested = new();
        public int _int;
        public string _string;
        public bool _bool;
        public float _float;
        public Color _color;
        public Object _object;
        public LayerMask _layerMask;
        public TestEnum _enum;
        public Vector2 _vector2;
        public Vector3 _vector3;
        public Vector4 _vector4;
        public Rect _rect;
        public AnimationCurve _curve = new();
        public Bounds _bounds;
        public Gradient _gradient = new();
        public Quaternion _quaternion = Quaternion.identity;
        public Vector2Int _vector2Int;
        public Vector3Int _vector3Int;
        public RectInt _rectInt;
        public BoundsInt _boundsInt;
        public int[] _array = Array.Empty<int>();
        public List<TestNested> _list = new();
    }
}
