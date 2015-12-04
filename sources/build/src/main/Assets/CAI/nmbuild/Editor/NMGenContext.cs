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
    /// An NMGen build context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Holds objects related to an NMGen build during the build process.  The builder
    /// defines which assets are available at which point in the build.
    /// </para>
    /// </remarks>
    public class NMGenContext
        : BuildContext
    {
        private readonly int mTileX;
        private readonly int mTileZ;

        private readonly NMGenParams mConfig;

        private bool mNoResult;

        private Heightfield mHeightfield;
        private CompactHeightfield mCompactField;
        private ContourSet mContours;
        private PolyMesh mPolyMesh;
        private PolyMeshDetail mDetailMesh;

        /// <summary>
        /// The associated build configuation.
        /// </summary>
        public NMGenParams Config { get { return mConfig; } }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileX { get { return mTileX; } }

        /// <summary>
        /// The z-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileZ { get { return mTileZ; } }

        /// <summary>
        /// The build has not produced a result.
        /// </summary>
        public bool NoResult 
        { 
            get { return mNoResult; }
            set { mNoResult = true; }
        }

        /// <summary>
        /// The heightfield.
        /// </summary>
        public Heightfield Heightfield
        {
            get { return mHeightfield; }
            set { mHeightfield = value; }
        }

        /// <summary>
        /// The compact field.
        /// </summary>
        public CompactHeightfield CompactField
        {
            get { return mCompactField; }
            set { mCompactField = value; }
        }

        /// <summary>
        /// The contour set.
        /// </summary>
        public ContourSet Contours
        {
            get { return mContours; }
            set { mContours = value; }
        }

        /// <summary>
        /// The polygon mesh.
        /// </summary>
        public PolyMesh PolyMesh
        {
            get { return mPolyMesh; }
            set { mPolyMesh = value; }
        }

        /// <summary>
        /// The detail mesh.
        /// </summary>
        public PolyMeshDetail DetailMesh
        {
            get { return mDetailMesh; }
            set { mDetailMesh = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tx">The x-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="tz">The z-index of the tile within the tile grid. (tx, tz)</param>
        /// <param name="config">The build configuration.</param>
        public NMGenContext(int tx, int tz, NMGenParams config)
        {
            mTileX = tx;
            mTileZ = tz;

            mConfig = config;
        }

        /// <summary>
        /// Sets the context as having produced no result.
        /// </summary>
        public void SetAsNoResult()
        {
            mHeightfield = null;
            mCompactField = null;
            mContours = null;
            mPolyMesh = null;
            mDetailMesh = null;
            mNoResult = true;
        }
    }
}
