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
using System.Collections.Generic;
using org.critterai.geom;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// A builder for the <see cref="ChunkyTriMesh"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The standard use case is as follows:
    /// </para>
    /// <ol>
    /// <li>Create the builder using <see cref="Create"/></li>
    /// <li>Call <see cref="Build"/> until it returns true.</li>
    /// <li>Get the result from <see cref="Result"/>.</li>
    /// </ol>
    /// <para>
    /// A builder object cannot be re-used. (Single use.)
    /// </para>
    /// <para>
    /// The builder is safe to run on a separate thread as long as the objects used to create 
    /// the builder are not mutated while the build is in-progress.
    /// </para>
    /// </remarks>
    public sealed class ChunkyTriMeshBuilder
    {
        private enum BuildState
        {
            PreProcessing,
            Running,
            Complete,
        }

        private struct BoundsItem
        {
            public float xmin;
            public float zmin;
            public float xmax;
            public float zmax;
            public int i;
        }

        private class BoundsItemCompareX
            : IComparer<BoundsItem>
        {
            public int Compare(BoundsItem x, BoundsItem y)
            {
                if (x.xmin < y.xmin)
                    return -1;
                if (x.xmin > y.xmin)
                    return 1;
                return 0;
            }
        }

        private class BoundsItemCompareZ
            : IComparer<BoundsItem>
        {
            public int Compare(BoundsItem x, BoundsItem y)
            {
                if (x.zmin < y.zmin)
                    return -1;
                if (x.zmin > y.zmin)
                    return 1;
                return 0;
            }
        }

        private class BuildContext
        {
            private readonly Stack<Subdivide> mPool = new Stack<Subdivide>();

            public int[] tris;
            public byte[] areas;
            public ChunkyTriMeshNode[] nodes;
            public BoundsItem[] items;
            public int[] inTris;
            public byte[] inAreas;
            public int curNode;
            public int curTri;
            public int trisPerChunk;

            public Subdivide Get(int imin, int imax)
            {
                if (mPool.Count == 0)
                    return new Subdivide(imin, imax);

                Subdivide result = mPool.Pop();
                result.Reset(imin, imax);
                return result;
            }

            public void Return(Subdivide item)
            {
                mPool.Push(item);
            }

            public void Reset()
            {
                mPool.Clear();
                tris = null;
                areas = null;
                nodes = null;
                items = null;
                inTris = null;
                inAreas = null;
            }
        }

        private class Subdivide
        {
            private enum State
            {
                None,
                Right,
                Finalize,
                Complete
            }

            private State state = State.None;
            private int icur;
            private int imin;
            private int imax;
            private int isplit;  // Yes, this is necessary.
            private int inode;

            public Subdivide(int imin, int imax)
            {
                this.imin = imin;
                this.imax = imax;
            }

            public void Reset(int imin, int imax)
            {
                state = State.None;
                icur = 0;
                this.imin = imin;
                this.imax = imax;
                isplit = 0;
                inode = 0;
            }

            public Subdivide Build(BuildContext data)
            {
                switch (state)
                {
                    case State.None:

                        Subdivide c = Initialize(data);

                        state = (c == null) ? State.Complete : State.Right;

                        return c;

                    case State.Right:

                        state = State.Finalize;

                        return (data.Get(isplit, imax));

                    case State.Finalize:

                        int iescape = data.curNode - icur;

                        // Negative index means escape.
                        data.nodes[inode].i = -iescape;

                        state = State.Complete;

                        break;
                }

                return null;
            }

            private Subdivide Initialize(BuildContext data)
            {
                int inum = imax - imin;
                icur = data.curNode;

                ChunkyTriMeshNode[] nodes = data.nodes;
                BoundsItem[] items = data.items;

                if (data.curNode > nodes.Length)
                    return null;

                inode = data.curNode++;

                if (inum <= data.trisPerChunk)
                {
                    // Leaf
                    DeriveExtents(items
                        , imin
                        , imax
                        , ref nodes[inode]);

                    // Copy triangles.
                    nodes[inode].i = data.curTri;
                    nodes[inode].count = inum;

                    for (int i = imin; i < imax; ++i)
                    {
                        int pi = items[i].i * 3;
                        int pd = data.curTri * 3;

                        data.tris[pd + 0] = data.inTris[pi + 0];
                        data.tris[pd + 1] = data.inTris[pi + 1];
                        data.tris[pd + 2] = data.inTris[pi + 2];

                        data.areas[data.curTri] = data.inAreas[items[i].i];

                        data.curTri++;
                    }
                }
                else
                {
                    // Split
                    DeriveExtents(items, imin, imax, ref nodes[inode]);

                    int axis =
                        (nodes[inode].zmax - nodes[inode].zmin
                            > nodes[inode].xmax - nodes[inode].xmin)
                            ? 1 : 0;

                    if (axis == 0)
                    {
                        // Sort along x-axis
                        Array.Sort<BoundsItem>(items, imin, inum, mCompareX);
                    }
                    else if (axis == 1)
                    {
                        // Sort along y-axis
                        Array.Sort<BoundsItem>(items, imin, inum, mCompareZ);
                    }

                    isplit = imin + inum / 2;

                    // Left
                    return data.Get(imin, isplit);
                }

                return null;
            }
        }

        /// <summary>
        /// The minimum allowed triangles per chunk.
        /// </summary>
        public const int MinAllowedTrisPerChunk = 64;

        private static readonly BoundsItemCompareX mCompareX = new BoundsItemCompareX();
        private static readonly BoundsItemCompareZ mCompareZ = new BoundsItemCompareZ();

        private static int mTriangleIterations = 100000;
        private static int mNodeIterations = 100;

        /// <summary>
        /// The number of nodes the build will process in a single build step. [>= 1]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used for tuning responsiveness.
        /// </para>
        /// </remarks>
        public static int ChunkTuneValue
        {
            get { return mNodeIterations; }
            set { mNodeIterations = Math.Max(1, value); }
        }

        /// <summary>
        /// The number of triangles the build will process in a single build step. [>= 1]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used for tuning responsiveness.
        /// </para>
        /// </remarks>
        public static int PreprocessTuneValue
        {
            get { return mTriangleIterations; }
            set { mTriangleIterations = Math.Max(1, value); }
        }

        private readonly int mTriCount;
        private readonly Vector3[] mVerts;
        private readonly int mVertCount;
        private ChunkyTriMesh mMesh;

        private int mIter = 0;
        private BuildState mState;

        private readonly BuildContext buildData = new BuildContext();
        private readonly Stack<Subdivide> mStack = new Stack<Subdivide>();
        
        /// <summary>
        /// The mesh created by the build. (Only available on successful completion.)
        /// </summary>
        public ChunkyTriMesh Result { get { return mMesh; } }

        /// <summary>
        /// True if the build is finished.
        /// </summary>
        public bool IsFinished { get { return (mMesh != null); } }

        private ChunkyTriMeshBuilder(Vector3[] verts
            , int vertCount
            , int[] tris
            , byte[] areas
            , int triCount
            , int trisPerChunk)
        {
            mVerts = verts;
            mVertCount = vertCount;

            buildData.inTris = tris;
            buildData.inAreas = areas;

            buildData.items = new BoundsItem[triCount];
            buildData.tris = new int[triCount * 3];
            buildData.areas = new byte[triCount];

            int nchunks = (triCount + trisPerChunk - 1) / trisPerChunk;
            buildData.nodes = new ChunkyTriMeshNode[nchunks * 4];

            buildData.trisPerChunk = trisPerChunk;
            mTriCount = triCount;
        }

        /// <summary>
        /// Performs a single build step.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be called repeatedly until it resturns false in order
        /// to complete the build. (Useful in GUI environments.)
        /// </para>
        /// </remarks>
        /// <returns>
        /// True if the build is still underway and another call is required. False if the build 
        /// is finished.
        /// </returns>
        public bool Build()
        {
            switch (mState)
            {
                case BuildState.PreProcessing:

                    UpdateInitialize();
                    return true;

                case BuildState.Running:

                    UpdateSubdivide();
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Performs the build in a single step.
        /// </summary>
        public void BuildAll()
        {
            while (Build()) { }
        }

        private void UpdateSubdivide()
        {
            mIter = 0;
            while (mStack.Count > 0 && mIter < mNodeIterations)
            {
                Subdivide c = mStack.Peek().Build(buildData);

                if (c == null)
                    // Finished.
                    buildData.Return(mStack.Pop());
                else
                    // New sub-divide.
                    mStack.Push(c);

                mIter++;
            }

            if (mStack.Count == 0)
            {
                mMesh = new ChunkyTriMesh(mVerts
                    , mVertCount
                    , buildData.tris
                    , buildData.areas
                    , mTriCount
                    , buildData.nodes
                    , buildData.curNode);

                buildData.Reset();  // Release references.

                mState = BuildState.Complete;
            }
        }

        private void UpdateInitialize()
        {
            BoundsItem[] items = buildData.items;
            int[] tris = buildData.inTris;

            int target = Math.Min(mTriCount, mIter + mTriangleIterations);

            for (; mIter < target; mIter++)
            {
                int pi = mIter * 3;
                items[mIter].i = mIter;

                // Calc triangle XZ bounds.
                items[mIter].xmin =
                    items[mIter].xmax = mVerts[tris[pi]].x;
                items[mIter].zmin =
                    items[mIter].zmax = mVerts[tris[pi]].z;

                for (int j = 1; j < 3; j++)
                {
                    // const float* v = &verts[t[j] * 3];

                    float val = mVerts[tris[pi + j]].x;

                    if (val < items[mIter].xmin)
                        items[mIter].xmin = val;

                    if (val > items[mIter].xmax)
                        items[mIter].xmax = val;

                    val = mVerts[tris[pi + j]].z;

                    if (val < items[mIter].zmin)
                        items[mIter].zmin = val;

                    if (val > items[mIter].zmax)
                        items[mIter].zmax = val;
                }
            }

            if (target < mTriCount)
                // Still more to do.
                return;

            buildData.curNode = 0;
            buildData.curTri = 0;

            mStack.Push(new Subdivide(0, mTriCount));

            mState = BuildState.Running;
        }

        private static void DeriveExtents(BoundsItem[] items,
                int imin, int imax,
                ref ChunkyTriMeshNode node)
        {
            node.xmin = items[imin].xmin;
            node.zmin = items[imin].zmin;

            node.xmax = items[imin].xmax;
            node.zmax = items[imin].zmax;

            for (int i = imin + 1; i < imax; ++i)
            {
                if (items[i].xmin < node.xmin)
                    node.xmin = items[i].xmin;

                if (items[i].zmin < node.zmin)
                    node.zmin = items[i].zmin;

                if (items[i].xmax > node.xmax)
                    node.xmax = items[i].xmax;

                if (items[i].zmax > node.zmax)
                    node.zmax = items[i].zmax;
            }
        }

        /// <summary>
        /// Creates a single use builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return null on parameter errors, an input mesh with zero triangles,
        /// and structural issues in the mesh object or area array.  Does not perform a validation 
        /// of the mesh or area content.  (E.g. Does not check for invalid mesh indices.)
        /// </para>
        /// </remarks>
        /// <param name="mesh">The triangle mesh to chunk.</param>
        /// <param name="areas">The areas for each triangle in the mesh. (Null not allowed.)</param>
        /// <param name="trisPerChunk">
        /// The maximum number of triangles per chunk. 
        /// [Limit: >= <see cref="MinAllowedTrisPerChunk"/>]
        /// </param>
        /// <returns>A builder, or null on error.</returns>
        public static ChunkyTriMeshBuilder Create(TriangleMesh mesh
            , byte[] areas
            , int trisPerChunk)
        {
            if (mesh == null 
                || areas == null
                || mesh.triCount == 0
                || !TriangleMesh.IsValid(mesh, false)
                || areas.Length < mesh.triCount)
            {
                return null;
            }

            trisPerChunk = Math.Max(MinAllowedTrisPerChunk, trisPerChunk);

            return new ChunkyTriMeshBuilder(mesh.verts, mesh.vertCount
                , mesh.tris, areas, mesh.triCount
                , trisPerChunk);
        }
    }
}
