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
    /// Represents the result of a NMGen build.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All assets will be null if there is no result.  Otherwise the only asset guarenteed 
    /// to be present is the <see cref="PolyMesh"/> object.
    /// </para>
    /// </remarks>
    public struct NMGenAssets
    {
        private readonly int mTileX;
        private readonly int mTileZ;

        private readonly PolyMesh mPolyMesh;
        private readonly PolyMeshDetail mDetailMesh;

        private readonly Heightfield mHeightfield;
        private readonly CompactHeightfield mCompactField;
        private readonly ContourSet mContours;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// A 'no result' object will be created if the <paramref name="polyMesh"/> parameter 
        /// is null or has a polygon count of zero.
        /// </para>
        /// </remarks>
        /// <param name="tx">The x-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="tz">The z-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="polyMesh">The polymesh.</param>
        /// <param name="detailMesh">The detail mesh.</param>
        /// <param name="heightfield">The heightfield.</param>
        /// <param name="compactField">The compact field.</param>
        /// <param name="contours">The contour set.</param>
        public NMGenAssets(int tx, int tz
            , PolyMesh polyMesh
            , PolyMeshDetail detailMesh
            , Heightfield heightfield
            , CompactHeightfield compactField
            , ContourSet contours)
        {
            mTileX = tx;
            mTileZ = tz;

            if (polyMesh == null || polyMesh.PolyCount == 0)
            {
                mPolyMesh = null;
                mDetailMesh = null;
                mHeightfield = null;
                mCompactField = null;
                mContours = null;
            }
            else
            {
                mPolyMesh = polyMesh;
                mDetailMesh = detailMesh;  // OK to be null.
                mHeightfield = heightfield;
                mCompactField = compactField;
                mContours = contours;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <para>
        /// A 'no result' object will be created if the <paramref name="polyMesh"/> parameter 
        /// is null or has a polygon count of zero.
        /// </para>
        /// </remarks>
        /// <param name="tx">The x-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="tz">The z-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="polyMesh">The polymesh.</param>
        /// <param name="detailMesh">The detail mesh. (Optional)</param>
        public NMGenAssets(int tx, int tz, PolyMesh polyMesh, PolyMeshDetail detailMesh)
        {
            mTileX = tx;
            mTileZ = tz;

            if (polyMesh == null || polyMesh.PolyCount == 0)
            {
                mPolyMesh = null;
                mDetailMesh = null;
            }
            else
            {
                mPolyMesh = polyMesh;
                mDetailMesh = detailMesh;  // OK to be null.
            }

            mHeightfield = null;
            mCompactField = null;
            mContours = null;
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
        /// The polygon mesh.
        /// </summary>
        public PolyMesh PolyMesh { get { return mPolyMesh; } }

        /// <summary>
        /// The detail mesh.
        /// </summary>
        public PolyMeshDetail DetailMesh { get { return mDetailMesh; } }

        /// <summary>
        /// The heightfield.
        /// </summary>
        public Heightfield Heightfield { get { return mHeightfield; } }

        /// <summary>
        /// The compact field.
        /// </summary>
        public CompactHeightfield CompactField { get { return mCompactField; } }

        /// <summary>
        /// The contour set.
        /// </summary>
        public ContourSet Contours { get { return mContours; } }

        /// <summary>
        /// The assets are emtpy.  (The build did not produce any results.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// If true, all assets will be null.
        /// </para>
        /// </remarks>
        public bool NoResult { get { return (mPolyMesh == null); } }
    }
}
