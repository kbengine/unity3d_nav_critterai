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

namespace org.critterai.nmbuild.u3d.editor
{
	internal class TileSelection
	{
        public const int NoSelection = -1;

        private readonly NavmeshBuild mBuild;

        private bool mIsDirty;

        private int mSelectedX = NoSelection;
        private int mSelectedZ = NoSelection;
        private int mZoneSize = 1;

        public NavmeshBuild Build { get { return mBuild ? mBuild : null; } } 
        
        public int SelectedX { get { return mSelectedX; } }
        public int SelectedZ { get { return mSelectedZ; } }

        public int ZoneSize
        {
            get { return mZoneSize; }
            set
            {
                if (mZoneSize != value)
                {
                    mZoneSize = Mathf.Max(0, value);
                    mIsDirty = true;
                }
            }
        }

        public bool IsDirty
        {
            get { return mIsDirty; }
            set { mIsDirty = value; }
        }

        public TileZone Zone
        {
            get
            {
                if (!mBuild)
                    return new TileZone();

                return new TileZone(Mathf.Max(0, mSelectedX - mZoneSize)
                    , Mathf.Max(0, mSelectedZ - mZoneSize)
                    , Mathf.Min(mBuild.TileSetDefinition.Width - 1, mSelectedX + mZoneSize)
                    , Mathf.Min(mBuild.TileSetDefinition.Depth - 1, mSelectedZ + mZoneSize));
            }
        }

        public bool HasSelection
        {
            get 
            { 
                return mBuild && !(mSelectedX == NoSelection || mBuild.TileSetDefinition == null); 
            }
        }

        public TileSelection(NavmeshBuild build)
        {
            if (!build)
                throw new System.ArgumentNullException();

            mBuild = build;
        }

        public bool Validate()
        {
            SetSelection(mSelectedX, mSelectedZ);
            return (mSelectedX != NoSelection);
        }

        public void SetSelection(int tx, int tz)
        {
            if (!mBuild)
                return;

            TileSetDefinition tdef = mBuild.TileSetDefinition;

            if (tx < 0 || tz < 0 
                || tdef == null
                || tx > tdef.Width - 1
                || tz > tdef.Depth - 1)
            {
                ClearSelection();
                return;
            }

            if (mSelectedX != tx)
            {
                mSelectedX = tx;
                mIsDirty = true;
            }

            if (mSelectedZ != tz)
            {
                mSelectedZ = tz;
                mIsDirty = true;
            }
        }

        public void ClearSelection()
        {
            if (mSelectedX == NoSelection)
                return;

            mSelectedX = NoSelection;
            mSelectedZ = NoSelection;

            mIsDirty = true;
        }
	}
}
