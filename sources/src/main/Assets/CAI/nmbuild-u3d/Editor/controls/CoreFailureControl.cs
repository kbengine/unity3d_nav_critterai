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
using org.critterai.u3d.editor;

namespace org.critterai.nmbuild.u3d.editor
{
    internal sealed class CoreFailureControl
        : BuildControl
    {
        private static GUIStyle mBoxStyle = null;

        public CoreFailureControl() { }

        public override bool Enter(ControlContext context, DebugViewContext debug)
        {
            if (base.Enter(context, debug))
            {
                // Unforgiving.
                Context.Build.ResetBuild();
                return true;
            }

            return false;
        }

        public override void Exit() 
        {
            if (Context.Build != null)
                // Still unforgiving.
                Context.Build.ResetBuild();

            base.Exit();
        }

        protected override void OnGUIMain()
        {
            NavmeshBuild build = Context.Build;

            if (mBoxStyle == null)
            {
                mBoxStyle = new GUIStyle(EditorUtil.ErrorStyle);
                mBoxStyle.alignment = TextAnchor.MiddleCenter;
            }

            GUI.Box(Context.MainArea
                , (build ? build.name : "Unknown build") + " is not ready to build!\n"
                    + "Issue(s) must be corrected before a build can be started."
                , mBoxStyle);
        }

        protected override void OnGUIButtons()
        {
            ControlUtil.BeginButtonArea(Context.ButtonArea);
            ControlUtil.EndButtonArea();
        }
    }
}
