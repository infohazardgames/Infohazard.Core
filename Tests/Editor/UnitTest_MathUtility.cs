// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Complex = System.Numerics.Complex;
using static Infohazard.Core.Tests.Editor.TestUtil;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_MathUtility {
        #region Float Operations

        [TestCase(7.4f, 5f, 5f)]
        [TestCase(7.6f, 5f, 10f)]
        [TestCase(-7.6f, 5f, -10f)]
        [TestCase(0.26f, 0.25f, 0.25f)]
        public void RoundToNearest_Float(float value, float factor, float expected) {
            AssertApprox(expected, MathUtility.RoundToNearest(value, factor));
        }

        [Test]
        public void RoundToNearest_Vector() {
            AssertApprox(new Vector3(5, 10, -5), MathUtility.RoundToNearest(new Vector3(6, 8, -4), 5));
        }

        [TestCase(3f, 1f)]
        [TestCase(-0.1f, -1f)]
        [TestCase(0f, 0f)]
        [TestCase(-0f, 0f)]
        public void SignZero(float value, float expected) {
            Assert.That(MathUtility.SignZero(value), Is.EqualTo(expected));
        }

        #endregion

        #region Polynomial Solvers

        private static IEnumerable<Complex> Roots(params Complex[] roots) => roots;

        private static Complex EvaluatePolynomial(Complex x, params Complex[] coefficients) {
            Complex result = 0;
            foreach (Complex c in coefficients) result = result * x + c;
            return result;
        }

        [Test]
        public void ComplexCubeRoot_EachRootCubedEqualsInput() {
            var input = new Complex(8, 3);
            var (r1, r2, r3) = MathUtility.ComplexCubeRoot(input);
            foreach (Complex r in new[] { r1, r2, r3 }) {
                Assert.That(Complex.Abs(r * r * r - input), Is.LessThan(1e-9));
            }
        }

        [Test]
        public void SolveQuadratic_RealRoots() {
            var (r1, r2) = MathUtility.SolveQuadratic(1, -3, 2);
            AssertSameRoots(Roots(1, 2), Roots(r1, r2), 1e-9);
        }

        [Test]
        public void SolveQuadratic_ComplexRoots() {
            var (r1, r2) = MathUtility.SolveQuadratic(1, 0, 1);
            AssertSameRoots(Roots(Complex.ImaginaryOne, -Complex.ImaginaryOne), Roots(r1, r2), 1e-9);
        }

        [Test]
        public void SolveQuadratic_NonUnitLeadingCoefficient() {
            var (r1, r2) = MathUtility.SolveQuadratic(2, 0, -8);
            AssertSameRoots(Roots(2, -2), Roots(r1, r2), 1e-9);
        }

        // Coefficients are for (x - roots...), highest power first.
        private static readonly object[] CubicCases = {
            new object[] { new Complex[] { 1, -6, 11, -6 }, new Complex[] { 1, 2, 3 } },
            new object[] { new Complex[] { 1, -2, 1, -2 }, new Complex[] { 2, Complex.ImaginaryOne, -Complex.ImaginaryOne } },
            new object[] { new Complex[] { 1, -4, 5, -2 }, new Complex[] { 1, 1, 2 } },
            new object[] { new Complex[] { 2, -12, 22, -12 }, new Complex[] { 1, 2, 3 } },
        };

        [TestCaseSource(nameof(CubicCases))]
        public void SolveCubic_FindsKnownRoots(Complex[] coefficients, Complex[] expectedRoots) {
            var (r1, r2, r3) = MathUtility.SolveCubic(coefficients[0], coefficients[1], coefficients[2],
                                                      coefficients[3]);
            AssertSameRoots(expectedRoots, Roots(r1, r2, r3), 1e-4);
            foreach (Complex r in new[] { r1, r2, r3 }) {
                Assert.That(Complex.Abs(EvaluatePolynomial(r, coefficients)), Is.LessThan(1e-6));
            }
        }

        [Test]
        public void SolveCubic_TripleRoot() {
            var (r1, r2, r3) = MathUtility.SolveCubic(1, -3, 3, -1);
            AssertSameRoots(Roots(1, 1, 1), Roots(r1, r2, r3), 1e-4);
        }

        private static readonly object[] QuarticCases = {
            // Roots symmetric about their mean make the depressed quartic biquadratic (B = 0).
            new object[] { new Complex[] { 1, -10, 35, -50, 24 }, new Complex[] { 1, 2, 3, 4 } },
            new object[] { new Complex[] { 1, -11, 41, -61, 30 }, new Complex[] { 1, 2, 3, 5 } },
            new object[] {
                new Complex[] { 1, -3, 3, -3, 2 },
                new Complex[] { 1, 2, Complex.ImaginaryOne, -Complex.ImaginaryOne },
            },
            new object[] { new Complex[] { 2, -20, 70, -100, 48 }, new Complex[] { 1, 2, 3, 4 } },
        };

        [TestCaseSource(nameof(QuarticCases))]
        public void SolveQuartic_FindsKnownRoots(Complex[] coefficients, Complex[] expectedRoots) {
            var (r1, r2, r3, r4) = MathUtility.SolveQuartic(coefficients[0], coefficients[1], coefficients[2],
                                                            coefficients[3], coefficients[4]);
            AssertSameRoots(expectedRoots, Roots(r1, r2, r3, r4), 1e-3);
        }

        #endregion

        #region Angle Operations

        [TestCase(-720f, 0f)]
        [TestCase(-1f, 359f)]
        [TestCase(0f, 0f)]
        [TestCase(360f, 0f)]
        [TestCase(725f, 5f)]
        public void NormalizeAngle(float angle, float expected) {
            AssertApprox(expected, MathUtility.NormalizeAngle(angle));
        }

        [TestCase(180f, 180f)]
        [TestCase(181f, -179f)]
        [TestCase(-180f, 180f)]
        [TestCase(-190f, 170f)]
        [TestCase(370f, 10f)]
        public void NormalizeInnerAngle(float angle, float expected) {
            AssertApprox(expected, MathUtility.NormalizeInnerAngle(angle));
        }

        [TestCase(350f, -5f, 5f, -5f)]
        [TestCase(30f, -5f, 5f, 5f)]
        [TestCase(2f, -5f, 5f, 2f)]
        public void ClampInnerAngle(float angle, float min, float max, float expected) {
            AssertApprox(expected, MathUtility.ClampInnerAngle(angle, min, max));
        }

        [Test]
        public void AngleVectorVariants_ApplyPerComponent() {
            var angles = new Vector3(-1, 725, 181);
            AssertApprox(new Vector3(359, 5, 181), MathUtility.NormalizeAngles(angles));
            AssertApprox(new Vector3(-1, 5, -179), MathUtility.NormalizeInnerAngles(angles));
            AssertApprox(new Vector3(-1, 5, -90),
                         MathUtility.ClampInnerAngles(angles, new Vector3(-10, -10, -90), new Vector3(10, 10, 90)));
        }

        #endregion

        #region Vector Operations

        [Test]
        public void ComponentWiseOperations() {
            var a = new Vector3(2, 3, 4);
            var b = new Vector3(4, 6, 8);
            AssertApprox(new Vector3(8, 18, 32), MathUtility.Multiply(a, b));
            AssertApprox(new Vector3(0.5f, 0.5f, 0.5f), MathUtility.Divide(a, b));
            AssertApprox(new Vector3(0.5f, 1f / 3f, 0.25f), MathUtility.Reciprocal(a));
            AssertApprox(new Vector3(6, 4, 3), MathUtility.Divide(12f, a));
        }

        [Test]
        public void Dot_Vector3Int() {
            Assert.That(MathUtility.Dot(new Vector3Int(1, 2, 3), new Vector3Int(4, -5, 6)), Is.EqualTo(12));
        }

        private static readonly Vector3[] PerpendicularInputs = {
            Vector3.right, Vector3.up, Vector3.forward, -Vector3.right, new Vector3(1, 2, 3),
        };

        [TestCaseSource(nameof(PerpendicularInputs))]
        public void GetPerpendicularVector_IsPerpendicularUnitVector(Vector3 input) {
            Vector3 result = input.GetPerpendicularVector();
            AssertApprox(0, Vector3.Dot(result, input));
            AssertApprox(1, result.magnitude);
        }

        [Test]
        public void Swizzles_ReturnExpectedComponents() {
            var v3 = new Vector3(1, 2, 3);
            var v2 = new Vector2(4, 5);

            Assert.That(v3.WithX(9), Is.EqualTo(new Vector3(9, 2, 3)));
            Assert.That(v3.WithY(9), Is.EqualTo(new Vector3(1, 9, 3)));
            Assert.That(v3.WithZ(9), Is.EqualTo(new Vector3(1, 2, 9)));
            Assert.That(v2.WithX(9), Is.EqualTo(new Vector2(9, 5)));
            Assert.That(v2.WithY(9), Is.EqualTo(new Vector2(4, 9)));
            Assert.That(v2.WithZ(9), Is.EqualTo(new Vector3(4, 5, 9)));

            Assert.That(v2.AsXY(), Is.EqualTo(new Vector3(4, 5, 0)));
            Assert.That(v2.AsYX(), Is.EqualTo(new Vector3(5, 4, 0)));
            Assert.That(v2.AsXZ(), Is.EqualTo(new Vector3(4, 0, 5)));
            Assert.That(v2.AsZX(), Is.EqualTo(new Vector3(5, 0, 4)));
            Assert.That(v2.AsYZ(), Is.EqualTo(new Vector3(0, 4, 5)));
            Assert.That(v2.AsZY(), Is.EqualTo(new Vector3(0, 5, 4)));

            Assert.That(v3.ToXY(), Is.EqualTo(new Vector2(1, 2)));
            Assert.That(v3.ToYX(), Is.EqualTo(new Vector2(2, 1)));
            Assert.That(v3.ToXZ(), Is.EqualTo(new Vector2(1, 3)));
            Assert.That(v3.ToZX(), Is.EqualTo(new Vector2(3, 1)));
            Assert.That(v3.ToYZ(), Is.EqualTo(new Vector2(2, 3)));
            Assert.That(v3.ToZY(), Is.EqualTo(new Vector2(3, 2)));

            Assert.That(v3.ToV4(), Is.EqualTo(new Vector4(1, 2, 3, 0)));
            Assert.That(v3.ToV4Pos(), Is.EqualTo(new Vector4(1, 2, 3, 1)));
        }

        #endregion

        #region Lines and Segments

        [Test]
        public void GetNearestPointOnLines_SkewLines() {
            var line1 = new Ray(Vector3.zero, Vector3.right);
            var line2 = new Ray(new Vector3(2, 1, 0), Vector3.forward);
            Assert.That(MathUtility.GetNearestPointOnLines(line1, line2, out float t1, out float t2), Is.True);
            AssertApprox(2, t1);
            AssertApprox(0, t2);
        }

        [Test]
        public void GetNearestPointOnLines_IntersectingLines() {
            var line1 = new Ray(Vector3.zero, Vector3.right);
            var line2 = new Ray(new Vector3(3, -2, 0), Vector3.up);
            Assert.That(MathUtility.GetNearestPointOnLines(line1, line2, out float t1, out float t2), Is.True);
            AssertApprox(3, t1);
            AssertApprox(2, t2);
        }

        [Test]
        public void GetNearestPointOnLines_ParallelLines_ReturnsFalse() {
            var line1 = new Ray(Vector3.zero, Vector3.right);
            var line2 = new Ray(Vector3.up, Vector3.right);
            Assert.That(MathUtility.GetNearestPointOnLines(line1, line2, out _, out _), Is.False);
        }

        [Test]
        public void GetNearestPointOnLine_ReturnsProjectedDistance() {
            var line = new Ray(Vector3.zero, Vector3.right);
            AssertApprox(3, MathUtility.GetNearestPointOnLine(line, new Vector3(3, 5, 0)));
            AssertApprox(-2, MathUtility.GetNearestPointOnLine(line, new Vector3(-2, 1, 1)));
        }

        [TestCase(5f, 3f, true, 5f)]
        [TestCase(0f, 1f, true, 0f)]
        [TestCase(10f, 1f, true, 10f)]
        [TestCase(-1f, 1f, false, 0f)]
        [TestCase(11f, 0f, false, 0f)]
        public void GetNearestPointOnSegment(float x, float y, bool expectedResult, float expectedX) {
            bool result = MathUtility.GetNearestPointOnSegment(Vector3.zero, new Vector3(10, 0, 0),
                                                               new Vector3(x, y, 0), out Vector3 point);
            Assert.That(result, Is.EqualTo(expectedResult));
            if (expectedResult) AssertApprox(new Vector3(expectedX, 0, 0), point);
        }

        #endregion

        #region Triangles

        private static readonly Vector3 TriV1 = Vector3.zero;
        private static readonly Vector3 TriV2 = Vector3.right;
        private static readonly Vector3 TriV3 = Vector3.up;

        [Test]
        public void GetNearestPointOnTriangle_PointAboveInterior_ProjectsOntoPlane() {
            bool result = MathUtility.GetNearestPointOnTriangle(TriV1, TriV2, TriV3, new Vector3(0.2f, 0.2f, 5),
                                                                out Vector3 point);
            Assert.That(result, Is.True);
            AssertApprox(new Vector3(0.2f, 0.2f, 0), point);
        }

        [Test]
        public void GetNearestPointOnTriangle_PointOutside_ReturnsFalse() {
            Assert.That(MathUtility.GetNearestPointOnTriangle(TriV1, TriV2, TriV3, new Vector3(2, 2, 0), out _),
                        Is.False);
        }

        [TestCase(0.2f, 0.2f, 1f, 0.2f, 0.2f, 0f)] // Inside, above plane.
        [TestCase(0.5f, -1f, 0f, 0.5f, 0f, 0f)]   // Nearest on edge v1-v2.
        [TestCase(1f, 1f, 0f, 0.5f, 0.5f, 0f)]    // Nearest on edge v2-v3.
        [TestCase(-1f, 0.5f, 0f, 0f, 0.5f, 0f)]   // Nearest on edge v3-v1.
        [TestCase(-1f, -1f, 0f, 0f, 0f, 0f)]      // Nearest at v1.
        [TestCase(2f, -1f, 0f, 1f, 0f, 0f)]       // Nearest at v2.
        [TestCase(-1f, 2f, 0f, 0f, 1f, 0f)]       // Nearest at v3.
        public void GetNearestPointOnTriangleIncludingBounds(float px, float py, float pz,
                                                             float ex, float ey, float ez) {
            Vector3 result = MathUtility.GetNearestPointOnTriangleIncludingBounds(
                TriV1, TriV2, TriV3, new Vector3(px, py, pz));
            AssertApprox(new Vector3(ex, ey, ez), result);
        }

        [Test]
        public void IsPointInsideBound_ChecksSideOfEdge() {
            Vector3 normal = Vector3.forward;
            Assert.That(MathUtility.IsPointInsideBound(TriV1, TriV2, normal, new Vector3(0.5f, 1, 0)), Is.True);
            Assert.That(MathUtility.IsPointInsideBound(TriV1, TriV2, normal, new Vector3(0.5f, -1, 0)), Is.False);
        }

        [Test]
        public void DoesSegmentIntersectTriangle_Hit_ReturnsT() {
            bool result = MathUtility.DoesSegmentIntersectTriangle(
                TriV1, TriV2, TriV3, new Vector3(0.2f, 0.2f, -1), new Vector3(0.2f, 0.2f, 1), out float t);
            Assert.That(result, Is.True);
            AssertApprox(0.5f, t);
        }

        [TestCase(2f, 2f, -1f, 2f, 2f, 1f)]           // Misses to the side.
        [TestCase(0.2f, 0.2f, 1f, 0.2f, 0.2f, 2f)]    // Segment ends before reaching the plane.
        [TestCase(0.2f, 0.2f, 1f, 0.5f, 0.2f, 1f)]    // Parallel to the triangle.
        public void DoesSegmentIntersectTriangle_Miss(float x1, float y1, float z1, float x2, float y2, float z2) {
            Assert.That(MathUtility.DoesSegmentIntersectTriangle(
                            TriV1, TriV2, TriV3, new Vector3(x1, y1, z1), new Vector3(x2, y2, z2), out _),
                        Is.False);
        }

        #endregion

        #region Quaternion Operations

        private static readonly Vector3 Primary = new Vector3(1, 2, 3).normalized;
        private static readonly Vector3 Hint = new Vector3(-1, 1, 0.5f).normalized;

        // Each case: builder, local axis that must match the primary exactly,
        // local axis that should lean toward the hint, and the remaining local axis.
        private static readonly object[] RotationCases = {
            new object[] { "XY", Vector3.right, Vector3.up, Vector3.forward },
            new object[] { "YX", Vector3.up, Vector3.right, Vector3.forward },
            new object[] { "XZ", Vector3.right, Vector3.forward, Vector3.up },
            new object[] { "ZX", Vector3.forward, Vector3.right, Vector3.up },
            new object[] { "YZ", Vector3.up, Vector3.forward, Vector3.right },
            new object[] { "ZY", Vector3.forward, Vector3.up, Vector3.right },
        };

        private static Quaternion BuildRotation(string builder, Vector3 primary, Vector3 hint) {
            return builder switch {
                "XY" => MathUtility.XYRotation(primary, hint),
                "YX" => MathUtility.YXRotation(primary, hint),
                "XZ" => MathUtility.XZRotation(primary, hint),
                "ZX" => MathUtility.ZXRotation(primary, hint),
                "YZ" => MathUtility.YZRotation(primary, hint),
                "ZY" => MathUtility.ZYRotation(primary, hint),
                _ => throw new ArgumentException(builder),
            };
        }

        [TestCaseSource(nameof(RotationCases))]
        public void AxisRotation_MatchesPrimaryAndLeansTowardHint(string builder, Vector3 primaryAxis,
                                                                  Vector3 hintAxis, Vector3 otherAxis) {
            Quaternion rotation = BuildRotation(builder, Primary, Hint);
            AssertApprox(Primary, rotation * primaryAxis);
            Assert.That(Vector3.Dot(rotation * hintAxis, Hint), Is.GreaterThan(0));
            AssertApprox(0, Vector3.Dot(rotation * otherAxis, Hint));
        }

        [TestCaseSource(nameof(RotationCases))]
        public void AxisRotation_AlignedInputs_ReturnIdentity(string builder, Vector3 primaryAxis,
                                                              Vector3 hintAxis, Vector3 otherAxis) {
            Quaternion rotation = BuildRotation(builder, primaryAxis, hintAxis);
            Assert.That(Quaternion.Angle(rotation, Quaternion.identity), Is.LessThan(0.01f));
        }

        #endregion

        #region Bounds Operations

        [Test]
        public void GetCorners_ReturnsAllEightCorners() {
            var bounds = new Bounds(new Vector3(1, 2, 3), new Vector3(2, 4, 6));
            var corners = new Vector3[8];
            bounds.GetCorners(corners);

            Assert.That(corners.Distinct().Count(), Is.EqualTo(8));
            foreach (Vector3 corner in corners) {
                Vector3 offset = corner - bounds.center;
                AssertApprox(bounds.extents, new Vector3(Mathf.Abs(offset.x), Mathf.Abs(offset.y),
                                                         Mathf.Abs(offset.z)));
            }
        }

        [Test]
        public void BoundsToScreenRect_IdentityProjection_ReturnsXYExtents() {
            var bounds = new Bounds(Vector3.zero, Vector3.one * 2);
            bool result = MathUtility.BoundsToScreenRect(null, bounds, p => p, out Rect rect);
            Assert.That(result, Is.True);
            AssertApprox(new Vector2(-1, -1), rect.min);
            AssertApprox(new Vector2(1, 1), rect.max);
        }

        [Test]
        public void BoundsToScreenRect_AllPointsBehindCamera_ReturnsFalse() {
            var bounds = new Bounds(Vector3.zero, Vector3.one * 2);
            bool result = MathUtility.BoundsToScreenRect(null, bounds, p => new Vector3(p.x, p.y, -1),
                                                         out Rect rect);
            Assert.That(result, Is.False);
            Assert.That(rect, Is.EqualTo(default(Rect)));
        }

        #endregion

        #region Rect Operations

        [Test]
        public void SplitHorizontal_TwoWay_FillsRectWithGap() {
            var rect = new Rect(10, 5, 100, 20);
            MathUtility.SplitHorizontal(rect, 10, out Rect out1, out Rect out2);

            AssertApprox(10, out1.xMin);
            AssertApprox(45, out1.width);
            AssertApprox(out1.xMax + 10, out2.xMin);
            AssertApprox(rect.xMax, out2.xMax);
            Assert.That(out2.y, Is.EqualTo(rect.y));
            Assert.That(out2.height, Is.EqualTo(rect.height));
        }

        [Test]
        public void SplitHorizontal_TwoWay_CustomDivision() {
            var rect = new Rect(0, 0, 100, 20);
            MathUtility.SplitHorizontal(rect, 0, out Rect out1, out Rect out2, 0.25f);
            AssertApprox(25, out1.width);
            AssertApprox(75, out2.width);
        }

        [TestCase(1f / 3f, 2f / 3f)]
        [TestCase(0.25f, 0.5f)]
        public void SplitHorizontal_ThreeWay_FillsRectWithGaps(float div1, float div2) {
            var rect = new Rect(10, 5, 120, 20);
            const float gap = 6;
            MathUtility.SplitHorizontal(rect, gap, out Rect out1, out Rect out2, out Rect out3, div1, div2);

            AssertApprox(rect.xMin, out1.xMin);
            AssertApprox(out1.xMax + gap, out2.xMin);
            AssertApprox(out2.xMax + gap, out3.xMin);
            AssertApprox(rect.xMax, out3.xMax);
            Assert.That(out3.width, Is.GreaterThan(0));
        }

        #endregion
    }
}
