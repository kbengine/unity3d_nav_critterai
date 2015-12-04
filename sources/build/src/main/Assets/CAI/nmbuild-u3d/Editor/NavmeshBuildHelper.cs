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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class NavmeshBuildHelper
    {
        private readonly NavmeshBuild mBuild;
        private readonly UnityBuildContext mContext = new UnityBuildContext();

        public NavmeshBuildHelper(NavmeshBuild build)
        {
            if (!build)
                throw new System.ArgumentNullException();

            this.mBuild = build;
        }

        public void Build()
        {
            if (!mBuild)
                return;

            mBuild.ResetBuild();

            // Note: The 'finally' takes care of all cleanup.

            try
            {
                EditorUtility.DisplayCancelableProgressBar("Build & Bake"
                    , "Preparing..."
                    , 0);

                // Prepare the build.

                if (!CompileInput())
                    return;

                if (!InitializeBuild())
                    return;

                // Build the tiles.
                NavmeshParams nconfig = null;
                NavmeshTileData[] tiles = null;
                bool success = true;

                if (mBuild.TileSetDefinition == null)
                {
                    if (!BuildSingleTile())
                        return;

                    tiles = new NavmeshTileData[1] { mBuild.BuildData.GetTileData(0, 0) };

                    nconfig = NavUtil.DeriveConfig(tiles[0]);
                }
                else if (BuildMultiTiled())
                {
                    success = mBuild.BuildData.GetMeshBuildData(mBuild.TileSetDefinition.BoundsMin
                        , mBuild.TileSetDefinition.TileWorldSize
                        , out nconfig, out tiles);
                }
                else
                    return;

                if (!success)
                {
                    mContext.PostError("Navigation mesh creation failed.", mBuild);
                    return;
                }

                // Bake the mesh.

                NavStatus nstatus = 
                    mBuild.BuildTarget.Load(nconfig, tiles, NMBEditorUtil.GetConfig(mBuild));

                if ((nstatus & NavStatus.Sucess) == 0)
                    mContext.PostError("Bake to target: Target reported failure.", mBuild);
                else
                    EditorUtility.SetDirty((Object)mBuild.BuildTarget);
            }
            finally
            {
                mBuild.ResetBuild();
                EditorUtility.ClearProgressBar();
            }
        }

        private bool InitializeBuild()
        {
            mContext.ResetLog();

            if (!mBuild.InitializeBuild(mContext, false))
            {
                mContext.PostError("Build initialization failed.", mBuild);
                return false;
            }
            // Not helpful.
            //else
            //    mContext.PostTrace("Initialized build.", mBuild);

            mContext.ResetLog();

            return true;
        }

        private bool BuildSingleTile()
        {
            TileBuildData tdata = mBuild.BuildData;
            NMGenConfig config = mBuild.Config;
            InputGeometry geom = mBuild.InputGeom;

            mContext.ResetLog();

            /*
             * Design note:
             * 
             * Not using the build task since it doesn't provide enough progress
             * feedback for a single tile.
             * 
             */

            // Create the NMGen builder.

            IncrementalBuilder builder = IncrementalBuilder.Create(config.GetConfig()
                , config.ResultOptions
                , geom
                , mBuild.NMGenProcessors);

            if (builder == null)
            {
                mContext.PostError("Unexpected failure creating NMGen builder.", mBuild);
                tdata.SetAsFailed(0, 0);
                return false;
            }
            else if (builder.IsFinished)
            {
                if (builder.State == NMGenState.NoResult)
                {
                    mContext.PostError("NMGen build did not produce a result. (Early exit.)"
                        , builder.GetMessages(), mBuild);
                    tdata.SetAsFailed(0, 0);
                    return false;
                }
                else
                {
                    mContext.PostError("Unexpected NMGen builder completion."
                        , builder.GetMessages(), mBuild);
                    tdata.SetAsFailed(0, 0);
                    return false;
                }
            }

            mBuild.BuildData.SetAsInProgress(0, 0);

            // Run the NMGen builder.

            while (!builder.IsFinished)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Build Single Tile Mesh"
                    , IncrementalBuilder.ToLabel(builder.State)
                    , IncrementalBuilder.ToProgress(builder.State)))
                {
                    return false;
                }

                builder.Build();
            }

            // Handle NMGen failures.

             mContext.Log(builder.GetMessages());  // Single tile build.  So go ahead an record.

            switch (builder.State)
            {
                case NMGenState.Aborted:

                    mContext.PostError("NMGen build failed.", mBuild);
                    tdata.SetAsFailed(0, 0);
                    return false;

                case NMGenState.NoResult:

                    mContext.PostError("NMGen build did not produce a result.", mBuild);
                    tdata.SetAsFailed(0, 0);
                    return false;
            }

            mContext.Log(string.Format("Completed NMGen build: {0} polygons."
                , builder.Result.PolyMesh.PolyCount)
                , mBuild);

            // Build the tile.

            NMGenAssets result = builder.Result;

            NavmeshTileBuildData tbd = org.critterai.nmbuild.NMBuild.GetBuildData(
                mContext, 0, 0
                , result.PolyMesh.GetData(false), result.DetailMesh.GetData(false)
                , mBuild.Connections
                , (config.BuildFlags & NMGenBuildFlag.BVTreeEnabled) != 0);

            if (tbd == null)
            {
                // No need to log the error.  The above method takes care of that.
                tdata.SetAsFailed(0, 0);
                return false;
            }

            NavmeshTileData td = NavmeshTileData.Create(tbd);

            if (td.Size == 0)
            {
                mContext.PostError(
                    "Could not create {0} object. Cause unknown." + typeof(NavmeshTileData)
                    , mBuild);
                tdata.SetAsFailed(0, 0);
                return false;
            }

            // Finalize the tile.

            tdata.SetWorkingData(0, 0, result.PolyMesh, result.DetailMesh);
            tdata.SetWorkingData(0, 0, td, tbd.PolyCount);

            mContext.PostTrace("Completed single tile build.", mBuild);

            return true;
        }

        private bool BuildMultiTiled()
        {
            TileSetDefinition tdef = mBuild.TileSetDefinition;
            TileBuildData tdata = mBuild.BuildData;

            mContext.ResetLog();

            int total = tdef.Width * tdef.Depth;

            string msg = string.Format("Multi-tile build: {0} tiles ({1}x{2})"
                , total, tdef.Width, tdef.Depth);

            mContext.Log(msg, mBuild);

            int count = 0; // For the progress bar.
            for (int tx = 0; tx < tdef.Width; tx++)
            {
                for (int tz = 0; tz < tdef.Depth; tz++)
                {
                    count++;

                    string tileText = string.Format("({0},{1})", tx, tz);

                    if (EditorUtility.DisplayCancelableProgressBar("Multi-tiled Build & Bake"
                        , string.Format("Tile: {0}  ({1} of {2})", tileText, count, total)
                        , (float)count / total))
                    {
                        return false;
                    }

                    // Create the NMGen builder.

                    IncrementalBuilder builder = IncrementalBuilder.Create(tx, tz
                       , mBuild.Config.ResultOptions
                       , mBuild.TileSetDefinition
                       , mBuild.NMGenProcessors);

                    if (builder == null)
                    {
                        mContext.PostError(
                            "Unexpected failure creating NMGen builder: Tile: " + tileText
                            , mBuild);
                        tdata.SetAsFailed(tx, tz);
                        return false;
                    }

                    mBuild.BuildData.SetAsInProgress(tx, tz);

                    // Create and run the build task.

                    NMGenTask ntask = NMGenTask.Create(builder, 0);

                    ntask.Run();

                    if (ntask.TaskState == BuildTaskState.Aborted)
                    {
                        mContext.PostError("NMGen build task failed: Tile: " + tileText
                            , ntask.Messages, mBuild);
                        tdata.SetAsFailed(tx, tz);
                        return false;
                    }

                    NMGenAssets nr = ntask.Result;

                    if (nr.NoResult)
                    {
                        mContext.PostTrace("NMGen complete. Empty tile: " + tileText
                            , builder.GetMessages()
                            , mBuild);
                        tdata.SetAsEmpty(tx, tz);
                        continue;
                    }

                    msg = string.Format("NMGen complete. Tile {0} has {1} polygons."
                            , tileText, nr.PolyMesh.PolyCount);

                    mContext.PostTrace(msg, builder.GetMessages(), mBuild);

                    TileBuildTask ttask = TileBuildTask.Create(tx, tz
                        , nr.PolyMesh.GetData(false), nr.DetailMesh.GetData(false)
                        , mBuild.Connections
                        , (mBuild.Config.BuildFlags & NMGenBuildFlag.BVTreeEnabled) != 0
                        , false, 0);

                    ttask.Run();

                    if (ttask.TaskState == BuildTaskState.Aborted)
                    {
                        mContext.PostError("Tile build task failed: Tile: " + tileText
                            , ttask.Messages
                            , mBuild);
                        tdata.SetAsFailed(tx, tz);
                        return false;
                    }

                    TileBuildAssets tr = ttask.Result;

                    tdata.SetWorkingData(tx, tz, nr.PolyMesh, nr.DetailMesh);
                    tdata.SetWorkingData(tx, tz, tr.Tile, tr.PolyCount);
                }
            }

            int bakeable = tdata.BakeableCount();

            if (bakeable == 0)
            {
                mContext.PostError("Build did not produce any usuable tiles. (All tiles empty?)"
                    , mBuild);
                return false;
            }

            msg = string.Format("Tile build complete. {0} tiles produced. {1} empty tiles."
                , bakeable, tdata.GetStateCount(TileBuildState.Empty));

            mContext.PostTrace(msg, mBuild);

            return true;
        }

        private bool CompileInput()
        {
            mContext.ResetLog();

            InputAssets assets = BuildInput(false);

            if (assets.geometry == null)
            {
                mContext.PostError("No input geometry generated.", mBuild);
                return false;
            }

            org.critterai.geom.TriangleMesh mesh = assets.geometry;

            InputGeometryBuilder gbuilder = InputGeometryBuilder.UnsafeCreate(mesh
                , assets.areas
                , mBuild.Config.GetConfig().WalkableSlope
                , true);

            if (gbuilder == null)
            {
                mContext.PostError("Could not create input geometry builder. (Internal error.)"
                    , mBuild);
                return false;
            }

            gbuilder.BuildAll();

            if (mBuild.SetInputData(mContext, gbuilder.Result
                , assets.info, assets.processors, assets.conns
                , false))
            {
                mContext.PostTrace("Input compile complete.", mBuild);
            }

            return true;
        }

        public InputAssets BuildInput()
        {
            if (!mBuild)
                return new InputAssets();
            return BuildInput(true);
        }

        private InputAssets BuildInput(bool ownProgress)
        {
            InputBuildOption options = 
                (mBuild.AutoCleanGeometry ? InputBuildOption.AutoCleanGeometry : 0);

            InputBuilder builder = 
                InputBuilder.Create(mBuild.SceneQuery, mBuild.GetInputProcessors(), options);

            if (builder == null)
            {
                mContext.LogError("Could not create input builder.", mBuild);
                return new InputAssets();
            }

            try
            {
                while (!builder.IsFinished)
                {
                    builder.Build();
                    if (EditorUtility.DisplayCancelableProgressBar("Compile Input"
                        , InputBuilder.ToLabel(builder.State)
                        , InputBuilder.ToProgress(builder.State)))
                    {
                        return new InputAssets();
                    }
                }
            }
            finally
            {
                if (ownProgress)
                    EditorUtility.ClearProgressBar();
            }

            mContext.Log(builder.Messages);

            if (builder.State != InputBuildState.Complete)
            {
                mContext.LogError("Input builder aborted.", mBuild);
                return new InputAssets();
            }

            InputAssets assets = builder.Result;

            org.critterai.geom.TriangleMesh mesh = assets.geometry;

            if (!InputGeometryBuilder.IsValid(mesh, assets.areas))
            {
                mContext.LogError("Input geometry failed validation. (Malformed data.)", mBuild);
                return new InputAssets();
            }

            return builder.Result;
        }
    }
}
