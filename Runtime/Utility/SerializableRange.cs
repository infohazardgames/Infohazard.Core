// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    [Serializable]
    public struct SerializableRange {
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
    }
}
