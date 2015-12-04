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
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav
{
    /// <summary>
    /// Represents local corner data for a path within a path corridor. 
    /// (Generated during path straightening.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// When path straightening occurs on a path corridor, the waypoints can include corners lying 
    /// on the vertex of a polygon's solid wall segment.  These are the vertices included in this 
    /// data structure.
    /// </para>
    /// <para>
    /// If path straightening does not result in any corners (e.g. path end point is visible) then 
    /// the <see cref="cornerCount"/> will be zero.  So <see cref="cornerCount"/> can't be used to 
    /// detect 'no path'.
    /// </para>
    /// <para>
    /// Certain methods which take objects of this type require a fixed buffer size equal to 
    /// <see cref="MarshalBufferSize"/>.  So be careful when initializing and using objects of 
    /// this type.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class CornerData
    {
        /*
         * Design note:
         * 
         * Implemented as a class to permit use as a buffer.
         * 
         * The layout is designed for efficent marshalling on the native side
         * of interop.  That's why the Vector and poly refs are separated
         * rather than using NavmeshPoint;
         * 
         */

        /// <summary>
        /// The the buffer size required for the object used for interop method calls.
        /// </summary>
        public const int MarshalBufferSize = 4;

        /// <summary>
        /// The corner vertices. [Length: <see cref="MaxCorners"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public Vector3[] verts;

        /// <summary>
        /// The <see cref="WaypointFlag"/>'s for each corner.
        /// [Length: <see cref="MaxCorners"/>]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public WaypointFlag[] flags;

        /// <summary>
        /// The polygon references for each corner. [Length: <see cref="MaxCorners"/>]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The reference is for the polygon being entered at the corner.
        /// </para>
        /// </remarks>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarshalBufferSize)]
        public uint[] polyRefs;

        /// <summary>
        /// Number of corners in the local path. [Limits: 0 &lt;= value &lt;= maxCorners]
        /// </summary>
        public int cornerCount = 0;

        /// <summary>
        /// The corner for the specified index.
        /// </summary>
        /// <param name="index">The corner index. [Limit: &lt;= <see cref="cornerCount"/></param>
        /// <returns>The corner point.</returns>
        public NavmeshPoint this[int index]
        {
            get
            {
                return new NavmeshPoint(polyRefs[index], verts[index]);
            }
        }

        /// <summary>
        /// The maximum number of corners the buffers can hold.
        /// </summary>
        public int MaxCorners { get { return polyRefs.Length; } }

        /// <summary>
        /// Creates an object with buffers sized for use with interop method calls. 
        /// (<see cref="MaxCorners"/> = <see cref="MarshalBufferSize"/>)
        /// </summary>
        public CornerData()
        {
            verts = new Vector3[MarshalBufferSize];
            flags = new WaypointFlag[MarshalBufferSize];
            polyRefs = new uint[MarshalBufferSize];
        }

        /// <summary>
        /// Creates an object with a non-standard buffer size.
        /// </summary>
        /// <param name="maxCorners">The maximum number of corners the buffers can hold.</param>
        public CornerData(int maxCorners)
        {
            verts = new Vector3[maxCorners];
            flags = new WaypointFlag[maxCorners];
            polyRefs = new uint[maxCorners];
        }

        /// <summary>
        /// Copies the contents of the corner buffers from the source to destination.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Data will be lost if the destination buffers are too small to hold the corners 
        /// contained by the source.
        /// </para>
        /// </remarks>
        /// <param name="source">The object to copy from.</param>
        /// <param name="desitation">The object to copy to.</param>
        public static void Copy(CornerData source, CornerData desitation)
        {
            int size = Math.Min(source.MaxCorners, desitation.MaxCorners);

            Array.Copy(source.flags, desitation.flags, size);
            Array.Copy(source.polyRefs, desitation.polyRefs, size);
            Array.Copy(source.verts, desitation.verts, size);
            desitation.cornerCount = Math.Min(size, source.cornerCount);
        }

        /// <summary>
        /// Validates the structure of the the corner buffers. (No content validation is performed.)
        /// </summary>
        /// <param name="buffer">The buffer to validate.</param>
        /// <param name="forMarshalling">True if the buffer must be sized for marshalling.</param>
        /// <returns>True if the structure of the corner buffers is valid.</returns>
        public static bool IsValid(CornerData buffer, bool forMarshalling)
        {
            if (buffer.flags == null
                || buffer.polyRefs == null
                || buffer.verts == null)
            {
                return false;
            }

            int size = (forMarshalling ? MarshalBufferSize : buffer.MaxCorners);

            if (size == 0)
                return false;

            if (buffer.flags.Length == size
                && buffer.polyRefs.Length == size
                && buffer.verts.Length == 3 * size)
            {
                return true;
            }

            return false;
        }
    }
}
