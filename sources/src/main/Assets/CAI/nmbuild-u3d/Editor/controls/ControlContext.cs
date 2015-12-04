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
using UnityEngine;
using System.Collections.Generic;
using org.critterai.nmgen;

namespace org.critterai.nmbuild.u3d.editor
{
	internal sealed class ControlContext
	{
        private Rect mMainArea;
        private Rect mButtonArea;
        private bool mHideMain;

        private readonly BuildTaskProcessor mTaskProcessor;
        private readonly NavmeshBuild mBuild;
        private readonly TileSelection mSelection;

        private readonly List<TileBuildTask> mTileTasks = new List<TileBuildTask>();
        private readonly List<NMGenTask> mNMGenTasks = new List<NMGenTask>();

        public Rect MainArea
        {
            get { return mMainArea; }
            set { mMainArea = value; }
        }

        public Rect ButtonArea
        {
            get { return mButtonArea; }
            set { mButtonArea = value; }
        }

        public bool HideMain
        {
            get { return mHideMain; }
            set { mHideMain = value; }
        }

        public List<TileBuildTask> TileTasks { get { return mTileTasks; } }
        public List<NMGenTask> NMGenTasks { get { return mNMGenTasks; } } 
        public NavmeshBuild Build { get { return mBuild ? mBuild : null; } }
        public TileSelection Selection { get { return mSelection; } }
        public int TaskCount { get { return mNMGenTasks.Count + mTileTasks.Count; } }

        public ControlContext(NavmeshBuild build, BuildTaskProcessor manager)
        {
            if (!build || manager == null)
                throw new System.ArgumentNullException();

            mTaskProcessor = manager;

            mBuild = build;
            mSelection = new TileSelection(build);
        }

        public bool QueueTask(InputBuildTask task)
        {
            if (task == null || task.TaskState != BuildTaskState.Inactive)
                return false;

            return mTaskProcessor.QueueTask(task);
        }

        public bool QueueTask(BuildContext context
            , int tx, int tz
            , PolyMesh polyMesh, PolyMeshDetail detailMesh
            , bool bvTreeEnabled
            , int priority)
        {
            TileBuildTask task = TileBuildTask.Create(tx, tz
                , polyMesh.GetData(false)
                , (detailMesh == null ? null : detailMesh.GetData(true))
                , Build.Connections
                , bvTreeEnabled
                , true
                , priority);

            if (!mTaskProcessor.QueueTask(task))
            {
                context.LogError("Task processor rejected task.", this);
                return false;
            }

            mTileTasks.Add(task);

            return true;
        }

        public bool QueueTask(int tx, int tz, int priority, BuildContext logger)
        {
            // Check for existing task and purge it.

            NavmeshBuild build = Build;

            if (!build)
                return false;

            TileBuildData tdata = build.BuildData;

            if (build.TileSetDefinition == null && (tx > 0 || tz > 0))
            {
                logger.LogError("Tile build requested, but no tile set found.", this);
                return false;
            }

            if (AbortRequest(tx, tz, "Overriden by new task."))
            {
                tdata.ClearUnbaked(tx, tz);

                logger.LogWarning(string.Format(
                    "Existing build task overridden by new task. ({0}, {1})"
                        , tx, tz), this);
            }

            IncrementalBuilder builder;
            NMGenConfig config = build.Config;

            if (build.TileSetDefinition == null)
            {
                InputGeometry geom = build.InputGeom;

                if (geom == null)
                {
                    logger.LogError("Input geometry not available.", this);
                    tdata.SetAsFailed(tx, tz);
                    return false;
                }

                builder = IncrementalBuilder.Create(config.GetConfig()
                    , config.ResultOptions
                    , geom
                    , build.NMGenProcessors);
            }
            else
            {
                builder = IncrementalBuilder.Create(tx, tz
                    , config.ResultOptions
                    , build.TileSetDefinition
                    , build.NMGenProcessors);
            }

            if (builder == null)
            {
                logger.LogError(string.Format("Tile set did not produce a builder. Tile: ({0},{1})"
                        , tx, tz)
                    , this);
                return false;
            }

            NMGenTask task = NMGenTask.Create(builder, priority);

            if (!mTaskProcessor.QueueTask(task))
            {
                logger.LogError(string.Format("Processor rejected task. Tile: ({0},{1})"
                        , tx, tz), this);
                return false;
            }
            
            mNMGenTasks.Add(task);
            tdata.SetAsQueued(tx, tz);

            return true;
        }

        private bool AbortRequest(int tx, int tz, string reason)
        {
            bool result = false;

            // This is a thorough check, taking into account potential errors.

            for (int i = mNMGenTasks.Count - 1; i >= 0; i--)
            {
                NMGenTask item = mNMGenTasks[i];
                if (item.TileX == tx && item.TileZ == tz)
                {
                    item.Abort(reason);
                    mNMGenTasks.RemoveAt(i);
                    mBuild.BuildData.ClearUnbaked(tx, tz);
                    result = true;
                }
            }

            for (int i = mTileTasks.Count - 1; i >= 0; i--)
            {
                TileBuildTask item = mTileTasks[i];
                if (item.TileX == tx && item.TileZ == tz)
                {
                    item.Abort(reason);
                    mTileTasks.RemoveAt(i);
                    mBuild.BuildData.ClearUnbaked(tx, tz);
                    result = true;
                }
            }

            return result;
        }

        public void AbortAllReqests(string reason)
        {

            foreach (NMGenTask item in mNMGenTasks)
            {
                item.Abort(reason);
                if (mBuild)  // Important: Might be aborting due to lost build component.
                    mBuild.BuildData.ClearUnbaked(item.TileX, item.TileZ);                
            }

            mNMGenTasks.Clear();

            foreach (TileBuildTask item in mTileTasks)
            {
                item.Abort(reason);
                if (mBuild != null)  // Important: Might be aborting due to lost build component.
                    mBuild.BuildData.ClearUnbaked(item.TileX, item.TileZ);   
            }

            mTileTasks.Clear();
        }
	}
}
