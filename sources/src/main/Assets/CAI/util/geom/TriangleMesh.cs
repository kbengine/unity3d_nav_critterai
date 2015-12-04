/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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
using System;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.geom
{
    /// <summary>
    /// A basic indexed triangle mesh.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The buffers may contain unused space.
    /// </para>
    /// </remarks>
    public class TriangleMesh
    {
        /// <summary>
        /// Vertices [Length: >= <see cref="vertCount"/>]
        /// </summary>
        public Vector3[] verts;

        /// <summary>
        /// Triangles [(vertAIndex, vertBIndex, vertCIndex) * <see cref="triCount"/>]
        /// [Length: >= (<see cref="triCount"/> * 3)]
        /// </summary>
        public int[] tris;

        /// <summary>
        /// The number of vertices.
        /// </summary>
        public int vertCount;

        /// <summary>
        /// The number of triangles.
        /// </summary>
        public int triCount;

        /// <summary>
        /// Default constuctor. (Un-initialized, not content.)
        /// </summary>
        public TriangleMesh() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxVerts">
        /// The maximum number of vertices the <see cref="verts"/> buffer needs to hold. 
        /// [Limit: >= 3]
        /// </param>
        /// <param name="maxTris">
        /// The maximum number of triangles the <see cref="tris"/> buffer needs to hold. 
        /// [Limit: >= 1]
        /// </param>
        public TriangleMesh(int maxVerts, int maxTris)
        {
            this.verts = new Vector3[Math.Max(3, maxVerts)];
            this.tris = new int[Math.Max(1, maxTris) * 3];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor assigns the provided arrays to the object.  (No copying.)
        /// </para>
        /// </remarks>
        /// <param name="verts">
        /// The vertices. [Length: >= <typeparamref name="vertCount"/>]
        /// </param>
        /// <param name="vertCount">The number of vertices.</param>
        /// <param name="tris">
        /// The triangles. [(vertAIndex, vertBIndex, vertCIndex) * triCount]
        /// [Length: >= (<typeparamref name="triCount"/>) * 3]
        /// </param>
        /// <param name="triCount">The number of triangles.</param>
        public TriangleMesh(Vector3[] verts, int vertCount, int[] tris, int triCount)
        {
            this.verts = verts;
            this.vertCount = vertCount;
            this.tris = tris;
            this.triCount = triCount;
        }

        /// <summary>
        /// Gets the AABB bounds of the mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Do not call this method on an uninitialized mesh.
        /// </para>
        /// </remarks>
        /// <param name="boundsMin">The minimum bounds of the mesh.</param>
        /// <param name="boundsMax">The maximum bounds of the mesh.</param>
        public void GetBounds(out Vector3 boundsMin, out Vector3 boundsMax)
        {
            boundsMin = verts[tris[0]];
            boundsMax = verts[tris[0]];

            for (int i = 1; i < triCount * 3; i++)
            {
                Vector3 v = verts[tris[i]];
                boundsMin.x = Math.Min(boundsMin.x, v.x);
                boundsMin.y = Math.Min(boundsMin.y, v.y);
                boundsMin.z = Math.Min(boundsMin.z, v.z);
                boundsMax.x = Math.Max(boundsMax.x, v.x);
                boundsMax.y = Math.Max(boundsMax.y, v.y);
                boundsMax.z = Math.Max(boundsMax.z, v.z);
            }
        }

        /// <summary>
        /// True if the minimum bounds is less than the maximum bounds on all axes.
        /// </summary>
        /// <param name="boundsMin">The minimum AABB bounds.</param>
        /// <param name="boundsMax">The maximum AABB bounds.</param>
        /// <returns>
        /// True if the minimum bounds is less than the maximum bounds on all axes.
        /// </returns>
        public static bool IsBoundsValid(Vector3 boundsMin, Vector3 boundsMax)
        {
            return !(boundsMax.x < boundsMin.x 
                || boundsMax.y < boundsMin.y 
                || boundsMax.z < boundsMin.z);
        }

        /// <summary>
        /// Validates the structure and, optionally, the content of the mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The basic structural validation includes null checks, array size checks, etc.
        /// </para>
        /// <para>
        /// The optional content validation checks that the indices refer to valid vertices 
        /// and that triangles do not contain duplicate vertices.
        /// </para>
        /// </remarks>
        /// <param name="verts">The mesh vertices.</param>
        /// <param name="vertCount">The vertex count.</param>
        /// <param name="tris">The triangle indices.</param>
        /// <param name="triCount">The triangle count.</param>
        /// <param name="includeContent">
        /// If true, the content will be checked.  Otherwise only the structure will be checked.
        /// </param>
        /// <returns>True if the validation tests pass.</returns>
        public static bool IsValid(Vector3[] verts, int vertCount
            , int[] tris, int triCount
            , bool includeContent)
        {
            if (tris == null || verts == null
                || triCount * 3 > tris.Length
                || vertCount > verts.Length
                || triCount < 0 || vertCount < 0)
            {
                return false;
            }

            if (includeContent)
            {
                int length = triCount * 3;

                for (int p = 0; p < length; p += 3)
                {
                    int a = tris[p + 0];
                    int b = tris[p + 1];
                    int c = tris[p + 2];

                    if (a < 0 || a >= vertCount
                        || b < 0 || b >= vertCount
                        || c < 0 || c >= vertCount
                        || a == b || b == c || c == a)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the structure and, optionally, the content of the mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The basic structural validation includes null checks, array size checks, etc.
        /// </para>
        /// <para>
        /// The optional content validation checks that the indices refer to valid vertices
        /// and that triangles do not contain duplicate vertices.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The mesh to check.</param>
        /// <param name="includeContent">
        /// If true, the content will be checked.  Otherwise only the structure will be checked.
        /// </param>
        /// <returns>True if the validation tests pass.</returns>
        public static bool IsValid(TriangleMesh mesh, bool includeContent)
        {
            if (mesh == null)
                return false;

            return IsValid(mesh.verts, mesh.vertCount
                , mesh.tris, mesh.triCount
                , includeContent);
        }
    }
}
