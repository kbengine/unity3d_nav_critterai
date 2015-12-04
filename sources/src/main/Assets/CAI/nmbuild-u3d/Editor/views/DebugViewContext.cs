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
using org.critterai.nav.u3d.editor;
using org.critterai.nav.u3d;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class DebugViewContext
    {
        private static GUIContent mShowInputLabel = new GUIContent(
            "Input Geometry"
            , "Framerate Killer!");

        private readonly InputDebugView mInputView = new InputDebugView();
        private readonly TileGridDebugView mGridView = new TileGridDebugView();
        private readonly SelectionDebugView mSelectionView;
        private readonly MeshDebugView mMeshView = new MeshDebugView();

        private readonly NavmeshBuild mBuild;
        private readonly TileSelection mSelection;

        private bool mNeedsRepaint;

        public DebugViewContext(NavmeshBuild build, TileSelection selection) 
        {
            if (!build || selection == null)
                throw new System.ArgumentNullException();

            mSelectionView = new SelectionDebugView();
            mSelectionView.Enabled = true;
            mSelectionView.Show = true;
            mSelectionView.IncludeRootTile = true;

            mBuild = build;
            mSelection = selection;
        }

        public bool NeedsRepaint
        {
            get
            {
                return mNeedsRepaint
                    || mSelectionView.NeedsRepaint
                    || mMeshView.NeedsRepaint
                    || mInputView.NeedsRepaint
                    || mGridView.NeedsRepaint;
            }
            set
            {
                mNeedsRepaint = value;
                if (!mNeedsRepaint)
                {
                    mSelectionView.NeedsRepaint = false;
                    mMeshView.NeedsRepaint = false;
                    mInputView.NeedsRepaint = false;
                    mGridView.NeedsRepaint = false;
                }
            }
        }

        public void DisableAllViews()
        {
            mInputView.Enabled = false;
            mGridView.Enabled = false;
            mSelectionView.Enabled = false;
            mMeshView.Enabled = false;
        }

        public void SetViews(ViewOption options)
        {
            // Debug.Log("Set Debug Views: " + options);
            mInputView.Enabled = (options & ViewOption.Input) != 0;
            mGridView.Enabled = (options & ViewOption.Grid) != 0;
            mSelectionView.Enabled = (options & ViewOption.Selection) != 0;
            mMeshView.Enabled = (options & ViewOption.Mesh) != 0;
        }

        public void OnSceneGUI()
        {
            if (!mBuild)
                return;

            mInputView.OnRenderObject(mBuild);
            mGridView.OnRenderObject(mBuild);
            mSelectionView.OnRenderObject(mBuild, mSelection);
            mMeshView.OnRenderObject(mBuild, mSelection);
        }

        public void OnGUIDebugExtras()
        {
            // Design note: Not allowing the selection display to be disabled.
            // It is too important.

            bool guiEnabled = GUI.enabled;

            if (mInputView.Enabled)
                mInputView.Show = GUILayout.Toggle(mInputView.Show, "Input Bounds");

            if (mGridView.Enabled)
            {
                mGridView.Show = GUILayout.Toggle(mGridView.Show, "Tile Grid");

                GUILayout.Space(ControlUtil.MarginSize);

                GUI.enabled = guiEnabled && mGridView.Show;

                mGridView.YOffset =
                    GUILayout.HorizontalSlider(mGridView.YOffset, 0, 1);

                GUI.enabled = guiEnabled;
            }

            if (mSelectionView.Enabled)
            {
                mSelectionView.Show = GUILayout.Toggle(mSelectionView.Show, "Selection Bounds");
            }
        }

        public void OnGUIMeshDisplayOptions()
        {
            if (!mBuild || !mMeshView.Enabled)
                return;

            bool guiEnabled = GUI.enabled;

            MeshDebugView meshView = mMeshView;

            INavmeshData bnm = mBuild.BuildTarget;
            NavmeshSceneDraw sceneDraw = NavmeshSceneDraw.Instance;

            bool showBaked = false;

            if (bnm != null)
            {
                showBaked = sceneDraw.OnGUI(bnm, "Baked Navmesh", false, false);
                if (showBaked)
                    meshView.Show = MeshDebugOption.None;
            }

            if (GUILayout.Toggle(meshView.Show == MeshDebugOption.WorkingMesh
                , "Working Navmesh"))
            {
                meshView.Show = MeshDebugOption.WorkingMesh;
                showBaked = false;
            }
            else if (meshView.Show == MeshDebugOption.WorkingMesh)
                meshView.Show = MeshDebugOption.None;

            GUI.enabled = guiEnabled
                && meshView.Show == MeshDebugOption.WorkingMesh || showBaked;

            bool orig = sceneDraw.ColorByArea;
            sceneDraw.ColorByArea = GUILayout.Toggle(sceneDraw.ColorByArea, "Color by Area");
            if (sceneDraw.ColorByArea != orig)
                mNeedsRepaint = true;

            GUILayout.Space(ControlUtil.MarginSize);

            GUI.enabled = guiEnabled 
                && (mSelection.HasSelection || !mBuild.BuildData.IsTiled);

            if (GUILayout.Toggle(meshView.Show == MeshDebugOption.PolyMesh
                , "PolyMesh"))
            {
                meshView.Show = MeshDebugOption.PolyMesh;
                showBaked = false;
            }
            else if (meshView.Show == MeshDebugOption.PolyMesh)
                meshView.Show = MeshDebugOption.None;

            if (GUILayout.Toggle(meshView.Show == MeshDebugOption.Detailmesh
                , "Detail Mesh"))
            {
                meshView.Show = MeshDebugOption.Detailmesh;
                showBaked = false;
            }
            else if (meshView.Show == MeshDebugOption.Detailmesh)
                meshView.Show = MeshDebugOption.None;

            if (mBuild.TileSetDefinition != null)
            {
                if (GUILayout.Toggle(meshView.Show == MeshDebugOption.InputGeometry
                    , mShowInputLabel))
                {
                    meshView.Show = MeshDebugOption.InputGeometry;
                    showBaked = false;
                }
                else if (meshView.Show == MeshDebugOption.InputGeometry)
                    meshView.Show = MeshDebugOption.None;
            }

            GUI.enabled = guiEnabled;

            if (showBaked)
                meshView.Show = MeshDebugOption.None;
            else
                sceneDraw.Hide();
        }
    }
}
