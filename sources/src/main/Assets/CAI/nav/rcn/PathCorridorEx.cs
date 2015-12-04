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
    internal static class PathCorridorEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtpcAlloc(int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcFree(IntPtr corridor);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcReset(IntPtr corridor
            , NavmeshPoint position);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcFindCorners(IntPtr corridor
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtpcOptimizePathVisibility(IntPtr corridor
            , [In] ref Vector3 next
            , float pathOptimizationRange
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcOptimizePathVisibilityExt(IntPtr corridor
            , [In] ref Vector3 next
            , float pathOptimizationRange
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcOptimizePathTopology(IntPtr corridor
             , IntPtr navquery
             , IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcOptimizePathTopologyExt(IntPtr corridor
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcMoveOverOffmeshConnection(IntPtr corridor
            , uint offMeshConRef
            , [In, Out] uint[] refs // size 2
            , ref Vector3 startPos
            , ref Vector3 endPos
            , ref NavmeshPoint resultPos
            , IntPtr navquery);
    	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcMovePosition(IntPtr corridor
            , [In] ref Vector3 npos
            , ref NavmeshPoint pos
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcMoveTargetPosition(IntPtr corridor
            , [In] ref Vector3 npos
            , ref NavmeshPoint pos
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcMove(IntPtr corridor
            , [In] ref Vector3 npos
            , [In] ref Vector3 ntarget
            , ref NavmeshPoint pos
            , ref NavmeshPoint target
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcSetCorridor(IntPtr corridor
            , [In] ref Vector3 target
            , [In] uint[] path
            , int pathCount
            , ref NavmeshPoint resultTarget
            , [In, Out] Vector3[] cornerVerts
            , [In, Out] WaypointFlag[] cornerFlags
            , [In, Out] uint[] cornerPolys
            , int maxCorners
            , IntPtr navquery
            , IntPtr filter);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcGetPath(IntPtr corridor
             , [In, Out] uint[] path
             , int maxPath);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtpcGetPathCount(IntPtr corridor);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcGetData(IntPtr corridor
            , [In, Out] PathCorridorData data);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtpcIsValid(IntPtr corridor
            , int maxLookAhead
            , IntPtr navquery
            , IntPtr filter);
    }
}
