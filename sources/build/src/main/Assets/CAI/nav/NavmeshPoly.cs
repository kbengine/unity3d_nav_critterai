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
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Polygon data for a polygon in a navigation mesh tile.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This structure is provided for debug purposes.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct NavmeshPoly
    {
        /// <summary>
        /// Index to first link in the linked list. 
        /// (Or <see cref="Navmesh.NullLink"/> if there is no link.)
        /// </summary>
        public uint firstLink;

        // Yes, the size has been confirmed as correct. The size is not
        // effected by the maxVertsPerPoly used to construct the navmesh.
        /// <summary>
        /// Indices to the polygon's vertices. 
        /// [Length: <see cref="Navmesh.MaxAllowedVertsPerPoly"/>]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The indices refer vertices in the polygon's <see cref="NavmeshTileData">tile</see>.
        /// </para>
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray
            , SizeConst = Navmesh.MaxAllowedVertsPerPoly)]
	    public ushort[] indices;	                    

        /// <summary>
        /// Packed data representing neighbor polygon ids and flags for each edge.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Length: <see cref="Navmesh.MaxAllowedVertsPerPoly"/>.</para>
        /// <para>
        /// Each entry represents data for the edge starting at the vertex of the same index.  
        /// E.g. The entry at index <c>n</c> represents the edge data for <c>vertex[n]</c> 
        /// to <c>vertex[n+1]</c>.
        /// </para>
        /// <para>
        /// A value of zero indicates the edge has no polygon connection.
        /// (It makes up the border of the navigation mesh.)
        /// </para>
        /// <para>
        /// The polygon id can be found as follows: <c>(int)neighborPolyRefs[n] &amp; 0xff</c>
        /// </para>
        /// <para>
        /// The edge is an external (portal) edge if the following test is true: 
        /// <c>(neighborPolyRefs[n] &amp; Navmesh.ExternalLink) == 0</c>
        /// </para>
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray
            , SizeConst = Navmesh.MaxAllowedVertsPerPoly)]
        public ushort[] neighborPolyRefs;

        /// <summary>
        /// The polygon flags.
        /// </summary>
	    public ushort flags;

        /// <summary>
        /// The number of vertices in the polygon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value will be between 3 and <see cref="Navmesh.MaxAllowedVertsPerPoly"/> inclusive 
        /// for  standard polygons, and 2 for off-mesh connections.
        /// </para>
        /// </remarks>
	    public byte vertCount;

        /// <summary>
        /// A packed value.  See associated properties.
        /// </summary>
        private byte mAreaAndType;		
	
        /// <summary>
        /// The polygon's user defined area.
        /// </summary>
        public byte Area
        {
            get { return (byte)(mAreaAndType & 0x3f); }
        }

        /// <summary>
        /// The type of polygon.
        /// </summary>
        public NavmeshPolyType Type
        {
            get { return (NavmeshPolyType)(mAreaAndType >> 6); }
        }
    }
}
