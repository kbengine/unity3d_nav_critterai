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
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class ContourSetEx
    {
        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmcsBuildSet(IntPtr context
            , [In] CompactHeightfield chf
            , float maxError
            , int maxEdgeLen
            , [In, Out] ContourSetEx cset
            , ContourBuildFlags flags);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void nmcsFreeSetData([In, Out] ContourSetEx cset);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool nmcsGetContour([In] ContourSetEx cset
            , int index
            , [In, Out] Contour result);

        public IntPtr contours = IntPtr.Zero;    // rcContour[contourCount]
        public int contourCount = 0;

        public Vector3 boundsMin;
        public Vector3 boundsMax;

        public float xzCellSize = 0;
        public float yCellSize = 0;
        public int width = 0;
        public int depth = 0;
        public int borderSize = 0;

        public void Reset()
        {
            contourCount = 0;
            contours = IntPtr.Zero;
            boundsMax = Vector3Util.Zero;
            boundsMin = Vector3Util.Zero;
            xzCellSize = 0;
            yCellSize = 0;
            width = 0;
            depth = 0;
            borderSize = 0;
        }
    }
}
