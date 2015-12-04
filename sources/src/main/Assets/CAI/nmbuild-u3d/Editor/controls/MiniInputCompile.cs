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
using UnityEditor;
using org.critterai.geom;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class MiniInputCompile
    {
        private enum State
        {
            Builder,
            Task,
            Finished
        }

        private const float MarginSize = ControlUtil.MarginSize;

        private readonly ControlContext mContext;
        private readonly UnityBuildContext mLogger = new UnityBuildContext();

        private InputBuilder mBuilder;
        private InputBuildTask mTask;

        private InputAssets mAssets;
        private InputGeometry mGeometry;

        private State mState;

        private double mChunkTime;

        public bool IsFinished { get { return (mState == State.Finished); } }

        public bool HasData { get { return mGeometry != null; } }

        public ConnectionSet Connections
        {
            get { return (mGeometry == null ? null : mAssets.conns); }
        }

        public InputBuildInfo Info
        {
            get { return (mGeometry == null ? new InputBuildInfo() : mAssets.info); }
        }

        public InputGeometry Geometry { get { return mGeometry; } }
        
        public INMGenProcessor[] Processors 
        { 
            get { return (mGeometry == null ? null : mAssets.processors); } 
        }

        public int TriCount
        {
            get { return (mGeometry == null? 0 : mGeometry.TriCount); }
        }

        public MiniInputCompile(ControlContext context) 
        {
            mContext = context;

            NavmeshBuild build = context.Build;

            if (!build)
            {
                FinalizeOnFail("The control context's build does not exist.", true);
                return;
            }

            InputBuildOption options = InputBuildOption.ThreadSafeOnly;
            options |= (build.AutoCleanGeometry ? InputBuildOption.AutoCleanGeometry : 0);

            mBuilder = InputBuilder.Create(build.SceneQuery
                , build.GetInputProcessors()
                , options);

            if (mBuilder == null)
            {
                FinalizeOnFail("Could not create input builder.", true);
                return;
            }

            mState = State.Builder;
        }

        public void Update()
        {
            if (mState == State.Finished)
                // Nothing to do.
                return;

            NavmeshBuild build = mContext.Build;

            if (!build)
            {
                FinalizeOnFail("Build has been deleted.", true);
                return;
            }

            if (mContext.Build.BuildState == NavmeshBuildState.Invalid)
            {
                FinalizeOnFail("Build has become invalid. Discarded input compile", true);
                return;
            }

            if (mState == State.Builder)
                UpdateBuilder();
            else
                UpdateTask();
        }

        private void UpdateTask()
        {
            if (!mTask.IsFinished)
                return;

            mLogger.Log(mTask.Messages);

            if (mTask.TaskState == BuildTaskState.Aborted)
            {
                FinalizeOnFail("Input geometry build failed.", true);
                return;
            }

            mGeometry = mTask.Result;

            mLogger.PostTrace("Completed input geometry build.", mTask.Messages, mContext.Build);
            mTask = null;

            mState = State.Finished;
        }

        private void UpdateBuilder()
        {
            if (!mBuilder.IsFinished)
            {
                mBuilder.Build();
                return;
            }

            mLogger.Log(mBuilder.Messages);

            NavmeshBuild build = mContext.Build;  // Caller has validated.

            if (mBuilder.State == InputBuildState.Aborted)
            {
                FinalizeOnFail("Input data compile failed: Builder aborted.", true);
                return;
            }

            mAssets = mBuilder.Result;

            TriangleMesh mesh = mAssets.geometry;

            if (!InputGeometryBuilder.IsValid(mesh, mAssets.areas))
            {
                FinalizeOnFail("Input geometry failed validation. (Malformed data.)", true);
                return;
            }

            InputGeometryBuilder gbuilder = InputGeometryBuilder.UnsafeCreate(mesh
                , mAssets.areas
                , build.Config.GetConfig().WalkableSlope
                , true);

            if (gbuilder == null)
            {
                FinalizeOnFail("Could not create input geometry builder. (Internal error.)", true);
                return;
            }

            // Release unneeded assets.
            mAssets.geometry = null;
            mAssets.areas = null;
            mBuilder = null;

            mTask = InputBuildTask.Create(gbuilder, BuildTaskProcessor.HighPriority);

            if (mTask == null)
                FinalizeOnFail("Task creation failed. (Internal error.)", true);
            else if (mContext.QueueTask(mTask))
            {
                mLogger.PostTrace("Completed input build. Submitted geometry build task."
                    , mContext.Build);
                mState = State.Task;
            }
            else
                FinalizeOnFail("Task submission failed. (Internal error.)", true);

        }

        public void OnGUI(Rect area)
        {
            switch (mState)
            {
                case State.Builder:

                    EditorGUI.ProgressBar(area
                        , InputBuilder.ToProgress(mBuilder.State)
                        , InputBuilder.ToLabel(mBuilder.State));

                    break;

                case State.Task:

                    float delta =  (float)(EditorApplication.timeSinceStartup - mChunkTime);

                    if (delta > 15)
                    {
                        mChunkTime = EditorApplication.timeSinceStartup;
                        delta = 0;
                    }

                    EditorGUI.ProgressBar(area
                        , (delta / 15)
                        , "Chunking geometry...");

                    break;
            }
        }

        public void Abort()
        {
            FinalizeOnFail("User requested abort.", false);
        }

        private void FinalizeOnFail(string message, bool postError)
        {
            if (mTask != null)
            {
                mTask.Abort(message);
                mTask = null;
            }

            if (postError)
                mLogger.PostError(message, mContext.Build);

            mBuilder = null;
            mGeometry = null;
            mAssets = new InputAssets();
            mState = State.Finished;
        }
    }
}
