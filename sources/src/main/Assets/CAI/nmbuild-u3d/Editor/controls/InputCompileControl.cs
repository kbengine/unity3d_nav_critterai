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
using org.critterai.u3d.editor;
using System;
using org.critterai.nav.u3d.editor;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class InputCompileControl
        : BuildControl
    {
        private const string Category = "Input Compile";
        private const float MarginSize = ControlUtil.MarginSize;

        private MiniInputCompile mCompiler;

        public override void Exit()
        {
            if (Context != null)
            {
                if (mCompiler != null && !mCompiler.IsFinished)
                    Debug.LogError("Input compile aborted. (Forced exit.)");
            }

            mCompiler = null;

            base.Exit();
        }

        public override void Update()
        {
            if (Context == null || mCompiler == null)
                // Either an error, or nothing to do.
                return;

            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            if (Context.Build.BuildState == NavmeshBuildState.Invalid
                && mCompiler != null)
            {
                Logger.PostError("Build has become invalid. Discarded input compile.", Context.Build);
                mCompiler = null;
                return;
            }

            if (!mCompiler.IsFinished)
            {
                mCompiler.Update();
                return;
            }

            if (!mCompiler.HasData)
            {
                Logger.PostError("Input data compile failed.", null);
                mCompiler = null;
            }
            else if (build.HasInputData)
            {
                // Note: Don't apply the changes if it will cause
                // a state transition. It creates GUI control issues.
                ApplyData();
            }
        }

        public bool HasInputData { get { return (mCompiler != null && mCompiler.HasData); } }

        protected override void OnGUIMain()
        {
            NavmeshBuild build = Context.Build;

            if (!build)
                return;
            
            Rect statusArea = Context.MainArea;

            if (build.BuildState == NavmeshBuildState.Invalid)
                return;

            InputBuildInfo info;
            InputGeometry geometry = null;
            ConnectionSet connections = null;
            int triCount = 0;
            int processorCount = 0;

            ViewOption viewFlags = 0;
            bool hasData = false;

            string topLabel;

            if (mCompiler != null)
            {
                if (mCompiler.IsFinished)
                {
                    if (mCompiler.HasData)
                        topLabel = "Input successfully compiled. (Needs to be accepted.)";
                    else
                        topLabel = "No input data produced.";
                }
                else
                    topLabel = "Compiling";

                info = mCompiler.Info;
                geometry = mCompiler.Geometry;
                connections = mCompiler.Connections;
                triCount = mCompiler.TriCount;
                processorCount = (mCompiler.Processors == null ? 0 : mCompiler.Processors.Length);

                if (geometry != null)
                    hasData = true;
            }
            else if (build.HasInputData)
            {
                topLabel = "Current Input";

                viewFlags = (ViewOption.Input | ViewOption.Grid);

                info = build.InputInfo;
                geometry = build.InputGeom;
                connections = build.Connections;
                triCount = geometry.TriCount;
                processorCount = build.NMGenProcessors.Count;

                hasData = true;
            }
            else
            {
                topLabel = "Input needs to be compiled.";
                info = new InputBuildInfo();
            }

            DebugContext.SetViews(viewFlags);

            if (!hasData && triCount > 0)
            {
                GUI.Box(Context.MainArea
                    , string.Format("{0} {1:N0} triangles", topLabel, triCount)
                    , EditorUtil.HelpStyle);

                OnGUICompiler(statusArea);

                return;
            }

            GUILayout.BeginArea(statusArea, GUI.skin.box);

            string currScene = System.IO.Path.GetFileName(EditorApplication.currentScene);

            int idx = currScene.LastIndexOf(".");
            if (idx >= 0)
                currScene = currScene.Substring(0, idx);

            if (currScene.Length == 0)
                currScene = "None";

            GUILayout.BeginHorizontal();

            GUILayout.Label("Input scene:");

            GUILayout.Label(" Current: " + currScene);

            GUILayout.Label(" Last: "
                + NavEditorUtil.SceneDisplayName(build.BuildTarget.BuildInfo));

            GUILayout.EndHorizontal();

            if (NavEditorUtil.SceneMismatch(build.BuildTarget.BuildInfo))
            {
                GUILayout.Box("Current scene does not match last input scene."
                    , EditorUtil.WarningStyle);
            }

            GUILayout.Space(MarginSize);

            GUILayout.Label(topLabel);

            if (hasData)
            {
                GUILayout.Space(ControlUtil.MarginSize * 3);

                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();

                GUILayout.Label("Geometry");

                GUILayout.Space(ControlUtil.MarginSize);

                GUILayout.Label(string.Format("Triangles: {0:N0}", triCount));

                GUILayout.Space(ControlUtil.MarginSize);

                GUILayout.Label("Min Bounds: " + Vector3Util.ToString(geometry.BoundsMin));

                GUILayout.Label("Max Bounds: " + Vector3Util.ToString(geometry.BoundsMax));

                GUILayout.Space(ControlUtil.MarginSize);

                Vector3 diff = geometry.BoundsMax - geometry.BoundsMin;
                GUILayout.Label(string.Format("WxHxD: {0:f3} x {1:f3} x {2:f3}"
                    , diff.x, diff.y, diff.z));

                GUILayout.Space(ControlUtil.MarginSize * 2);

                // Note: The build press interprets zero root objects as a global search.
                    
                GUILayout.Space(ControlUtil.MarginSize);
                GUILayout.Label("Components");
                GUILayout.Space(ControlUtil.MarginSize);

                GUILayout.Label("Pre-filter: " + info.compCountPre);
                GUILayout.Label("Post-filter: " + info.compCountPost);

                GUILayout.EndVertical();

                GUILayout.BeginVertical();

                GUILayout.Label("Modifiers");

                GUILayout.Space(ControlUtil.MarginSize);

                GUILayout.Label("Component loaders: " + info.loaderCount);
                GUILayout.Label("Component filters: " + info.filterCount);
                GUILayout.Label("Area assignment: " + info.areaModifierCount);
                GUILayout.Label("Component compilers: " + info.compilerCount);
                GUILayout.Label("Input post-processors: " + info.postCount);
                GUILayout.Label("NMGen processors: " + processorCount);
                GUILayout.Label("Off-Mesh connections: " + connections.Count);

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();

            OnGUICompiler(statusArea);
        }

        private void OnGUICompiler(Rect statusArea)
        {
            if (!(mCompiler == null || mCompiler.IsFinished))
            {
                // Assuming that the draw area is at least 25 in height.
                Rect area = new Rect(statusArea.x, statusArea.yMax - 25
                    , statusArea.width, 25);

                mCompiler.OnGUI(area);
            }
        }

        private void ApplyData()
        {
            NavmeshBuild build = Context.Build;  // Caller checks for null.

            if (!build.SetInputData(Logger, mCompiler.Geometry
                , mCompiler.Info, mCompiler.Processors, mCompiler.Connections
                , true))
            {
                Logger.PostError("Could not apply input data.", build);
                return;  // Let the compiler persist so user can review it.
            }

            mCompiler = null;
        }

        protected override void OnGUIButtons()
        {
            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            if (build.BuildState == NavmeshBuildState.Invalid)
            {
                GUI.Box(Context.ButtonArea, "");
                return;
            }

            bool hasLocalData = (mCompiler != null && mCompiler.HasData);
            bool dataExists = (build.HasInputData || hasLocalData);
            bool isCompiling = !(mCompiler == null || mCompiler.IsFinished);

            bool guiEnabled = GUI.enabled;

            GUI.enabled = !isCompiling;

            ControlUtil.BeginButtonArea(Context.ButtonArea);

            float origFVal = build.Config.WalkableSlope;

            GUILayout.Label(NMGenConfig.SlopeLabel);

            build.Config.WalkableSlope = EditorGUILayout.FloatField(build.Config.WalkableSlope);

            if (origFVal != build.Config.WalkableSlope)
                build.IsDirty = true;

            GUILayout.Space(MarginSize);

            string compileButtonText;

            GUIStyle style = (GUI.enabled && !dataExists) 
                ? ControlUtil.HighlightedButton : GUI.skin.button; 

            if (dataExists)
                compileButtonText = "Recompile";
            else
                compileButtonText = "Compile";

            if (GUILayout.Button(compileButtonText, style))
            {
                GC.Collect();
                Logger.ResetLog();
                mCompiler = new MiniInputCompile(Context);

                if (mCompiler.IsFinished)
                    mCompiler = null;
            }

            if (hasLocalData)
            {
                GUILayout.Space(MarginSize);

                style = (GUI.enabled ? ControlUtil.HighlightedButton : GUI.skin.button);

                if (GUILayout.Button("Accept", style))
                    ApplyData();
            }

            GUI.enabled = guiEnabled;

            if (isCompiling)
            {
                GUILayout.Space(MarginSize);

                if (GUILayout.Button("Cancel Compile"))
                {
                    mCompiler.Abort();
                    mCompiler = null;
                }
            }
            else
            {
                bool resetOK = (hasLocalData || build.HasInputData || build.HasBuildData);

                if (ControlUtil.OnGUIStandardButtons(Context, DebugContext, resetOK))
                    // Reset button clicked.
                    mCompiler = null;
            }

            ControlUtil.EndButtonArea();
        }
    }
}
