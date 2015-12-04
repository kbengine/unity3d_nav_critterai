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
using org.critterai.nav.u3d;

namespace org.critterai.nav.u3d.editor
{
    /// <summary>
    /// Controls the debug visualization of <see cref="INavmeshData"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implemented as a singleton to allow multiple components to manage the display.
    /// </para>
    /// <para>
    /// Also provides shared GUI features for the Editor.
    /// </para>
    /// </remarks>
    public sealed class NavmeshSceneDraw
    {
        private static NavmeshSceneDraw mInstance;

        private INavmeshData mNavmeshData;
        private Navmesh mNavmesh;
        private int mDataVersion;
        private SceneView.OnSceneFunc mDelegate;

        private bool mColorByArea = false;

        private NavmeshSceneDraw() { }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static NavmeshSceneDraw Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new NavmeshSceneDraw();
                return mInstance;
            }
        }

        /// <summary>
        /// True if color should be assigned by area, false to color by tile.
        /// </summary>
        public bool ColorByArea
        {
            get { return mColorByArea; }
            set { mColorByArea = value; }
        }

        /// <summary>
        /// True if the object is currently assigned for visualization.
        /// </summary>
        /// <param name="data">The object to visualize.</param>
        /// <returns>True if the object is currently assigned for visualization.</returns>
        public bool IsShown(INavmeshData data)
        {
            return ((ScriptableObject)data && data == mNavmeshData);
        }

        /// <summary>
        /// True if the visualization is active.
        /// </summary>
        /// <returns>True if an object is currently assigned for visualization.</returns>
        public bool IsShown()
        {
            return (ScriptableObject)mNavmeshData;
        }

        /// <summary>
        /// Show the visualization for the object.
        /// </summary>
        /// <param name="data">The object to visualize.</param>
        public void Show(INavmeshData data)
        {
            Hide();

            if (!(ScriptableObject)data)
                return;

            mNavmeshData = data;

            mDelegate = new SceneView.OnSceneFunc(OnSceneGUI);

            SceneView.onSceneGUIDelegate += mDelegate;
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Disable the visualization.
        /// </summary>
        public void Hide()
        {
            if (mDelegate != null)
            {
                SceneView.onSceneGUIDelegate -= mDelegate;
                SceneView.RepaintAll();
            }

            mNavmeshData = null;
            mDataVersion = -1;
            mNavmesh = null;
            mDelegate = null;
        }

        /// <summary>
        /// Provides a standard Editor GUI for managing scene drawing.
        /// </summary>
        /// <param name="target">The object being managed by the GUI.</param>
        /// <param name="label">The label of the 'show' toggle.</param>
        /// <param name="isInspector">True if the inspector format should be used.</param>
        /// <param name="includeAreaOption">
        /// True if the option to color by area should be displayed.
        /// </param>
        /// <returns>True the display of the target has been toggled on.</returns>
        public bool OnGUI(INavmeshData target
            , string label
            , bool isInspector
            , bool includeAreaOption)
        {
            if (!(ScriptableObject)target)
                return false;

            bool guiEnabled = GUI.enabled;

            bool origChanged = GUI.changed;
            GUI.changed = false;

            bool orig = IsShown(target);
            bool curr;

            if (isInspector)
            {
                curr = EditorGUILayout.Toggle(label, orig);
                GUI.enabled = guiEnabled && curr;
                if (includeAreaOption)
                    mColorByArea = EditorGUILayout.Toggle("Color by area", mColorByArea);
            }
            else
            {
                curr = GUILayout.Toggle(orig, label);
                GUI.enabled = guiEnabled && curr;
                if (includeAreaOption)
                    mColorByArea = GUILayout.Toggle(mColorByArea, "Color by area");
            }

            GUI.enabled = guiEnabled;

            if (orig != curr)
            {
                if (curr)
                    Show(target);
                else
                    Hide();
            }

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }

            GUI.changed = origChanged;

            return curr;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!(ScriptableObject)mNavmeshData)
            {
                Hide();
                return;
            }

            if (!mNavmeshData.HasNavmesh)
            {
                mDataVersion = -1;
                mNavmesh = null;
                return;
            }

            if (mNavmesh == null || mNavmeshData.Version != mDataVersion)
            {
                mNavmesh = mNavmeshData.GetNavmesh();
                mDataVersion = mNavmeshData.Version;

                if (mNavmesh == null)
                    return;
            }

            NavDebug.Draw(mNavmesh, mColorByArea);
        }
    }
}