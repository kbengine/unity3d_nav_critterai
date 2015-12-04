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
	internal class SelectionDebugView
	{
        private bool mEnabled;
        private bool mShow;
        private bool mIncludeRootTile;
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

        public bool IncludeRootTile
        {
            get { return mIncludeRootTile; }
            set 
            {
                if (mIncludeRootTile != value)
                {
                    mIncludeRootTile = value;
                    mNeedsRepaint = true;
                }
            }
        }

        public void OnRenderObject(NavmeshBuild build, TileSelection selection)
        {
            if (!build)
                return;

            TileSetDefinition tdef = build.TileSetDefinition;

            if (!mShow || !mEnabled || build != selection.Build || tdef == null)
                return;

            Color color = ControlUtil.SelectionColor;

            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            GL.Color(color);

            if (selection.Validate())
            {
                Vector3 bmin;
                Vector3 bmax;
                Vector3 trash;

                TileZone zone = selection.Zone;

                tdef.GetTileBounds(zone.xmin, zone.zmin, true, out bmin, out trash);
                tdef.GetTileBounds(zone.xmax, zone.zmax, true, out trash, out bmax);

                DebugDraw.AppendBounds(bmin, bmax);

                if (mIncludeRootTile)
                {
                    GL.Color(new Color(0.93f, 0.58f, 0.11f)); // Orange

                    tdef.GetTileBounds(selection.SelectedX, selection.SelectedZ, true
                        , out bmin, out bmax);

                    DebugDraw.AppendBounds(bmin, bmax);
                }
            }
            else
            {
                Vector3 bmax = tdef.BoundsMin;
                float tileSize = build.Config.TileWorldSize;
                bmax.x += tileSize * tdef.Width;
                bmax.y = tdef.BoundsMax.y;
                bmax.z += tileSize * tdef.Depth;

                DebugDraw.AppendBounds(tdef.BoundsMin, bmax);
            }

            GL.End();
        }
	}
}
