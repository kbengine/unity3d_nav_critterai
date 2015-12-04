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
using org.critterai.geom;
using org.critterai.nav;
using org.critterai.nav.u3d.editor;
using org.critterai.nav.u3d;
using org.critterai.nmgen;
using org.critterai.u3d;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
	internal class MeshDebugView
	{
        private class Geom
        {
            public TriangleMesh mesh;
            public byte[] areas;
        }

        private bool mEnabled;
        private MeshDebugOption mShow;
        private bool mNeedsRepaint;

        private System.Object mDebugObject;

        private int mLastVersion;
        private InputGeometry mLastGeom;
        private int mLastX;
        private int mLastZ;
        private int mLastSize;

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

        public MeshDebugOption Show
        {
            get { return mShow; }
            set 
            {
                if (mShow != value)
                {
                    mShow = value;
                    mNeedsRepaint = true;
                    mDebugObject = null;
                }
            }
        }

        private void HandleWorkingNavmesh(TileSelection selection)
        {
            NavmeshBuild build = selection.Build;
            TileBuildData tdata = build.BuildData;

            if (mDebugObject == null)
            {
                Navmesh navmesh = null;

                if (tdata.BakeableCount() == 0)
                    // Nothing to display.
                    return;

                bool success = true;

                TileSetDefinition tdef = build.TileSetDefinition;

                NavmeshParams nconfig;
                NavmeshTileData[] tiles;

                if (tdef == null)
                {
                    tiles = new NavmeshTileData[1] { tdata.GetTileData(0, 0) };
                    nconfig = NavUtil.DeriveConfig(tiles[0]);
                }
                else
                {
                    TileZone zone;

                    if (selection.HasSelection)
                        zone = selection.Zone;
                    else
                        zone = new TileZone(0, 0, tdef.Width - 1, tdef.Depth - 1);

                    success = tdata.GetMeshBuildData(tdef.BoundsMin, tdef.TileWorldSize, zone
                        , out nconfig, out tiles);
                }

                NavStatus status = NavStatus.Sucess;

                if (success)
                {
                    status = Navmesh.Create(nconfig, out navmesh);

                    if ((status & NavStatus.Failure) == 0)
                    {
                        foreach (NavmeshTileData tile in tiles)
                        {
                            uint trash;
                            status = navmesh.AddTile(tile, Navmesh.NullTile, out trash);

                            if ((status & NavStatus.Sucess) == 0)
                            {
                                navmesh = null;
                                break;
                            }
                        }
                    }
                }

                if ((status & NavStatus.Sucess) == 0)
                {
                    Show = MeshDebugOption.None;  // Use property.
                    Debug.LogError("Mesh Debug View: Error creating working navigation mesh: "
                            + status + ". Disabled display.", build);
                }
                else
                    mDebugObject = navmesh;
            }

            if (mDebugObject != null)
            {
                Navmesh nm = (Navmesh)mDebugObject;
                NavDebug.Draw(nm, NavmeshSceneDraw.Instance.ColorByArea);
            }

        }

        private void HandleInputGeom(NavmeshBuild build, int tx, int tz)
        {
            if (build.InputGeom != mLastGeom)
            {
                // Input geometry has changed.  Clear debug object.
                mLastGeom = build.InputGeom;
                mDebugObject = null;
            }

            if (build.InputGeom == null)
                return;

            if (mDebugObject == null)
            {
                TileSetDefinition tdef = build.TileSetDefinition;

                Vector3 bmin;
                Vector3 bmax;

                tdef.GetTileBounds(tx, tz, true, out bmin, out bmax);

                Geom geom = new Geom();

                geom.mesh = 
                    build.InputGeom.ExtractMesh(bmin.x, bmin.z, bmax.x, bmax.z, out geom.areas);

                mDebugObject = geom;
            }

            if (mDebugObject != null)
            {
                Geom geom = (Geom)mDebugObject;
                if (geom.mesh.triCount > 0)
                {
                    DebugDraw.TriangleMesh(geom.mesh.verts
                        , geom.mesh.tris, geom.areas, geom.mesh.triCount
                        , true, 0.25f);
                }
            }
        }

        public void HandlePolyMesh(NavmeshBuild build, int tx, int tz)
        {
            if (!build)
                return;

            if (mDebugObject == null)
            {
                PolyMesh mesh = build.BuildData.GetPolyMesh(tx, tz);

                if (mesh != null)
                    mDebugObject = mesh.GetData(false);
            }

            if (mDebugObject != null)
                NMGenDebug.Draw((PolyMeshData)mDebugObject);
        }

        public void HandleDetailMesh(NavmeshBuild build, int tx, int tz)
        {
            if (!build)
                return;

            if (mDebugObject == null)
            {
                PolyMeshDetail mesh = build.BuildData.GetDetailMesh(tx, tz);

                if (mesh != null)
                    mDebugObject = mesh.GetData(false);
            }

            if (mDebugObject != null)
                NMGenDebug.Draw((PolyMeshDetailData)mDebugObject);
        }

        public void OnRenderObject(NavmeshBuild build, TileSelection selection)
        {
            if (!build)
                return;

            TileBuildData tdata = build.BuildData;

            if (!mEnabled
                || mShow == MeshDebugOption.None
                || tdata == null  // This restriction is appropriate.
                || build != selection.Build)  // Important error check.
            {
                return;
            }

            INavmeshData target = build.BuildTarget;

            if (target != null && target.HasNavmesh && NavmeshSceneDraw.Instance.IsShown(target))
                // Don't overdraw the target mesh's display.  It has priority.
                return;

            if (tdata.Version != mLastVersion)
            {
                // Build data has changed.  Clear debug object.
                mLastVersion = tdata.Version;
                mDebugObject = null;
            }

            int tx = 0;
            int tz = 0;
            int size = 0;

            if (tdata.IsTiled)
            {
                tx = selection.SelectedX;
                tz = selection.SelectedZ;
                size = selection.ZoneSize;
            }

            if (mLastX != tx || mLastZ != tz || mLastSize != size)
            {
                // Change in selection.  Clear debug object.
                mLastX = tx;
                mLastZ = tz;
                mLastSize = size;
                mDebugObject = null;
                // Debug.Log("Clear debug on selection change.");
            }

            if (mShow == MeshDebugOption.WorkingMesh)
            {
                HandleWorkingNavmesh(selection);
                return;
            }
            else if (tdata.IsTiled && !selection.Validate())
            {
                // The mesh is tiled with no valid selection.
                // Can't display any of the other meshes.
                mLastX = -1;
                mLastZ = -1;
                mLastSize = -1;
                return;
            }

            // Can only display a single tile for all other display options.
            // Choose the tile to display.

            switch (mShow)
            {
                case MeshDebugOption.PolyMesh:

                    HandlePolyMesh(build, tx, tz);
                    break;

                case MeshDebugOption.Detailmesh:

                    HandleDetailMesh(build, tx, tz);
                    break;

                case MeshDebugOption.InputGeometry:

                    if (build.TileSetDefinition != null)
                        HandleInputGeom(build, tx, tz);
                    break;
            }
        }
	}
}
