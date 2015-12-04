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
using org.critterai.geom;
using System.Collections.Generic;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Used to compile input geometry in a dynamic fashion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The standard use case is to use this class to compile input
    /// geometry from various sources, then use <see cref="InputGeometryBuilder"/> to create
    /// the finalized input geometry for the build.
    /// </para>
    /// </remarks>
    /// <seealso cref="InputGeometry"/>
    /// <seealso cref="InputGeometryBuilder"/>
    public sealed class InputGeometryCompiler
    {
        private readonly List<Vector3> mVerts;
        private readonly List<int> mTris;
        private readonly List<byte> mAreas;

        /// <summary>
        /// The number of loaded vertices.
        /// </summary>
        public int VertCount { get { return mVerts.Count; } }

        /// <summary>
        /// The number of loaded triangles.
        /// </summary>
        public int TriCount { get { return mTris.Count / 3; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initVertCount">The initial vertex buffer size.</param>
        /// <param name="initTriCount">The initial triangle buffer size.</param>
        public InputGeometryCompiler(int initVertCount, int initTriCount)
        {
            initVertCount = System.Math.Max(9, initVertCount);
            initTriCount = System.Math.Max(1, initTriCount);

            mVerts = new List<Vector3>(initVertCount);
            mTris = new List<int>(initTriCount * 3);
            mAreas = new List<byte>(initTriCount);
        }

        /// <summary>
        /// Adds a single triangle.
        /// </summary>
        /// <param name="vertA">Vertex A of triangle ABC.</param>
        /// <param name="vertB">Vertex B of triangle ABC.</param>
        /// <param name="vertC">Vertex C of triangle ABC.</param>
        /// <param name="area">The triangle area.</param>
        public void AddTriangle(Vector3 vertA, Vector3 vertB, Vector3 vertC, byte area)
        {
            mTris.Add(mVerts.Count);
            mVerts.Add(vertA);

            mTris.Add(mVerts.Count);
            mVerts.Add(vertB);

            mTris.Add(mVerts.Count);
            mVerts.Add(vertC);

            mAreas.Add(area);
        }

        /// <summary>
        /// Adds an arbitrary group of triangles.  
        /// </summary>
        /// <remarks>
        /// <para>
        /// All triangles will default to <see cref="NMGen.MaxArea"/> if the 
        /// <paramref name="areas"/> parameter is null.
        /// </para>
        /// </remarks>
        /// <param name="verts">
        /// The triangle vertices. [Length: >= <paramref name="vertCount"/>]
        /// </param>
        /// <param name="vertCount">The number of vertices. [Length: >= 3]</param>
        /// <param name="tris">
        /// The triangles. [(vertAIndex, vertBIndex, vertCIndex) * triCount]
        /// [Length: >= 3 * <paramref name="triCount"/>]
        /// </param>
        /// <param name="areas">
        /// The triangle areas. (Optional) [Length: >= <paramref name="triCount"/>]
        /// </param>
        /// <param name="triCount">The number of triangles. [Limit: > 0]</param>
        /// <returns>True if the triangles were successfully added.</returns>
        public bool AddTriangles(Vector3[] verts, int vertCount
            , int[] tris, byte[] areas, int triCount)
        {
            if (triCount < 1 || vertCount < 3
                || verts == null || verts.Length < vertCount
                || tris == null || tris.Length < triCount * 3
                || areas != null && areas.Length < triCount)
            {
                return false;
            }

            if (areas == null)
                areas = NMGen.CreateDefaultAreaBuffer(triCount);

            int iVertOffset = mVerts.Count;

            if (vertCount == verts.Length)
                mVerts.AddRange(verts);
            else
            {
                mVerts.Capacity += vertCount;

                for (int p = 0; p < vertCount; p++)
                {
                    mVerts.Add(verts[p]);
                }
            }

            int length = triCount * 3;

            mTris.Capacity += length;

            for (int p = 0; p < length; p++)
            {
                mTris.Add(tris[p] + iVertOffset);
            }

            if (areas.Length == triCount)
                mAreas.AddRange(areas);
            else
            {
                mAreas.Capacity += triCount;

                for (int i = 0; i < triCount; i++)
                {
                    mAreas.Add(areas[i]);
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a triangle mesh.  
        /// </summary>
        /// <remarks>
        /// <para>
        /// All triangles will default to <see cref="NMGen.MaxArea"/> if the 
        /// <paramref name="areas"/> parameter is null.
        /// </para>
        /// <para>
        /// Will return false if the mesh triangle count is zero.
        /// </para>
        /// </remarks>
        /// <param name="mesh">The triangle mesh.</param>
        /// <param name="areas">
        /// The triangle areas. (Optional)[Length: >= mesh.triCount]
        /// </param>
        /// <returns>True if the triangles were successfully added.</returns>
        public bool AddTriangles(TriangleMesh mesh, byte[] areas)
        {
            if (mesh == null || mesh.triCount == 0)
                return false;

            return AddTriangles(mesh.verts, mesh.vertCount, mesh.tris, areas, mesh.triCount);
        }

        /// <summary>
        /// Checks for an removes invalid triangles.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A triangle is considered invalid in the following cases:
        /// </para>
        /// <ul>
        /// <li>A vertex index is out of range.</li>
        /// <li>The triangle contains duplicate vertices. (E.g. vertAIndex = vertBIndex)</li>
        /// </ul>
        /// </remarks>
        /// <returns>The number of triangles removed.</returns>
        public int CleanTriangles()
        {
            int triCount = TriCount;
            int vertCount = mVerts.Count;

            if (triCount == 0)
                return 0;

            int result = 0;

            for (int i = triCount - 1; i >= 0; i--)
            {
                int p = i * 3;

                int a = mTris[p + 0];
                int b = mTris[p + 1];
                int c = mTris[p + 2];

                if (a < 0 || a >= vertCount
                    || b < 0 || b >= vertCount
                    || c < 0 || c >= vertCount
                    || a == b || b == c || c == a)
                {
                    // Bad triangle.
                    mTris.RemoveRange(p, 3);
                    mAreas.RemoveAt(i);
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates geometry from the compiled data.
        /// </summary>
        /// <param name="areas">The triangle areas.</param>
        /// <returns>The triangle mesh, or null if the compiler is empty.</returns>
        public TriangleMesh CreateGeometry(out byte[] areas)
        {
            if (mTris.Count == 0)
            {
                areas = null;
                return null;
            }

            areas = mAreas.ToArray();

            return new TriangleMesh(mVerts.ToArray(), mVerts.Count
                , mTris.ToArray(), mTris.Count / 3);
        }

        /// <summary>
        /// Resets the compiler to an empty state.
        /// </summary>
        public void Reset()
        {
            mTris.Clear();
            mVerts.Clear();
            mAreas.Clear();
        }
    }
}
