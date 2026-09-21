// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    public static class ApplyTransform {
        [MenuItem("Tools/Infohazard/Apply Transform/Context Menu %&a")]
        public static void ShowApplyTransformContextMenu() {
            if (Selection.gameObjects.Length == 0) return;

            GenericMenu menu = new();
            
            menu.AddDisabledItem(new GUIContent("Apply Transform"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Position"), false, ApplyPosition);
            menu.AddItem(new GUIContent("Local Position"), false, ApplyLocalPosition);
            menu.AddItem(new GUIContent("Collider Offset to Position"), false, ApplyColliderOffsetToPosition);
            menu.AddItem(new GUIContent("Rotation"), false, ApplyRotation);
            menu.AddItem(new GUIContent("Local Rotation"), false, ApplyLocalRotation);
            menu.AddItem(new GUIContent("Scale"), false, ApplyScale);
            menu.AddItem(new GUIContent("All"), false, ApplyAll);

            menu.ShowAsContext();
        }
        
        [MenuItem("Tools/Infohazard/Apply Transform/Position")]
        public static void ApplyPosition() {
            foreach (GameObject obj in Selection.gameObjects) {
                if (!obj || obj.transform.position == Vector3.zero) continue;
                RecordUndo(obj, "Apply Position");
                Vector3[] childPositions = new Vector3[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    childPositions[i] = child.position;
                }

                Vector3 offset = obj.transform.InverseTransformVector(obj.transform.position);
                obj.transform.position = Vector3.zero;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    obj.transform.GetChild(i).position = childPositions[i];
                }

                OffsetAllColliders(obj, offset);
            }
        }

        [MenuItem("Tools/Infohazard/Apply Transform/Local Position")]
        public static void ApplyLocalPosition() {
            foreach (GameObject obj in Selection.gameObjects) {
                if (!obj || obj.transform.localPosition == Vector3.zero) continue;
                RecordUndo(obj, "Apply Local Position");
                Vector3[] childPositions = new Vector3[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    childPositions[i] = child.position;
                }

                Vector3 offset = Quaternion.Inverse(obj.transform.localRotation) * obj.transform.localPosition;
                obj.transform.localPosition = Vector3.zero;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    obj.transform.GetChild(i).position = childPositions[i];
                }

                OffsetAllColliders(obj, offset);
            }
        }

        [MenuItem("Tools/Infohazard/Apply Transform/Collider Offset to Position")]
        public static void ApplyColliderOffsetToPosition() {
            foreach (GameObject obj in Selection.gameObjects) {
                if (!obj || !obj.TryGetComponent(out Collider collider)) continue;

                Vector3 colliderOffset = collider switch {
                    SphereCollider s => s.center,
                    CapsuleCollider c => c.center,
                    BoxCollider b => b.center,
                    _ => Vector3.zero,
                };

                if (colliderOffset == Vector3.zero) continue;
                colliderOffset.Scale(obj.transform.localScale);

                Vector3 offsetToSelf = obj.transform.localRotation * colliderOffset;

                Undo.RecordObject(obj.transform, "Apply Collider Offset");
                Undo.RecordObject(collider, "Apply Collider Offset");
                obj.transform.localPosition += offsetToSelf;

                switch (collider) {
                    case SphereCollider s:
                        s.center = Vector3.zero;
                        break;
                    case CapsuleCollider c:
                        c.center = Vector3.zero;
                        break;
                    case BoxCollider b:
                        b.center = Vector3.zero;
                        break;
                }
            }
        }

        private static void OffsetAllColliders(GameObject obj, Vector3 offset) {
            foreach (var col in obj.GetComponents<Collider>()) {
                SetColliderOffset(col, GetColliderOffset(col) + offset);
            }

            foreach (var col in obj.GetComponents<Collider2D>()) {
                col.offset += (Vector2)offset;
            }
        }

        private static Vector3 GetColliderOffset(Collider col) {
            switch (col) {
                case BoxCollider box:
                    return box.center;
                case SphereCollider sphere:
                    return sphere.center;
                case CapsuleCollider capsule:
                    return capsule.center;
                default:
                    return Vector3.zero;
            }
        }

        private static void SetColliderOffset(Collider col, Vector3 offset) {
            switch (col) {
                case BoxCollider box:
                    box.center = offset;
                    break;
                case SphereCollider sphere:
                    sphere.center = offset;
                    break;
                case CapsuleCollider capsule:
                    capsule.center = offset;
                    break;
            }
        }

        [MenuItem("Tools/Infohazard/Apply Transform/Rotation")]
        public static void ApplyRotation() {
            foreach (GameObject obj in Selection.gameObjects) {
                if (!obj || obj.transform.rotation == Quaternion.identity) continue;
                RecordUndo(obj, "Apply Rotation");

                (Quaternion rot, Vector3 pos)[] childRotations = new (Quaternion, Vector3)[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    var child = obj.transform.GetChild(i);
                    childRotations[i].rot = child.rotation;
                    childRotations[i].pos = child.position;
                }

                Quaternion offset = obj.transform.rotation;
                obj.transform.rotation = Quaternion.identity;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    child.rotation = childRotations[i].rot;
                    child.position = childRotations[i].pos;
                }

                foreach (Collider col in obj.GetComponents<Collider>()) {
                    SetColliderOffset(col, offset * GetColliderOffset(col));
                    switch (col) {
                        case BoxCollider box:
                            box.size = offset * box.size;
                            break;
                        case CapsuleCollider capsule:
                            capsule.direction = RotateCapuleDirection(capsule.direction, offset);
                            break;
                    }
                }

                foreach (Collider2D col in obj.GetComponents<Collider2D>()) {
                    col.offset = offset * col.offset;
                    switch (col) {
                        case BoxCollider2D box:
                            box.size = offset * box.size;
                            break;
                        case CapsuleCollider2D capsule:
                            capsule.direction = RotateCapuleDirection2D(capsule.direction, offset);
                            break;
                        case EdgeCollider2D edge:
                            edge.points = RotatePoints(edge.points, offset);
                            break;
                        case PolygonCollider2D poly:
                            poly.points = RotatePoints(poly.points, offset);
                            break;
                    }
                }
            }
        }

        [MenuItem("Tools/Infohazard/Apply Transform/Local Rotation")]
        public static void ApplyLocalRotation() {
            foreach (GameObject obj in Selection.gameObjects) {
                if (!obj || obj.transform.localRotation == Quaternion.identity) continue;
                RecordUndo(obj, "Apply Local Rotation");

                (Quaternion rot, Vector3 pos)[] childRotations = new (Quaternion, Vector3)[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    childRotations[i].rot = child.rotation;
                    childRotations[i].pos = child.position;
                }

                Quaternion offset = obj.transform.localRotation;
                obj.transform.localRotation = Quaternion.identity;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    Transform child = obj.transform.GetChild(i);
                    child.rotation = childRotations[i].rot;
                    child.position = childRotations[i].pos;
                }

                foreach (Collider col in obj.GetComponents<Collider>()) {
                    SetColliderOffset(col, offset * GetColliderOffset(col));
                    switch (col) {
                        case BoxCollider box:
                            box.size = offset * box.size;
                            break;
                        case CapsuleCollider capsule:
                            capsule.direction = RotateCapuleDirection(capsule.direction, offset);
                            break;
                    }
                }

                foreach (Collider2D col in obj.GetComponents<Collider2D>()) {
                    col.offset = offset * col.offset;
                    switch (col) {
                        case BoxCollider2D box:
                            box.size = offset * box.size;
                            break;
                        case CapsuleCollider2D capsule:
                            capsule.direction = RotateCapuleDirection2D(capsule.direction, offset);
                            break;
                        case EdgeCollider2D edge:
                            edge.points = RotatePoints(edge.points, offset);
                            break;
                        case PolygonCollider2D poly:
                            poly.points = RotatePoints(poly.points, offset);
                            break;
                    }
                }
            }
        }

        private static Vector3 GetCapsuleDirection(int dir) {
            if (dir == 0) {
                return Vector3.right;
            } else if (dir == 1) {
                return Vector3.up;
            } else {
                return Vector3.forward;
            }
        }

        private static int RotateCapuleDirection(int dir, Quaternion rot) {
            Vector3 vector = GetCapsuleDirection(dir);

            vector = rot * vector;
            float ax = Mathf.Abs(vector.x);
            float ay = Mathf.Abs(vector.y);
            float az = Mathf.Abs(vector.z);

            if (ax >= ay && ax >= az) {
                return 0;
            } else if (ay >= ax && ay >= az) {
                return 1;
            } else {
                return 2;
            }
        }

        private static Vector2 GetCapsuleDirection2D(CapsuleDirection2D dir) {
            if (dir == CapsuleDirection2D.Horizontal) {
                return Vector2.right;
            } else {
                return Vector2.up;
            }
        }

        private static CapsuleDirection2D RotateCapuleDirection2D(CapsuleDirection2D dir, Quaternion rot) {
            Vector2 vector = GetCapsuleDirection2D(dir);
            vector = rot * vector;
            float ax = Mathf.Abs(vector.x);
            float ay = Mathf.Abs(vector.y);

            if (ax >= ay) {
                return CapsuleDirection2D.Horizontal;
            } else {
                return CapsuleDirection2D.Vertical;
            }
        }

        private static Vector2[] RotatePoints(Vector2[] points, Quaternion rot) {
            for (int i = 0; i < points.Length; i++) {
                points[i] = rot * points[i];
            }
            return points;
        }

        [MenuItem("Tools/Infohazard/Apply Transform/Scale")]
        public static void ApplyScale() {
            foreach (var obj in Selection.gameObjects) {
                if (obj && obj.transform.localScale != Vector3.one) {
                    RecordUndo(obj, "Apply Scale");
                    Vector3 oldScale = obj.transform.localScale;
                    obj.transform.localScale = Vector3.one;

                    for (int i = 0; i < obj.transform.childCount; i++) {
                        var child = obj.transform.GetChild(i);
                        Vector3 v = child.transform.localPosition;
                        v.x *= oldScale.x;
                        v.y *= oldScale.y;
                        v.z *= oldScale.z;
                        child.transform.localPosition = v;

                        v = child.transform.localScale;
                        var map = GetScaleMapping(child.transform.localRotation);
                        var sr = map.MultiplyVector(oldScale);
                        v.x *= sr.x;
                        v.y *= sr.y;
                        v.z *= sr.z;
                        child.transform.localScale = v;
                    }

                    foreach (var col in obj.GetComponents<Collider>()) {
                        var colOffset = GetColliderOffset(col);
                        colOffset.Scale(oldScale);
                        SetColliderOffset(col, colOffset);
                        switch (col) {
                            case BoxCollider box:
                                Vector3 size = box.size;
                                size.Scale(oldScale);
                                box.size = size;
                                break;
                            case SphereCollider sphere:
                                sphere.radius *= Mathf.Max(oldScale.x, oldScale.y, oldScale.z);
                                break;
                            case CapsuleCollider capsule:
                                Vector3 dir = GetCapsuleDirection(capsule.direction);
                                capsule.height *= Vector3.Dot(oldScale, dir);
                                Vector3 v = Vector3.ProjectOnPlane(oldScale, dir);
                                capsule.radius *= Mathf.Max(v.x, v.y, v.z);
                                break;
                        }
                    }

                    foreach (var col in obj.GetComponents<Collider2D>()) {
                        var colOffset = col.offset;
                        colOffset.Scale(oldScale);
                        col.offset = colOffset;
                        switch (col) {
                            case BoxCollider2D box:
                                Vector2 size = box.size;
                                size.Scale(oldScale);
                                box.size = size;
                                break;
                            case CapsuleCollider2D capsule:
                                size = capsule.size;
                                size.Scale(oldScale);
                                capsule.size = size;
                                break;
                            case EdgeCollider2D edge:
                                edge.points = ScalePoints(edge.points, oldScale);
                                break;
                            case PolygonCollider2D poly:
                                poly.points = ScalePoints(poly.points, oldScale);
                                break;
                        }
                    }
                }
            }
        }

        private static Vector2[] ScalePoints(Vector2[] points, Vector2 scale) {
            for (int i = 0; i < points.Length; i++) {
                points[i].Scale(scale);
            }
            return points;
        }

        private static void RecordUndo(GameObject obj, string message) {
            var objs = new List<Object>();
            objs.Add(obj.transform);
            for (int i = 0; i < obj.transform.childCount; i++) {
                objs.Add(obj.transform.GetChild(i));
            }
            objs.AddRange(obj.GetComponents<Collider>());
            objs.AddRange(obj.GetComponents<Collider2D>());
            Undo.RecordObjects(objs.ToArray(), message);
        }

        private static Matrix4x4 GetScaleMapping(Quaternion rotation) {
            Vector3 y = rotation * Vector3.up;
            Vector3 z = rotation * Vector3.forward;

            float yx = Mathf.Abs(Vector3.Dot(y, Vector3.right));
            float yy = Mathf.Abs(Vector3.Dot(y, Vector3.up));
            float yz = Mathf.Abs(Vector3.Dot(y, Vector3.forward));

            float zx = Mathf.Abs(Vector3.Dot(z, Vector3.right));
            float zy = Mathf.Abs(Vector3.Dot(z, Vector3.up));
            float zz = Mathf.Abs(Vector3.Dot(z, Vector3.forward));

            Vector3 fwd, up;

            if (zx > zy && zx >= zz) {
                fwd = Vector3.right;
            } else if (zy > zx && zy >= zz) {
                fwd = Vector3.up;
            } else {
                fwd = Vector3.forward;
            }

            if (yx >= yy && yx > yz) {
                up = Vector3.right;
            } else if (yy >= yx && yy > yz) {
                up = Vector3.up;
            } else {
                up = Vector3.forward;
            }

            Quaternion q = Quaternion.LookRotation(fwd, up);
            Matrix4x4 m = Matrix4x4.Rotate(q);
            if (m.m20 < 0) m.m20 *= -1;
            return m;
        }

        [MenuItem("Tools/Infohazard/Apply Transform/All")]
        public static void ApplyAll() {
            ApplyPosition();
            ApplyRotation();
            ApplyScale();
        }
    }
}
