// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    [Serializable]
    public struct SerializableRange : IEquatable<SerializableRange> {
        [SerializeField]
        private int _start;

        [SerializeField]
        private int _length;

        public int Start {
            get => _start;
            set => _start = value;
        }

        public int Length {
            get => _length;
            set => _length = value;
        }

        public int End {
            get => _start + _length;
            set => _length = value - _start;
        }

        public SerializableRange(int start, int length) {
            _start = start;
            _length = length;
        }

        public override string ToString() {
            return $"[{Start}, {End}] (length = {_length})";
        }

        public bool Equals(SerializableRange other) {
            return _start == other._start && _length == other._length;
        }

        public override bool Equals(object obj) {
            return obj is SerializableRange other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_start, _length);
        }

        public static bool operator ==(SerializableRange left, SerializableRange right) {
            return left.Equals(right);
        }

        public static bool operator !=(SerializableRange left, SerializableRange right) {
            return !left.Equals(right);
        }
    }
}
