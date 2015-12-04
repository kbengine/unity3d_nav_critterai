/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using org.critterai.geom;
using org.critterai.interop;
using org.critterai.nmgen.rcn;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// A heightfield representing obstructed space.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When used in the context of a heighfield, the term voxel refers to an area 
    /// <see cref="XZCellSize"/> in width, <see cref="XZCellSize"/> in depth, 
    /// and <see cref="YCellSize"/> in height.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Heightfield
        : IManagedObject
    {
        /*
         * Design notes:
         * 
         * I ran into complications with implementing this class with a data
         * layout matching the native structure. The cause appears to be the 
         * pointer to a pointer field in the native structure. So I switched
         * to the root pattern with some duplication of data on this size
         * of the boundary for performance reasons.
         * 
         * The AddSpan method is not supported yet because of a bug in Recast.
         * http://code.google.com/p/recastnavigation/issues/detail?id=167
         * 
         */

        private int mWidth = 0;
        private int mDepth = 0;

        private Vector3 mBoundsMin;
        private Vector3 mBoundsMax;

        private float mXZCellSize = 0;
        private float mYCellSize = 0;

        internal IntPtr root;

        /// <summary>
        /// The width of the heightfield. (Along the x-axis in cell units.)
        /// </summary>
        public int Width { get { return mWidth; } }

        /// <summary>
        /// The depth of the heighfield. (Along the z-axis in cell units.)
        /// </summary>
        public int Depth { get { return mDepth; } }

        /// <summary>
        /// The minimum bounds of the heightfield in world space. 
        /// </summary>
        /// <returns>The minimum bounds of the heighfield.</returns>
        public Vector3 BoundsMin { get { return mBoundsMin; } }

        /// <summary>
        /// The maximum bounds of the heightfield in world space. 
        /// </summary>
        /// <returns>The maximum bounds of the heightfield.</returns>
        public Vector3 BoundsMax { get { return mBoundsMax; } }

        /// <summary>
        /// The width/depth size of each cell. (On the xz-plane.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The smallest span can be 
        /// <c>(XZCellSize width * XZCellSize depth * YCellSize height)</c>.
        /// </para>
        /// <para>
        /// A width or depth value within the field can be converted to world units as follows:
        /// </para>
        /// <code>
        /// boundsMin[0] + (width * XZCellSize)
        /// boundsMin[2] + (depth * XZCellSize)
        /// </code>
        /// </remarks>
        public float XZCellSize { get { return mXZCellSize; } }

        /// <summary>
        /// The height increments for span data.  (On the y-axis.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The smallest span can be 
        /// <c>(XZCellSize width * XZCellSize depth * YCellSize height)</c>.
        /// </para>
        /// <para>
        /// A height within the field is converted to world units as follows:
        /// </para> 
        /// <code>
        /// boundsMin[1] + (height * YCellSize)
        /// </code>
        /// </remarks>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return root == IntPtr.Zero; } }

        private Heightfield(IntPtr root
            , int width, int depth
            , Vector3 boundsMin, Vector3 boundsMax
            , float xzCellSize, float yCellSize)
        {
            this.root = root;
            mWidth = width;
            mDepth = depth;
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
            mYCellSize = yCellSize;
            mXZCellSize = xzCellSize;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Heightfield()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Creates a new heightfield object.
        /// </summary>
        /// <param name="width">The width of the field. [Limit: >= 1] [Units: Cells]</param>
        /// <param name="depth">The depth of the field. [Limit: >= 1] [Units: Cells]</param>
        /// <param name="boundsMin">The minimum bounds of the field's AABB. [Units: World]</param>
        /// <param name="boundsMax">The maximum bounds of the field's AABB. [Units: World]</param>
        /// <param name="xzCellSize">
        /// The xz-plane cell size. [Limit:>= <see cref="NMGen.MinCellSize"/>] [Units: World]
        /// </param>
        /// <param name="yCellSize">
        /// The y-axis span increments. [Limit:>= <see cref="NMGen.MinCellSize"/>] [Units: World]
        /// </param>
        /// <returns>The heightfield, or null on error.</returns>
        public static Heightfield Create(int width, int depth
            , Vector3 boundsMin, Vector3 boundsMax
            , float xzCellSize, float yCellSize)
        {
            if (width < 1 || depth < 1
                || !TriangleMesh.IsBoundsValid(boundsMin, boundsMax)
                || xzCellSize < NMGen.MinCellSize
                || yCellSize < NMGen.MinCellSize)
            {
                return null;
            }

            IntPtr root = HeightfieldEx.nmhfAllocField(width, depth
                , ref boundsMin, ref boundsMax, xzCellSize, yCellSize);

            if (root == IntPtr.Zero)
                return null;

            return new Heightfield(root
                , width, depth
                , boundsMin, boundsMax
                , xzCellSize, yCellSize);
        }

        /// <summary>
        /// Frees all resources and marks object as disposed.
        /// </summary>
        public void RequestDisposal() 
        {
            if (!IsDisposed)
            {
                HeightfieldEx.nmhfFreeField(root);
                root = IntPtr.Zero;
                mWidth = 0;
                mDepth = 0;
                mXZCellSize = 0;
                mYCellSize = 0;
                mBoundsMin = Vector3Util.Zero;
                mBoundsMax = Vector3Util.Zero;
            }
        }

        /// <summary>
        /// The number of spans in the field.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a non-trivial method call.  Cache the result when possible.
        /// </para>
        /// </remarks>
        /// <returns>The number of spans in the field.</returns>
        public int GetSpanCount()
        {
            if (IsDisposed)
                return 0;
            return HeightfieldEx.nmhfGetHeightFieldSpanCount(root);
        }

        /// <summary>
        /// Gets an buffer that is sized to fit the maximum number of spans within a column of 
        /// the field.
        /// </summary>
        /// <returns>A buffer that is sized to fit the maximum spans within a column.</returns>
        public HeightfieldSpan[] GetSpanBuffer()
        {
            if (IsDisposed)
                return null;

            int size = HeightfieldEx.nmhfGetMaxSpansInColumn(root);
            return new HeightfieldSpan[size];
        }

        /// <summary>
        /// Gets the spans within the specified column.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The spans will be ordered from lowest height to highest.
        /// </para>
        /// <para>
        /// The <see cref="GetSpanBuffer"/> method can be used to get a properly sized buffer.
        /// </para>
        /// </remarks>
        /// <param name="widthIndex">
        /// The width index. [Limits: 0 &lt;= value &lt; <see cref="Width"/>]
        /// </param>
        /// <param name="depthIndex">
        /// The depth index. [Limits: 0 &lt;= value &lt; <see cref="Depth"/>]
        /// </param>
        /// <param name="buffer">
        /// The buffer to load the result into. [Size: Maximum spans in a column]
        /// </param>
        /// <returns>The number of spans returned.</returns>
        public int GetSpans(int widthIndex
            , int depthIndex
            , HeightfieldSpan[] buffer)
        {
            if (IsDisposed)
                return -1;

            return HeightfieldEx.nmhfGetSpans(root
                , widthIndex
                , depthIndex
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Marks non-walkable spans as walkable if their maximum is within walkableStep of a 
        /// walkable neighbor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example of test: <c>Math.Abs(currentSpan.Max - neighborSpan.Max) &lt; walkableStep</c>
        /// </para>
        /// <para
        /// >Allows the formation of walkable regions that will flow over low lying objects such 
        /// as curbs, and up structures such as stairways.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableStep">
        /// The maximum allowed difference between span maximum's for the step to be considered 
        /// waklable. [Limit: > 0]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool MarkLowObstaclesWalkable(BuildContext context, int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfFilterLowHangingWalkableObstacles(context.root
                , walkableStep
                , root);
        }

        /// <summary>
        /// Marks spans that are ledges as not-walkable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A ledge is a span with a neighbor whose maximum is farther away than walkableStep.  
        /// Example: <c>Math.Abs(currentSpan.Max - neighborSpan.Max) > walkableStep</c>
        /// </para>
        /// <para>
        /// This method removes the impact of the overestimation of conservative voxelization so 
        /// the resulting mesh will not have regions hanging in the air over ledges.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableHeight">
        /// The maximum floor to ceiling height that is considered still walkable. 
        /// [Limit: > <see cref="NMGen.MinWalkableHeight"/>]
        /// </param>
        /// <param name="walkableStep">
        /// The maximum allowed difference between span maximum's for the step to be considered 
        /// walkable. [Limit: > 0]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool MarkLedgeSpansNotWalkable(BuildContext context
            , int walkableHeight, int walkableStep)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfFilterLedgeSpans(context.root
                , walkableHeight
                , walkableStep
                , root);
        }

        /// <summary>
        /// Marks walkable spans as not walkable if the clearence above the span is less than the 
        /// specified height.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For this method, the clearance above the span is the distance from the span's maximum 
        /// to the next higher span's minimum. (Same column.)
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="walkableHeight">
        /// The maximum allowed floor to ceiling height that is considered still walkable.
        /// [Limit: > <see cref="NMGen.MinWalkableHeight"/>]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool MarkLowHeightSpansNotWalkable(BuildContext context, int walkableHeight)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfFilterWalkableLowHeightSpans(context.root
                , walkableHeight
                , root);
        }

        /// <summary>
        /// Voxelizes a triangle into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The triangle vertices. [(vertA, vertB, vertC)]</param>
        /// <param name="area">
        /// The id of the area the triangle belongs to. [Limit: &lt;= <see cref="NMGen.MaxArea"/>]
        /// </param>
        /// <param name="flagMergeThreshold">
        /// The distance where the walkable flag is favored over the non-walkable flag. 
        /// [Limit: >= 0] [Normal: 1]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool AddTriangle(BuildContext context
            , Vector3[] verts, byte area, int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfRasterizeTriangle(context.root
                , verts
                , area
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the triangles in the provided mesh into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="mesh">The triangle mesh.</param>
        /// <param name="areas">
        /// The ids of the areas the triangles belong to. 
        /// [Limit: &lt;= <see cref="NMGen.MaxArea"/>] [Size: >= mesh.triCount]
        /// </param>
        /// <param name="flagMergeThreshold">
        /// The distance where the walkable flag is favored over the non-walkable flag. 
        /// [Limit: >= 0] [Normal: 1]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context, TriangleMesh mesh, byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfRasterizeTriMesh(context.root
                , mesh.verts
                , mesh.vertCount
                , mesh.tris
                , areas
                , mesh.triCount
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the triangles from the provided <see cref="ChunkyTriMesh"/> into the 
        /// heightfield.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The chunks that are voxelized is controled by the bounds parameters.
        /// </para>
        /// </remarks>
        /// <param name="context">The build context.</param>
        /// <param name="mesh">The mesh.</param>
        /// <param name="boundsMin">The minimum bounds for the mesh query.</param>
        /// <param name="boundsMax">The maximum bounds for the mesh query.</param>
        /// <param name="flagMergeThreshold">
        /// The distance where the walkable flag is favored over the non-walkable flag. 
        /// [Limit: >= 0] [Normal: 1]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context, ChunkyTriMesh mesh
            , Vector3 boundsMin, Vector3 boundsMax
            , int flagMergeThreshold)
        {
            if (IsDisposed || mesh == null || mesh.IsDisposed)
                return false;

            List<ChunkyTriMeshNode> nodeList = new List<ChunkyTriMeshNode>();
            
            int triCount =  mesh.GetChunks(boundsMin.x, boundsMin.z
                , boundsMax.x, boundsMax.z
                , nodeList);

            if (triCount == 0)
                return true;

            return HeightfieldEx.nmhfRasterizeNodes(context.root
                , mesh.verts
                , mesh.tris
                , mesh.areas
                , nodeList.ToArray()
                , nodeList.Count
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the provided triangles into the heightfield.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unlike many other methods in the library, the arrays must be sized exactly to the 
        /// content.  If you need to pass buffers, use the method that takes a 
        /// <see cref="TriangleMesh"/> object.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The vertices. [Length: >= vertCount] (No buffering allowed.)</param>
        /// <param name="tris">
        /// The triangles. [(vertAIndex, vertBIndex, vertCIndex) * triCount]
        /// </param>
        /// <param name="areas">
        /// The ids of the areas the triangles belong to. 
        /// [Limit: &lt;= <see cref="NMGen.MaxArea"/>] [Size: >= triCount]
        /// </param>
        /// <param name="flagMergeThreshold">
        /// The distance where the walkable flag is favored over the non-walkable flag. 
        /// [Limit: >= 0] [Normal: 1]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context
            , Vector3[] verts, ushort[] tris, byte[] areas
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfRasterizeTriMeshShort(context.root
                , verts
                , verts.Length / 3
                , tris
                , areas
                , tris.Length / 3
                , root
                , flagMergeThreshold);
        }

        /// <summary>
        /// Voxelizes the provided triangles into the heightfield.
        /// </summary>
        /// <param name="context">The context to use for the operation</param>
        /// <param name="verts">The triangles. [(vertA, vertB, vertC) * triCount]</param>
        /// <param name="areas">
        /// The ids of the areas the triangles belong to.
        /// [Limit: &lt;= <see cref="NMGen.MaxArea"/>] [Size: >= triCount]
        /// </param>
        /// <param name="triCount">The number of triangles in the vertex array.</param>
        /// <param name="flagMergeThreshold">
        /// The distance where the walkable flag is favored over the non-walkable flag. 
        /// [Limit: >= 0] [Normal: 1]
        /// </param>
        /// <returns>True if the operation was successful.</returns>
        public bool AddTriangles(BuildContext context
            , Vector3[] verts, byte[] areas, int triCount
            , int flagMergeThreshold)
        {
            if (IsDisposed)
                return false;

            return HeightfieldEx.nmhfRasterizeTriangles(context.root
                , verts, areas, triCount
                , root
                , flagMergeThreshold);
        }
    }
}
