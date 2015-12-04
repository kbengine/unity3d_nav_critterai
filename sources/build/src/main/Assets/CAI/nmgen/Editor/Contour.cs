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

namespace org.critterai.nmgen
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in voxel space.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the height of the border will vary, the contour will always form a simple polygon 
    /// when projected onto the xz-plane.
    /// </para>
    /// <para>
    /// Minimum bounds and cell size information is needed in order to translate vertex 
    /// coordinates into world space.
    /// </para>
    /// <code>
    /// worldX = boundsMin[0] + vertX * xzCellSize
    /// worldY = boundsMin[1] + vertY * yCellSize
    /// worldZ = boundsMin[2] + vertZ * xzCellSize
    /// </code>
    /// <para>
    /// A contour only exists within the context of a <see cref="ContourSet"/>.
    /// </para>
    /// <para>
    /// Behavior is undefined if used after disposal.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class Contour
        : IManagedObject
    {
        /// <summary>
        /// The mask to apply to the forth element of the vertices data in order to extract region 
        /// ids. (Removes flags from the element.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Example:
        /// </para>
        /// <code>
        /// int region = verts[i * 4 + 3] &amp; Contour.RegionMask;
        /// </code>
        /// </remarks>
        public const int RegionMask = 0xfff;

	    private IntPtr mVerts;			// int[vertCount * 4]
        private int mVertCount;
        private IntPtr mRawVerts;		// int[rawVertCount * 4]
        private int mRawVertCount;
        private ushort mRegion;
        private byte mArea;

        /// <summary>
        /// The number of vertices in the simplified contour.
        /// </summary>
        public int VertCount { get { return mVertCount; } }

        /// <summary>
        /// The number of vertices in the raw contour.
        /// </summary>
        public int RawVertCount { get { return mRawVertCount; } }

        /// <summary>
        /// The region id associated with the contour.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note: The <see cref="RegionMask"/> is not associated with this field.
        /// </para>
        /// </remarks>
        public ushort Region { get { return mRegion; } }

        /// <summary>
        /// The area associated with the contour.
        /// </summary>
        public byte Area { get { return mArea; } }

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public bool IsDisposed { get { return (mVerts == IntPtr.Zero); } }

        /// <summary>
        /// The type of unmanaged resources within the object.
        /// </summary>
        public AllocType ResourceType 
        { 
            get { return AllocType.ExternallyManaged; }
        }

        internal Contour() { }

        internal void Reset()
        {
            mVertCount = 0;
            mRawVertCount = 0;
            mRegion = NMGen.NullRegion;
            mArea = NMGen.NullArea;
            mVerts = IntPtr.Zero;
            mRawVerts = IntPtr.Zero;
        }

        /// <summary>
        /// Has no effect on the object. (The object owner will handle disposal.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Objects of this type are always owned by a <see cref="ContourSet"/> object, which 
        /// manages its disposal.
        /// </para>
        /// </remarks>
        public void RequestDisposal()
        {
            // Can't be disposed manually.  Owner will use reset.
        }

        /// <summary>
        /// Loads the vertices and connection data for the simplified contour into the specified 
        /// buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The simplified contour is a version of the raw contour with all 'unnecessary' vertices 
        /// removed.  Whether a vertex is considered unnecessary depends on the contour build 
        /// process.
        /// </para>
        /// <para>
        /// The data is represented as follows: <c>(x, y, z, r) * VertCount</c>.
        /// </para>
        /// <para>
        /// A contour edge is formed by the current and next vertex. The r-value indicates the 
        /// region and connection information for the edge.
        /// </para>
        /// <para>
        /// The region id is obtained by applying <see cref="RegionMask"/>. 
        /// E.g. <c>regionId = (vert[i * 4 + 3] &amp; RegionMask)</c>
        /// </para>
        /// <para>
        /// The edge is not connected if the region id is <see cref="NMGen.NullRegion"/>.
        /// </para>
        /// <para>
        /// If the r-value has the <see cref="ContourFlags.AreaBorder"/> flag set, then the edge 
        /// represents an change in area as well as region.
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into. [Size: >= 4 * VertCount]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool GetVerts(int[] buffer)
        {
            if (IsDisposed)
                return false;

            Marshal.Copy(mVerts, buffer, 0, mVertCount * 4);

            return true;
        }

        /// <summary>
        /// Loads the vertices and connection data for the raw contour into the specified buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See the <see cref="GetVerts"/> method for details on the  element layout.
        /// </para>
        /// </remarks>
        /// <param name="buffer">The buffer to load the data into. [Size: >= 4 * VertCount]</param>
        /// <returns>True if the operation completed successfully.</returns>
        public bool GetRawVerts(int[] buffer)
        {
            if (IsDisposed)
                return false;

            Marshal.Copy(mRawVerts, buffer, 0, mVertCount * 4);

            return true;
        }
    }
}
