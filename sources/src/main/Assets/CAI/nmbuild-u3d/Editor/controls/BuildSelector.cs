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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace org.critterai.nmbuild.u3d.editor
{
    internal class BuildSelector
    {
        private class UnityComparer
            : IComparer<NavmeshBuild>
        {
            public int Compare(NavmeshBuild x, NavmeshBuild y)
            {
                return string.Compare(x.name, y.name, true);
            }
        }

        private static BuildSelector mInstance;

        private readonly UnityComparer mComparer = new UnityComparer();

        public event BuildDelegate OnSelect;

        private List<NavmeshBuild> mBuilds = new List<NavmeshBuild>();
        private string[] mNames;

        private NavmeshBuild mSelected;
        // This next field is needed to detect when the selection has been lost due to
        // object deletion.
        private bool mHasSelection = false;

        public int Count
        {
            get
            {
                Validate();
                return mBuilds.Count;
            }
        }

        public NavmeshBuild Selected
        {
            get
            {
                Validate();
                return mSelected;
            }
        }

        public void Select(NavmeshBuild build)
        {
            Validate();

            // Note: Can't set to null.
            if (!build || build == mSelected || !mBuilds.Contains(build))
                return;

            mSelected = build;

            HandleSelectionChange();
        }

        public void Remove(NavmeshBuild build)
        {
            if (!build || !mBuilds.Contains(build))
                return;

            Validate();
            mBuilds.Remove(build);
            RefreshNames();

            if (mSelected == build)
            {
                mSelected = (mBuilds.Count > 0 ? mBuilds[0] : null);
                HandleSelectionChange();
            }
        }

        public void Add(NavmeshBuild build)
        {
            if (!build || mBuilds.Contains(build))
                return;

            mBuilds.Add(build);
            mBuilds.Sort(mComparer);
            RefreshNames();

            Validate();
        }

        public void OnGUI(Rect area)
        {
            Validate();

            if (!mSelected)
                return;

            int iOrig = mBuilds.IndexOf(mSelected);
            int iSelected;

            if (mBuilds.Count < 5)
                iSelected = GUI.Toolbar(area, iOrig, mNames);
            else
            {
                GUILayout.BeginArea(area);
                GUILayout.BeginHorizontal();
                iSelected = EditorGUILayout.Popup(iOrig, mNames);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (iSelected != iOrig)
            {
                mSelected = mBuilds[iSelected];
                HandleSelectionChange();
            }
        }

        private void Validate()
        {
            bool changed = false;

            // Check for and remove nulls.  Check for name change.
            for (int i = mBuilds.Count - 1; i >= 0; i--)
            {
                if (!mBuilds[i])
                {
                    mBuilds.RemoveAt(i);
                    changed = true;
                }
                else if (mBuilds[i].name != mNames[i])
                {
                    mBuilds.Sort(mComparer);
                    changed = true;
                }
            }

            if (changed)
                RefreshNames();

            // Has the selection changed?
            if (!mSelected)
            {
                if (mBuilds.Count > 0)
                    mSelected = mBuilds[0];

                if (mSelected || mHasSelection)
                    HandleSelectionChange();
            }
        }

        private void HandleSelectionChange()
        {
            mHasSelection = (mSelected ? true : false);

            if (OnSelect != null)
                OnSelect(mSelected);
        }

        private void RefreshNames()
        {
            mNames = new string[mBuilds.Count];

            for (int i = 0; i < mBuilds.Count; i++)
            {
                mNames[i] = mBuilds[i].name;
            }
        }

        public static BuildSelector Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new BuildSelector();
                return mInstance;
            }
        }
    }
}
