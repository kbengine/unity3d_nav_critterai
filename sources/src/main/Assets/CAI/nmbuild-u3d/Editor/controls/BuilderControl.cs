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
using org.critterai.nav.u3d.editor;

namespace org.critterai.nmbuild.u3d.editor
{
	internal abstract class BuilderControl
        : BuildControl
	{
        // TODO: EVAL: v0.5: Try for better solution.
        // Basically, this only adds shared input build functionality.

        private MiniInputCompile mInputCompile = null;

        protected bool IsBaseBusy { get { return (mInputCompile != null); } }

        public override void Exit()
        {
            Context.AbortAllReqests("Exiting build.");
            mInputCompile = null;

            base.Exit();
        }

        protected void OnGUIMainStandard()
        {
            if (mInputCompile != null)
            {
                Rect area = Context.MainArea;

                area = new Rect(area.x + area.width * 0.25f
                    , area.y + area.height * 0.66f
                    , area.width * 0.50f
                    , 25);

                mInputCompile.OnGUI(area);
            }
        }

        protected bool OnGUIStandardButtons()
        {
            bool result = false;

            NavmeshBuild build = Context.Build;

            if (!build)
                return result;

            bool guiEnabled = GUI.enabled;

            GUI.enabled = guiEnabled
                && mInputCompile == null
                && Context.TaskCount == 0
                && !NavEditorUtil.SceneMismatch(build.BuildTarget.BuildInfo);

            if (GUILayout.Button("Recompile Input"))
            {
                mInputCompile = new MiniInputCompile(Context);

                if (mInputCompile.IsFinished)
                    mInputCompile = null;
            }

            GUI.enabled = guiEnabled 
                && mInputCompile == null 
                && Context.TaskCount == 0;

            if (GUILayout.Button("Reinitialize Builder"))
            {
                result = true;
                build.DiscardBuildData();
            }

            GUI.enabled = guiEnabled;

            return result;
        }

        public override void Update()
        {
            base.Update();

            if (Context == null || mInputCompile == null)
                return;

            mInputCompile.Update();

            if (!mInputCompile.IsFinished)
                return;

            NavmeshBuild build = Context.Build;

            if (!build)
                return;

            UnityBuildContext mLogger = new UnityBuildContext();

            if (mInputCompile.HasData)
            {
                if (!build.SetInputData(mLogger, mInputCompile.Geometry
                    , mInputCompile.Info, mInputCompile.Processors, mInputCompile.Connections
                    , true))
                {
                   mLogger.PostError("Could not apply input data.", build);
                }
            }
            else
                mLogger.PostError("Input compile did not produce anything.", build);

            mInputCompile = null;
        }
    }
}
