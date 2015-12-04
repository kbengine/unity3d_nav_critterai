/*
 * Copyright (c) 2012 Stephen A. Pratt
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class ColliderHelper
    {
        private GameObject mSphere;
        private GameObject mCube;
        private Mesh mSphereMesh;
        private Mesh mCubeMesh;

        public ColliderHelper()
        {
            mSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mSphereMesh = mSphere.GetComponent<MeshFilter>().sharedMesh;

            mCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mCubeMesh = mCube.GetComponent<MeshFilter>().sharedMesh;
        }

        public void Dispose()
        {
            if (mSphere == null)
                return;

            Object.DestroyImmediate(mSphere);
            Object.DestroyImmediate(mCube);

            mSphere = null;
            mCube = null;
            mSphereMesh = null;
            mCubeMesh = null;
        }

        public bool Get(Collider filter, out CombineInstance result)
        {
            result = new CombineInstance();

            if (mSphere == null || !IsSupported(filter))
                return false;

            if (filter is SphereCollider)
            {
                SphereCollider sphereCollider = (SphereCollider)filter;

                result.mesh = mSphereMesh;
                result.transform = sphereCollider.transform.localToWorldMatrix
                    * Matrix4x4.TRS(sphereCollider.center
                        , Quaternion.identity
                        , Vector3.one * sphereCollider.radius * 2.0f);

                return true;
            }

            if (filter is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)filter;

                result.mesh = mCubeMesh;
                result.transform = boxCollider.transform.localToWorldMatrix
                    * Matrix4x4.TRS(boxCollider.center, Quaternion.identity, boxCollider.size);

                return true;
            }

            if (filter is MeshCollider)
            {
                MeshCollider collider = (MeshCollider)filter;

                if (collider.sharedMesh)
                {
                    result.mesh = collider.sharedMesh;
                    result.transform = collider.transform.localToWorldMatrix;

                    return true;
                }
            }

            return false;
        }

        public static bool IsSupported(Collider item)
        {
            return (item is SphereCollider || item is BoxCollider || item is MeshCollider);
        }
    }
}
