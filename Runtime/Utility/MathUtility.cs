// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using Complex = System.Numerics.Complex;

namespace Infohazard.Core {
    /// <summary>
    /// Contains utility methods for working with mathematical types and solving math equations.
    /// </summary>
    public static class MathUtility {
        #region Float Operations

        /// <summary>
        /// Round a value to the nearest multiple of a given factor.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="factor">Value to round to a multiple of.</param>
        /// <returns>Rounded value.</returns>
        public static float RoundToNearest(float value, float factor) {
            return Mathf.Round(value / factor) * factor;
        }

        /// <summary>
        /// Same as Mathf.Sign, except that if the input is zero, it returns zero.
        /// </summary>
        /// <param name="value">A number to get the sign of.</param>
        /// <returns>1 if the number is positive, -1 if the number is negative, 0 if the number is 0.</returns>
        public static float SignZero(float value) {
            if (value > 0) return 1;
            if (value < 0) return -1;
            else return 0;
        }

        /// <summary>
        /// Evaluate all cubic roots of this Complex.
        /// </summary>
        /// <param name="complex">The number to get the cube roots of.</param>
        /// <returns>All three complex cube roots.</returns>
        public static (Complex, Complex, Complex) ComplexCubeRoot(Complex complex) {
            // Modified from https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/ComplexExtensions.cs
            double r = Math.Pow(complex.Magnitude, 1.0 / 3.0);
            double theta = complex.Phase / 3.0;
            const double shift = Math.PI * 2.0 / 3.0;
            return (Complex.FromPolarCoordinates(r, theta),
                Complex.FromPolarCoordinates(r, theta + shift),
                Complex.FromPolarCoordinates(r, theta - shift));
        }

        /// <summary>
        /// Solve a quadratic equation (find x such that the result is zero) in the form ax^2 + bx + c = 0.
        /// </summary>
        /// <param name="a">The coefficient for the x^2 term.</param>
        /// <param name="b">The coefficient for the x term.</param>
        /// <param name="c">The constant term.</param>
        /// <returns>The two roots of the quadratic equation, which may be complex.</returns>
        public static (Complex r1, Complex r2) SolveQuadratic(Complex a, Complex b, Complex c) {
            Complex sqrt = Complex.Sqrt(b * b - 4 * a * c);
            Complex denom = 2 * a;

            return ((-b + sqrt) / denom, (-b - sqrt) / denom);
        }

        /// <summary>
        /// Solve a cubic equation (find x such that the result is zero) in the form ax^3 + bx^2 + cx + d = 0.
        /// </summary>
        /// <param name="a">The coefficient for the x^3 term.</param>
        /// <param name="b">The coefficient for the x^2 term.</param>
        /// <param name="c">The coefficient for the x term.</param>
        /// <param name="d">The constant term.</param>
        /// <returns>The three roots of the cubic, which may be complex.</returns>
        public static (Complex r1, Complex r2, Complex r3) SolveCubic(Complex a, Complex b, Complex c, Complex d) {
            Complex delta0 = (b * b) - (3 * a * c);
            Complex delta1 = (2 * b * b * b) - (9 * a * b * c) + (27 * a * a * d);

            Complex root = Complex.Sqrt((delta1 * delta1) - (4 * delta0 * delta0 * delta0));

            double epsilon = 0.00001;
            if (Math.Abs(delta1.Real - root.Real) < epsilon && Math.Abs(delta1.Imaginary - root.Imaginary) < epsilon) {
                root = -root;
            }

            var (C1, C2, C3) = ComplexCubeRoot((delta1 - root) / 2.0);

            Complex r1 = -(1.0 / (3.0 * a)) * (b + C1 + (delta0 / C1));
            Complex r2 = -(1.0 / (3.0 * a)) * (b + C2 + (delta0 / C2));
            Complex r3 = -(1.0 / (3.0 * a)) * (b + C3 + (delta0 / C3));

            return (r1, r2, r3);
        }

