// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

#nullable enable

using System;
using System.Collections.Generic;

namespace Infohazard.Core {
    /// <summary>
    /// Generic handle struct that can be used to manage the lifetime of an object.
    /// Handle producers should provide a single-use ID and dispose action,
    /// enabling safe double-dispose and object reuse.
    /// </summary>
    /// <typeparam name="T">Type of object the handle points to.</typeparam>
    public readonly struct Handle<T> : IDisposable, IEquatable<Handle<T>> {
        /// <summary>
        /// The object being wrapped by the handle.
        /// If the handle is no longer valid, this will throw an InvalidOperationException.
        /// </summary>
        public T Value => _isValidFunc?.Invoke(_id) is not false
            ? _value
            : throw new InvalidOperationException("Handle is no longer valid.");

        /// <summary>
        /// Check if the handle is still valid.
        /// If a validity check function was not provided, this will always return true.
        /// </summary>
        public bool IsValid => _isValidFunc?.Invoke(_id) is not false;

        private readonly T _value;
        private readonly Action<long>? _disposeAction;
        private readonly Func<long, bool>? _isValidFunc;
        private readonly long _id;

        /// <summary>
        /// Create a new handle for an object.
        /// </summary>
        /// <param name="value">The data being wrapped.</param>
        /// <param name="disposeAction">
        /// Action to invoke in order to release/dispose the handle. Ideally, the caller should cache this as an
        /// <c>Action&lt;long&gt;</c> to avoid allocations.
        /// </param>
        /// <param name="id">Identifier to pass to the dispose action.</param>
        public Handle(T value, Action<long>? disposeAction = null, long id = 0) {
            _value = value;
            _disposeAction = disposeAction;
            _isValidFunc = null;
            _id = id;
        }

        /// <summary>
        /// Create a new handle for an object with a validity check.
        /// </summary>
        /// <param name="value">The data being wrapped.</param>
        /// <param name="isValidFunc">
        /// Function to check if the handle is still valid. Will be invoked every time the handle is used to access
        /// the underlying value. Ideally, the caller should cache this as a <c>Func&lt;long, bool&gt;</c> to avoid
        /// allocations.
        /// </param>
        /// <param name="disposeAction">
        /// Action to invoke in order to release/dispose the handle. Ideally, the caller should cache this as an
        /// <c>Action&lt;long&gt;</c> to avoid allocations.
        /// </param>
        /// <param name="id">Identifier to pass to the dispose action.</param>
        public Handle(T value, Func<long, bool>? isValidFunc, Action<long>? disposeAction, long id) {
            _value = value;
            _disposeAction = disposeAction;
            _isValidFunc = isValidFunc;
            _id = id;
        }

        /// <summary>
        /// Dispose the handle, invoking the dispose action if provided.
        /// The handle itself does not guard against double-dispose; the callback passed to it must do so.
        /// </summary>
        public void Dispose() {
            _disposeAction?.Invoke(_id);
        }

        /// <summary>
        /// Check if this handle is equal to another handle.
        /// </summary>
        /// <param name="other">The other handle to compare against.</param>
        /// <returns>True if the handles are equal, false otherwise.</returns>
        public bool Equals(Handle<T> other) {
            return EqualityComparer<T>.Default.Equals(_value, other._value) &&
                   Equals(_isValidFunc, other._isValidFunc) &&
                   Equals(_disposeAction, other._disposeAction) &&
                   _id == other._id;
        }

        /// <summary>
        /// Check if this handle is equal to another object.
        /// </summary>
        /// <param name="obj">The other object to compare against.</param>
        /// <returns>True if the object is a handle and is equal to this handle, false otherwise.</returns>
        public override bool Equals(object? obj) {
            return obj is Handle<T> other && Equals(other);
        }

        /// <summary>
        /// Get the hash code for this handle.
        /// </summary>
        /// <returns>The hash code for this handle.</returns>
        public override int GetHashCode() {
            return HashCode.Combine(_value, _isValidFunc, _disposeAction, _id);
        }

        /// <summary>
        /// Check if two handles are equal.
        /// </summary>
        /// <param name="left">First handle to check.</param>
        /// <param name="right">Second handle to check.</param>
        /// <returns>True if the handles are equal, false otherwise.</returns>
        public static bool operator ==(Handle<T> left, Handle<T> right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Check if two handles are not equal.
        /// </summary>
        /// <param name="left">First handle to check.</param>
        /// <param name="right">Second handle to check.</param>
        /// <returns>True if the handles are not equal, false otherwise.</returns>
        public static bool operator !=(Handle<T> left, Handle<T> right) {
            return !left.Equals(right);
        }
    }
}
