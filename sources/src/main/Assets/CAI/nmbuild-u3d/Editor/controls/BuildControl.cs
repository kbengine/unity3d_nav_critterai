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

namespace org.critterai.nmbuild.u3d.editor
{
    internal abstract class BuildControl
        : IBuildControl
    {
        private ControlContext mContext;
        private DebugViewContext mDebugContext;
        private readonly UnityBuildContext mLogger = new UnityBuildContext();

        protected UnityBuildContext Logger { get { return mLogger; } } 
        protected ControlContext Context { get { return mContext; } }
        protected DebugViewContext DebugContext { get { return mDebugContext; } }

        public bool IsActive { get { return (mContext != null); } }

        public virtual bool Enter(ControlContext context, DebugViewContext debug)
        {
            if (mContext != null || context == null || !context.Build || debug == null)
                // Strict.
                return false;

            mContext = context;
            mDebugContext = debug;

            return true; 
        }

        public virtual void Exit() 
        {
            mContext = null;
            mDebugContext = null;
            mLogger.ResetLog();
        }

        public virtual void Update() { }

        public void OnGUI()
        {
            if (mContext == null)
                return;

            if (!mContext.HideMain)
                OnGUIMain();

            OnGUIButtons();
        }

        protected abstract void OnGUIMain();
        protected abstract void OnGUIButtons();
    }
}