        /// <summary>
        /// Solve a quartic equation (find x such that the result is zero) of the form ax^4 + bx^3 + cx^2 + dx + e = 0.
        /// </summary>
        /// <param name="a">The coefficient for the x^4 term.</param>
        /// <param name="b">The coefficient for the x^3 term.</param>
        /// <param name="c">The coefficient for the x^2 term.</param>
        /// <param name="d">The coefficient for the x term.</param>
        /// <param name="e">The constant term.</param>
        /// <returns>The four roots of the quartic, which may be complex.</returns>
        public static (Complex r1, Complex r2, Complex r3, Complex r4) SolveQuartic(
            Complex a, Complex b, Complex c, Complex d, Complex e) {
            // https://math.stackexchange.com/a/57688

            Complex A = (c / a) - ((3 * b * b) / (8 * a * a));
            Complex B = (d / a) - ((b * c) / (2 * a * a)) + ((b * b * b) / (8 * a * a * a));
            Complex C = (e / a) - ((b * d) / (4 * a * a)) + ((b * b * c) / (16 * a * a * a)) -
                        ((3 * b * b * b * b) / (256 * a * a * a * a));

            var (s1, s2, s3) = SolveCubic(8.0, -4.0 * A, -8.0 * C, (4.0 * A * C) - (B * B));

            Complex cmp1 = 0.5 * Complex.Sqrt((2.0 * s1) - A);
            Complex cmp2 = (-2.0 * s1) - A;
            Complex cmp3 = (2.0 * B) / Complex.Sqrt((2.0 * s1) - A);
            Complex cmp4 = -b / (4.0 * a);

            Complex sqrt1 = 0.5 * Complex.Sqrt(cmp2 + cmp3);
            Complex sqrt2 = 0.5 * Complex.Sqrt(cmp2 - cmp3);

            Complex r1 = -cmp1 + sqrt1 + cmp4;
            Complex r2 = -cmp1 - sqrt1 + cmp4;
            Complex r3 = cmp1 + sqrt2 + cmp4;
            Complex r4 = cmp1 - sqrt2 + cmp4;

            return (r1, r2, r3, r4);
        }

        #endregion

        #region Angle Operations

        /// <summary>
        /// Normalize an angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle">Input angle.</param>
        /// <returns>Angle between 0 and 360.</returns>
        public static float NormalizeAngle(float angle) {
            return ((angle % 360) + 360) % 360;
        }

        /// <summary>
        /// Normalize a set of euler angles to values between 0 and 360.
        /// </summary>
        /// <param name="angles">Input angles.</param>
        /// <returns>Angles between 0 and 360.</returns>
        public static Vector3 NormalizeAngles(Vector3 angles) {
            return new Vector3(
                NormalizeAngle(angles.x),
                NormalizeAngle(angles.y),
                NormalizeAngle(angles.z));
        }

        /// <summary>
        /// Normalize an angle to a value between -180 and 180.
        /// </summary>
        /// <param name="angle">Input angle.</param>
        /// <returns>Angle between -180 and 180.</returns>
        public static float NormalizeInnerAngle(float angle) {
            float result = NormalizeAngle(angle);
            if (result > 180) {
                result -= 360;
            }

            return result;
        }
        
        /// <summary>
        /// Normalize a set of euler angles to values between -180 and 180.
        /// </summary>
        /// <param name="angles">Input angles.</param>
        /// <returns>Angles between -180 and 180.</returns>
        public static Vector3 NormalizeInnerAngles(Vector3 angles) {
            return new Vector3(
                NormalizeInnerAngle(angles.x),
                NormalizeInnerAngle(angles.y),
                NormalizeInnerAngle(angles.z));
        }

        /// <summary>
        /// Normalize an angle to a value between -180 and 180, then clamp it in the given range.
        /// </summary>
        /// <param name="angle">Input angle.</param>
        /// <param name="min">Min clamp value (applied after normalize).</param>
        /// <param name="max">Max Clamp value (applied after normalize).</param>
        /// <returns>Angle between min and max.</returns>
        public static float ClampInnerAngle(float angle, float min, float max) {
            return Mathf.Clamp(NormalizeInnerAngle(angle), min, max);
        }
        
