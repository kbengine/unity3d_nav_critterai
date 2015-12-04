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
    [StructLayout(LayoutKind.Sequential)]
    internal struct PolyMeshEx
    {
        /*
         * Design notes:
         * 
         * Must be structure so it can be passed across interop boundary
         * in an array.
         * 
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpmBuildSerializedData([In] byte[] meshData
            , int dataSize
            , ref PolyMeshEx polyMesh
            , ref int maxVerts
            , ref float walkableHeight
            , ref float walkableRadius
            , ref float walkableStep);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpmFreeMeshData(ref PolyMeshEx polyMesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpmGetSerializedData(ref PolyMeshEx polyMesh
            , int maxVerts
            , float walkableHeight
            , float waklableRadius
            , float walkableStep
            , bool includeBuffer
            , ref IntPtr data
            , ref int dataSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool rcpmBuildFromContourSet(IntPtr context
            , [In] ContourSetEx cset
            , int maxVertsPerPoly
            , ref PolyMeshEx polyMesh
            , ref int maxVerts);

        /// <summary>
        /// ushort buffer
        /// </summary>
        public IntPtr verts;

        /// <summary>
        /// ushort buffer
        /// </summary>
        public IntPtr polys;

        /// <summary>
        /// ushort buffer
        /// </summary>
        public IntPtr regions;

        /// <summary>
        /// ushort buffer
        /// </summary>
        public IntPtr flags;

        /// <summary>
        /// byte buffer
        /// </summary>
        public IntPtr areas;

        public int vertCount;
        public int polyCount;

        public int maxPolys;

        public int maxVertsPerPoly;

        public Vector3 boundsMin;
        public Vector3 boundsMax;
        public float xzCellSize;
        public float yCellSize;
        public int borderSize;

        public void Reset()
        {
            verts = IntPtr.Zero;
            polys = IntPtr.Zero;
            regions = IntPtr.Zero;
            areas = IntPtr.Zero;
            vertCount = 0;
            polyCount = 0;
            maxPolys = 0;
            maxVertsPerPoly = 0;
            boundsMin = Vector3Util.Zero;
            boundsMax = Vector3Util.Zero;
            xzCellSize = 0;
            yCellSize = 0;
            borderSize = 0;
        }
    }
}
