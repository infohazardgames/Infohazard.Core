// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Complex = System.Numerics.Complex;

namespace Infohazard.Core.Tests.Editor {
    /// <summary>
    /// Shared assertion helpers for Infohazard.Core tests.
    /// </summary>
    public static class TestUtil {
        public const float DefaultTolerance = 1e-4f;

        public static void AssertApprox(float expected, float actual, float tolerance = DefaultTolerance) {
            Assert.That(actual, Is.EqualTo(expected).Within(tolerance));
        }

        public static void AssertApprox(Vector2 expected, Vector2 actual, float tolerance = DefaultTolerance) {
            Assert.That((actual - expected).magnitude, Is.LessThanOrEqualTo(tolerance),
                        $"Expected {expected:F5} but was {actual:F5}");
        }

        public static void AssertApprox(Vector3 expected, Vector3 actual, float tolerance = DefaultTolerance) {
            Assert.That((actual - expected).magnitude, Is.LessThanOrEqualTo(tolerance),
                        $"Expected {expected:F5} but was {actual:F5}");
        }

        /// <summary>
        /// Assert that two collections of complex numbers contain the same values, in any order.
        /// </summary>
        public static void AssertSameRoots(IEnumerable<Complex> expected, IEnumerable<Complex> actual,
                                           double tolerance) {
            List<Complex> actualList = actual.ToList();
            List<Complex> remaining = actualList.ToList();
            List<Complex> expectedList = expected.ToList();
            Assert.That(remaining.Count, Is.EqualTo(expectedList.Count), "Root count mismatch.");

            foreach (Complex root in expectedList) {
                int index = remaining.FindIndex(r => Complex.Abs(r - root) <= tolerance);
                Assert.That(index, Is.GreaterThanOrEqualTo(0),
                            $"Expected root {root} not found in [{string.Join(", ", actualList)}].");
                remaining.RemoveAt(index);
            }
        }
    }
}