        /// <summary>
        /// Normalize a set of euler angles to values between -180 and 180, then clamp them in the given ranges.
        /// </summary>
        /// <param name="angles">Input angles.</param>
        /// <param name="min">Min clamp values (applied after normalize).</param>
        /// <param name="max">Max Clamp values (applied after normalize).</param>
        /// <returns>Angles between min and max.</returns>
        public static Vector3 ClampInnerAngles(Vector3 angles, Vector3 min, Vector3 max) {
            return new Vector3(
                ClampInnerAngle(angles.x, min.x, max.x),
                ClampInnerAngle(angles.y, min.y, max.y),
                ClampInnerAngle(angles.z, min.z, max.z));
        }

        #endregion

        #region Vector Operations

        /// <summary>
        /// Multiply the components of left by the components of right.
        /// </summary>
        public static Vector3 Multiply(Vector3 left, Vector3 right) {
            return new Vector3(left.x * right.x, left.y * right.y, left.z * right.z);
        }

        /// <summary>
        /// Divide the components of left by the components of right.
        /// </summary>
        public static Vector3 Divide(Vector3 left, Vector3 right) {
            return new Vector3(left.x / right.x, left.y / right.y, left.z / right.z);
        }

        /// <summary>
        /// Take the reciprocal of each component of a vector.
        /// </summary>
        public static Vector3 Reciprocal(Vector3 vector) => Divide(1.0f, vector);

        /// <summary>
        /// Divide a float by each component of a vector.
        /// </summary>
        public static Vector3 Divide(float left, Vector3 right) {
            return new Vector3(left / right.x, left / right.y, left / right.z);
        }
        
        /// <summary>
        /// Round a each component of a vector to the nearest multiple of a given factor.
        /// </summary>
        /// <param name="vector">Input values.</param>
        /// <param name="factor">Value to round to a multiple of.</param>
        /// <returns>Rounded values.</returns>
        public static Vector3 RoundToNearest(Vector3 vector, float factor) {
            return new Vector3(
                RoundToNearest(vector.x, factor),
                RoundToNearest(vector.y, factor),
                RoundToNearest(vector.z, factor)
            );
        }

        /// <summary>
        /// Find the point along each line where the lines come closest to each other.
        /// </summary>
        /// <remarks>
        /// If the lines are parallel, then return false.
        /// </remarks>
        /// <param name="line1">The first line.</param>
        /// <param name="line2">The second line.</param>
        /// <param name="t1">The point along the first line where they are closest to intersecting.</param>
        /// <param name="t2">The point along the second line where they are closest to intersecting.</param>
        /// <returns>False if the lines are parallel, true otherwise.</returns>
        public static bool GetNearestPointOnLines(Ray line1, Ray line2, out float t1, out float t2) {
            // https://stackoverflow.com/a/2316934
            // mua = ( d1343 d4321 - d1321 d4343 ) / ( d2121 d4343 - d4321 d4321 )

            Vector3 p1 = line1.origin;
            Vector3 v1 = line1.direction;
            Vector3 p2 = line2.origin;
            Vector3 v2 = line2.direction;

            float D(Vector3 a, Vector3 b) => Vector3.Dot(a, b);

            t1 = ((D(p1 - p2, v2) * D(v2, v1)) - (D(p1 - p2, v1) * D(v2, v2))) /
                 ((D(v1, v1) * D(v2, v2)) - (D(v2, v1) * D(v2, v1)));

            t2 = (D(p1 - p2, v2) + t1 * D(v2, v1)) / D(v2, v2);

            return !float.IsNaN(t1) && !float.IsNaN(t2) && !float.IsInfinity(t1) && !float.IsInfinity(t2);
        }

