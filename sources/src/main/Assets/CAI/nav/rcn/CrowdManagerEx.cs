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
    internal static class CrowdManagerEx
    {
        /*
         * Design note: In order to stay compatible with Unity iOS, all
         * extern methods must be unique and match DLL entry point.
         * (Can't use EntryPoint.)
         */

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtcDetourCrowdAlloc(int maxAgents
            , float maxAgentRadius
            , IntPtr navmesh);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcDetourCrowdFree(IntPtr crowd);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcSetObstacleAvoidanceParams(IntPtr crowd
            , int index
            , [In] CrowdAvoidanceParams obstacleParams);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcGetObstacleAvoidanceParams(IntPtr crowd
            , int index
            , [In, Out] CrowdAvoidanceParams config);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtcGetAgent(IntPtr crowd, int idx);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtcGetAgentCount(IntPtr crowd);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtcAddAgent(IntPtr crowd
            , [In] ref Vector3 pos
            , ref CrowdAgentParams agentParams
            , ref IntPtr agent
            , ref CrowdAgentCoreState initialState);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcUpdateAgentParameters(IntPtr crowd
            , int index
            , ref CrowdAgentParams agentParams);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcRemoveAgent(IntPtr crowd
            , int index);

        /* 
         * On the native side, the query filter is a constant pointer.  But I'm 
         * purposefully not protecting it on the managed side.  
         * I want the filter to be mutable and this is a dirty but quick way 
         * of doing it.
         */
        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtcGetFilter(IntPtr crowd);


        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcGetQueryExtents(IntPtr crowd
            , ref Vector3 extents);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern int dtcGetVelocitySampleCount(IntPtr crowd);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtcGetGrid(IntPtr crowd);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern void dtcUpdate(IntPtr crowd
            , float deltaTime
            , [In, Out] CrowdAgentCoreState[] coreStates);

        [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern IntPtr dtcGetNavMeshQuery(IntPtr crowd);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtcRequestMoveTarget(IntPtr crowd
            , int agentIndex
            , NavmeshPoint position);

	    [DllImport(InteropUtil.PLATFORM_DLL)]
        public static extern bool dtcAdjustMoveTarget(IntPtr crowd
            , int agentIndex
            , NavmeshPoint position);
    }
}
