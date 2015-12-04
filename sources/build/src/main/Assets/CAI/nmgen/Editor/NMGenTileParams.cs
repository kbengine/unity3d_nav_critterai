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
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen
{
    /// <summary>
    /// Tile configuration parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These values are derived during the build process.
    /// </para>
    /// <para>
    /// Implemented as a class with public fields in order to support Unity serialization.  Care 
    /// must be taken not to set the fields to invalid values.
    /// </para>
    /// </remarks>
    /// <seealso cref="NMGenParams"/>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class NMGenTileParams
    {
        /// <summary>
        /// The x-index of the tile.
        /// </summary>
        public int tileX;

        /// <summary>
        /// The z-index of the tile.
        /// </summary>
        public int tileZ;

        /// <summary>
        /// The minimum bounds of the tile.
        /// </summary>
        public Vector3 boundsMin;

        /// <summary>
        /// The maximum bounds of the tile.
        /// </summary>
        public Vector3 boundsMax;

        /// <summary>
        /// The z-index of the tile within the tile grid. [Limit: >= 0]
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is a zero based tile index representing the position of the tile along the 
        /// depth of the tile grid.
        /// </para>
        /// <para>
        /// The value is auto-clamped to a valid value.
        /// </para>
        /// </remarks>
        public int TileZ
        {
            get { return tileZ; }
            set { tileZ = Math.Max(0, value); }
        }

        /// <summary>
        /// The x-index of the tile within the tile grid. [Limit: >= 0]
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is a zero based tile index representing the position of the tile along the 
        /// width of the tile grid.
        /// </para>
        /// <para>
        /// The value is auto-clamped to a valid value.
        /// </para>
        /// </remarks>
        public int TileX
        {
            get { return tileX; }
            set { tileX = Math.Max(0, value); }
        }

        /// <summary>
        /// The minimum bounds of the tile's AABB. [Units: World]
        /// </summary>
        public Vector3 BoundsMin 
        { 
            get { return boundsMin; }
            set { boundsMin = value; }
        }

        /// <summary>
        /// The maximum bounds of the tile's AABB. [Units: World]
        /// </summary>
        public Vector3 BoundsMax
        {
            get { return boundsMax; }
            set { boundsMax = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tx">The x-index of the tile within the tile grid. [Limit: >= 0]</param>
        /// <param name="tz">The z-index of the tile within the tile grid. [Limit: >= 0]</param>
        /// <param name="boundsMin">The minimum bounds of the tile.</param>
        /// <param name="boundsMax">The maximum bounds of the tile.</param>
        public NMGenTileParams(int tx, int tz, Vector3 boundsMin, Vector3 boundsMax) 
        {
            // Note: Use properties for auto-clamping.
            TileX = tx;
            TileZ = tz;
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
        }
    }
}
