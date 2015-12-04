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
using org.critterai.interop;
using org.critterai.nmgen.rcn;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

// Unity is improperly indicating that mSpan is unused.
#pragma warning disable 414

namespace org.critterai.nmgen
{
    /// <summary>
    /// Provides a representation of the open (unobstructed) space above the solid surfaces of 
    /// a voxel field.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For this type of heightfield, the spans represent the floor and ceiling of the open spaces.
    /// </para>
    /// <para>
    /// Data is stored in a compact, efficient manner.
    /// </para>
    /// <para>
    /// The following process can be used to iterate spans:
    /// </para>
    /// <code>
    /// int w = chf.Width;
    /// int d = chf.Depth;
    /// 
    /// CompactCell[] cells = new CompactCell[w * d];
    /// chf.GetCellData(cells);
    /// 
    /// CompactSpan[] spans = new CompactSpan[chf.SpanCount];
    /// chf.GetSpanData(spans);
    /// 
    /// for (int z = 0; z &lt; d; ++z)
    /// {
    ///     for (int x = 0; x &lt; w; ++x)
    ///     {
    ///         CompactCell c = cells[x + z * w];
    ///         for (int i = (int)c.Index, ni = (int)(c.Index + c.Count)
    ///             ; i &lt; ni
    ///             ; ++i)
    ///         {
    ///             CompactSpan s = spans[i];
    ///             
    ///             // Do something...
    ///             
    ///             // If you have extracted area and distance data, you
    ///             // can access it with the same index.
    ///             // E.g. areas[i] or distance[i].
	///			
    ///             // To access neighbor information...
    ///             
    ///             for (int dir = 0; dir &lt; 4; ++dir)
    ///             {
    ///                 if (s.GetConnection(dir) != CompactSpan.NotConnected)
    ///                 {
    ///                     int nx = x + CompactSpan.GetDirOffsetX(dir);
    ///                     int nz = z + CompactSpan.GetDirOffsetZ(dir);
    ///                     int ni = (int)cells[ax + az * w].Index 
    ///                         + s.GetConnection(dir);
    ///                    
    ///                     // ni represents the index of the neighbor.
    ///                     // So spans[ni], areas[ni], directions[ni]
    ///                     // gets the neighbor.
    ///                 }
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <para>
    /// Spans contain neighbor connection data that can be used to locate axis-neighbors.  
    /// Axis neighbors are spans that are offset from the current cell column as follows:
    /// </para>
    /// <para>
    /// Direction 0 = (-1, 0)<br/>
    /// Direction 1 = (0, 1)<br/>
    /// Direction 2 = (1, 0)<br/>
    /// Direction 3 = (0, -1)
    /// </para>
    /// <para>
    /// These standard offset can be obtained from the <see cref="CompactSpan.GetDirOffsetX"/>
    /// and  <see cref="CompactSpan.GetDirOffsetZ"/> methods.
    /// </para>
    /// <para>
    /// See the earlier example code for information on how to use connection information.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class CompactHeightfield
        : IManagedObject
    {
        private int mWidth = 0;
        private int mDepth = 0;
	    private int mSpanCount = 0;	
	    private int mWalkableHeight = 0;
        private int mWalkableStep = 0;	
	    private int mBorderSize = 0;	
	    private ushort mMaxDistance = 0;
	    private ushort mMaxRegions = 0;

        private Vector3 mBoundsMin;

        private Vector3 mBoundsMax;

	    private float mXZCellSize = 0;
        private float mYCellSize = 0;
        private IntPtr mCells = IntPtr.Zero;   // rcCompactCell[width*depth]
        private IntPtr mSpans = IntPtr.Zero;	// rcCompactSpan[spanCount]
        private IntPtr mDistanceToBorder = IntPtr.Zero;	// ushort[spanCount]
        private IntPtr mAreas = IntPtr.Zero;	// byte[spanCount]

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
        /// The smallest span can be <c>XZCellSize width * XZCellSize depth * YCellSize</c> height.
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
        /// <c>(XZCellSize width * XZCellSize depth * YCellSize)</c> height.
        /// </para>
        /// <para>
        /// A height within the field is converted to world units as follows:</para>
        /// <code>
        /// boundsMin[1] + (height * YCellSize)
        /// </code>
        /// </remarks>
        public float YCellSize { get { return mYCellSize; } }

        /// <summary>
        /// The number of spans in the field.
        /// </summary>
        public int SpanCount { get { return mSpanCount; } }

        /// <summary>
        /// The walkable height used during the build of the field.
        /// </summary>
        public int WalkableHeight { get { return mWalkableHeight; } }

        /// <summary>
        /// The walkable step used during the build of the field.
        /// </summary>
        public int WalkableStep { get { return mWalkableStep; } }

        /// <summary>
        /// The AABB border size used during the build of the field.
        /// </summary>
        public int BorderSize { get { return mBorderSize; } }

        /// <summary>
        /// The maximum distance value for any span within the field.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is only useful if the distance field has been built.
        /// </para>
        /// </remarks>
        public ushort MaxDistance { get { return mMaxDistance; } }

        /// <summary>
        /// The maximum region id for any span within the field.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value is only useful if the regions have been built.
        /// </para>
        /// </remarks>
        public ushort MaxRegion { get { return mMaxRegions; } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType { get { return AllocType.External; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mCells == IntPtr.Zero); } }

        private CompactHeightfield() { }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~CompactHeightfield()
        {
            RequestDisposal();
        }

        /// <summary>
        /// Frees all unmanaged resources controlled by the object and marks it as disposed.
        /// </summary>
        public void RequestDisposal()
        {
            if (!IsDisposed)
            {
                CompactHeightfieldEx.nmcfFreeFieldData(this);
                mWidth = 0;
                mDepth = 0;
                mBorderSize = 0;
                mBoundsMin = Vector3Util.Zero;
                mBoundsMax = Vector3Util.Zero;
                mMaxDistance = 0;
                mMaxRegions = 0;
                mSpanCount = 0;
                mWalkableHeight = 0;
                mWalkableStep = 0;
                mXZCellSize = 0;
                mYCellSize = 0;
            }
        }

        /// <summary>
        /// Loads the heighfield's <see cref="CompactCell"/> data into the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the data into. [Size: >= Width * Depth]</param>
        /// <returns>True if the buffer was successfully loaded.</returns>
        public bool GetCellData(CompactCell[] buffer)
        {
            if (IsDisposed)
                return false;

            return CompactHeightfieldEx.nmcfGetCellData(this
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Loads the heightfield's <see cref="CompactSpan"/> data into the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to load the data into. [Size: >= SpanCount]</param>
        /// <returns>True if the buffer was successfully loaded.</returns>
        public bool GetSpanData(CompactSpan[] buffer)
        {
            if (IsDisposed)
                return false;

            return CompactHeightfieldEx.nmcfGetSpanData(this
                , buffer
                , buffer.Length);
        }

        /// <summary>
        /// Loads the heightfield's distance field data into the provided buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This data is only available after the distance field has been built.
        /// </para>
        /// <para>
        /// Each value represents the estimated distance of the span from the nearest 
        /// boundary/obstruction. The index is the same as for the span data.  
        /// E.g. span[i], distance[i]
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into. [Size: >= SpanCount]</param>
        /// <returns>True if the buffer was successfully loaded.</returns>
        public bool GetDistanceData(ushort[] buffer)
        {
            if (IsDisposed
                || mDistanceToBorder == IntPtr.Zero
                || buffer.Length < mSpanCount)
            {
                return false;
            }

            UtilEx.Copy(mDistanceToBorder
                , buffer
                , mSpanCount);

            return true;
        }

        /// <summary>
        /// Loads the heightfield's area data into the provided buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each value represents the id of the area the span belongs to. The index is the same 
        /// as for the span data. E.g. span[i], area[i]
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into. [Size: >= SpanCount]</param>
        /// <returns>True if the buffer was successfully loaded.</returns>
        public bool GetAreaData(byte[] buffer)
        {
            if (IsDisposed || buffer.Length < mSpanCount)
                return false;

            Marshal.Copy(mAreas
                , buffer
                , 0
                , mSpanCount);

            return true;
        }

        /// <summary>
        /// True if distance data is available.
        /// </summary>
        public bool HasDistanceData 
        { 
            get { return (mDistanceToBorder != IntPtr.Zero); } 
        }

        /// <summary>
        /// Erodes the walkable area within the heightfield by the specified radius.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Basically, any spans that are closer to a boundary or obstruction than the specified 
        /// radius are marked as unwalkable.
        /// </para>
        /// <para>
        /// This method is usually called immediately after the heightfield has been created.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use during the operation. </param>
        /// <param name="radius">The radius to apply. [Units: Spans]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool ErodeWalkableArea(BuildContext context, int radius)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfErodeWalkableArea(context.root
                , radius
                , this);
        }

        /// <summary>
        /// Applies a median filter to the walkable areas. (Removes noise.)
        /// </summary>
        /// <param name="context">The context to use duing the operation. </param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool ApplyMedianFilter(BuildContext context)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfMedianFilterWalkableArea(context.root, this);
        }

