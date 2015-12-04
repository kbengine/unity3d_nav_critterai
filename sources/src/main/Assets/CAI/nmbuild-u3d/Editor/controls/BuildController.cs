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
using System.Collections.Generic;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// 
    /// </summary>
	internal class BuildController
	{
        private enum ControlType : byte
        {
            Build = 0x01,
            Config = 0x02,
            Input = 0x04,
        }

        private static GUIStyle mSelectedStyle;
        private static GUIStyle mUnselectedStyle;

        private const float MarginSize = ControlUtil.MarginSize;
        private const float SelectAreaHeight = 20;

        private readonly ControlContext mContext;
        private readonly DebugViewContext mDebugContext;
        private readonly UnityBuildContext mLogger = new UnityBuildContext();

        private bool mIsActive = false;
        private NavmeshBuildState mLastBuildState;

        private readonly InputCompileControl mInputCon;
        private readonly NMGenConfigControl mConfigCon;
        private IBuildControl mBuildCon;  // Can be null;

        private ControlType mSelectedControl = 0;

        private IBuildControl mOverrideControl;

        public NavmeshBuild Build { get { return mContext.Build; } }
        public int ActiveTasks { get { return mContext.TaskCount; } }

        public bool BuildIsActive
        {
            get 
            {  
                NavmeshBuildState bs = mContext.Build.BuildState;
                return ((bs != NavmeshBuildState.Inactive && bs != NavmeshBuildState.Invalid)
                    || mInputCon.HasInputData);
            }
                
        }

        public BuildController(NavmeshBuild build, BuildTaskProcessor manager)
        {
            if (!build || manager == null)
                throw new System.ArgumentNullException();

            mContext = new ControlContext(build, manager);
            mDebugContext = new DebugViewContext(build, mContext.Selection);

            mInputCon = new InputCompileControl();
            mConfigCon = new NMGenConfigControl();
        }

        public bool Enter()
        {
            // Note: Never returns false. Displays a failure control instead.

            if (mIsActive)
                return true;

            NavmeshBuild build = mContext.Build;

            if (!build)
            {
                SetCriticalFailure("The build object has been deleted.");
                return true;
            }

            build.CleanBuild();

            if (build.HasBuildData)
            {
                Debug.LogWarning("Build component contains intermediate build data." 
                    + " Potential causes:\n"
                    + "Editor reset or entered play mode in the middle of a build.\n"
                    + "User forgot to exit the build.\n"
                    + "User keeping the data around for later use.\n"
                    , build);
            }

            if (build.BuildState == NavmeshBuildState.NeedsRecovery)
            {
                // Special handling is needed since the normal state transition
                // never expects this state.
                if (SetInputControl(true))
                {
                    SetConfigControl(true);
                }
                mSelectedControl = ControlType.Input;
                mLastBuildState = NavmeshBuildState.NeedsRecovery;
            }
            else
                PerformStateTransition();

            mIsActive = true;
            return true;
        }

        public void Exit()
        {
            ExitChildControls();

            if (mContext.Build != null)
            {
                mContext.AbortAllReqests("Build controller exiting");
                mContext.Build.CleanBuild();
            }

            mLogger.ResetLog();
            mIsActive = false;
        }

        public void Update()
        {
            if (!mIsActive)
                return;

            NavmeshBuild build = mContext.Build;

            if (!build)
                // Asset was deleted.
                return;

            NavmeshBuildState buildState = build.BuildState;

            if (HasCriticalFailure)
            {
                if (buildState == NavmeshBuildState.Inactive)
                {
                    // A reset has been performed.  OK to go back to normal
                    // operation.
                    PerformStateTransition();
                }
                // Never do more than a recovery during an update.
                return;
            }

            if (mLastBuildState != buildState)
                PerformStateTransition();

            if (mOverrideControl != null)
            {
                mOverrideControl.Update();
                // No other operations allowed.
            }
            else
            {
                if (mInputCon.IsActive)
                    mInputCon.Update();

                if (mConfigCon.IsActive)
                    mInputCon.Update();

                if (mBuildCon != null)
                    mBuildCon.Update();

                ManageNMGenRequests();
                ManageTileRequests();
            }

            if (build.IsDirty)
            {
                EditorUtility.SetDirty(build);
                build.IsDirty = false;
            }
        }

        private void ManageNMGenRequests()
        {
            mLogger.ResetLog();

            TileBuildData tdata = mContext.Build.BuildData;

            // Due to concurrency with the input build, this method
            // does not log things via the context.

            List<NMGenTask> requests = mContext.NMGenTasks;

            for (int i = requests.Count - 1; i >= 0; i--)
            {
                NMGenTask item = requests[i];

                if (item.IsFinished)
                {
                    requests.RemoveAt(i);

                    NavmeshBuild build = mContext.Build;

                    if (!build)
                        // Asset was deleted.
                        continue;

                    string tileText = string.Format("({0},{1})", item.TileX, item.TileZ);

                    switch (item.TaskState)
                    {
                        case BuildTaskState.Aborted:

                            mLogger.Log(item.Messages);
                            mLogger.PostError("Tile build failed: " + tileText, build);

                            tdata.SetAsFailed(item.TileX, item.TileZ);

                            break;

                        case BuildTaskState.Complete:

                            NMGenAssets r = item.Result;

                            if (r.NoResult)
                            {
                                mLogger.PostTrace("NMGen build complete. Tile is empty: " + tileText
                                    , item.Messages, build);

                                tdata.SetAsEmpty(r.TileX, r.TileZ);
                            }
                            else
                            {
                                tdata.SetWorkingData(r.TileX, r.TileZ, r.PolyMesh, r.DetailMesh);

                                mLogger.PostTrace("NMGen build complete: " + tileText
                                    , item.Messages, build);

                                mContext.QueueTask(mLogger,r.TileX, r.TileZ
                                    , r.PolyMesh, r.DetailMesh
                                    , (build.Config.BuildFlags & NMGenBuildFlag.BVTreeEnabled) != 0
                                    , item.Priority);
                            }

                            break;
                    }
                }
                else if (item.TaskState == BuildTaskState.InProgress
                    && tdata.GetState(item.TileX, item.TileZ) != TileBuildState.InProgress)
                {
                    // Transition to the in-progress state.
                    tdata.SetAsInProgress(item.TileX, item.TileZ);
                }
            }
        }

        private void ManageTileRequests()
        {
            TileBuildData tdata = mContext.Build.BuildData;

            mLogger.ResetLog();

            // Due to concurrency with the input build, this method
            // does not log things via the context.

            List<TileBuildTask> requests = mContext.TileTasks;

            for (int i = requests.Count - 1; i >= 0; i--)
            {
                TileBuildTask item = requests[i];

                if (item.IsFinished)
                {
                    requests.RemoveAt(i);

                    NavmeshBuild build = mContext.Build;

                    if (!build)
                        // Asset was deleted.
                        continue;

                    string tileText = string.Format("({0},{1})", item.TileX, item.TileZ);

                    switch (item.TaskState)
                    {
                        case BuildTaskState.Aborted:

                            mLogger.Log(item.Messages);
                            mLogger.PostError("Tile build failed: " + tileText, build);

                            tdata.SetAsFailed(item.TileX, item.TileZ);

                            break;

                        case BuildTaskState.Complete:

                            TileBuildAssets r = item.Result;

                            string msg;

                            if (r.NoResult)
                            {
                                msg = "Tile build complete. Tile is empty: " + tileText;
                                tdata.SetAsEmpty(r.TileX, r.TileZ);
                            }
                            else
                            {
                                msg = "Tile build complete: " + tileText;
                                tdata.SetWorkingData(r.TileX, r.TileZ, r.Tile, r.PolyCount);
                            }

                            mLogger.PostTrace(msg, item.Messages, build);

                            break;
                    }
                }
            }
        }

        public void OnGUI(Rect area, bool includeMain)
        {
            if (!mIsActive)
                return;

            if (mSelectedStyle == null)
            {
                // Initial setup required.
                mSelectedStyle = new GUIStyle(GUI.skin.box);
                mSelectedStyle.normal.textColor = ControlUtil.StandardHighlight;

                mUnselectedStyle = new GUIStyle(GUI.skin.box);

                Color c = Color.white;
                c.a = 0.75f;

                mUnselectedStyle.normal.textColor = c;

            }

            // Setup...

            mContext.HideMain = !includeMain;

            Rect selectArea =  new Rect(area.x
                , area.y
                , area.width - ControlUtil.ButtonAreaWidth - MarginSize
                , SelectAreaHeight);

            Rect ma = new Rect(area.x
                , selectArea.yMax + MarginSize
                , selectArea.width
                , area.height - selectArea.height - MarginSize);

            mContext.MainArea = ma;

            mContext.ButtonArea = new Rect(selectArea.xMax + MarginSize
               , area.y
               , ControlUtil.ButtonAreaWidth
               , area.height);

            // Handle the main display area.

            if (mOverrideControl != null)
                mOverrideControl.OnGUI();
            else
            {
                switch (mSelectedControl)
                {
                    case ControlType.Build:

                        mBuildCon.OnGUI();
                        break;

                    case ControlType.Config:

                        mConfigCon.OnGUI();
                        break;

                    case ControlType.Input:

                        mInputCon.OnGUI();
                        break;
                }
            }

            if (includeMain)
            {
                // Draw the panel selection controls.

                const float bwidth = 100;

                GUILayout.BeginArea(selectArea);
                GUILayout.BeginHorizontal();

                GUI.enabled = mInputCon.IsActive;

                GUIStyle style = (GUI.enabled && mSelectedControl == ControlType.Input)
                    ? mSelectedStyle : mUnselectedStyle;

                if (GUILayout.Button("Input", style, GUILayout.Width(bwidth)))
                    mSelectedControl = ControlType.Input;

                GUI.enabled = mConfigCon.IsActive;

                style = (GUI.enabled && mSelectedControl == ControlType.Config)
                    ? mSelectedStyle : mUnselectedStyle;

                if (GUILayout.Button("Configuration", style, GUILayout.Width(bwidth)))
                    mSelectedControl = ControlType.Config;

                GUI.enabled = (mBuildCon != null);

                style = (GUI.enabled && mSelectedControl == ControlType.Build)
                    ? mSelectedStyle : mUnselectedStyle;

                if (GUILayout.Button("Builder", style, GUILayout.Width(bwidth)))
                    mSelectedControl = ControlType.Build;

                GUI.enabled = true;

                GUILayout.FlexibleSpace();

                UnityBuildContext.TraceEnabled =
                    GUILayout.Toggle(UnityBuildContext.TraceEnabled, "Trace");

                GUILayout.Space(MarginSize);

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (mDebugContext.NeedsRepaint || mContext.Selection.IsDirty)
            {
                // Debug.Log("Build Controller: Repaint Triggered");
                mDebugContext.NeedsRepaint = false;
                mContext.Selection.IsDirty = false;
                SceneView.RepaintAll();
            }
        }

        public void OnSceneGUI()
        {
            mDebugContext.OnSceneGUI();
        }

        private void PerformStateTransition()
        {
            mContext.Selection.ClearSelection();

            mSelectedControl = 0;  // Will always change during a transition.

            if (mOverrideControl != null)
            {
                // Always get rid of the override control. A new one
                // will be created as needed.
                mOverrideControl.Exit();
                mOverrideControl = null;
            }

            NavmeshBuild build = mContext.Build;  // Note: Caller already checked for null.
            NavmeshBuildState buildState = build.BuildState;

            // No early exits after this point.

            // Debug.Log("Transition: " + buildState);

            switch (buildState)
            {
                case NavmeshBuildState.Inactive:

                    // Needs first time compile of input data.

                    ExitChildControls();

                    if (SetInputControl(true))
                        mSelectedControl = ControlType.Input;

                    break;

                case NavmeshBuildState.InputCompiled:

                    // Let user update config before preparing the build.

                    SetBuildControl(false);

                    if (SetConfigControl(true) && SetInputControl(true))
                        mSelectedControl = ControlType.Config;

                    break;

                case NavmeshBuildState.NeedsRecovery:

                    ExitChildControls();
                    SetCriticalFailure("Internal error: Unexpected loss of input data.");

                    break;

                case NavmeshBuildState.Buildable:

                    if (SetBuildControl(true)
                        && SetConfigControl(true)
                        && SetInputControl(true))
                    {
                        mSelectedControl = ControlType.Build;
                    }

                    break;

                case NavmeshBuildState.Invalid:

                    ExitChildControls();

                    mOverrideControl = new CoreFailureControl();
                    mOverrideControl.Enter(mContext, mDebugContext);

                    break;

                default:

                    SetCriticalFailure("Internal error. Unhandled build state: " + buildState);
                    break;
            }

            // Note: Don't requery the build.  This is the state that was
            // handled.
            mLastBuildState = buildState;

            mDebugContext.NeedsRepaint = true;
        }

        private bool SetInputControl(bool enabled)
        {
            if (enabled)
            {
                if (!mInputCon.IsActive)
                {
                    if (!mInputCon.Enter(mContext, mDebugContext))
                    {
                        SetCriticalFailure(mInputCon.GetType());
                        return false;
                    }
                }
            }
            else if (mInputCon.IsActive)
                mInputCon.Exit();

            return true;
        }

        private bool SetConfigControl(bool enabled)
        {
            if (enabled)
            {
                if (!mConfigCon.IsActive)
                {
                    if (!mConfigCon.Enter(mContext, mDebugContext))
                    {
                        SetCriticalFailure(mConfigCon.GetType());
                        return false;
                    }
                }
            }
            else if (mConfigCon.IsActive)
                mConfigCon.Exit();

            return true;
        }

        private bool SetBuildControl(bool enabled)
        {
            // Note: Can't be used to change tye type of the build control.

            if (enabled)
            {
                if (mBuildCon != null)
                    return true;

                IBuildControl con;

                if (mContext.Build.BuildData.IsTiled)
                    con = new MultiTileBuildControl();
                else
                    con = new SingleTileBuildControl();

                if (con.Enter(mContext, mDebugContext))
                {
                    mBuildCon = con;
                    return true;
                }

                return false;
            }

            if (mBuildCon != null)
            {
                mBuildCon.Exit();
                mBuildCon = null;
            }

            return true;
        }

        private void ExitChildControls()
        {
            if (mOverrideControl != null)
            {
                mOverrideControl.Exit();
                mOverrideControl = null;
            }

            SetInputControl(false);
            SetConfigControl(false);
            SetBuildControl(false);

            mSelectedControl = 0;
        }

        private bool HasCriticalFailure
        {
            get { return (mOverrideControl is FailureControl); }
        }

        private void SetCriticalFailure(string message)
        {
            ExitChildControls();

            // Cleanup, just in case the critical failure effected task 
            // management...
            mContext.AbortAllReqests("Critical failure.");

            mOverrideControl = new FailureControl(message);
            mOverrideControl.Enter(mContext, mDebugContext);
        }

        private void SetCriticalFailure(System.Type type)
        {
            SetCriticalFailure("Internal error: Coult not initialize control: "
                    + type.ToString());
        }

        public static string FlattenMessages(string[] msgs)
        {
            if (msgs == null || msgs.Length == 0)
                return "";

            if (msgs.Length == 0)
                return "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (string msg in msgs)
            {
                sb.AppendLine(msg);
            }

            return sb.ToString().Trim();
        }

        public static void PostError(System.Type typ
            , string summary
            , string[] messages
            , Object context)
        {
            Debug.LogError(string.Format("{0}: {1}\n{2}"
                    , typ.Name
                    , summary
                    , FlattenMessages(messages)), context);
        }

        public static void PostTrace(System.Type typ
            , string summary
            , string[] messages
            , Object context)
        {
            Debug.Log(string.Format("{0}: {1}\n{2}"
                    , typ.Name
                    , summary
                    , FlattenMessages(messages)), context);
        }
    }
}
