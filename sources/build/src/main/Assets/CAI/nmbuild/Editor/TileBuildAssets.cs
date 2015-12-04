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
using org.critterai.nav;
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Represents the result of a tile build.
    /// </summary>
    public struct TileBuildAssets
    {
        private readonly int mTileX;
        private readonly int mTileZ;

        private readonly NavmeshTileData mTile;
        private readonly int mPolyCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// A 'no result' object will be created if the <paramref name="tile"/> parameter 
        /// is null or polygon count is less than one.
        /// </para>
        /// </remarks>
        /// <param name="tx">The x-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="tz">The z-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="tile">The tile data.</param>
        /// <param name="polyCount">The polygons in the tile.</param>
        public TileBuildAssets(int tx, int tz, NavmeshTileData tile, int polyCount)
        {
            mTileX = tx;
            mTileZ = tz;

            if (tile == null || polyCount < 1)
            {
                mTile = null;
                mPolyCount = 0;
            }
            else
            {
                mTile = tile;
                mPolyCount = polyCount;
            }
        }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileX { get { return mTileX; } }

        /// <summary>
        /// The z-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileZ { get { return mTileZ; } }

        /// <summary>
        /// The tile data.
        /// </summary>
        public NavmeshTileData Tile { get { return mTile; } }

        /// <summary>
        /// The number of polygons in the tile.
        /// </summary>
        public int PolyCount { get { return mPolyCount; } }

        /// <summary>
        /// There are no results for the tile.  (Polygon count is zero.)
        /// </summary>
        public bool NoResult { get { return (mTile == null); } }
    }
}
