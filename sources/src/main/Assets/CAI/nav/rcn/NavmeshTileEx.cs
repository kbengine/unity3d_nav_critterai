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
    internal static class NavmeshTileEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtnmBuildTileData(NavmeshTileBuildData sourceData
            , [In, Out] NavmeshTileData resultTile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtnmBuildTileDataRaw([In] byte[] rawData
            , int dataSize
            , [In, Out] NavmeshTileData resultTile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnmFreeTileData(NavmeshTileData tileData);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetTileDataHeader([In] byte[] rawData
            , int dataSize
            , ref NavmeshTileHeader resultHeader);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmGetTileDataHeaderAlt(IntPtr rawData
            , int dataSize
            , ref NavmeshTileHeader resultHeader);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern uint dtnmGetTileRef(IntPtr navmesh, IntPtr tile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileStateSize(IntPtr navmesh, IntPtr tile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmStoreTileState(IntPtr navmesh
            , IntPtr tile
            , [In, Out] byte[] stateData
            , int dataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnmRestoreTileState(IntPtr navmesh
            , IntPtr tile
            , [In] byte[] stateData
            , int dataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtnmGetTileHeader(IntPtr tile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern uint dtnmGetPolyRefBase(IntPtr navmesh, IntPtr tile);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileVerts(IntPtr tile
            , [In, Out] Vector3[] verts
            , int vertsCount);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTilePolys(IntPtr tile
            , [In, Out] NavmeshPoly[] polys
            , int polysSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileDetailVerts(IntPtr tile
            , [In, Out] Vector3[] verts
            , int vertsCount);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileDetailTris(IntPtr tile
            , [In, Out] byte[] tris
            , int trisSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileDetailMeshes(IntPtr tile
            , [In, Out] NavmeshDetailMesh[] detailMeshes
            , int meshesSize);


        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileLinks(IntPtr tile
            , [In, Out] NavmeshLink[] links
            , int linksSize);


        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileBVTree(IntPtr tile
            , [In, Out] NavmeshBVNode[] nodes
            , int nodesSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtnmGetTileConnections(IntPtr tile
            , [In, Out] NavmeshConnection[] conns
            , int connsSize);
    }
}