        /// <summary>
        /// Applies the area to all spans within the specified bounding box. (AABB)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method will return false if the AABB is completely outside of the heightfield.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="boundsMin">The minimum bounds of the AABB.</param>
        /// <param name="boundsMax">The maximum bounds of the AABB. </param>
        /// <param name="area">The area to apply.</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool MarkBoxArea(BuildContext context
            , Vector3 boundsMin, Vector3 boundsMax
            , byte area)
        {
            if (IsDisposed)
                return false;

            return CompactHeightfieldEx.nmcfMarkBoxArea(context.root
                , ref boundsMin
                , ref boundsMax
                , area
                , this);
        }

        /// <summary>
        /// Applies the area to the all spans within the specified convex polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The y-values of the polygon vertices are ignored.  So the polygon is effectively 
        /// projected onto the xz-plane at yMin, then extruded to yMax.
        /// </para>
        /// <para>
        /// The method will return false if the polygon is completely outside of the heightfield.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="verts">The vertices of the polygon [Length: vertCount]</param>
        /// <param name="yMin">The height of the base of the polygon.</param>
        /// <param name="yMax">The height of the top of the polygon.</param>
        /// <param name="area">The area to apply.</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool MarkConvexPolyArea(BuildContext context
            , Vector3[] verts, float yMin, float yMax
            , byte area)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfMarkConvexPolyArea(context.root
                , verts
                , verts.Length
                , yMin
                , yMax
                , area
                , this);
        }

        /// <summary>
        /// Applied the area to all spans within the specified cylinder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method will return false if the cylinder is completely outside of the heightfield.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="centerBase">The center of the base of the cylinder.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="area">The area to apply.</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool MarkCylinderArea(BuildContext context
            , Vector3 centerBase, float radius, float height
            , byte area)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfMarkCylinderArea(context.root
                , ref centerBase
                , radius
                , height
                , area
                , this);
        }

        /// <summary>
        /// Builds the distance field for the heightfield.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be called before attempting to build region data.
        /// </para>
        /// <para>
        /// The distance data is avaiable via <see cref="MaxDistance"/> and 
        /// <see cref="GetDistanceData"/>.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool BuildDistanceField(BuildContext context)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfBuildDistanceField(context.root, this);
        }

        /// <summary>
        /// Builds region data for the heightfield using watershed partitioning.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Non-null regions consist of connected, non-overlapping walkable spans that form a 
        /// single contour.
        /// </para>
        /// <para>
        /// The region data is available via <see cref="MaxRegion"/> and <see cref="GetSpanData"/>.
        /// </para>
        /// <para>
        /// If a region forms an area that is smaller than <paramref name="minRegionArea"/>, 
        /// all spans in the region is set to <see cref="NMGen.NullRegion"/>.
        /// </para>
        /// <para>
        /// Watershed partitioning can result in smaller than necessary regions, especially 
        /// in diagonal corridors.  <paramref name="mergeRegionArea"/> helps reduce unecessarily 
        /// small regions.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="borderSize">The AABB border size to apply.</param>
        /// <param name="minRegionArea">
        /// The minimum area allowed for unconnected (island) regions. [Units: Spans]
        /// </param>
        /// <param name="mergeRegionArea">
        /// The maximum region size that will be considered for merging with another region.
        /// [Units: Spans]
        /// </param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool BuildRegions(BuildContext context
            , int borderSize, int minRegionArea, int mergeRegionArea)
        {
            if (IsDisposed)
                return false;
            return CompactHeightfieldEx.nmcfBuildRegions(context.root
                , this
                , borderSize
                , minRegionArea
                , mergeRegionArea);
        }

        /// <summary>
        /// Builds region data for the heightfield using simple monotone partitioning.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Non-null regions consist of connected, non-overlapping walkable spans that form a 
        /// single contour.
        /// </para>
        /// <para>
        /// The region data is available via <see cref="MaxRegion"/> and <see cref="GetSpanData"/>.
        /// </para>
        /// <para>
        /// If a region forms an area that is smaller than <paramref name="minRegionArea"/>, 
        /// all spans in the region is set to <see cref="NMGen.NullRegion"/>.
        /// </para>
        /// <para>
        /// Partitioning can result in smaller than necessary regions, especially 
        /// in diagonal corridors.  <paramref name="mergeRegionArea"/> helps reduce unecessarily 
        /// small regions.
        /// </para>
        /// </remarks>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="borderSize">The AABB border size to apply.</param>
        /// <param name="minRegionArea">
        /// The minimum area allowed for unconnected (island) regions. [Units: Spans]
        /// </param>
        /// <param name="mergeRegionArea">
        /// The maximum region size that will be considered for merging with another region.
        /// [Units: Spans]
        /// </param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool BuildRegionsMonotone(BuildContext context
            , int borderSize, int minRegionArea, int mergeRegionArea)
        {
            if (IsDisposed)
                return false;

            return CompactHeightfieldEx.nmcfBuildRegionsMonotone(context.root
                , this
                , borderSize
                , minRegionArea
                , mergeRegionArea);
        }

        /// <summary>
        /// Creates a compact open heightfield from a solid heightfield.
        /// </summary>
        /// <param name="context">The context to use duing the operation.</param>
        /// <param name="sourceField">
        /// The solid heighfield to derive the compact heightfield from.
        /// </param>
        /// <param name="walkableHeight">
        /// The minimum floor to ceiling height that is still considered walkable. 
        /// [Limit: >= <see cref="NMGen.MinWalkableHeight"/>]
        /// </param>
        /// <param name="walkableStep">
        /// The maximum floor to floor step that is still considered walkable.</param>
        /// <returns>True if the operation completed successfully.</returns>
        public static CompactHeightfield Build(BuildContext context
            , Heightfield sourceField
            , int walkableHeight
            , int walkableStep)
        {
            if (context == null
                || sourceField == null || sourceField.IsDisposed
                || walkableHeight < NMGen.MinWalkableHeight
                || walkableStep < 0)
            {
                return null;
            }

            CompactHeightfield field = new CompactHeightfield();

            if (CompactHeightfieldEx.nmcfBuildField(context.root
                , walkableHeight
                , walkableStep
                , sourceField.root
                , field))
            {
                return field;
            }

            return null;
        }

    }
}

#pragma warning restore 414	