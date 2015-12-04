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
using UnityEngine;
using Math = System.Math;  // Used for rounding.
using org.critterai.nmgen;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Represents a configuration for a navigation mesh build in Unity. (Editor Only)
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see cref="NMGenParams"/> for details on the various properties. The primary difference 
    /// between this class and see cref="NMGenParams"/> is that some values in this class are in 
    /// world units rather than cell units.
    /// </para>
    /// </remarks>
    [System.Serializable]
    internal sealed class NMGenConfig
    {
        public const NMGenBuildFlag DefaultBuildFlags = 
            NMGenBuildFlag.LowHeightSpansNotWalkable
            | NMGenBuildFlag.LowObstaclesWalkable
            | NMGenBuildFlag.ApplyPolyFlags
            | NMGenBuildFlag.BVTreeEnabled;

        #region Standard Param Labels

        public const string TileSizeLabel = "Tile Size (vx)";

        public const string XZSizeLabel = "XZ Cell Size";

        public const string YSizeLabel = "Y Cell Size";

        public const string HeightLabel = "Walkable Height";

        public const string StepLabel = "Walkable Step";

        public const string SlopeLabel = "Walkable Slope";

        public const string RadiusLabel = "Walkable Radius";

        public const string EdgeLenLabel = "Max Edge Length";

        public const string EdgeDevLabel = "Edge Max Deviation";

        public const string DetailSampleLabel = "Detail Sample Dist";

        public const string DetailDevLabel = "Detail Max Deviation";

        public const string IslandRegionLabel = "Min Island Area";

        public const string MaxPolyVertLabel = "Max Vertices Per Polygon";

        public const string HFBorderLabel = "Border Size (vx)";

        public const string TileBorderLabel = "Tile Border Size (vx)";

        public const string MergeSizeLabel = "Merge Region Area";

        public const string LedgeSpansLabel = "Ledges Not Walkable";

        public const string BVTreeLabel = "BVTreeEnabled";

        public const string LowHeightLabel = "Low Height Not Walkable";

        public const string LowObstacleLabel = "Low Obstacles Walkable";

        public const string TessAreasLabel =  "Tessellate Area Edges";

        public const string TessWallsLabel = "Tessellate Wall Edges";

        public const string UseMonoLabel = "Use Monotone Partitioning";

        public const string FlagPolysLabel = "Apply Poly Flag";

        #endregion

        private static int mXZResolution = 1000;
        private static int mYResolution = 1000;
        private static int mDeviationFactor = 20;
        private static int mSampleResolution = 100;
        private static int mMaxCells = 1500;
        private static int mStandardCells = 1000;

        public static int MaxCells
        {
            get { return mMaxCells; }
            set { mMaxCells = Mathf.Max(4, value); }
        }

        public static int StandardCells
        {
            get { return mStandardCells; }
            set { mStandardCells = Mathf.Max(4, value); }
        }

        /// <summary>
        /// The xz-plane resolution to use when deriving a configuration.
        /// </summary>
        public static int XZResolution
        {
            get { return mXZResolution; }
            set { mXZResolution = Mathf.Max(1, value); }
        }

        /// <summary>
        /// The y-axis resolution to use when deriving a configuration.
        /// </summary>
        public static int YResolution
        {
            get { return mYResolution; }
            set { mYResolution = Mathf.Max(1, value); }
        }

        /// <summary>
        /// The detail deviation factor to use when deriving a configuration.
        /// </summary>
        public static int DetailDeviation
        {
            get { return mDeviationFactor; }
            set { mDeviationFactor = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The sample resolution to use when deriving a configuration.
        /// </summary>
        public static int DetailSampleResolution
        {
            get { return mSampleResolution; }
            set { mSampleResolution = Mathf.Max(1, value); }
        }

        // Remember:  All locally stored fields are ignored
        // in the root.  So the root is not valid until the local
        // data is transferred into it.
        [SerializeField]
        private NMGenParams mRoot;

        // World unit parameters.
        [SerializeField]
        private float mWalkableRadius;
        [SerializeField]
        private float mMaxEdgeLength;
        [SerializeField]
        private float mMinRegionArea;
        [SerializeField]
        private float mMergeRegionArea;
        [SerializeField]
        private float mWalkableHeight;
        [SerializeField]
        private float mWalkableStep;

        [SerializeField]
        private NMGenBuildFlag mBuildFlags;

        [SerializeField]
        private NMGenAssetFlag mResultOptions = NMGenAssetFlag.DetailMesh | NMGenAssetFlag.PolyMesh;

        public NMGenAssetFlag ResultOptions
        {
            get { return mResultOptions; }
            set { mResultOptions = value; }
        }

        public ContourBuildFlags ContourOptions
        {
            get { return mRoot.ContourOptions; }
            set { mRoot.ContourOptions = value; }
        }

        public bool UseMonotone
        {
            get { return mRoot.UseMonotone; }
            set { mRoot.UseMonotone = value; }
        }

        /// <summary>
        /// The width/depth size of the tile on the xz-plane.
        /// </summary>
        public int TileSize
        {
            get { return mRoot.TileSize; }
            set { mRoot.TileSize = value; }
        }

        public float TileWorldSize
        {
            get { return mRoot.TileWorldSize; }
        }

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// </summary>
        /// <remarks>
        /// <para>Also the 'grid size' or 'voxel size'.</para>
        /// </remarks>
        public float XZCellSize
        {
            get { return mRoot.XZCellSize; }
            set { mRoot.XZCellSize = value; }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        /// <remarks>
        /// <para>Also the 'voxel size' for the y-axis.</para>
        /// </remarks>
        public float YCellSize
        {
            get { return mRoot.YCellSize; }
            set { mRoot.YCellSize = value; }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the
        /// floor area to be considered traversable.
        /// </summary>
        /// <remarks>
        /// <para>Usually the maximum client height.</para>
        /// </remarks>
        public float WalkableHeight
        {
            get { return mWalkableHeight; }
            set 
            { 
                mWalkableHeight =  Mathf.Max(
                    NMGen.MinWalkableHeight * NMGen.MinCellSize, value); 
            }
        }

        /// <summary>
        /// Maximum ledge height that is considered to still be
        /// traversable.
        /// </summary>
        /// <remarks>
        /// <para>Usually set to how far up/down the client can step.</para>
        /// </remarks>
        public float WalkableStep
        {
            get { return mWalkableStep; }
            set { mWalkableStep = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum slope that is considered walkable.
        /// </summary>
        public float WalkableSlope
        {
            get { return mRoot.WalkableSlope; }
            set { mRoot.WalkableSlope = value; }
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an
        /// obstruction in the source geometry.
        /// </summary>
        /// <remarks>
        ///  Usually the client radius.
        /// </remarks>
        public float WalkableRadius
        {
            get { return mWalkableRadius; }
            set { mWalkableRadius = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane AABB of the
        /// source geometry.
        /// </summary>
        public int BorderSize
        {
            get { return mRoot.BorderSize; }
            set { mRoot.BorderSize = value; }
        }

        /// <summary>
        /// The maximum allowed length of triangle edges on the border of the
        /// mesh.
        /// </summary>
        /// <remarks>
        /// <para>Extra vertices will be inserted if needed.</para>
        /// <para>A value of zero disabled this feature.</para>
        /// </remarks>
        public float MaxEdgeLength
        {
            get { return mMaxEdgeLength; }
            set { mMaxEdgeLength = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from
        /// the source geometry.
        /// </summary>
        /// <remarks>
        /// <para>Applies only to the xz-plane.</para>
        /// </remarks>
        public float EdgeMaxDeviation
        {
            get { return mRoot.EdgeMaxDeviation; }
            set { mRoot.EdgeMaxDeviation = value; }
        }

        /// <summary>
        /// Sets the sampling distance to use when matching the
        /// mesh surface to the source geometry.
        /// [Limits: 0 or >= 0.9]
        /// </summary>
        public float DetailSampleDistance
        {
            get { return mRoot.DetailSampleDistance; }
            set { mRoot.DetailSampleDistance = value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the
        /// surface of the source geometry. 
        /// [Limit: >= 0]
        /// </summary>
        public float DetailMaxDeviation
        {
            get { return mRoot.DetailMaxDeviation; }
            set { mRoot.DetailMaxDeviation = value; }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// [Limit: >= 0]
        /// </summary>
        /// <remarks>
        /// <para>Prevents the formation of meshes that are too small to be
        /// of use.</para>
        /// </remarks>
        public float MinRegionArea
        {
            get { return mMinRegionArea; }
            set { mMinRegionArea = Mathf.Max(0, value); }
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, 
        /// if possible, be merged with larger regions.
        /// [Limit: >= 0]
        /// </summary>
        public float MergeRegionArea
        {
            get { return mMergeRegionArea; }
            set { mMergeRegionArea = Mathf.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons
        /// generated during the contour to polygon conversion process.
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return mRoot.MaxVertsPerPoly; }
            set
            {
                mRoot.MaxVertsPerPoly = value;
            }
        }

        /// <summary>
        /// Flags used to control optional build steps. 
        /// </summary>
        public NMGenBuildFlag BuildFlags
        {
            get { return mBuildFlags; }
            set { mBuildFlags = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public NMGenConfig()
        {
            Reset();
        }

        public void Reset()
        {
            mRoot = new NMGenParams();
            UpdateLocalsFrom(mRoot);
            mBuildFlags = DefaultBuildFlags;
        }

        /// <summary>
        /// Sets the configuration to match the provided configuration.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="config"/> parameter will be cleaned during this operation.
        /// </para>
        /// </remarks>
        /// <param name="config">The configuration to match.</param>
        public void SetConfig(NMGenParams config)
        {
            if (config == null)
                return;

            mRoot = config.Clone();
            mRoot.Clean();
            UpdateLocalsFrom(mRoot);
        }

        private void UpdateLocalsFrom(NMGenParams config)
        {
            mMaxEdgeLength = config.WorldMaxEdgeLength;
            mMergeRegionArea = config.WorldMergeRegionArea;
            mMinRegionArea = config.WorldMinRegionArea;

            mWalkableHeight = config.WorldWalkableHeight;
            mWalkableRadius = config.WorldWalkableRadius;
            mWalkableStep = config.WorldWalkableStep;
        }

        private void ApplyLocalsTo(NMGenParams config)
        {
            config.SetMaxEdgeLength(mMaxEdgeLength);
            config.SetMergeRegionArea(mMergeRegionArea);
            config.SetMinRegionArea(mMinRegionArea);

            config.SetWalkableHeight(mWalkableHeight);
            config.SetWalkableRadius(mWalkableRadius);
            config.SetWalkableStep(mWalkableStep);
        }

        /// <summary>
        /// Attempts the derive the best configuration for the provided source geometry AABB.
        /// </summary>
        /// <param name="boundsMin">The minimum bounds of the source geometry.</param>
        /// <param name="boundsMax">The maximum bounds of the source geometry.</param>
        public void Derive(Vector3 boundsMin, Vector3 boundsMax)
        {
            // Order is important.
            XZCellSize = DeriveXZCellSize(this);
            YCellSize = DeriveYCellSize(this);
            TileSize = DeriveTileSize(this, boundsMin, boundsMax);
            BorderSize = DeriveBorderSize(this);
            DetailSampleDistance =
                DeriveDetailSampleDistance(this, boundsMin, boundsMax);

            DetailMaxDeviation = DeriveDetailMaxDeviation(this);
        }

        public static float DeriveXZCellSize(NMGenConfig config)
        {
            return (float)Math.Round(Mathf.Max(0.05f, config.WalkableRadius / 2), 2);
        }

        public static float DeriveYCellSize(NMGenConfig config)
        {
            return (float)Math.Round(Mathf.Max(0.05f, config.WalkableStep / 3), 2);
        }

        public static float DeriveDetailMaxDeviation(NMGenConfig config)
        {
            return (float)Math.Round(config.YCellSize * mDeviationFactor, 2);
        }

        public static int DeriveBorderSize(NMGenConfig config)
        {
            if (config.TileSize > 0)
            {
                return (int)Mathf.Ceil(
                    config.WalkableRadius / config.mRoot.xzCellSize) + 3;
            }
            return 0;
        }

        public static float DeriveDetailSampleDistance(
            NMGenConfig config
            , Vector3 boundsMin
            , Vector3 boundsMax)
        {
            Vector3 diff = boundsMax - boundsMin;

            float maxXZLength = Mathf.Max(diff.x, diff.z);

            int maxCells = Mathf.CeilToInt(maxXZLength / config.XZCellSize);

            if (config.TileSize == 0 || maxCells <= MaxCells)
            {
                return (float)Math.Round(
                    Mathf.Max(0.9f, maxXZLength / mSampleResolution), 2);
            }
            else
                return (float)Math.Round(Mathf.Max(0.9f
                    , config.TileSize * config.XZCellSize / mSampleResolution), 2);
        }

        public static int DeriveTileSize(
            NMGenConfig config
            , Vector3 boundsMin
            , Vector3 boundsMax)
        {
            Vector3 diff = boundsMax - boundsMin;

            float maxXZLength = Mathf.Max(diff.x, diff.z);

            int maxCells = Mathf.CeilToInt(maxXZLength / config.XZCellSize);

            if (maxCells <= MaxCells)
                return 0;
            else
                return Mathf.Min(mStandardCells, maxCells / 2);
        }

        /// <summary>
        /// Generates a <see cref="NMGenParams"/> based on the the object 
        /// values.
        /// </summary>
        /// <returns>A configuration based on the object values.</returns>
        public NMGenParams GetConfig()
        {
            NMGenParams result = mRoot.Clone();

            ApplyLocalsTo(result);

            return result;
        }

        /// <summary>
        /// Duplicates the object.
        /// </summary>
        /// <returns>A duplicate of the object.</returns>
        public NMGenConfig Clone()
        {
            NMGenConfig result = new NMGenConfig();
            result.mRoot = mRoot;
            result.mMaxEdgeLength = mMaxEdgeLength;
            result.mMergeRegionArea = mMergeRegionArea;
            result.mMinRegionArea = mMinRegionArea;
            result.mWalkableHeight = mWalkableHeight;
            result.mWalkableRadius = mWalkableRadius;
            result.mWalkableStep = mWalkableStep;
            result.mBuildFlags = mBuildFlags;
            return result;
        }

        public void Clean()
        {
            mRoot.Clean();
            UpdateLocalsFrom(mRoot);
        }

        public void ApplyDecimalLimits()
        {
            XZCellSize = (float)Math.Round(XZCellSize, 2);
            YCellSize = (float)Math.Round(YCellSize, 2);

            DetailMaxDeviation =
                (float)Math.Round(DetailMaxDeviation, 2);
            DetailSampleDistance =
                (float)Math.Round(DetailSampleDistance, 2);
            EdgeMaxDeviation =
                (float)Math.Round(EdgeMaxDeviation, 2);

            MaxEdgeLength = (float)Math.Round(MaxEdgeLength, 2);
            MergeRegionArea = (float)Math.Round(MergeRegionArea, 2);
            MinRegionArea = (float)Math.Round(MinRegionArea, 2);
            WalkableHeight = (float)Math.Round(WalkableHeight, 2);
            WalkableRadius = (float)Math.Round(WalkableRadius, 2);
            WalkableSlope = (float)Math.Round(WalkableSlope, 2);
            WalkableStep = (float)Math.Round(WalkableStep, 2);
        }
    }
}
