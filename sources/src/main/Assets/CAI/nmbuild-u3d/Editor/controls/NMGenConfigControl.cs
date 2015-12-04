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
using org.critterai.nmgen;
using org.critterai.u3d.editor;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class NMGenConfigControl
        : BuildControl
    {
        private const float MarginSize = ControlUtil.MarginSize;

        private const string DefaultStatusMessage = 
            "Values in parentheses represent the effective"
                + " world units based on cell size.";

        public override void Exit()
        {
            base.Exit();
            Logger.ResetLog();
        }

        protected override void OnGUIMain()
        {
            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            NMGenConfig config = build.Config;

            GUI.Box(Context.MainArea, "");

            Rect colA = Context.MainArea;
            colA.width = colA.width * 0.5f - 4 * MarginSize;

            Rect colB = colA;

            colA.x += MarginSize;

            colB.x += Context.MainArea.width * 0.5f + MarginSize;

            GUILayout.BeginArea(colA);

            OnGUIPrimary(build, config, false);

            if (GUI.changed)
                // Conservative: Just in case the tile size was altered.
                DebugContext.NeedsRepaint = true;

            GUILayout.EndArea();

            GUILayout.BeginArea(colB);
            OnGUIAdvanced(config, false);
            GUILayout.EndArea();

            if (GUI.changed)
                build.IsDirty = true;
        }

        protected override void OnGUIButtons()
        {
            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            NMGenConfig config = build.Config;

            // Buttons area.

            ControlUtil.BeginButtonArea(Context.ButtonArea);

            // Standard config buttons.

            OnGUIButtons(build, config, false);

            // Build initialialization buttons.

            bool guiEnabled = GUI.enabled;

            bool targetOK = build.CanLoadFromTarget(null, false);

            GUILayout.Space(MarginSize * 2);

            if (build.BuildState == NavmeshBuildState.Buildable)
            {
                GUI.enabled = (Context.TaskCount == 0);

                if (GUILayout.Button("Reinitialize Builder"))
                    build.DiscardBuildData();
            }
            else if (build.HasInputData)
            {
                if (targetOK)
                {
                    if (GUILayout.Button("Load Target's Config"))
                    {
                        if (!build.SetConfigFromTarget(Logger))
                            Logger.PostError("Could not load target config.", Context.Build);
                        Logger.ResetLog();
                    }

                    GUILayout.Space(MarginSize);
                    GUILayout.Label("Initialize Build:");
                    GUILayout.Space(MarginSize);

                    if (GUILayout.Button("From Scratch", ControlUtil.HighlightedButton))
                        InitializeBuild(false);

                    if (GUILayout.Button("Based on Target"))
                        InitializeBuild(true);

                    GUILayout.Space(MarginSize);
                    GUILayout.Box("Basing your build on the target's navigation"
                            + " mesh will automatically lock you in to the"
                            + " target's configuration settings."
                        , EditorUtil.HelpStyle
                        , GUILayout.ExpandWidth(true));
                }
                else
                {
                    if (GUILayout.Button("Ready to Build", ControlUtil.HighlightedButton))
                        InitializeBuild(false);
                }
            }

            GUI.enabled = guiEnabled;

            ViewOption option = build.HasInputData
                ? (ViewOption.Grid | ViewOption.Input) : 0;

            DebugContext.SetViews(option);

            ControlUtil.OnGUIStandardButtons(Context, DebugContext, true);

            ControlUtil.EndButtonArea();

            if (GUI.changed)
                build.IsDirty = true;
        }

        private void InitializeBuild(bool fromTarget)
        {
            if (!Context.Build.InitializeBuild(Logger, fromTarget))
                Logger.PostError("Could not initialize the build.", Context.Build);
        }

        public static void OnGUIPrimary(NavmeshBuild build
            , NMGenConfig config
            , bool includeSlope)
        {
            if (!build)
                return;

            bool guiEnabled = GUI.enabled;

            EditorGUIUtility.LookLikeControls(155);

            float xz = config.XZCellSize;
            float y = config.YCellSize;
            float a = xz * xz;
            float effective;

            //////////////////////////////////////////////////////////////

            GUILayout.Label("Agent Settings");
            GUILayout.Space(MarginSize);

            TileBuildData tdata = build.BuildData;

            GUI.enabled = guiEnabled && (tdata == null);

            effective = (float)Mathf.Ceil(config.WalkableHeight / y) * y;

            config.WalkableHeight = EditorGUILayout.FloatField(
                NMGenConfig.HeightLabel + Effective(effective)
                , config.WalkableHeight);


            effective = (float)Mathf.Floor(config.WalkableStep / y) * y;
            config.WalkableStep = EditorGUILayout.FloatField(
                NMGenConfig.StepLabel + Effective(effective)
                , config.WalkableStep);

            effective = (float)Mathf.Ceil(config.WalkableRadius / xz) * xz;
            config.WalkableRadius = EditorGUILayout.FloatField(
                NMGenConfig.RadiusLabel + Effective(effective)
                , config.WalkableRadius);

            GUI.enabled = guiEnabled;

            if (includeSlope)
            {
                config.WalkableSlope = EditorGUILayout.FloatField(
                    NMGenConfig.SlopeLabel
                    , config.WalkableSlope);
            }

            /////////////////////////////////////////////////////////////////

            GUILayout.Space(2 * MarginSize);
            GUILayout.Label("Resolution and Tile Settings");
            GUILayout.Space(MarginSize);

            GUI.enabled = guiEnabled && (tdata == null);

            config.XZCellSize = EditorGUILayout.FloatField(
                NMGenConfig.XZSizeLabel
                , config.XZCellSize);

            config.YCellSize = EditorGUILayout.FloatField(
                NMGenConfig.YSizeLabel
                , config.YCellSize);

            config.TileSize = EditorGUILayout.IntField(
                NMGenConfig.TileSizeLabel
                    + " (" + config.TileSize * config.XZCellSize + ")"
                , config.TileSize);


            config.BorderSize = EditorGUILayout.IntField(
                NMGenConfig.HFBorderLabel
                , config.BorderSize);

            GUI.enabled = guiEnabled;

            int derBorderSize = NMGenConfig.DeriveBorderSize(config);
            float derXZ = NMGenConfig.DeriveXZCellSize(config);
            float derY = NMGenConfig.DeriveYCellSize(config);

            if ((config.TileSize == 0 && config.BorderSize != derBorderSize)
                || config.BorderSize < derBorderSize
                || config.XZCellSize > derXZ
                || config.YCellSize > derY)
            {
                GUILayout.Space(MarginSize);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine("Recommendations:");

                if (config.XZCellSize > derXZ)
                    sb.AppendLine(NMGenConfig.XZSizeLabel + " of " + derXZ + " or less.");

                if (config.YCellSize > derY)
                    sb.AppendLine(NMGenConfig.YSizeLabel + " of " + derY + " or less.");

                if (config.TileSize == 0 && config.BorderSize != derBorderSize)
                    sb.AppendLine("Border Size of " + derBorderSize + ".");
                else if (config.BorderSize < derBorderSize)
                    sb.AppendLine("Border Size of " + derBorderSize + " or higher.");

                GUILayout.Box(sb.ToString().Trim(), EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));
            }

            if (build.HasInputData)
            {
                InputGeometry geom = build.InputGeom;

                Vector3 bmin = geom.BoundsMin;
                Vector3 bmax = geom.BoundsMax;

                float w = bmax.x - bmin.x;
                float d = bmax.z - bmin.z;

                GUILayout.Space(MarginSize);

                int tw = Mathf.CeilToInt(w / xz);
                int td = Mathf.CeilToInt(d / xz);
                GUILayout.Label(string.Format("Cells: {0:N0} ({1:N0} x {2:N0})"
                    , tw * td, tw, td));

                if (config.TileSize > 0)
                {
                    tw = Mathf.Max(1, Mathf.CeilToInt((float)tw / config.TileSize));
                    td = Mathf.Max(1, Mathf.CeilToInt((float)td / config.TileSize));
                }
                else
                {
                    tw = 1;
                    td = 1;
                }
                GUILayout.Label(string.Format("Tiles: {0:N0} ({1:N0} x {2:N0})"
                    , tw * td, tw, td));
            }

            /////////////////////////////////////////////////////////////////

            GUILayout.Space(2 * MarginSize);
            GUILayout.Label("Miscellaneous Settings");
            GUILayout.Space(MarginSize);

            config.DetailSampleDistance = EditorGUILayout.FloatField(
                NMGenConfig.DetailSampleLabel
                , config.DetailSampleDistance);

            config.DetailMaxDeviation = EditorGUILayout.FloatField(
                NMGenConfig.DetailDevLabel
                , config.DetailMaxDeviation);

            effective = Mathf.Ceil(config.MinRegionArea / a) * a;
            config.MinRegionArea = EditorGUILayout.FloatField(
                NMGenConfig.IslandRegionLabel + Effective(effective)
                , config.MinRegionArea);
        }

        public static void OnGUIAdvanced(NMGenConfig config
            , bool isInspector)
        {
            GUILayout.Label("Advanced Settings");

            EditorGUIUtility.LookLikeControls(170);

            float xz = config.XZCellSize;

            float a = xz * xz;
            float effective;

            /////////////////////////////////////////////////////////////

            GUILayout.Space(MarginSize);

            effective = Mathf.Ceil(config.MaxEdgeLength / xz) * xz;

            config.MaxEdgeLength = EditorGUILayout.FloatField(
                NMGenConfig.EdgeLenLabel + Effective(effective)
                , config.MaxEdgeLength);

            config.EdgeMaxDeviation = EditorGUILayout.FloatField(
                NMGenConfig.EdgeDevLabel
                , config.EdgeMaxDeviation);

            config.MaxVertsPerPoly = EditorGUILayout.IntSlider(
                NMGenConfig.MaxPolyVertLabel
                , config.MaxVertsPerPoly
                , 3
                , NMGen.MaxAllowedVertsPerPoly);

            effective = Mathf.Ceil(config.MergeRegionArea / a) * a;

            config.MergeRegionArea = EditorGUILayout.FloatField(
                NMGenConfig.MergeSizeLabel + Effective(effective)
                , config.MergeRegionArea);

            GUILayout.Space(MarginSize * 2);

            NMGenBuildFlag flags = config.BuildFlags;

            HandleFlagGUI(ref flags
                , NMGenConfig.LedgeSpansLabel
                , NMGenBuildFlag.LedgeSpansNotWalkable
                , isInspector);

            HandleFlagGUI(ref flags
                , NMGenConfig.LowHeightLabel
                , NMGenBuildFlag.LowHeightSpansNotWalkable
                , isInspector);

            HandleFlagGUI(ref flags
                , NMGenConfig.LowObstacleLabel
                , NMGenBuildFlag.LowObstaclesWalkable
                , isInspector);

            ContourBuildFlags cflags = config.ContourOptions;

            HandleFlagGUI(ref cflags
                , NMGenConfig.TessWallsLabel
                , ContourBuildFlags.TessellateWallEdges
                , isInspector);

            HandleFlagGUI(ref cflags
                , NMGenConfig.TessAreasLabel
                , ContourBuildFlags.TessellateAreaEdges
                , isInspector);

            config.ContourOptions = cflags;

            if (isInspector)
            {
                config.UseMonotone = EditorGUILayout.Toggle(NMGenConfig.UseMonoLabel
                    , config.UseMonotone);
            }
            else
            {
                config.UseMonotone = GUILayout.Toggle(config.UseMonotone
                    , NMGenConfig.UseMonoLabel);
            }

            HandleFlagGUI(ref flags
                , NMGenConfig.FlagPolysLabel
                , NMGenBuildFlag.ApplyPolyFlags
                , isInspector);

            bool includeDetail;
            if (isInspector)
            {
                includeDetail = EditorGUILayout.Toggle("Include Detail Mesh"
                    , (config.ResultOptions & NMGenAssetFlag.DetailMesh) != 0);
            }
            else
            {
                includeDetail = GUILayout.Toggle(
                    (config.ResultOptions & NMGenAssetFlag.DetailMesh) != 0
                    , "Include Detail Mesh");
            }

            if (includeDetail)
                config.ResultOptions |= NMGenAssetFlag.DetailMesh;
            else
                config.ResultOptions &= ~NMGenAssetFlag.DetailMesh;

            HandleFlagGUI(ref flags
                , NMGenConfig.BVTreeLabel
                , NMGenBuildFlag.BVTreeEnabled
                , isInspector);

            config.BuildFlags = flags;
        }

        public static void OnGUIButtons(NavmeshBuild build
            , NMGenConfig config
            , bool isInspector)
        {
            if (!build)
                return;

            if (build.HasBuildData)
                // Not an option if the build is in progress.
                return;

            if (isInspector)
                EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clean"))
            {
                config.Clean();
                config.ApplyDecimalLimits();
                GUI.changed = true;
            }

            if (GUILayout.Button("Reset"))
            {
                config.Reset();
                GUI.changed = true;
            }

            if (!isInspector)
                GUILayout.Space(2 * MarginSize);

            if (build.HasInputData)
            {
                if (GUILayout.Button("Derive"))
                {
                    InputGeometry geom = build.InputGeom;

                    config.Derive(geom.BoundsMin, geom.BoundsMax);
                    config.ApplyDecimalLimits();

                    GUI.changed = true;
                }
            }

            if (isInspector)
                EditorGUILayout.EndHorizontal();

        }

        private static string Effective(float value)
        {
            return "  (" + System.Math.Round(value, 2) + ")";
        }

        private static void HandleFlagGUI(ref ContourBuildFlags flags
            , string label
            , ContourBuildFlags flag
            , bool isInspector)
        {
            if (isInspector)
            {
                flags = EditorGUILayout.Toggle(label, (flags & flag) != 0)
                    ? (flags | flag)
                    : (flags & ~flag);
            }
            else
            {
                flags = GUILayout.Toggle((flags & flag) != 0, label)
                    ? (flags | flag)
                    : (flags & ~flag);
            }
        }

        private static void HandleFlagGUI(ref NMGenBuildFlag flags
            , string label
            , NMGenBuildFlag flag
            , bool isInspector)
        {
            if (isInspector)
            {
                flags = EditorGUILayout.Toggle(label, (flags & flag) != 0)
                    ? (flags | flag) 
                    : (flags & ~flag);
            }
            else
            {
                flags = GUILayout.Toggle((flags & flag) != 0, label)
                    ? (flags | flag) 
                    : (flags & ~flag);
            }
        }
    }
}
