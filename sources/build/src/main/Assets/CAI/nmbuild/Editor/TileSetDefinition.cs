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
using System.Collections.Generic;
using org.critterai.geom;
using org.critterai.nmgen;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Represents the definition for a set of tiles that make up a tiled navigation mesh.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object provides common configuration settings and input geometry for a tiled
    /// navigation mesh build.  For example, the <see cref="IncrementalBuilder"/> will create
    /// a builder from a tile set defintion.
    /// </para>
    /// <para>
    /// Objects of this type are thread-safe and immutable.
    /// </para>
    /// </remarks>
    public sealed class TileSetDefinition
    {
        private readonly InputGeometry mGeometry;
        private readonly NMGenParams mBaseConfig;
        private readonly Vector3 mBoundsMin;
        private readonly Vector3 mBoundsMax;

        private int mWidth;
        private int mDepth;

        private TileSetDefinition(int width, int depth
            , Vector3 boundsMin, Vector3 boundsMax
            , NMGenParams config
            , InputGeometry geom)
        {
            // Note: The constructor is private, which is why
            // the references are being stored.
            mBaseConfig = config.Clone();
            mGeometry = geom;
            mWidth = width;
            mDepth = depth;
            mBoundsMin = boundsMin;
            mBoundsMax = boundsMax;
        }

        /// <summary>
        /// The number of tiles along the x-axis.
        /// </summary>
        public int Width { get { return mWidth; } }

        /// <summary>
        /// The number of tiles along the z-axis.
        /// </summary>
        public int Depth { get { return mDepth; } }

        /// <summary>
        /// The minimum AABB bounds of the tile set for build purposes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value will be the origin of the navigation mesh created from the tile set.
        /// </para>
        /// </remarks>
        public Vector3 BoundsMin { get { return mBoundsMin; } }

        /// <summary>
        /// The maximum AABB bounds of the tile set for build purposes.
        /// </summary>
        public Vector3 BoundsMax { get { return mBoundsMax; } }

        /// <summary>
        /// The shared input geometry used to build tiles in the set.
        /// </summary>
        public InputGeometry Geometry { get { return mGeometry; } }
 
        /// <summary>
        /// The world size of the tiles in the set.
        /// </summary>
        public float TileWorldSize { get { return mBaseConfig.TileWorldSize; } }

        /// <summary>
        /// Gets a copy of the shared NMGen configuration.
        /// </summary>
        /// <returns>A copy of the shared NMGen configuration.</returns>
        public NMGenParams GetBaseConfig()
        {
            return mBaseConfig.Clone();
        }

        /// <summary>
        /// Gets the bounds of the specified tile.
        /// </summary>
        /// <param name="tx">
        /// The x-index of the tile within the tile grid. (x, z) 
        /// [0 &lt;= value &lt; <see cref="Width"/>]
        /// </param>
        /// <param name="tz">
        /// The z-index of the tile within the tile grid. (x, z) 
        /// [0 &lt;= value &lt; <see cref="Depth"/>]
        /// </param>
        /// <param name="includeBorder">
        /// True if the bounds should be expanded by the border size configuration setting.
        /// </param>
        /// <param name="boundsMin">The minimum AABB bounds of the tile.</param>
        /// <param name="boundsMax">The maximum AABB bounds of the tile.</param>
        /// <returns>True if the tile is valid, otherwise false.</returns>
        public bool GetTileBounds(int tx, int tz, bool includeBorder
            , out Vector3 boundsMin, out Vector3 boundsMax)
        {
            boundsMin = mBoundsMin;
            boundsMax = mBoundsMin;

            if (tx < 0 || tz < 0 || tx >= mWidth || tz >= mDepth)
            {
                boundsMin = Vector3Util.Zero;
                boundsMax = Vector3Util.Zero;
                return false;
            }

            float tcsFactor = mBaseConfig.TileSize * mBaseConfig.XZCellSize;  // World tile size.

            // Note: The minimum bounds of the base configuration is
            // considered to be the origin of the mesh set.

            boundsMin = mBoundsMin;
            boundsMax = mBoundsMin;  // This is not an error.

            boundsMin.x += tx * tcsFactor;
            boundsMin.z += tz * tcsFactor;

            boundsMax.x += (tx + 1) * tcsFactor;
            boundsMax.y = mBoundsMax.y;
            boundsMax.z += (tz + 1) * tcsFactor;

            if (includeBorder)
            {
                float borderOffset = mBaseConfig.BorderSize * mBaseConfig.XZCellSize;

                boundsMin.x -=  borderOffset;
                boundsMin.z -= borderOffset;

                boundsMax.x += borderOffset;
                boundsMax.z += borderOffset;
            }

            return true;
        }

        /// <summary>
        /// Creates a new tile set.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bounds is normally based on the desired origin of the navigation mesh
        /// and the maximum bounds of the input geometry.
        /// </para>
        /// </remarks>
        /// <param name="boundsMin">The minimum AABB bounds of the set.</param>
        /// <param name="boundsMax">The maximum AABB counds of the set.</param>
        /// <param name="config">The shared NMGen configuration.</param>
        /// <param name="geom">The input geometry.</param>
        /// <returns>A new tile set, or null on error.</returns>
        public static TileSetDefinition Create(Vector3 boundsMin, Vector3 boundsMax
            , NMGenParams config
            , InputGeometry geom)
        {
            if (config == null || !config.IsValid()
                || !TriangleMesh.IsBoundsValid(boundsMin, boundsMax)
                || geom == null
                || config.tileSize <= 0)
            {
                return null;
            }

            int w;
            int d;

            NMGen.DeriveSizeOfTileGrid(boundsMin, boundsMax
                , config.XZCellSize
                , config.tileSize
                , out w, out d);

            if (w < 1 || d < 1)
                return null;

            return new TileSetDefinition(w, d, boundsMin, boundsMax, config, geom);
        }
    }
}
