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
using UnityEditor;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
	internal sealed class MultiTileBuildControl
        : BuilderControl
	{
        private const float MarginSize = ControlUtil.MarginSize;
        private const int GridCellSize = 32;

        private Vector3[] mCellVerts = new Vector3[4];

        private static GUIStyle mBlackLabel;

        private Vector2 mScrollPos = Vector2.zero;

        private int mMouseX = TileSelection.NoSelection;
        private int mMouseZ = TileSelection.NoSelection;

        public override bool Enter(ControlContext context, DebugViewContext debug)
        {
            if (base.Enter(context, debug))
            {
                NavmeshBuild build = context.Build;

                if (build.TileSetDefinition == null)
                {
                    Debug.LogError(typeof(MultiTileBuildControl) 
                        + ": Build data has not been initialized, or not a tiled build.");
                    return false;
                }

                return true;
            }
            return false;
        }

        protected override void OnGUIMain()
        {
            if (mBlackLabel == null)
            {
                // Need to initialize shared style.
                mBlackLabel = new GUIStyle(GUI.skin.label);
                mBlackLabel.normal.textColor = Color.black;
                mBlackLabel.fontStyle = FontStyle.Bold;
            }

            TileSelection selection = Context.Selection;

            selection.Validate();

            TileBuildData tdata = Context.Build.BuildData;

            if (tdata == null)
                return;

            Rect mainArea = Context.MainArea;

            // The box and shift makes it look better.
            GUI.Box(mainArea, "");
            mainArea.x += MarginSize;
            mainArea.y += MarginSize;

            // Draw the status grid.

            // Note: View is expanded by one grid size in order to minimize
            // grid/slider overlap.
            Rect view
                = new Rect(0, 0, tdata.Width * GridCellSize + GridCellSize
                    , tdata.Depth * GridCellSize + GridCellSize);

            mScrollPos = GUI.BeginScrollView(mainArea, mScrollPos, view);

            OnGUIStatusGrid(view.height - GridCellSize);

            GUI.EndScrollView();

            OnGUIMainStandard();

            if (IsBaseBusy)
                return;

            // Handle the mouse, including click selection.

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            if (mainArea.Contains(mousePos))
            {
                Vector2 gridPos = mousePos;

                gridPos.x -= mainArea.xMin - mScrollPos.x;
                gridPos.y -= mainArea.yMin - mScrollPos.y;

                int x = Mathf.FloorToInt(gridPos.x / GridCellSize);
                // For the depth, we need to invert the y-axis.
                int z = tdata.Depth - Mathf.FloorToInt(gridPos.y / GridCellSize) - 1;

                if (x < tdata.Width && z >= 0 && z < tdata.Depth)
                {
                    GUI.Label(new Rect(mousePos.x - 20, mousePos.y - 20, 120, 25)
                        , "(" + x + "," + z + "): " + tdata.GetState(x, z)
                        , mBlackLabel);

                    mMouseX = x;
                    mMouseZ = z;

                    if (evt.type == EventType.MouseDown && evt.button == 0)
                    {
                        if (selection.SelectedX == mMouseX && selection.SelectedZ == mMouseZ)
                            // Clicked on same tile. Deselect.
                            selection.ClearSelection();
                        else
                            selection.SetSelection(mMouseX, mMouseZ);
                    }
                }
                else
                {
                    mMouseX = TileSelection.NoSelection;
                    mMouseZ = TileSelection.NoSelection;
                }
            }
            else
            {
                mMouseX = TileSelection.NoSelection;
                mMouseZ = TileSelection.NoSelection;
            }
        }

        private void OnGUIStatusGrid(float areaHeight)
        {
            TileBuildData tdata = Context.Build.BuildData;
            
            for (int tx = 0; tx < tdata.Width; tx++)
            {
                for (int tz = 0; tz < tdata.Depth; tz++)
                {
                    Vector3 origin = new Vector3(tx * GridCellSize, areaHeight - tz * GridCellSize);

                    mCellVerts[0] = origin;

                    mCellVerts[1] = origin;
                    mCellVerts[1].x += GridCellSize;

                    mCellVerts[2] = origin;
                    mCellVerts[2].x += GridCellSize;
                    mCellVerts[2].y -= GridCellSize;

                    mCellVerts[3] = origin;
                    mCellVerts[3].y -= GridCellSize;

                    Color c = ToColor(tdata.GetState(tx, tz));

                    Handles.DrawSolidRectangleWithOutline(mCellVerts, c, Color.black);
                }
            }

            OnGUISelection(areaHeight);
        }

        private void OnGUISelection(float areaHeight)
        {
            Vector3 origin;
            int xSize = GridCellSize;
            int ySize = GridCellSize;

            TileSelection selection = Context.Selection;

            if (selection.Validate())
            {
                // Draw the tile marker.
                origin = new Vector3(selection.SelectedX * GridCellSize
                    , areaHeight - selection.SelectedZ * GridCellSize);

                mCellVerts[0] = origin;

                mCellVerts[1] = origin;
                mCellVerts[1].x += GridCellSize;

                mCellVerts[2] = origin;
                mCellVerts[2].x += GridCellSize;
                mCellVerts[2].y -= GridCellSize;

                mCellVerts[3] = origin;
                mCellVerts[3].y -= GridCellSize;

                Handles.DrawSolidRectangleWithOutline(mCellVerts
                    , Color.clear
                    , new Color(0.93f, 0.58f, 0.11f));  // Orange.

                TileZone zone = selection.Zone;

                origin = 
                    new Vector3(zone.xmin * GridCellSize, areaHeight - zone.zmin * GridCellSize);

                xSize = zone.Width * GridCellSize;
                ySize = zone.Depth * GridCellSize;
            }
            else
            {
                origin = new Vector3(0, areaHeight);

                TileBuildData tdata = Context.Build.BuildData;

                xSize = tdata.Width * GridCellSize;
                ySize = tdata.Depth * GridCellSize;
            }

            mCellVerts[0] = origin;

            mCellVerts[1] = origin;
            mCellVerts[1].x += xSize;

            mCellVerts[2] = origin;
            mCellVerts[2].x += xSize;
            mCellVerts[2].y -= ySize;

            mCellVerts[3] = origin;
            mCellVerts[3].y -= ySize;

            Handles.DrawSolidRectangleWithOutline(mCellVerts
                , Color.clear
                , ControlUtil.SelectionColor);
        }

        protected override void OnGUIButtons()
        {
            DebugContext.SetViews(ViewOption.Grid | ViewOption.Selection | ViewOption.Mesh);

            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            TileBuildData tdata = build.BuildData;

            if (tdata == null)
                return;

            TileSelection selection = Context.Selection;

            bool hasSelection = selection.Validate();
            bool needBaking = (tdata.NeedsBakingCount() > 0);
            int activeCount = Context.TaskCount;
            int bakeableCount = tdata.BakeableCount();

            bool origGUIEnabled = GUI.enabled;

            bool guiEnabled = !IsBaseBusy;

            GUI.enabled = guiEnabled;

            ControlUtil.BeginButtonArea(Context.ButtonArea);

            if (GUILayout.Button("Build All"))
                HandleBuildRequest(true);

            GUI.enabled = guiEnabled && hasSelection;

            if (GUILayout.Button("Build Zone"))
                HandleBuildRequest(false);

            ////////////////////////////////////////////////////////////////////
            GUILayout.Space(MarginSize);

            // Only disable baking if there is nothing at all that can be baked.
            GUI.enabled = guiEnabled && activeCount == 0 && (bakeableCount > 0);

            GUIStyle style = (bakeableCount > 0 && activeCount == 0)
                ? ControlUtil.HighlightedButton : GUI.skin.button;

            if (GUILayout.Button("Bake All", style))
                HandleBake();

            ////////////////////////////////////////////////////////////////////
            GUILayout.Space(MarginSize);

            // Note: Technically only the last condition is needed.  But checking the
            // other conditions first saves processing time.
            GUI.enabled = guiEnabled && activeCount == 0
                && tdata.GetStateCount(TileBuildState.NotBuilt) < tdata.Width * tdata.Depth;

            if (GUILayout.Button((needBaking ? "Revert Unbaked" : "Clear All")))
                HandleClear();

            GUI.enabled = guiEnabled && (activeCount != 0);

            if (GUILayout.Button("Abort Builds"))
                Context.AbortAllReqests("User requested.");

            ////////////////////////////////////////////////////////////////////
            GUILayout.Space(ControlUtil.MarginSize);

            GUI.enabled = guiEnabled;
            if (OnGUIStandardButtons())
            {
                // Special case.  Build was discarded.
                ControlUtil.EndButtonArea();
                return;
            }
            
            ///////////////////////////////////////////////////////////////////
            GUILayout.Space(MarginSize);

            GUI.enabled = guiEnabled && hasSelection;

            EditorGUIUtility.LookLikeControls(100);
            selection.ZoneSize = EditorGUILayout.IntField("Zone Size", selection.ZoneSize);
            EditorGUIUtility.LookLikeControls();

            GUI.enabled = guiEnabled;

            ////////////////////////////////////////////////////////////////////
            GUILayout.Space(MarginSize);

            GUILayout.Label("Bakeable Tiles: " + bakeableCount);

            ControlUtil.OnGUIStandardButtons(Context, DebugContext, true);

            ControlUtil.EndButtonArea();

            GUI.enabled = origGUIEnabled;
        }

        private void HandleBake()
        {
            const string Category = "Bake To Target";

            NavmeshBuild build = Context.Build;  // Caller checks for null.
            TileBuildData tdata = build.BuildData;

            // Double check.
            if (tdata.BakeableCount() == 0)
            {
                Debug.LogWarning(Category + ": No tiles were produced.  (All tiles empty?)", build);
                return;
            }

            if (Context.TaskCount > 0)
            {
                Debug.LogWarning(Category + ": There are in-progress background builds."
                        + " The tiles associated with these builds will not be baked."
                        + " In-progress builds: " + Context.TaskCount
                    , build);
            }

            NavmeshParams nconfig;
            NavmeshTileData[] tiles;

            bool success = tdata.GetMeshBuildData(build.TileSetDefinition.BoundsMin
                , build.TileSetDefinition.TileWorldSize
                , out nconfig, out tiles);

            if (!success)
            {
                Logger.PostError("Bake to target: Error creating navigation mesh from build data."
                    , Context.Build);
                return;
            }

            NavStatus status = 
                build.BuildTarget.Load(nconfig, tiles, NMBEditorUtil.GetConfig(build));

            if ((status & NavStatus.Failure) == 0)
            {
                build.BuildData.SetAsBaked();
                EditorUtility.SetDirty((Object)build.BuildTarget);
            }
            else
                Logger.PostError("Bake to target: Target reported failure."
                    , (Object)Context.Build.BuildTarget);
        }

        private void HandleClear()
        {
            // Abort any in-progress builds.
            Context.AbortAllReqests("User requested.");

            TileBuildData tdata = Context.Build.BuildData;

            int w = tdata.Width;
            int d = tdata.Depth;

            bool needsBaking =
                (tdata.NeedsBakingCount() == 0) ? false : true;

            for (int tx = 0; tx < w; tx++)
            {
                for (int tz = 0; tz < d; tz++)
                {
                    if (needsBaking)
                        tdata.ClearUnbaked(tx, tz);
                    else
                        tdata.Reset(tx, tz);
                }
            }
        }

        private void HandleBuildRequest(bool forceAll)
        {
            TileSelection sel = Context.Selection;

            int w;
            int d;
            int ix = 0;
            int iz = 0;

            int priority;

            if (!forceAll && sel.Validate())
            {
                TileZone zone = sel.Zone;

                w = zone.xmax + 1;
                d = zone.zmax + 1;

                ix = zone.xmin;
                iz = zone.zmin;

                priority = BuildTaskProcessor.MediumPriority;
            }
            else
            {
                TileBuildData tdata = Context.Build.BuildData;

                w = tdata.Width;
                d = tdata.Depth;

                priority = BuildTaskProcessor.LowPriority;
            }

            // Note: The iteration order appears odd, but it makes for better 
            // progress visualizations.  Filling downward.
            for (int tz = d - 1; tz >= iz; tz--)
            {
                for (int tx = ix; tx < w; tx++)
                {
                    if (!Context.QueueTask(tx, tz, priority--, Logger))
                    {
                        Logger.PostError(string.Format("Build task failed: ({0},{1})", tx, tz)
                            , Context.Build);
                    }
                }
            }
        }

        private static Color ToColor(TileBuildState state)
        {
            switch (state)
            {
                case TileBuildState.Baked:

                    return new Color(0.07f, 0.43f, 0.09f);  // Green

                case TileBuildState.Built:

                    return new Color(0.08f, 0.34f, 0.63f); // Dark blue

                case TileBuildState.Empty:

                    return new Color(0.3f, 0.3f, 0.3f);  // Dark grey
                    
                case TileBuildState.Error:

                    return new Color(0.63f, 0.03f, 0);  // Red

                case TileBuildState.InProgress:

                    return new Color(0.12f, 0.5f, 0.92f); // Medium blue

                case TileBuildState.NotBuilt:

                    return new Color(0.6f, 0.6f, 0.6f);  // Light grey

                case TileBuildState.Queued:

                    return new Color(0.4f, 0.65f, 0.93f); // Light blue
            }

            return Color.white;  // Should never get here.
        }


    }
}
