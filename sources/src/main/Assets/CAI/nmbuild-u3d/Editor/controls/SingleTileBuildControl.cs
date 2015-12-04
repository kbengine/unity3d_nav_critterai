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
using UnityEditor;
using UnityEngine;
using org.critterai.nav;
using org.critterai.u3d.editor;

namespace org.critterai.nmbuild.u3d.editor
{
	internal sealed class SingleTileBuildControl
        : BuilderControl
	{
        private static GUIStyle mBoxStyle;

        private float mProgress = 0;
        private double mLastTime = 0;

        public override bool Enter(ControlContext context, DebugViewContext debug)
        {
            if (base.Enter(context, debug))
            {
                NavmeshBuild build = context.Build;

                if (build.TileSetDefinition != null || build.Config.TileSize > 0)
                {
                    Debug.LogError(typeof(SingleTileBuildControl) + ": The build is tiled.", build);
                    return false;
                }

                return true;
            }
            return false;
        }

        protected override void OnGUIMain()
        {
            if (mBoxStyle == null)
            {
                mBoxStyle = new GUIStyle(EditorUtil.HelpStyle);
                mBoxStyle.alignment = TextAnchor.MiddleCenter;
            }

            TileBuildState bstate = Context.Build.BuildData.GetState(0, 0);

            switch (bstate)
            {
                case TileBuildState.NotBuilt:

                    ShowStatus("Ready to build.");
                    break;

                case TileBuildState.Empty:

                    ShowStatus("Build did not produce any results.");
                    break;

                case TileBuildState.Queued:

                    ShowStatus("Build is queued for background processing.");
                    break;

                case TileBuildState.Error:

                    ShowStatus("Build failed on error!");
                    break;

                case TileBuildState.Baked:

                    ShowStatus("Bake completed.");
                    break;

                case TileBuildState.Built:

                    ShowStatus("Ready to bake.");
                    break;

                case TileBuildState.InProgress:

                    ShowStatus("Build in progress...");

                    Rect area = Context.MainArea;

                    area = new Rect(area.x + area.width * 0.25f
                        , area.y + area.height * 0.66f
                        , area.width * 0.50f
                        , 20);

                    double now = EditorApplication.timeSinceStartup;

                    // Progress will fill up once every 10 seconds.
                    // Just something for the user to look at.
                    mProgress += (float)(now - mLastTime) * 0.05f;
                    mProgress = (mProgress > 1 ? 0 : mProgress);

                    mLastTime = now;

                    EditorGUI.ProgressBar(area , mProgress , "");

                    break;
            }

            GUILayout.Space(ControlUtil.MarginSize);
            OnGUIMainStandard();
        }

        private void ShowStatus(string message)
        {
            GUI.Box(Context.MainArea, message, mBoxStyle);
        }

        protected override void OnGUIButtons()
        {
            DebugContext.SetViews(ViewOption.Mesh);

            NavmeshBuild build = Context.Build;

            if (!build)
                // Build deleted.
                return;

            TileBuildData tdata = Context.Build.BuildData;
            TileBuildState bstate = tdata.GetState(0, 0);

            bool canBake = 
                (bstate == TileBuildState.Built || bstate == TileBuildState.Baked);

            bool isBuilding = (Context.TaskCount > 0);

            ControlUtil.BeginButtonArea(Context.ButtonArea);

            EditorGUIUtility.LookLikeControls(75);

            bool guiEnabled = GUI.enabled;

            GUI.enabled = !isBuilding;

            GUIStyle style = (canBake || isBuilding)
                ? GUI.skin.button : ControlUtil.HighlightedButton;

            if (GUILayout.Button("Build", style))
            {
                mProgress = 0;

                mLastTime = EditorApplication.timeSinceStartup;

                if (!Context.QueueTask(0, 0, BuildTaskProcessor.LowPriority, Logger))
                    Logger.PostError("Build task failed.", Context.Build);
            }

            GUI.enabled = !isBuilding && canBake;

            style = GUI.enabled
                ? ControlUtil.HighlightedButton : GUI.skin.button;

            if (GUILayout.Button("Bake", style))
                HandleBake();

            GUILayout.Space(ControlUtil.MarginSize);

            GUI.enabled = isBuilding;

            if (GUILayout.Button("Abort Build"))
                Context.AbortAllReqests("User requested.");

            GUI.enabled = guiEnabled;

            if (OnGUIStandardButtons())
            {
                // Special case.  Build was discarded.
                ControlUtil.EndButtonArea();
                return;
            }

            ControlUtil.OnGUIStandardButtons(Context, DebugContext, true);

            ControlUtil.EndButtonArea();
        }

        private void HandleBake()
        {
            NavmeshBuild build = Context.Build;

            NavmeshTileData[] tiles = new NavmeshTileData[1] 
            {
                build.BuildData.GetTileData(0, 0)
            };

            NavmeshParams nconfig = NavUtil.DeriveConfig(tiles[0]);

            NavStatus status = build.BuildTarget.Load(nconfig, tiles, NMBEditorUtil.GetConfig(build));

            if ((status & NavStatus.Failure) == 0)
            {
                EditorUtility.SetDirty((Object)build.BuildTarget);
                // build.BuildData.SetAsBaked();
                build.BuildData.Reset(0, 0);  // Don't need to keep the data.
            }
            else
                Logger.PostError("Bake to target: Target reported failure.", (Object)build.BuildTarget);
        }
    }
}