        /// <summary>
        /// Get the point on a line where it is nearest to a position.
        /// </summary>
        /// <param name="line">The input line.</param>
        /// <param name="p">The input position.</param>
        /// <returns>THe point along the line where it is nearest to the position.</returns>
        public static float GetNearestPointOnLine(Ray line, Vector3 p) {
            Vector3 v = p - line.origin;
            return Vector3.Dot(line.direction, v);
        }
        
        
        /// <summary>
        /// Find the point on a bounded line segment where it is nearest to a position,
        /// and return whether that point is in the segment's bounds.
        /// </summary>
        /// <remarks>
        /// Does not return points on the ends of the segment.
        /// If the nearest point on the segment's line is outside the segment,
        /// will fail and not return a valid point.
        /// </remarks>
        /// <param name="v1">The start of the segment.</param>
        /// <param name="v2">The end of the segment.</param>
        /// <param name="point">The point to search for.</param>
        /// <param name="pointOnSegment">The point on the segment closest to the input point.</param>
        /// <returns>Whether the nearest point is within the segment's bounds.</returns>
        public static bool GetNearestPointOnSegment(Vector3 v1, Vector3 v2, Vector3 point, out Vector3 pointOnSegment) {
            pointOnSegment = default;
            
            Vector3 v1ToV2 = v2 - v1;

            if (Vector3.Dot(v1ToV2, point - v1) < 0) return false;
            if (Vector3.Dot(-v1ToV2, point - v2) < 0) return false;

            Vector3 proj = Vector3.Project(point - v1, v1ToV2);
            pointOnSegment = v1 + proj;
            return true;
        }
        
        /// <summary>
        /// Find the point on a triangle (including its bounds) where it is nearest to a position.
        /// </summary>
        /// <remarks>
        /// If nearest point is on the triangle's bounds, that point will be returned,
        /// unlike <see cref="GetNearestPointOnTriangle"/>.
        /// </remarks>
        /// <param name="v1">The first triangle point.</param>
        /// <param name="v2">The second triangle point.</param>
        /// <param name="v3">The third triangle point.</param>
        /// <param name="point">The point to search for.</param>
        /// <returns>The nearest point on the triangle to the given point.</returns>
        public static Vector3 GetNearestPointOnTriangleIncludingBounds(Vector3 v1, Vector3 v2, Vector3 v3,
                                                                       Vector3 point) {
            
            // Helper function to check a single edge of the triangle.
            static bool CheckNearestPointOnTriangleEdge(Vector3 v1, Vector3 v2, Vector3 point, 
                                                        ref Vector3 nearestOnEdge, ref float shortestEdgeSqrDist) {
            
                if (GetNearestPointOnSegment(v1, v2, point, out Vector3 edgePos)) {
                    CheckTrianglePointNearest(edgePos, point, ref nearestOnEdge, ref shortestEdgeSqrDist);
                    return true;
                }

                return false;
            }

            // Helper function to check a single point.
            static void CheckTrianglePointNearest(Vector3 pos, Vector3 point,
                                                  ref Vector3 nearestOnEdge, ref float shortestEdgeSqrDist) {
            
                float sqrDist = Vector3.SqrMagnitude(pos - point);
                if (sqrDist < shortestEdgeSqrDist) {
                    nearestOnEdge = pos;
                    shortestEdgeSqrDist = sqrDist;
                }
            }
            
            // Check the triangle itself to see if the nearest point is within the triangle.
            if (GetNearestPointOnTriangle(v1, v2, v3, point, out Vector3 tPos)) {
                return tPos;
            }

            Vector3 nearest = Vector3.zero;
            float nearestSqrDist = float.PositiveInfinity;
            
            // Check each edge of the triangle to see if the nearest point is within that edge.
            bool e12 = CheckNearestPointOnTriangleEdge(v1, v2, point, ref nearest, ref nearestSqrDist);
            bool e23 = CheckNearestPointOnTriangleEdge(v2, v3, point, ref nearest, ref nearestSqrDist);
            bool e31 = CheckNearestPointOnTriangleEdge(v3, v1, point, ref nearest, ref nearestSqrDist);
            
            // Check each vertex of the triangle to see if they are closer than the current best point.
            // Each vertex only needs to be checked if neither adjacent edge has a valid nearest point.
            // If either adjacent edge does have a valid nearest point,
            // then a closer point than that vertex has already been found.
            if (!e12 && !e31) CheckTrianglePointNearest(v1, point, ref nearest, ref nearestSqrDist);
            if (!e12 && !e23) CheckTrianglePointNearest(v2, point, ref nearest, ref nearestSqrDist);
            if (!e23 && !e31) CheckTrianglePointNearest(v3, point, ref nearest, ref nearestSqrDist);
            
            return nearest;
        }

