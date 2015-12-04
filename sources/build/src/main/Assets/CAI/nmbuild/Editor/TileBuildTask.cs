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
using org.critterai.nmgen;
using org.critterai.nav;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A task used to manage the build of an a tile from NMGen and connection data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This task performs the tile build step in the navigation mesh build pipeline.
    /// </para>
    /// </remarks>
    public sealed class TileBuildTask
        : BuildTask<TileBuildAssets>
    {
        private readonly int mTileX;
        private readonly int mTileZ;
        private readonly bool mIsThreadSafe;
        private readonly bool mBVTreeEnabled;
        private PolyMeshData mPolyData;
        private PolyMeshDetailData mDetailData;
        private ConnectionSet mConnections;

        private TileBuildTask(int tx, int tz
            , PolyMeshData polyData
            , PolyMeshDetailData detailData
            , ConnectionSet connections
            , bool bvTreeEnabled
            , bool isThreadSafe
            , int priority)
            : base(priority)
        {
            mTileX = tx;
            mTileZ = tz;
            mPolyData = polyData;
            mDetailData = detailData;
            mConnections = connections;
            mBVTreeEnabled = bvTreeEnabled;
            mIsThreadSafe = isThreadSafe;
        }

        /// <summary>
        /// If true, the task is safe to run on its own thread.
        /// </summary>
        public override bool IsThreadSafe { get { return mIsThreadSafe; } }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileX { get { return mTileX; } }

        /// <summary>
        /// The z-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileZ { get { return mTileZ; } }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The task should only be marked as thread-safe if the data parameters are treated 
        /// as immutable while the task is running.
        /// </para>
        /// <para>
        /// Creation will fail on null parameters, invalid tile indices, and an empty
        /// polygon mesh.
        /// </para>
        /// </remarks>
        /// <param name="tx">The x-index of the tile within the tile grid. (x, z)</param>
        /// <param name="tz">The z-index of the tile within the tile grid. (x, z)</param>
        /// <param name="polyData">The polygon mesh data.</param>
        /// <param name="detailData">The detail mesh data. (Optional)</param>
        /// <param name="conns">The off-mesh connection set.</param>
        /// <param name="bvTreeEnabled">True if bounding volumes should be generated.</param>
        /// <param name="isThreadSafe">True if the task is safe to run on its own thread.</param>
        /// <param name="priority">The task priority.</param>
        /// <returns>A new task, or null on error.</returns>
        public static TileBuildTask Create(int tx, int tz
            , PolyMeshData polyData
            , PolyMeshDetailData detailData
            , ConnectionSet conns
            , bool bvTreeEnabled
            , bool isThreadSafe
            , int priority)
        {
            if (tx < 0 || tz < 0
                || polyData == null || polyData.polyCount == 0
                || conns == null)
            {
                return null;
            }

            return new TileBuildTask(tx, tz
                , polyData, detailData, conns, bvTreeEnabled, isThreadSafe, priority);
        }

        /// <summary>
        /// Performs a work increment.
        /// </summary>
        /// <returns>True if the task is not yet finished.  Otherwise false.</returns>
        protected override bool LocalUpdate() 
        { 
            // All the work is done in GetResult().
            return false; 
        }

        /// <summary>
        /// Gets the result of the completed task.
        /// </summary>
        /// <param name="result">The result of the completed task.</param>
        /// <returns>True if the result is available, false if the task should abort with no
        /// result. (I.e. An internal abort.)</returns>
        protected override bool GetResult(out TileBuildAssets result)
        {
            BuildContext logger = new BuildContext();

            result = new TileBuildAssets();

            NavmeshTileBuildData tbd =
                NMBuild.GetBuildData(logger, mTileX, mTileZ
                , mPolyData, (mDetailData == null ? null : mDetailData), mConnections
                , mBVTreeEnabled);

            AddMessages(logger.GetMessages());

            if (tbd == null)
                return false;

            NavmeshTileData td = NavmeshTileData.Create(tbd);

            if (td.Size == 0)
            {
                AddMessage(string.Format(
                    "Could not create {2} object. Cause unknown."
                    + " Tile: ({0},{1})"
                    , mTileX, mTileZ, td.GetType().Name));

                return false;
            }

            result = new TileBuildAssets(mTileX, mTileZ, td, tbd.PolyCount);

            return true;
        }

        /// <summary>
        /// Finalize the task.
        /// </summary>
        protected override void FinalizeTask()
        {
            mPolyData = null;
            mDetailData = null;
            mConnections = null;
        }
    }
}
