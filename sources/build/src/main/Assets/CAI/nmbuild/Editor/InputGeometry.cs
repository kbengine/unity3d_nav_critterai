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
using System;
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
    /// Represents triangle mesh and area data used as input to the NMGen build process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects of this type are created using the <see cref="InputGeometryBuilder"/> class.
    /// </para>
    /// <para>
    /// Objects of this type are guarenteed to be thread-safe, immutable, and contain at least 
    /// one triangle. There is no empty state.
    /// </para>
    /// <para>
    /// The data storage is optimized for the NMGen build process, so this is not a general 
    /// use class.
    /// </para>
    /// </remarks>
    /// <seealso cref="InputGeometryCompiler"/>
    /// <seealso cref="InputGeometryBuilder"/>
	public sealed class InputGeometry
	{
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private readonly ChunkyTriMesh mMesh;

        internal InputGeometry(ChunkyTriMesh mesh, Vector3 boundsMin, Vector3 boundsMax)
        {
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
            mMesh = mesh;
        }

        /// <summary>
        /// The minimum bounds of the AABB.
        /// </summary>
        public Vector3 BoundsMin { get { return mBoundsMin; } }

        /// <summary>
        /// The maximum bounds of the AABB.
        /// </summary>
        public Vector3 BoundsMax { get { return mBoundsMax; } }

        /// <summary>
        /// The number of triangles. [Limit: > 0]
        /// </summary>
        public int TriCount { get { return mMesh.TriCount; } }

        internal ChunkyTriMesh Mesh { get { return mMesh; } }

        /// <summary>
        /// Extracts all input geometry for inspection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method exists to permit debugging.
        /// </para>
        /// </remarks>
        /// <param name="areas">The triangle areas.</param>
        /// <returns>The triangle mesh.</returns>
        public TriangleMesh ExtractMesh(out byte[] areas)
        {
            return mMesh.ExtractMesh(out areas);
        }

        /// <summary>
        /// Extracts all input geometry for a particular bounds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method exists to permit debugging.
        /// </para>
        /// <para>
        /// The returned result is only guarenteed to be the result 'seen' by the NMGen build
        /// process.
        /// </para>
        /// </remarks>
        /// <param name="xmin">The minimum x-axis bounds.</param>
        /// <param name="zmin">The minimum z-axis bounds.</param>
        /// <param name="xmax">The maximum x-axis bounds.</param>
        /// <param name="zmax">The maximum z-axis bounds.</param>
        /// <param name="areas">The triangle areas.</param>
        /// <returns>The triangle mesh.</returns>
        public TriangleMesh ExtractMesh(float xmin, float zmin, float xmax, float zmax
            , out byte[] areas)
        {
            byte[] lareas;

            TriangleMesh lmesh = mMesh.ExtractMesh(out lareas);

            List<ChunkyTriMeshNode> nodes = new List<ChunkyTriMeshNode>();

            int triCount = mMesh.GetChunks(xmin, zmin, xmax, zmax, nodes);

            if (triCount == 0)
            {
                areas = new byte[0];
                return new TriangleMesh();
            }

            TriangleMesh result = new TriangleMesh();
            result.verts = lmesh.verts;
            result.vertCount = lmesh.vertCount;

            result.tris = new int[triCount * 3];
            result.triCount = triCount;
            areas = new byte[triCount];

            int i = 0;
            foreach (ChunkyTriMeshNode node in nodes)
            {
                for (int j = 0; j < node.count; j++, i++)
                {
                    result.tris[i * 3 + 0] = lmesh.tris[(node.i + j) * 3 + 0];
                    result.tris[i * 3 + 1] = lmesh.tris[(node.i + j) * 3 + 1];
                    result.tris[i * 3 + 2] = lmesh.tris[(node.i + j) * 3 + 2];
                    areas[i] = lareas[node.i + j];
                }
            }

            return result;
        }
    }
}
