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

namespace org.critterai.nmgen
{
    /// <summary>
    /// Respresents a span within a <see cref="CompactHeightfield"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The span represents open, unobstructed space within a heightfield column.
    /// </para>
    /// <para>
    /// See the <see cref="CompactHeightfield"/> documentation for a discussion of iterating spans 
    /// and searching span connections.
    /// </para>
    /// <para>
    /// Useful instances of this type can only by obtained from a <see cref="CompactHeightfield"/> 
    /// object.
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CompactSpan
    {
        private static readonly int[] offsetX = new int[4] { -1, 0, 1, 0 };
        private static readonly int[] offsetZ = new int[4] { 0, 1, 0, -1 };

        /// <summary>
        /// The value returned by <see cref="GetConnection"/> if the specified direction is not 
        /// connected.
        /// </summary>
        public const int NotConnected = 0x3f;

        /// <summary>
        /// The maximum for <see cref="Height"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a span has no other span above it, its <see cref="Height"/> will return this value.
        /// </para>
        /// </remarks>
        public const int MaxHeight = 0xffff;

        private ushort mFloor;
        private ushort mRegion;
        private uint mPacked;    // [Cons, Height]

        /// <summary>
        /// The height of the span's lower extent. (Measured from the heightfield's base.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The span's floor height in world units can be derived as follows:
        /// </para>
        /// <code>
        /// fieldBoundsMin[1] + (Floor * fieldYCellSize)
        /// </code>
        /// </remarks>
        public ushort Floor { get { return mFloor; } }

        /// <summary>
        /// The id of the region the span belongs to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Non-null regions consist of connected, non-overlapping walkable spans.
        /// </para>
        /// <para>
        /// A value of <see cref="NMGen.NullRegion"/> indicates the span is not part of any region, 
        /// or region data has not been built.
        /// </para>
        /// </remarks>
        public ushort Region { get { return mRegion; } }

        /// <summary>
        /// The packed neighbor connection data for the span.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Normally <see cref="GetConnection"/> is used to get the unpacked version of this data.
        /// </para>
        /// </remarks>
        public uint Connections  { get { return (mPacked & 0xffffff); } }

        /// <summary>
        /// Gets neighbor connection data for the specified direction.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Can be used to locate the neighbor span in the data structure. See the 
        /// <see cref="CompactHeightfield"/> documentation for details.
        /// </para>
        /// </remarks>
        /// <param name="direction">The direction. [Limits: 0 &lt;= value &lt; 4]</param>
        /// <returns>
        /// The connection data for the specified direction, or <see cref="NotConnected"/> if 
        /// there is no connection.
        /// </returns>
        public int GetConnection(int direction)
        {
            return (int)((Connections >> (direction * 6)) & 0x3f);
        }

        /// <summary>
        /// The height of the span's upper extent. (Measured from the span's floor.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The span's height in world units can be derived as follows:
        /// </para>
        /// <code>
        /// fieldBoundsMin[1] + ((Floor + Height) * fieldYCellSize)
        /// </code>
        /// </remarks>
        public byte Height { get { return (byte)(mPacked >> 24); } }

        /// <summary>
        /// Gets the standard width offset for the specified direction.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction value will be automatically wrapped.  So the a value of 6 will be 
        /// interpreted as 2.
        /// </para>
        /// </remarks>
        /// <param name="direction">The direction. [Limits: 0 &lt;= value &lt; 4]</param>
        /// <returns>
        /// The width offset to the apply to the current cell position to move in the direction.
        /// </returns>
        public static int GetDirOffsetX(int direction)
        {
            return offsetX[direction & 0x03];
        }

        /// <summary>
        /// Gets the standard depth offset for the specified direction.
        /// </summary>
        /// <remarks>
        /// <para>The direction value will be automatically wrapped.  So the a value of 6 will be 
        /// interpreted as 2.
        /// </para>
        /// </remarks>
        /// <param name="direction">The direction. [Limits: 0 &lt;= value &lt; 4]</param>
        /// <returns>
        /// The depth offset to the apply to the current cell position to move in the direction.
        /// </returns>
        public static int GetDirOffsetZ(int direction)
        {
            return offsetZ[direction & 0x03];
        }
    }
}
