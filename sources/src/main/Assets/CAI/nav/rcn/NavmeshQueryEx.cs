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
    internal static class NavmeshQueryEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtnqBuildDTNavQuery(IntPtr navmesh
            , int maxNodes
            , ref IntPtr resultQuery);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtnqFree(ref IntPtr query);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqGetPolyWallSegments(IntPtr query
            , uint polyRef
            , IntPtr filter
            , [In, Out] Vector3[] segmentVerts
            , [In, Out] uint[] segmentPolyRefs
            , ref int segmentCount
            , int maxSegments);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindNearestPoly(IntPtr query
            , [In] ref Vector3 position
            , [In] ref Vector3 extents
		    , IntPtr filter
		    , ref NavmeshPoint nearest);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqClosestPointOnPoly(IntPtr query
            , uint polyRef
            , [In] ref Vector3 position
            , ref Vector3 resultPoint);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqClosestPointOnPolyBoundary(IntPtr query 
            , uint polyRef
            , [In] ref Vector3 position
            , [In] ref Vector3 resultPoint);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqQueryPolygons(IntPtr query
                , ref Vector3 position
                , ref Vector3 extents
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs
                , ref int resultCount
                , int maxResult);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindPolysAroundCircle(IntPtr query
                , uint startPolyRef
                , [In] ref Vector3 position
                , float radius
                , IntPtr filter
                , [In, Out] uint[] resultPolyRefs  // Optional
                , [In, Out] uint[] resultParentRefs // Optional
                , [In, Out] float[] resultCosts // Optional
                , ref int resultCount
                , int maxResult);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindPolysAroundShape(IntPtr query 
            , uint startPolyRef
            , [In] Vector3[] verts
            , int vertCount
	        , IntPtr filter
	        , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
            , [In, Out] float[] resultCosts
	        , ref int resultCount
            , int maxResult);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindLocalNeighbourhood(IntPtr query
            , uint startPolyRef
            , [In] ref Vector3 position
            , float radius
            , IntPtr filter
            , [In, Out] uint[] resultPolyRefs
            , [In, Out] uint[] resultParentRefs
            , ref int resultCount
            , int maxResult);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqGetPolyHeight(IntPtr query
            , NavmeshPoint position
            , ref float height);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindDistanceToWall(IntPtr query
            , NavmeshPoint position
            , float searchRadius
	        , IntPtr filter
	        , ref float distance
            , ref Vector3 closestPoint
            , ref Vector3 normal);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindPath(IntPtr query
            , NavmeshPoint startPosition
            , NavmeshPoint endPosition
            , IntPtr filter
            , [In, Out] uint[] resultPath
            , ref int pathCount
            , int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindPathExt(IntPtr query
            , ref NavmeshPoint startPosition
            , ref NavmeshPoint endPosition
            , [In] ref Vector3 extents
            , IntPtr filter
            , [In, Out] uint[] resultPath
            , ref int pathCount
            , int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtqIsInClosedList(IntPtr query
            , uint polyRef);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtqIsValidPolyRef(IntPtr query
            , uint polyRef
            , IntPtr filter);
	
	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqRaycast(IntPtr query
            , NavmeshPoint startPosition
            , [In] ref Vector3 endPosition
	        , IntPtr filter
	        , ref float hitParameter 
            , ref Vector3 hitNormal
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindStraightPath(IntPtr query
            , [In] ref Vector3 startPosition
            , [In] ref Vector3 endPosition
            , [In] uint[] path
            , int pathStart
            , int pathSize
            , [In, Out] Vector3[] straightPathPoints
            , [In, Out] WaypointFlag[] straightPathFlags
            , [In, Out] uint[] straightPathRefs
            , ref int straightPathCount
            , int maxStraightPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqMoveAlongSurface(IntPtr query
            , NavmeshPoint startPosition
            , [In] ref Vector3 endPosition
            , IntPtr filter
            , ref Vector3 resultPosition
            , [In, Out] uint[] visitedPolyRefs
            , ref int visitedCount
            , int maxVisited);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqInitSlicedFindPath(IntPtr query
            , NavmeshPoint startPosition
            , NavmeshPoint endPosition
            , IntPtr filter);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqUpdateSlicedFindPath(IntPtr query
            , int maxIterations
            , ref int actualIterations);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFinalizeSlicedFindPath(IntPtr query
            , [In, Out] uint[] path
            , ref int pathCount
            , int maxPath);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindRandomPoint(IntPtr query
            , IntPtr filter
            , ref NavmeshPoint randomPt);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern NavStatus dtqFindRandomPointCircle(IntPtr query
            , NavmeshPoint start
            , float radius
            , IntPtr filter
            , ref NavmeshPoint randomPt);
    }
}
