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
using org.critterai.interop;
#if NUNITY
using Vector3 = org.critterai.Vector3;
#else
using Vector3 = UnityEngine.Vector3;
#endif

namespace org.critterai.nmgen.rcn
{
    internal static class PolyMeshDetailEx
    {
        /*
         * Design note:
         * 
         * This class will have to be converted to a structure when the
         * merge mesh functionality is implemented.
         * 
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpdFreeMeshData([In, Out] PolyMeshDetail detailMesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpdGetSerializedData(
            [In] PolyMeshDetail detailMesh
                , bool includeBuffer
                , ref IntPtr resultData
                , ref int dataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpdBuildFromMeshData([In] byte[] meshData
        , int dataSize
        , [In, Out] PolyMeshDetail detailMesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpdFlattenMesh([In] PolyMeshDetail detailMesh
            , [In, Out] Vector3[] verts
            , ref int vertCount
            , int vertsSize
            , [In, Out] int[] tris
            , ref int triCount
            , int trisSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpdBuildPolyMeshDetail(IntPtr context
            , ref PolyMeshEx polyMesh
            , [In] CompactHeightfield chf
            , float sampleDist
            , float sampleMaxError
            , [In, Out] PolyMeshDetail detailMesh);
    }
}
