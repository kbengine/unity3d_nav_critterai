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
using System.Collections.Generic;
using org.critterai.nmgen;
using org.critterai.geom;

#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Provides a way of building navigation mesh data in incremental steps.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Processors will be called for processing after each step. (Post-processing)
    /// The assests available by default at each step will be as follows:
    /// </para>
    /// <para>
    /// HeightfieldBuild: <see cref="Heightfield"/><br/>
    /// CompactFieldBuild: <see cref="CompactHeightfield"/><br/>
    /// RegionBuild: <see cref="CompactHeightfield"/><br/>
    /// ContourBuild: <see cref="ContourSet"/> and <see cref="CompactHeightfield"/><br/>
    /// PolyMeshBuild: <see cref="PolyMesh"/> and <see cref="CompactHeightfield"/><br/>
    /// DetailMeshBuild: <see cref="PolyMeshDetail"/> and <see cref="CompactHeightfield"/><br/>
    /// </para>
    /// <para>
    /// More assets will be avialable based on <see cref="INMGenProcessor.PreserveAssets"/> 
    /// <see cref="ResultOptions"/> requirements.
    /// </para>
    /// <para>
    /// The detail mesh step will only occur if the <see cref="ResultOptions"/> settings 
    /// require it.
    /// </para>
    /// <para>
    /// Processors are allowed to replace assets.  But they must never set the assets to
    /// an invalid state.
    /// </para>
    /// <para>
    /// Each instance can be used to perform only a single build. (Single use.)
    /// </para>
    /// </remarks>
    public sealed class IncrementalBuilder
    {
        private readonly string mTileText;
        private readonly NMGenTileParams mTileConfig;
        private readonly NMGenParams mConfig;

        private readonly NMGenAssetFlag mResultOptions;

        private NMGenState mState = NMGenState.Initialized;

        private readonly InputGeometry mGeometry;
        private readonly ProcessorSet mProcessors;

        private NMGenContext mBuildContext;

        /// <summary>
        /// True if the builder is safe to run on a separate thread from which it was created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Thread-safely only applies to running the build. Builder objects are
        /// never safe to access from multiple threads at the same time.
        /// </para>
        /// </remarks>
        public bool IsThreadSafe { get { return mProcessors.IsThreadSafe; } }

        /// <summary>
        /// The current state of the builder.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is an unfinished state, then it represents the build step that will 
        /// occur the next time the <see cref="Build"/> method called.
        /// </para>
        /// </remarks>
        public NMGenState State { get { return mState; } }

        /// <summary>
        /// True if the the build has finished.  (Successfully or not.)
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return (mState == NMGenState.Aborted
                    || mState == NMGenState.Complete
                    || mState == NMGenState.NoResult);
            }
        }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileX { get { return mTileConfig.TileX; } }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileZ { get { return mTileConfig.TileZ; } }

        /// <summary>
        /// The result of the build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return a result only when in the <see cref="NMGenState.Complete"/> state.
        /// </para>
        /// </remarks>
        public NMGenAssets Result
        {
            get
            {
                if (mState == NMGenState.Complete)
                {
                    return new NMGenAssets(mTileConfig.TileX, mTileConfig.TileZ
                        , mBuildContext.PolyMesh, mBuildContext.DetailMesh
                        , mBuildContext.Heightfield, mBuildContext.CompactField, mBuildContext.Contours);
                }
                return new NMGenAssets();
            }
        }

        /// <summary>
        /// The NMGen assets that will be included in the <see cref="Result"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will always include <see cref="NMGenAssetFlag.PolyMesh"/>
        /// </para>
        /// </remarks>
        public NMGenAssetFlag ResultOptions { get { return mResultOptions; } }

        /// <summary>
        /// The number of messages generated by the build.
        /// </summary>
        public int MessageCount
        {
            get { return (IsFinished ? mBuildContext.MessageCount : 0); }
        }

        private IncrementalBuilder(NMGenTileParams tileConfig
            , NMGenParams config
            , NMGenAssetFlag resultOptions
            , InputGeometry source
            , ProcessorSet processors)
        {
            mConfig = config;
            mTileConfig = tileConfig;

            mGeometry = source;
            mProcessors = processors;
            mResultOptions = resultOptions;

            mBuildContext = new NMGenContext(tileConfig.TileX, tileConfig.TileZ, mConfig.Clone());

            mTileText = string.Format("({0},{1})", tileConfig.TileX, tileConfig.TileZ);

            mState = NMGenState.Initialized;
        }

        /// <summary>
        /// Gets available build messages, or a zero length array if no messages are available.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Messages will only be available when the build is finished.
        /// </para>
        /// </remarks>
        /// <returns>Available build messages.</returns>
        public string[] GetMessages()
        {
            return (IsFinished ? mBuildContext.GetMessages() : new string[0]);
        }

        /// <summary>
        /// Gets a new line delimited flattened version of all build messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Messages will only be available when the build is finished.
        /// </para>
        /// </remarks>
        /// <returns>All build messages.</returns>
        public string GetMessagesFlat()
        {
            return (IsFinished ? mBuildContext.GetMessagesFlat() : "");
        }

        /// <summary>
        /// Performs a single build step.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The result state will represent either a finished state or the build step that 
        /// will be performed during the next call to the method.
        /// </para>
        /// </remarks>
        /// <returns>The state at the end of the build step.</returns>
        public NMGenState Build()
        {
            switch (mState)
            {
                case NMGenState.Initialized:

                    mBuildContext.Log("Build: " + mTileText, this);

                    mProcessors.LogProcessors(mBuildContext);

                    mState = NMGenState.HeightfieldBuild;

                    break;

                case NMGenState.HeightfieldBuild:

                    //UnityEngine.Debug.Log("hfb");
                    BuildHeightfield();
                    break;

                case NMGenState.CompactFieldBuild:

                    //UnityEngine.Debug.Log("cfb");
                    BuildCompactField();
                    break;

                case NMGenState.RegionBuild:

                    //UnityEngine.Debug.Log("rb");
                    BuildRegions();
                    break;

                case NMGenState.ContourBuild:

                    //UnityEngine.Debug.Log("cb");
                    BuildContours();
                    break;

                case NMGenState.PolyMeshBuild:

                    BuildPolyMesh();
                    break;

                case NMGenState.DetailMeshBuild:

                    BuildDetailMesh();
                    break;
            }
            return mState;
        }

        /// <summary>
        /// Performs all build steps.
        /// </summary>
        public void BuildAll()
        {
            while (!IsFinished) { Build(); }
        }

        private void BuildHeightfield()
        {
            int width;
            int depth;

            NMGen.DeriveSizeOfCellGrid(mTileConfig.BoundsMin
                , mTileConfig.BoundsMax
                , mConfig.XZCellSize
                , out width
                , out depth);

            Heightfield hf = Heightfield.Create(width, depth
                , mTileConfig.BoundsMin, mTileConfig.BoundsMax
                , mConfig.XZCellSize, mConfig.YCellSize);


            hf.AddTriangles(mBuildContext
                , mGeometry.Mesh
                , mTileConfig.boundsMin
                , mTileConfig.boundsMax
                , mConfig.WalkableStep);  // Merge for any spans less than step.


            if (hf.GetSpanCount() < 1)
            {
                FinalizeNoResult("Complete at heightfield build. No spans.");
                return;
            }


            mBuildContext.Heightfield = hf;

            if (PostProcess() && PostHeightfieldCheck())
            {
                mBuildContext.Log("Voxelized triangles. Span count: " + hf.GetSpanCount(), this);
                mState = NMGenState.CompactFieldBuild;
            }
        }

        private bool PostHeightfieldCheck()
        {
            Heightfield hf = mBuildContext.Heightfield;

            if (hf == null || hf.IsDisposed)
            {
                FinalizeAbort("Custom processors destroyed the heightfield. (" + mState + " Post)");
                return false;
            }
            else if (hf.GetSpanCount() < 1)
            {
                FinalizeNoResult("Complete at heightfield build. No spans. (" + mState + " Post)");
                return false;
            }
            return true;
        }

        private bool PostProcess()
        {
            if (!mProcessors.Process(mBuildContext, mState))
            {
                FinalizeAbort("Abort requested by custom processors. (" + mState + " Post)");
                return false;
            }
            if (mBuildContext.NoResult)
            {
                FinalizeNoResult("Custom processors set as no result. (" + mState + " Post)");
                return false;
            }
            return true;
        }

        private void BuildCompactField()
        {
            Heightfield hf = mBuildContext.Heightfield;

            CompactHeightfield chf = CompactHeightfield.Build(mBuildContext
                , hf
                , mConfig.WalkableHeight
                , mConfig.WalkableStep);

            if (CanDispose(NMGenAssetFlag.Heightfield))
            {
                hf.RequestDisposal();
                mBuildContext.Heightfield = null;
            }

            if (chf == null)
            {
                FinalizeAbort("Aborted at compact heightfield build.");
                return;
            }

            if (chf.SpanCount < 1)
            {
                FinalizeNoResult("Complete at compact heightfield build. No spans.");
                return;
            }

            mBuildContext.CompactField = chf;

            // Note: Post process is done before eroding the walkable area
            // so that the processors can stamp additional obstructions into
            // the heightfield.
            if (PostProcess() && PostCompactFieldCheck())
            {
                if (mConfig.WalkableRadius > 0)
                {
                    chf = mBuildContext.CompactField;
                    chf.ErodeWalkableArea(mBuildContext, mConfig.WalkableRadius);
                    mBuildContext.Log("Eroded walkable area by radius: " + mConfig.walkableRadius
                        , this);
                }

                mBuildContext.Log("Built compact heightfield. Spans: " + chf.SpanCount, this);

                mState = NMGenState.RegionBuild;
            }
        }

        private bool PostCompactFieldCheck()
        {
            CompactHeightfield chf = mBuildContext.CompactField;

            if (chf == null || chf.IsDisposed)
            {
                FinalizeAbort(
                    "Custom processors destroyed the compact heightfield. (" + mState + " Post)");
                return false;
            }
            else if (chf.SpanCount < 1)
            {
                FinalizeNoResult(
                    "Complete at compact heightfield build. No spans. (" + mState + " Post)");
                return false;
            }
            return true;
        }

        private void BuildRegions()
        {
            CompactHeightfield chf = mBuildContext.CompactField;

            chf.BuildDistanceField(mBuildContext);
            mBuildContext.Log("Built distance field. Max Distance: " + chf.MaxDistance, this);

            if (mConfig.UseMonotone)
            {
                if (!chf.BuildRegionsMonotone(mBuildContext
                    , mConfig.BorderSize
                    , mConfig.MinRegionArea
                    , mConfig.MergeRegionArea))
                {
                    FinalizeAbort("Monotone region generation failed.");
                    return;
                }
            }
            else
            {
                if (!chf.BuildRegions(mBuildContext
                    , mConfig.BorderSize
                    , mConfig.MinRegionArea
                    , mConfig.MergeRegionArea))
                {
                    FinalizeAbort("Region generation failed.");
                    return;
                }
            }

            if (PostProcess() && PostCompactFieldCheck())
            {
                if (chf.MaxRegion < 2)
                {
                    // Null region counts as a region.  So expect
                    // at least 2.
                    FinalizeNoResult("Completed after region build. No useable regions formed.");
                    return;
                }

                mBuildContext.Log("Generated regions. Region Count: " + chf.MaxRegion, this);

                // Success.
                mState = NMGenState.ContourBuild;
            }
        }

        private void BuildContours()
        {
            ContourSet cset = ContourSet.Build(mBuildContext
                , mBuildContext.CompactField
                , mConfig.EdgeMaxDeviation
                , mConfig.MaxEdgeLength
                , mConfig.ContourOptions);

            if (cset == null)
            {
                FinalizeAbort("Aborted at contour set build.");
                return;
            }

            if (cset.Count < 1)
            {
                FinalizeNoResult("Completed after contour build. No useable contours generated.");
                return;
            }

            mBuildContext.Contours = cset;

            if (PostProcess() && PostContoursCheck() && PostCompactFieldCheck())
            {
                mBuildContext.Log("Built contour set. Contour count: " + cset.Count, this);
                mState = NMGenState.PolyMeshBuild;
            }
        }

        private bool PostContoursCheck()
        {
            ContourSet contours = mBuildContext.Contours;

            if (contours == null || contours.IsDisposed)
            {
                FinalizeAbort("Custom processors destroyed the contour set. (" + mState + " Post)");
                return false;
            }
            else if (contours.Count < 1)
            {
                FinalizeNoResult(
                    "Aborted after contour set build. No contours generated. (" + mState + " Post)");
                return false;
            }
            return true;
        }

        private void BuildPolyMesh()
        {
            ContourSet cset = mBuildContext.Contours;

            PolyMesh polyMesh = PolyMesh.Build(mBuildContext
                , cset
                , mConfig.MaxVertsPerPoly
                , mConfig.WalkableHeight
                , mConfig.WalkableRadius
                , mConfig.WalkableStep);

            if (CanDispose(NMGenAssetFlag.ContourSet))
            {
                cset.RequestDisposal();
                mBuildContext.Contours = null;
            }

            if (polyMesh == null)
            {
                FinalizeAbort("Aborted at poly mesh build.");
                return;
            }

            if (polyMesh.PolyCount < 1)
            {
                FinalizeNoResult("Aborted after poly mesh build. No polygons generated.");
                return;
            }

            mBuildContext.PolyMesh = polyMesh;

            if (PostProcess() & PostPolyMeshCheck() & PostCompactFieldCheck())
            {
                mBuildContext.Log("Built poly mesh. PolyCount: " + polyMesh.PolyCount, this);
                mState = NMGenState.DetailMeshBuild;
            }
        }

        private bool PostPolyMeshCheck()
        {
            PolyMesh polyMesh = mBuildContext.PolyMesh;

            if (polyMesh == null || polyMesh.IsDisposed)
            {
                FinalizeAbort("Custom processors destroyed the poly mesh. (" + mState + " Post)");
                return false;
            }
            else if (polyMesh.PolyCount < 1)
            {
                FinalizeNoResult(
                    "Aborted after poly mesh build. No polygons generated. (" + mState + " Post)");
                return false;
            }
            return true;
        }

        private void BuildDetailMesh()
        {
            if ((mResultOptions & NMGenAssetFlag.DetailMesh) == 0)
            {
                FinalizeComplete();
                return;
            }

            PolyMesh polyMesh = mBuildContext.PolyMesh;
            CompactHeightfield chf = mBuildContext.CompactField;

            PolyMeshDetail detailMesh = PolyMeshDetail.Build(mBuildContext
                , polyMesh
                , chf
                , mConfig.detailSampleDistance
                , mConfig.detailMaxDeviation);

            if (CanDispose(NMGenAssetFlag.CompactField))
            {
                chf.RequestDisposal();
                mBuildContext.CompactField = null;
            }

            if (detailMesh == null)
            {
                FinalizeAbort("Aborted at detail mesh build.");
                return;
            }

            mBuildContext.DetailMesh = detailMesh;

            if (detailMesh.MeshCount < 1)
            {
                // Will only happen on an error.
                FinalizeAbort("Aborted after detail mesh build. No detail meshes generated.");
                return;
            }

            if (PostProcess() & PostPolyMeshCheck() & PostDetailCheck())
            {
                mBuildContext.Log("Built detail mesh. TriangleCount: " + detailMesh.TriCount, this);
                FinalizeComplete();
            }
        }

        private void FinalizeComplete()
        {
            if ((mResultOptions & NMGenAssetFlag.Heightfield) == 0 && mBuildContext.Heightfield != null)
            {
                mBuildContext.Heightfield.RequestDisposal();
                mBuildContext.Heightfield = null;
            }

            if ((mResultOptions & NMGenAssetFlag.CompactField) == 0 && mBuildContext.CompactField != null)
            {
                mBuildContext.CompactField.RequestDisposal();
                mBuildContext.CompactField = null;
            }

            if ((mResultOptions & NMGenAssetFlag.ContourSet) == 0 && mBuildContext.Contours != null)
            {
                mBuildContext.Contours.RequestDisposal();
                mBuildContext.Contours = null;
            }

            // Polymesh is always kept.

            if ((mResultOptions & NMGenAssetFlag.DetailMesh) == 0 && mBuildContext.DetailMesh != null)
            {
                mBuildContext.DetailMesh.RequestDisposal();
                mBuildContext.DetailMesh = null;
            }

            mBuildContext.Log("Build Complete. Result: " + mResultOptions, this);
            mState = NMGenState.Complete;
        }

        private void FinalizeAbort(string message)
        {
            mBuildContext.Log(message, this);
            DisposeAssets();
            mState = NMGenState.Aborted;
        }

        private void FinalizeNoResult(string message)
        {
            mBuildContext.Log(message, this);
            DisposeAssets();
            mState = NMGenState.NoResult;
        }

        private void DisposeAssets()
        {
            if (mBuildContext.Heightfield != null)
                mBuildContext.Heightfield.RequestDisposal();

            if (mBuildContext.CompactField != null)
                mBuildContext.CompactField.RequestDisposal();

            if (mBuildContext.Contours != null)
                mBuildContext.Contours.RequestDisposal();

            if (mBuildContext.PolyMesh != null)
                mBuildContext.PolyMesh.RequestDisposal();

            if (mBuildContext.DetailMesh != null)
                mBuildContext.DetailMesh.RequestDisposal();
        }

        private bool PostDetailCheck()
        {
            PolyMeshDetail detailMesh = mBuildContext.DetailMesh;
            PolyMesh polyMesh = mBuildContext.PolyMesh;

            if (detailMesh == null || detailMesh.IsDisposed)
            {
                FinalizeAbort("Custom processors destroyed the detail mesh.  (" + mState + " Post)");
                return false;
            }
            else if (polyMesh.PolyCount != detailMesh.MeshCount)
            {
                FinalizeAbort("Custom processors returned with poly/detail count mismatch. ("
                    + mState + " Post)");
                return false;
            }

            return true;
        }

        private bool CanDispose(NMGenAssetFlag asset)
        {
            return ((mProcessors.PreserveAssets & asset) == 0 && (mResultOptions & asset) == 0);
        }

        /// <summary>
        /// Creates a new builder for a single-tile mesh build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bounds of the tile will be based on the geometry.
        /// </para>
        /// <para>
        /// Will return null if a processor requires the the detail mesh but
        /// the detail mesh is not included in the result options.
        /// </para>
        /// </remarks>
        /// <param name="buildConfig">The build configuration.</param>
        /// <param name="resultOptions">The assets to include in the result.</param>
        /// <param name="geometry">The input geometry.</param>
        /// <param name="processors">The processors to apply.</param>
        /// <returns>A new builder, or null on error.</returns>
        public static IncrementalBuilder Create(NMGenParams buildConfig
            , NMGenAssetFlag resultOptions
            , InputGeometry geometry
            , ProcessorSet processors)
        {
            if (buildConfig == null
                || geometry == null
                || processors == null)
            {
                return null;
            }

            resultOptions |= NMGenAssetFlag.PolyMesh;

            if ((processors.PreserveAssets & NMGenAssetFlag.DetailMesh) != 0
                && (resultOptions & NMGenAssetFlag.DetailMesh) == 0)
            {
                // The processors require the detail mesh, but the result won't include it.
                return null;
            }

            NMGenTileParams tileConfig =
                new NMGenTileParams(0, 0, geometry.BoundsMin, geometry.BoundsMax);

            return new IncrementalBuilder(tileConfig
                , buildConfig, resultOptions, geometry, processors);
        }

        /// <summary>
        /// Creates a new builder for a multi-tile mesh build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return null if a processor requires the the detail mesh but the detail mesh is 
        /// not included in the result options.
        /// </para>
        /// </remarks>
        /// <param name="tx">The x-index of the tile to build.</param>
        /// <param name="tz">The z-index of the tile to build.</param>
        /// <param name="resultOptions">The assets to include in the result.</param>
        /// <param name="tdef">The tile set definition to base the build on.</param>
        /// <param name="processors">The processors to apply.</param>
        /// <returns>A new builder, or null on error.</returns>
        public static IncrementalBuilder Create(int tx, int tz
            , NMGenAssetFlag resultOptions
            , TileSetDefinition tdef
            , ProcessorSet processors)
        {
            if (tdef == null || processors == null)
                return null;

            resultOptions |= NMGenAssetFlag.PolyMesh;

            if ((processors.PreserveAssets & NMGenAssetFlag.DetailMesh) != 0
                && (resultOptions & NMGenAssetFlag.DetailMesh) == 0)
            {
                // The processors require the detail mesh, but the result won't include it.
                return null;
            }

            Vector3 bmin;
            Vector3 bmax;

            // This next call checks for valid tx/tz.
            if (!tdef.GetTileBounds(tx, tz, true, out bmin, out bmax))
                return null;

            NMGenTileParams tileConfig = new NMGenTileParams(tx, tz, bmin, bmax);

            return new IncrementalBuilder(tileConfig, tdef.GetBaseConfig(), resultOptions
                , tdef.Geometry, processors);
        }

        /// <summary>
        /// Returns human friendly text for the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>Human friendly text.</returns>
        public static string ToLabel(NMGenState state)
        {
            switch (state)
            {
                case NMGenState.Aborted:
                    return "Aborted.";
                case NMGenState.CompactFieldBuild:
                    return "Building compact heightfield.";
                case NMGenState.Complete:
                    return "Complete";
                case NMGenState.ContourBuild:
                    return "Building contours.";
                case NMGenState.DetailMeshBuild:
                    return "Building detail mesh.";
                case NMGenState.HeightfieldBuild:
                    return "Building heightfield.";
                case NMGenState.PolyMeshBuild:
                    return "Building polygon mesh.";
                case NMGenState.RegionBuild:
                    return "Building regions.";
                case NMGenState.NoResult:
                    return "No result.";
            }
            return "Unhandled state: " + state;
        }

        /// <summary>
        /// Returns a progress value associated with the specified state. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value will be between 0 and 1.0, suitable for providing build progress feedback.
        /// </para>
        /// </remarks>
        /// <param name="state">The state.</param>
        /// <returns>A progress value for the state.</returns>
        public static float ToProgress(NMGenState state)
        {
            const float inc = 1 / 6f;
            switch (state)
            {
                case NMGenState.Initialized:
                    return 0;
                case NMGenState.HeightfieldBuild:
                    return inc * 1;
                case NMGenState.CompactFieldBuild:
                    return inc * 2;
                case NMGenState.RegionBuild:
                    return inc * 3;
                case NMGenState.ContourBuild:
                    return inc * 4;
                case NMGenState.PolyMeshBuild:
                    return inc * 5;
                case NMGenState.DetailMeshBuild:
                    return inc * 6;
                case NMGenState.Complete:
                    return 1;
                case NMGenState.Aborted:
                    return 1;
                case NMGenState.NoResult:
                    return 1;
            }
            return 1;
        }
    }
}
