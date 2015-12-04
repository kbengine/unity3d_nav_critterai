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

namespace org.critterai.nav.rcn
{
    internal static class CrowdAgentEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcaGetAgentParams(IntPtr agent
            , ref CrowdAgentParams config);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcaGetAgentCorners(IntPtr agent
            , [In, Out] CornerData resultData);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcaGetAgentCoreData(IntPtr agent
            , [In, Out] CrowdAgentCoreState resultData);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtcaGetAgentNeighbors(IntPtr agent
            , [In, Out] CrowdNeighbor[] neighbors
            , int neighborsSize);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcaGetPathCorridorData(IntPtr agent
            , [In, Out] PathCorridorData corridor);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcaGetLocalBoundary(IntPtr agent
            , [In, Out] LocalBoundaryData boundary);
    }
}