        /// <summary>
        /// Find the point on a triangle where it is nearest to a position,
        /// and return whether that point is in the triangle's bounds.
        /// </summary>
        /// <remarks>
        /// Does not return points on the edge of the triangle.
        /// If the nearest point on the triangle's plane is outside the triangle,
        /// will fail and not return a valid point.
        /// </remarks>
        /// <param name="v1">The first triangle point.</param>
        /// <param name="v2">The second triangle point.</param>
        /// <param name="v3">The third triangle point.</param>
        /// <param name="point">The point to search for.</param>
        /// <param name="pointOnTriangle">The point on the triangle closest to the input point.</param>
        /// <returns>Whether the nearest point is within the triangle's bounds.</returns>
        public static bool GetNearestPointOnTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 point,
                                                     out Vector3 pointOnTriangle) {
            pointOnTriangle = default;
            
            Vector3 normal = Vector3.Cross(v3 - v2, v1 - v2);

            if (!IsPointInsideBound(v1, v2, normal, point) ||
                !IsPointInsideBound(v2, v3, normal, point) ||
                !IsPointInsideBound(v3, v1, normal, point)) {
                return false;
            }

            Vector3 proj = Vector3.ProjectOnPlane(point - v1, normal);
            pointOnTriangle = v1 + proj;
            return true;
        }

        /// <summary>
        /// Returns true if a given point is on the inner side (defined by a given normal) of a segment.
        /// </summary>
        /// <param name="v1">The start of the segment.</param>
        /// <param name="v2">The end of the segment.</param>
        /// <param name="normal">The normal, defining which side is inside.</param>
        /// <param name="point">The point to search for.</param>
        /// <returns>Whether the point is on the inner side.</returns>
        public static bool IsPointInsideBound(Vector3 v1, Vector3 v2, Vector3 normal, Vector3 point) {
            Vector3 edge = v2 - v1;
            Vector3 cross = Vector3.Cross(normal, edge).normalized;
            Vector3 pointOffset = (point - v1).normalized;

            float dot = Vector3.Dot(pointOffset, cross);
            return dot > -.00001f;
        }
        
