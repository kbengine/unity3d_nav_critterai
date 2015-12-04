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
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace org.critterai.nmbuild.u3d.editor
{
	internal class BuildProcessor
	{
        private const string MaxConcurrKey = "org.critterai.nmbuild.MaxConcurrency";

        public static int DefaultConcurrency
        {
            get 
            { 
                // Bugs in Unity Editor 3.5 threading causing problems. 
                return 1;
                // return Mathf.Max(1, System.Environment.ProcessorCount - 2); 
            }
        }

        public static int MaxConcurrency
        {
            get
            {
                int val = EditorPrefs.GetInt(MaxConcurrKey);
                return (val == 0 ? DefaultConcurrency : val);
            }
            set
            {
                EditorPrefs.SetInt(MaxConcurrKey
                    , Mathf.Clamp(value, 1, System.Environment.ProcessorCount));
            }
        }

        private readonly List<BuildController> mControllers;

        private BuildTaskProcessor mTaskManager;

        public BuildTaskProcessor TaskManager { get { return mTaskManager; } }

        public int BuildCount { get { return mControllers.Count; } }

        public BuildProcessor()
        {
            mControllers = new List<BuildController>(3);

            mTaskManager = new BuildTaskProcessor(MaxConcurrency);
            Thread t = new Thread(new ThreadStart(mTaskManager.Run));
            t.Start();

            BuildSelector b = BuildSelector.Instance;

            b.OnSelect += HandleOnSelect;

            NavmeshBuild selected = b.Selected;
            if (selected)
                HandleOnSelect(selected);
        }

        private void HandleOnSelect(NavmeshBuild build)
        {
            if (!build || Contains(build))
                return;

            BuildController controller = new BuildController(build, mTaskManager);

            if (controller.Enter())
                mControllers.Add(controller);
            else
                Debug.LogError("Failed to add controller for build: " + build);
        }

        private bool Contains(NavmeshBuild build)
        {
            if (!build)
                return false;

            foreach (BuildController controller in mControllers)
            {
                if (controller.Build == build)
                    return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (mTaskManager == null)
                return;

            foreach (BuildController controller in mControllers)
            {
                controller.Exit();
            }
            mControllers.Clear();

            mTaskManager.Abort();
            mTaskManager = null;

            BuildSelector.Instance.OnSelect -= HandleOnSelect;
        }

        public void Update()
        {
            if (mTaskManager == null)
            {
                // Give an error in this case.
                Debug.LogError("Called update on build processor after processor disposal");
                return;
            }

            for (int i = mControllers.Count - 1; i >= 0; i--)
            {
                BuildController controller = mControllers[i];
                NavmeshBuild build = controller.Build;
                NavmeshBuild selected = BuildSelector.Instance.Selected;

                if (!build 
                    || build.BuildType != NavmeshBuildType.Advanced
                    || (build != selected && !controller.BuildIsActive))
                {
                    // Build component has been destroyed, is no longer advanced, or is
                    // inactive and not selected.  Get rid of it.
                    controller.Exit();
                    mControllers.RemoveAt(i);
                }
                else
                    controller.Update();
            }
        }

        public void OnGUI(Rect area, bool includeMain)
        {
            NavmeshBuild build = BuildSelector.Instance.Selected;

            if (!build)
                return;

            foreach (BuildController controller in mControllers)
            {
                if (controller.Build == build)
                {
                    controller.OnGUI(area, includeMain);
                    return;
                }
            }
        }

        public void OnSceneGUI()
        {
            NavmeshBuild build = BuildSelector.Instance.Selected;

            if (!build)
                return;

            foreach (BuildController controller in mControllers)
            {
                if (controller.Build == build)
                {
                    controller.OnSceneGUI();
                    return;
                }
            }
        }
	}
}
