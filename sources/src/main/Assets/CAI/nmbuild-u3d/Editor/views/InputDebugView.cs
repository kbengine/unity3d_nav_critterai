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
using org.critterai.u3d;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
	internal class InputDebugView
	{
        private bool mEnabled;
        private bool mShow;
        private bool mNeedsRepaint;

        public bool NeedsRepaint
        {
            get { return mNeedsRepaint; }
            set { mNeedsRepaint = value; }
        }

        public bool Enabled
        {
            get { return mEnabled; }
            set 
            {
                if (mEnabled != value)
                {
                    mEnabled = value;
                    mNeedsRepaint = true;
                }
            }
        }

        public bool Show
        {
            get { return mShow; }
            set 
            {
                if (mShow != value)
                {
                    mShow = value;
                    mNeedsRepaint = true;
                }
            }
        }

        public void OnRenderObject(NavmeshBuild build)
        {
            if (mEnabled && mShow && build && build.HasInputData)
            {
                InputGeometry geom = build.InputGeom;
                DebugDraw.Bounds(geom.BoundsMin, geom.BoundsMax, Color.grey);
            }
        }
	}
}
