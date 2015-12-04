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

namespace org.critterai.nmgen.rcn
{
    internal static class HeightfieldEx
    {
        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr nmhfAllocField(int width
            , int depth
            , [In] ref Vector3 boundsMin
            , [In] ref Vector3 boundsMax
            , float xzCellSize
            , float yCellSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void nmhfFreeField(IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfRasterizeTriangle(IntPtr context
            , [In] Vector3[] verts
            , byte area
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfRasterizeTriMesh(IntPtr context
            , [In] Vector3[] verts
            , int vertCount
            , [In] int[] tris
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfRasterizeNodes(IntPtr context
            , IntPtr verts
            , IntPtr tris
            , IntPtr areas
            , [In] ChunkyTriMeshNode[] nodes
            , int nodeCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfRasterizeTriMeshShort(IntPtr context
            , [In] Vector3[] verts
            , int vertCount
            , [In] ushort[] tris
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfRasterizeTriangles(IntPtr context
            , [In] Vector3[] verts
            , [In] byte[] areas
            , int triCount
            , IntPtr hf
            , int flagMergeThreshold);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfFilterLowHangingWalkableObstacles(IntPtr context
            , int walkableStep
            , IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfFilterLedgeSpans(IntPtr context
            , int walkableHeight
            , int walkableStep
            , IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmhfFilterWalkableLowHeightSpans(IntPtr context
            , int walkableHeight
            , IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int nmhfGetHeightFieldSpanCount(IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int nmhfGetMaxSpansInColumn(IntPtr hf);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int nmhfGetSpans(IntPtr hf
            , int widthIndex
            , int depthIndex
            , [In, Out] HeightfieldSpan[] spanBuffer
            , int bufferSize);
    }
}