        /// <summary>
        /// Raycast a line segment against a triangle, and return whether they intersect.
        /// </summary>
        /// <param name="v1">The first triangle point.</param>
        /// <param name="v2">The second triangle point.</param>
        /// <param name="v3">The third triangle point.</param>
        /// <param name="s1">The start of the segment.</param>
        /// <param name="s2">The end of the segment.</param>
        /// <param name="t">The point along the input segment where it intersects the triangle, or -1.</param>
        /// <returns>Whether the segment intersects the triangle.</returns>
        public static bool DoesSegmentIntersectTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 s1, Vector3 s2, out float t) {
            // Implements the Möller–Trumbore intersection algorithm
            // Ported from Wikipedia's C++ implementation:
            // https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
            
            Vector3 rayVector = s2 - s1;
            Vector3 rayOrigin = s1;
            t = -1;
            
            const float epsilon = float.Epsilon;
            
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;
            Vector3 h = Vector3.Cross(rayVector, edge2);
            float a = Vector3.Dot(edge1, h);
            if (a > -epsilon && a < epsilon) {
                return false;    // This ray is parallel to this triangle.
            }

            float f = 1.0f / a;
            Vector3 s = rayOrigin - v1;
            float u = f * Vector3.Dot(s, h);
            if (u < 0.0 || u > 1.0) {
                return false;
            }
            
            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(rayVector, q);
            if (v < 0.0 || u + v > 1.0) {
                return false;
            }
            
            // At this stage we can compute t to find out where the intersection point is on the line.
            t = f * Vector3.Dot(edge2, q);
            return t > 0 && t < 1;
        }

        /// <summary>
        /// Projects a point in the world onto a canvas in camera or overlay space.
        /// </summary>
        /// <remarks>
        /// Similar to Camera.WorldToScreenPoint, but scaled to the size of the canvas and its viewport.
        /// Logs an error if the canvas is in world space, as that is not supported.
        /// </remarks>
        /// <param name="camera">The camera to use for reference.</param>
        /// <param name="canvas">The canvas to use for reference.</param>
        /// <param name="point">The world point to find on the canvas.</param>
        /// <returns>The point on the canvas, usable as an anchoredPosition.</returns>
        public static Vector3 WorldToCanvasPoint(this Camera camera, Canvas canvas, Vector3 point) {
            Debug.Assert(canvas.renderMode != RenderMode.WorldSpace);

            Vector3 vp;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                vp = camera.WorldToViewportPoint(point);
            } else {
                vp = camera.WorldToScreenPoint(point);
                vp.x /= Screen.width;
                vp.y /= Screen.height;
            }

            return (canvas.GetComponent<RectTransform>().rect.size * vp.ToXY()).WithZ(vp.z);
        }

        /// <summary>
        /// Returns a vector that is perpendicular to the given vector.
        /// </summary>
        /// <param name="vector">Input vector.</param>
        /// <returns>A perpendicular vector.</returns>
        public static Vector3 GetPerpendicularVector(this Vector3 vector) {
            Vector3 crossRight = Vector3.Cross(vector, Vector3.right);
            if (crossRight.sqrMagnitude > 0) return crossRight.normalized;
            return Vector3.Cross(vector, Vector3.up).normalized;
        }

        /// <summary>
        /// Dot product of two int vectors.
        /// </summary>
        public static int Dot(Vector3Int v1, Vector3Int v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        #endregion

        #region Vector Component Swizzling

        /// <summary>Replace the X component of a vector.</summary>
        public static Vector3 WithX(this Vector3 v, float x) => new Vector3(x, v.y, v.z);
        
        /// <summary>Replace the Y component of a vector.</summary>
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        
        /// <summary>Replace the Z component of a vector.</summary>
        public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
        
        /// <summary>Replace the X component of a vector.</summary>
        public static Vector2 WithX(this Vector2 v, float x) => new Vector2(x, v.y);
        
        /// <summary>Replace the Y component of a vector.</summary>
        public static Vector2 WithY(this Vector2 v, float y) => new Vector2(v.x, y);
        
        /// <summary>Convert a Vector2 to a Vector3 with the given Z.</summary>
        public static Vector3 WithZ(this Vector2 v, float z) => new Vector3(v.x, v.y, z);
        
        /// <summary>Get a Vector3 with the components (x, y, 0).</summary>
        public static Vector3 AsXY(this Vector2 v) => new Vector3(v.x, v.y, 0);
        
        /// <summary>Get a Vector3 with the components (y, x, 0).</summary>
        public static Vector3 AsYX(this Vector2 v) => new Vector3(v.y, v.x, 0);
        
        /// <summary>Get a Vector3 with the components (x, 0, y).</summary>
        public static Vector3 AsXZ(this Vector2 v) => new Vector3(v.x, 0, v.y);
        
        /// <summary>Get a Vector3 with the components (y, 0, x).</summary>
        public static Vector3 AsZX(this Vector2 v) => new Vector3(v.y, 0, v.x);
        
        /// <summary>Get a Vector3 with the components (0, x, y).</summary>
        public static Vector3 AsYZ(this Vector2 v) => new Vector3(0, v.x, v.y);
        
        /// <summary>Get a Vector3 with the components (0, y, x).</summary>
        public static Vector3 AsZY(this Vector2 v) => new Vector3(0, v.y, v.x);
        
        /// <summary>Get a Vector2 with the components (x, y).</summary>
        public static Vector2 ToXY(this Vector3 v) => new Vector2(v.x, v.y);
        
        /// <summary>Get a Vector2 with the components (y, x).</summary>
        public static Vector2 ToYX(this Vector3 v) => new Vector2(v.y, v.x);
        
        /// <summary>Get a Vector2 with the components (x, z).</summary>
        public static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);
        
        /// <summary>Get a Vector2 with the components (z, x).</summary>
        public static Vector2 ToZX(this Vector3 v) => new Vector2(v.z, v.x);
        
        /// <summary>Get a Vector2 with the components (y, z).</summary>
        public static Vector2 ToYZ(this Vector3 v) => new Vector2(v.y, v.z);
        
        /// <summary>Get a Vector2 with the components (z, y).</summary>
        public static Vector2 ToZY(this Vector3 v) => new Vector2(v.z, v.y);
        
        /// <summary>Get a Vector4 with the components (x, y, z, 1).</summary>
        public static Vector4 ToV4Pos(this Vector3 vector) {
            return new Vector4(vector.x, vector.y, vector.z, 1.0f);
        }

        /// <summary>Get a Vector4 with the components (x, y, z, 0).</summary>
        public static Vector4 ToV4(this Vector3 vector) {
            return new Vector4(vector.x, vector.y, vector.z, 0.0f);
        }

        #endregion

        #region Quaternion Operations

        /// <summary>Get a quaternion based on a right vector and approximate up vector.</summary>
        public static Quaternion XYRotation(Vector3 right, Vector3 upHint) {
            Vector3 forward = Vector3.Cross(right, upHint);
            Vector3 up = Vector3.Cross(forward, right);
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>Get a quaternion based on a up vector and approximate right vector.</summary>
        public static Quaternion YXRotation(Vector3 up, Vector3 rightHint) {
            Vector3 forward = Vector3.Cross(rightHint, up);
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>Get a quaternion based on a right vector and approximate forward vector.</summary>
        public static Quaternion XZRotation(Vector3 right, Vector3 forwardHint) {
            Vector3 up = Vector3.Cross(forwardHint, right);
            Vector3 forward = Vector3.Cross(right, up);
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>Get a quaternion based on a forward vector and approximate right vector.</summary>
        public static Quaternion ZXRotation(Vector3 forward, Vector3 rightHint) {
            Vector3 up = Vector3.Cross(forward, rightHint);
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>Get a quaternion based on a up vector and approximate forward vector.</summary>
        public static Quaternion YZRotation(Vector3 up, Vector3 forwardHint) {
            Vector3 right = Vector3.Cross(up, forwardHint);
            Vector3 forward = Vector3.Cross(right, up);
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>Get a quaternion based on a forward vector and approximate up vector.</summary>
        public static Quaternion ZYRotation(Vector3 forward, Vector3 upHint) {
            return Quaternion.LookRotation(forward, upHint);
        }

        #endregion

        #region Bounds Operations

        /// <summary>
        /// A static array that can be used to store the output of <see cref="GetCorners"/>,
        /// as long as the values are copied from the array right away.
        /// </summary>
        public static readonly Vector3[] BoundsCornerArray = new Vector3[8];
        
        /// <summary>
        /// Get the eight corners of a bounding box and save them in the given array.
        /// </summary>
        /// <remarks>
        /// You can use <see cref="BoundsCornerArray"/> to avoid allocating here.
        /// </remarks>
        /// <param name="bounds">The input bounds.</param>
        /// <param name="corners">Array to save the values in.</param>
        public static void GetCorners(this Bounds bounds, Vector3[] corners) {
            Vector3 c = bounds.center;
            Vector3 e = bounds.extents;
            
            corners[0] = new Vector3(c.x + e.x, c.y + e.y, c.z + e.z);
            corners[1] = new Vector3(c.x + e.x, c.y + e.y, c.z - e.z);
            corners[2] = new Vector3(c.x + e.x, c.y - e.y, c.z + e.z);
            corners[3] = new Vector3(c.x + e.x, c.y - e.y, c.z - e.z);
            corners[4] = new Vector3(c.x - e.x, c.y + e.y, c.z + e.z);
            corners[5] = new Vector3(c.x - e.x, c.y + e.y, c.z - e.z);
            corners[6] = new Vector3(c.x - e.x, c.y - e.y, c.z + e.z);
            corners[7] = new Vector3(c.x - e.x, c.y - e.y, c.z - e.z);
        }
        
        /// <summary>
        /// Get a screen rect that encapsulates the given bounds.
        /// </summary>
        /// <param name="transform">Parent that the bounds are attached to (can be null).</param>
        /// <param name="bounds">The input bounds.</param>
        /// <param name="worldToScreen">A function that converts world points to screen points, such as Camera.WorldToScreenPoint.</param>
        /// <param name="rect">A screen rect that encapsulates the bounds.</param>
        /// <returns>Whether a screen rect could be calculated (false if completely off screen).</returns>
        public static bool BoundsToScreenRect(Transform transform, Bounds bounds, Func<Vector3, Vector3> worldToScreen, out Rect rect) {
            GetCorners(bounds, BoundsCornerArray);

            Vector2 min = Vector2.zero;
            Vector2 max = Vector2.zero;
            bool found = false;
            
            for (int i = 0; i < BoundsCornerArray.Length; i++) {
                Vector3 point = transform ? transform.TransformPoint(BoundsCornerArray[i]) : BoundsCornerArray[i];
                Vector3 screenPoint = worldToScreen(point);
                
                if (screenPoint.z < 0) continue;

                if (!found) {
                    min = max = screenPoint.ToXY();
                    found = true;
                } else {
                    min.x = Mathf.Min(min.x, screenPoint.x);
                    min.y = Mathf.Min(min.y, screenPoint.y);

                    max.x = Mathf.Max(max.x, screenPoint.x);
                    max.y = Mathf.Max(max.y, screenPoint.y);
                }
            }

            rect = found ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : default;

            return found;
        }

        #endregion

        #region Rect Operations

        /// <summary>
        /// Split a rect into two halves horizontally, with given gap between the halves.
        /// </summary>
        /// <param name="rect">Rect to split.</param>
        /// <param name="gap">Gap between the split halves.</param>
        /// <param name="out1">Output rect 1.</param>
        /// <param name="out2">Output rect 2.</param>
        /// <param name="div">The ratio of the total space taken up by the left rect.</param>
        public static void SplitHorizontal(Rect rect, float gap, out Rect out1, out Rect out2, float div = 0.5f) {
            gap /= 2;
            out1 = new Rect(rect.x, rect.y, rect.width * div - gap, rect.height);
            out2 = new Rect(out1.xMax + gap * 2, rect.y, rect.width * (1 - div) - gap, rect.height);
        }

        /// <summary>
        /// Split a rect into three thirds horizontally, with given gap between the thirds.
        /// </summary>
        /// <param name="rect">Rect to split.</param>
        /// <param name="gap">Gap between the split halves.</param>
        /// <param name="out1">Output rect 1.</param>
        /// <param name="out2">Output rect 2.</param>
        /// <param name="out3">Output rect 3.</param>
        /// <param name="div1">The ratio of the total space taken up by the left rect.</param>
        /// <param name="div2">The ratio of the total space taken up by the left and center rect.</param>
        public static void SplitHorizontal(Rect rect, float gap, out Rect out1, out Rect out2, out Rect out3,
                                           float div1 = 1.0f / 3.0f, float div2 = 2.0f / 3.0f) {
            gap /= 2;
            out1 = new Rect(rect.x, rect.y, rect.width * div1 - gap, rect.height);
            out2 = new Rect(out1.xMax + gap * 2, rect.y, rect.width * (div2 - div1) - gap, rect.height);
            out3 = new Rect(out2.xMax + gap * 2, rect.y, rect.width * (1 - (div1 + div2)) - gap, rect.height);
        }

        #endregion
    }
}