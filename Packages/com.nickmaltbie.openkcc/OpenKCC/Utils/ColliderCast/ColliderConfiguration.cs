// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// Enum to translate capsule direction to numeric value.
    /// </summary>
    public enum CapsuleDirection
    {
        X = 0,
        Y = 1,
        Z = 2,
    }

    /// <summary>
    /// List of primitive collider types.
    /// </summary>
    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule,
        Point,
    }

    /// <summary>
    /// Generic collider configuration.
    /// </summary>
    [Serializable]
    public class ColliderConfiguration : PropertyAttribute
    {
        /// <summary>
        /// Type of collider for this configuration.
        /// </summary>
        public ColliderType type = ColliderType.Box;

        /// <summary>
        /// Center of box collider.
        /// </summary>
        public Vector3 boxCenter = Vector3.zero;

        /// <summary>
        /// Size of the box collider.
        /// </summary>
        public Vector3 boxSize = Vector3.one;

        /// <summary>
        /// Center of sphere collider.
        /// </summary>
        public Vector3 sphereCenter = Vector3.zero;

        /// <summary>
        /// Radius of the sphere collider.
        /// </summary>
        public float sphereRadius = 0.5f;

        /// <summary>
        /// Center of capsule collider
        /// </summary>
        public Vector3 capsuleCenter = Vector3.zero;

        /// <summary>
        /// Radius of the capsule collider.
        /// </summary>
        public float capsuleRadius = 0.5f;

        /// <summary>
        /// Height of the capsule collider.
        /// </summary>
        public float capsuleHeight = 2.0f;

        /// <summary>
        /// Direction fo the capsule collider.
        /// </summary>
        public CapsuleDirection capsuleDirection = CapsuleDirection.Y;

        /// <summary>
        /// Center of point collider.
        /// </summary>
        public Vector3 pointCenter = Vector3.zero;

        /// <summary>
        /// Gets or sets the radius fo the collider (applies to Sphere or Capsule).
        /// </summary>
        public float Radius
        {
            get
            {
                switch (type)
                {
                    case ColliderType.Sphere:
                        return sphereRadius;
                    case ColliderType.Capsule:
                        return capsuleRadius;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (type)
                {
                    case ColliderType.Sphere:
                        sphereRadius = value;
                        break;
                    case ColliderType.Capsule:
                        capsuleRadius = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the center of the collider (applies to Box, Sphere, or Capsule).
        /// </summary>
        public Vector3 Center
        {
            get
            {
                switch (type)
                {
                    case ColliderType.Sphere:
                        return sphereCenter;
                    case ColliderType.Capsule:
                        return capsuleCenter;
                    case ColliderType.Box:
                        return boxCenter;
                    default:
                    case ColliderType.Point:
                        return pointCenter;
                }
            }
            set
            {
                switch (type)
                {
                    case ColliderType.Sphere:
                        sphereCenter = value;
                        break;
                    case ColliderType.Capsule:
                        capsuleCenter = value; ;
                        break;
                    case ColliderType.Box:
                        boxCenter = value;
                        break;
                    default:
                    case ColliderType.Point:
                        pointCenter = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the collider (applies to Box).
        /// </summary>
        public Vector3 Size
        {
            get => boxSize;
            set => boxSize = value;
        }

        /// <summary>
        /// Gets or sets the height of the collider (applies to Capsule).
        /// </summary>
        public float Height
        {
            get => capsuleHeight;
            set => capsuleHeight = value;
        }

        /// <summary>
        /// Gets or Sets Direction of capsule collider.
        /// </summary>
        public CapsuleDirection CapsuleDirection
        {
            get => capsuleDirection;
            set => capsuleDirection = value;
        }

        /// <summary>
        /// Constructs an instance of Collider configuration.
        /// </summary>
        /// <param name="type">Type of collider to create.</param>
        /// <param name="center">Center of the collider in 3D space.</param>
        /// <param name="size">Size of the collider (only applies to Box)</param>
        /// <param name="radius">Radius fo the collider (applies to Sphere or Capsule).</param>
        /// <param name="height">Height of the collider (applies to Capsule).</param>
        /// <param name="capsuleDirection">Direction of capsule collider.</param>
        public ColliderConfiguration(
            ColliderType type = ColliderType.Box,
            Vector3? center = null,
            Vector3? size = null,
            float radius = 0.5f,
            float height = 2.0f,
            CapsuleDirection capsuleDirection = CapsuleDirection.Y)
        {
            this.type = type;

            Center = center ?? Vector3.zero;
            Size = size ?? Vector3.one;
            Radius = radius;
            Height = height;
            CapsuleDirection = capsuleDirection;
        }

        /// <summary>
        /// Attaches a collider component to a given game object
        /// based on this collider configuration.
        /// </summary>
        /// <param name="go">Game object to attach collider to.</param>
        /// <param name="cleanupCollider">Should existing colliders on the object be cleaned up.</param>
        /// <returns>Collider attached to the given game object.</returns>
        public Collider AttachCollider(GameObject go, bool cleanupCollider = true)
        {
            if (cleanupCollider)
            {
                foreach (Collider col in go.GetComponents<Collider>())
                {
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        GameObject.DestroyImmediate(col);
                    }
                    else
                    {
                        GameObject.Destroy(col);
                    }
#else
                    GameObject.Destroy(col);
#endif

                }
            }

            switch (type)
            {
                case ColliderType.Box:
                    BoxCollider box = go.AddComponent<BoxCollider>();
                    box.size = Size;
                    box.center = Center;
                    return box;
                case ColliderType.Sphere:
                    SphereCollider sphere = go.AddComponent<SphereCollider>();
                    sphere.center = Center;
                    sphere.radius = Radius;
                    return sphere;
                case ColliderType.Capsule:
                    CapsuleCollider capsule = go.AddComponent<CapsuleCollider>();
                    capsule.center = Center;
                    capsule.radius = Radius;
                    capsule.height = Height;
                    capsule.direction = (int)CapsuleDirection;
                    return capsule;
            }

            return null;
        }
    }
}
