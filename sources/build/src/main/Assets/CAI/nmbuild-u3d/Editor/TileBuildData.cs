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
using org.critterai.nav;
using org.critterai.nmgen;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Undocumented class meant for internal use only. (Editor Only)
    /// </summary>
    [System.Serializable]
    public sealed class TileBuildData
    {
        /*
         * Design note:
         * 
         * The strange design is due to the need to support Unity serialization.
         */

        internal const int IndexError = -1;

        /// <summary>
        /// Undocumented.
        /// </summary>
        public BuildDataItem[] unsafeItems;

        /// <summary>
        /// Undocumented.
        /// </summary>
        public int unsafeWidth;

        /// <summary>
        /// Undocumented.
        /// </summary>
        public int unsafeDepth;

        /// <summary>
        /// Undocumented.
        /// </summary>
        public int unsafeVersion = 0;

        private bool mIsDirty;

        internal bool IsDirty 
        { 
            get { return mIsDirty; }
            set { mIsDirty = value; }
        }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal int Version { get { return unsafeVersion; } }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal int Width { get { return unsafeWidth; } }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal int Depth { get { return unsafeDepth; } }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal bool IsTiled { get { return unsafeDepth > 1 || unsafeWidth > 1; } }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal bool IsValid
        {
            get { return (unsafeDepth > 0 && unsafeWidth > 0); }
        }


        internal TileBuildData(int width, int depth)
        {
            Resize(width, depth);
        }

        /// <summary>
        /// Undocumented.
        /// </summary>
        internal TileBuildData()
        {
            unsafeWidth = 0;
            unsafeDepth = 0;
            unsafeItems = new BuildDataItem[0];
        }

        internal void Resize(int width, int depth)
        {
            unsafeWidth = System.Math.Max(0, width);
            unsafeDepth = System.Math.Max(0, depth);
            unsafeItems = new BuildDataItem[unsafeWidth * unsafeDepth];

            for (int tx = 0; tx < unsafeWidth; tx++)
            {
                for (int tz = 0; tz < unsafeDepth; tz++)
                {
                    unsafeItems[GetIndex(tx, tz)] = new BuildDataItem(tx, tz);
                }
            }

            mIsDirty = true;
            unsafeVersion++;
        }

        internal int GetStateCount(TileBuildState state)
        {
            int result = 0;
            foreach (BuildDataItem item in unsafeItems)
            {
                if (item.TileState == state)
                    result++;
            }
            return result;
        }

        /// <summary>
        /// The number of tiles that contain data that can be baked.
        /// </summary>
        /// <returns>The number of tiles that contain data that can be baked.</returns>
        internal int BakeableCount()
        {
            int result = 0;
            foreach (BuildDataItem item in unsafeItems)
            {
                TileBuildState bstate = item.TileState;
                if (bstate == TileBuildState.Built || bstate == TileBuildState.Baked)
                    result++;
            }

            return result;
        }

        /// <summary>
        /// The number of tiles that contain data that can be baked.
        /// </summary>
        /// <param name="readyToBake">The number of tiles that contain data that can be baked.</param>
        /// <param name="maxTilePolys">The maximum polygons contained by any bakeable tile.</param>
        internal void BakeableCount(out int readyToBake, out int maxTilePolys)
        {
            readyToBake = 0;
            maxTilePolys = 0;
            foreach (BuildDataItem item in unsafeItems)
            {
                switch (item.TileState)
                {
                    case TileBuildState.Built:

                        readyToBake++;
                        maxTilePolys = 
                            System.Math.Max(item.workingPolyCount, maxTilePolys);

                        break;

                    case TileBuildState.Baked:

                        readyToBake++;
                        maxTilePolys = 
                            System.Math.Max(item.bakedPolyCount, maxTilePolys);

                        break;

                }
            }
        }

        internal TileBuildState GetState(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return TileBuildState.Error;

            return unsafeItems[i].TileState;
        }

        internal NavmeshTileData GetTileData(int x, int z)
        {
            int trash;
            return GetTileData(x, z, out trash);
        }

        internal NavmeshTileData GetTileData(int x, int z, out int polyCount)
        {
            polyCount = 0;

            int i = GetIndex(x, z);
            if (i == IndexError)
                return null;

            BuildDataItem item = unsafeItems[i];

            /*
             * Important: Must use the state. This method must return
             * the same number of tiles as the BakeableCount method indicates.
             */

            switch (item.TileState)
            {
                case TileBuildState.Built:

                    polyCount = item.workingPolyCount;
                    return NavmeshTileData.Create(item.workingTile);

                case TileBuildState.Baked:

                    polyCount = item.bakedPolyCount;
                    return NavmeshTileData.Create(item.bakedTile);
            }

            polyCount = 0;
            return null;
        }

        // This method indicates only that there are tiles that are
        // no longer in their base state.  It doesn't mean that there is
        // anything that can be baked.  (All tiles may be empty.)
        internal int NeedsBakingCount()
        {
            int result = 0;

            foreach (BuildDataItem item in unsafeItems)
            {
                TileBuildState ts = item.TileState;
                result += (ts == TileBuildState.Built || ts == TileBuildState.Empty) 
                    ? 1 : 0;
            }

            return result;
        }

        internal int GetActive()
        {
            int result = 0;

            foreach (BuildDataItem item in unsafeItems)
            {
                TileBuildState ts = item.TileState;
                result += (ts == TileBuildState.InProgress
                    || ts == TileBuildState.Queued) ? 1 : 0;
            }

            return result;
        }

        internal void ClearUnbaked(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].ClearUnbaked();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal void SetAsEmpty(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetAsEmpty();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal void SetAsFailed(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetAsFailed();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal void SetAsQueued(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetAsQueued();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal void SetAsInProgress(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetAsInProgress();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal PolyMesh GetPolyMesh(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return null;

            BuildDataItem item = unsafeItems[i];

            if (item.polyMesh.Length == 0)
                return null;

            return PolyMesh.Create(item.polyMesh);
        }

        internal PolyMeshDetail GetDetailMesh(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return null;

            BuildDataItem item = unsafeItems[i];

            if (item.detailMesh.Length == 0)
                return null;

            return PolyMeshDetail.Create(item.detailMesh);
        }

        internal void SetWorkingData(int x, int z, PolyMesh polyMesh, PolyMeshDetail detailMesh)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetWorkingData(polyMesh, detailMesh);

            mIsDirty = true;
            unsafeVersion++;
        }

        internal void SetWorkingData(int x, int z, NavmeshTileData tile, int polyCount)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].SetWorkingData(tile, polyCount);

            mIsDirty = true;
            unsafeVersion++;
        }

        internal bool SetAsBaked()
        {
            bool result = false;
            for (int tx = 0; tx < unsafeWidth; tx++)
            {
                for (int tz = 0; tz < unsafeDepth; tz++)
                {
                    int i = GetIndex(tx, tz);

                    // Note: It is safe to call this method on all tiles.
                    if (unsafeItems[i].SetAsBaked())
                    {
                        mIsDirty = true;
                        unsafeVersion++;
                        result = true;
                    }
                }
            }
            return result;
        }

        internal bool SetAsBaked(int x, int z, byte[] tile, int polyCount)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return false;

            if (unsafeItems[i].SetAsBaked(tile, polyCount))
            {
                mIsDirty = true;
                unsafeVersion++;
                return true;
            }

            return false;
        }

        internal bool SetAsBaked(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return false;

            if (unsafeItems[i].SetAsBaked())
            {
                mIsDirty = true;
                unsafeVersion++;
                return true;
            }

            return false;
        }

        internal void Reset(int x, int z)
        {
            int i = GetIndex(x, z);
            if (i == IndexError)
                return;

            unsafeItems[i].Reset();

            mIsDirty = true;
            unsafeVersion++;
        }

        internal int GetIndex(int x, int z)
        {
            if (x < 0 || x >= unsafeWidth || z < 0 || z >= unsafeDepth)
                return IndexError;
            return (z * unsafeWidth) + x;
        }

        internal bool GetMeshBuildData(Vector3 origin, float tileWorldSize
            , out NavmeshParams config
            , out NavmeshTileData[] tiles)
        {
            return GetMeshBuildData(origin, tileWorldSize, new TileZone(0, 0, Width - 1, Depth - 1)
                , out config, out tiles);
        }

        internal bool GetMeshBuildData(Vector3 origin, float tileWorldSize, TileZone zone
            , out NavmeshParams config
            , out NavmeshTileData[] tiles)
        {
            // Is there anything to bake?

            config = null;
            tiles = null;

            int maxPolyCount;

            int tileCount;
            BakeableCount(out tileCount, out maxPolyCount);

            if (tileCount == 0)
                return false;

            config = new NavmeshParams(origin
                , tileWorldSize, tileWorldSize
                , Mathf.Max(1, tileCount)
                , Mathf.Max(1, maxPolyCount));

            // Add the tiles.

            List<NavmeshTileData> ltiles = new List<NavmeshTileData>();

            for (int tx = zone.xmin; tx <= zone.xmax; tx++)
            {
                for (int tz = zone.zmin; tz <= zone.zmax; tz++)
                {
                    int trash;
                    NavmeshTileData td = GetTileData(tx, tz, out trash);
                    if (td == null)
                        // Tile is not available.
                        continue;

                    ltiles.Add(td);
                }
            }

            tiles = ltiles.ToArray();

            return true;
        }

    }
}
