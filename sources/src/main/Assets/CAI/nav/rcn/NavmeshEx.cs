/*
 * Copyright (c) 2011 Stephen A. Pratt
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
using System;
using System.Runtime.InteropServices;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nav.rcn
{
    internal static class NavmeshEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmBuildSingleTileMesh(
            NavmeshTileBuildData buildData
            , ref IntPtr resultMesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmBuildDTNavMeshFromRaw([In] byte[] rawMeshData
            , int dataSize
            , bool safeStorage
            , ref IntPtr resultNavMesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmInitTiledNavMesh(NavmeshParams config
            , ref IntPtr navmesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmFreeNavMesh(ref IntPtr navmesh, bool freeTiles);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmGetParams(IntPtr navmesh
            , [In, Out] NavmeshParams config);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmAddTile(IntPtr navmesh
            , [In, Out] NavmeshTileData tileData
            , uint lastRef
            , ref uint resultRef);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmRemoveTile(IntPtr navmesh
            , uint tileRef
            , ref IntPtr resultData
            , ref int resultDataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmCalcTileLoc(IntPtr navmesh
            , [In] ref Vector3 position
            , ref int tx
            , ref int tz);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtnmGetTileAt(IntPtr navmesh
            , int x
            , int z
            , int layer);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTilesAt(IntPtr navmesh
            , int x
            , int z
            , [In, Out] IntPtr[] tiles
            , int tilesSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern uint dtnmGetTileRefAt(IntPtr navMesh
            , int x
            , int z
            , int layer);

        // The other get tile id method is in NavmeshTileEx.

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtnmGetTileByRef(IntPtr navmesh
            , uint tileRef);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetMaxTiles(IntPtr navmesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtnmGetTile(IntPtr navmesh
            , int tileIndex);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetTileAndPolyByRef(IntPtr navmesh
            , uint polyRef
            , ref IntPtr tile
            , ref IntPtr poly);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtnmIsValidPolyRef(IntPtr navmesh
            , uint polyRef);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetConnectionEndPoints(
            IntPtr navmesh
            , uint previousPolyRef
            , uint polyRef
            , [In, Out] ref Vector3 startPosition
            , [In, Out] ref Vector3 endPosition);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtnmGetOffMeshConnectionByRef(IntPtr navmesh
            , uint polyRef);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ref ushort flags);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmSetPolyFlags(IntPtr navmesh
            , uint polyRef
            , ushort flags);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetPolyArea(IntPtr navmesh
            , uint polyRef
            , ref byte area);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmSetPolyArea(IntPtr navmesh
            , uint polyRef
            , byte area);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmGetNavMeshRawData(IntPtr navmesh
            , ref IntPtr resultData
            , ref int dataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmFreeBytes(ref IntPtr data);
    }
}
