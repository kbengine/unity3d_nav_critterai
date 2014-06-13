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
#include <string.h>
#include "DetourCommon.h"
#include "DetourPathCorridor.h"
#include "DetourEx.h"

extern "C"
{
	EXPORT_API dtPathCorridor* dtpcAlloc(const int maxPath)
	{
		dtPathCorridor* corridor = new dtPathCorridor();
		if (corridor)
			corridor->init(maxPath);
		return corridor;
	}

	EXPORT_API void dtpcFree(dtPathCorridor* corridor)
	{
		if (corridor)
			corridor->~dtPathCorridor();
	}

	EXPORT_API void dtpcReset(dtPathCorridor* corridor
			, rcnNavmeshPoint pos)
	{
		// Note: The reset accepts the position without checking its validity.
		if (corridor)
			corridor->reset(pos.polyRef, &pos.point[0]);
	}

	EXPORT_API int dtpcFindCorners(dtPathCorridor* corridor
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (corridor)
			return corridor->findCorners(cornerVerts
				, cornerFlags
				, cornerPolys
				, maxCorners
				, navquery
				, filter);

		return -1;
	}

	EXPORT_API void dtpcOptimizePathVisibility(dtPathCorridor* corridor
		, const float* next
		, const float pathOptimizationRange
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (corridor)
			corridor->optimizePathVisibility(next
			, pathOptimizationRange
			, navquery
			, filter);
	}

	EXPORT_API int dtpcOptimizePathVisibilityExt(dtPathCorridor* corridor
		, const float* next
		, const float pathOptimizationRange
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (!corridor)
			return -1;

		corridor->optimizePathVisibility(next
			, pathOptimizationRange
			, navquery
			, filter);

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);

	 }

	EXPORT_API bool dtpcOptimizePathTopology(dtPathCorridor* corridor
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (!corridor)
			return corridor->optimizePathTopology(navquery, filter);
		return false;
	}
	
	EXPORT_API int dtpcOptimizePathTopologyExt(dtPathCorridor* corridor
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (!corridor)
			return false;

		corridor->optimizePathTopology(navquery, filter);

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);
	}

	 EXPORT_API bool dtpcMoveOverOffmeshConnection(dtPathCorridor* corridor
		 , dtPolyRef offMeshConRef
		 , dtPolyRef* refs
		 , float* startPos
		 , float* endPos
		 , rcnNavmeshPoint* resultPos
		 , dtNavMeshQuery* navquery)
	 {
		if (!corridor)
			return false;

		bool success = corridor->moveOverOffmeshConnection(offMeshConRef
				, refs
				, startPos
				, endPos
				, navquery);

		dtVcopy(&resultPos->point[0], corridor->getPos());
		resultPos->polyRef = corridor->getFirstPoly();

		return success;
	 }
	
	 EXPORT_API int dtpcMovePosition(dtPathCorridor* corridor
		, const float* npos
		, rcnNavmeshPoint* pos
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	 {
		if (!corridor)
			return -1;

		corridor->movePosition(npos, navquery, filter);

		if (pos)
		{
			dtVcopy(&pos->point[0], corridor->getPos());
			pos->polyRef = corridor->getFirstPoly();
		}

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);
	 }

	 EXPORT_API dtPolyRef dtpcMoveTargetPosition(dtPathCorridor* corridor
		, const float* npos
		, rcnNavmeshPoint* pos
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	 {
		if (!corridor)
			return -1;

		corridor->moveTargetPosition(npos, navquery, filter);

		if (pos)
		{
			dtVcopy(&pos->point[0], corridor->getTarget());
			pos->polyRef = corridor->getLastPoly();
		}

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);

	 }
	
	 EXPORT_API int dtpcMove(dtPathCorridor* corridor
		, const float* npos
		, const float* ntarget
		, rcnNavmeshPoint* pos
		, rcnNavmeshPoint* target
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	 {
		if (!corridor)
			return 0;

		if (ntarget)
			corridor->moveTargetPosition(ntarget, navquery, filter);

		if (npos)
			corridor->movePosition(npos, navquery, filter);

		if (pos)
		{
			dtVcopy(&pos->point[0], corridor->getPos());
			pos->polyRef = corridor->getFirstPoly();
		}

		if (target)
		{
			dtVcopy(&target->point[0], corridor->getTarget());
			target->polyRef = corridor->getLastPoly();
		}

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);
	 }

	 EXPORT_API int dtpcSetCorridor(dtPathCorridor* corridor
		, const float* target
		, const dtPolyRef* polys
		, const int npolys
		, rcnNavmeshPoint* resultTarget
		, float* cornerVerts
		, unsigned char* cornerFlags
		, dtPolyRef* cornerPolys
		, const int maxCorners
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	 {
		if (!(corridor && polys))
			return -1;

		corridor->setCorridor(target, polys, npolys);

		if (resultTarget)
		{
			dtVcopy(&resultTarget->point[0], corridor->getTarget());
			resultTarget->polyRef = corridor->getLastPoly();
		}

		return corridor->findCorners(cornerVerts
			, cornerFlags
			, cornerPolys
			, maxCorners
			, navquery
			, filter);
	 }
	
	 //EXPORT_API dtPolyRef dtpcGetPos(dtPathCorridor* corridor
		// , float* pos)
	 //{
		//if (!corridor || !pos)
		//	return 0;

		//dtVcopy(pos, corridor->getPos());
		//return corridor->getFirstPoly();
	 //}

	 //EXPORT_API dtPolyRef dtpcGetTarget(dtPathCorridor* corridor
		// , float* target)
	 //{
		//if (!corridor || !target)
		//	return 0;

		//dtVcopy(target, corridor->getTarget());
		//return corridor->getLastPoly();
	 //}
	 	
	 //EXPORT_API dtPolyRef dtpcGetFirstPoly(dtPathCorridor* corridor)
	 //{
		//if (corridor)
		//	return corridor->getFirstPoly();
		//return 0;
	 //}

	 //EXPORT_API dtPolyRef dtpcGetLastPoly(dtPathCorridor* corridor)
	 //{
		//if (corridor)
		//	return corridor->getLastPoly();
		//return 0;
	 //}
	
	 EXPORT_API int dtpcGetPath(dtPathCorridor* corridor
		 , dtPolyRef* path
		 , int maxPath)
	 {
		if (!corridor || !path || maxPath < 1)
			return 0;

		int count = dtMin(maxPath, corridor->getPathCount());

		memcpy(path, corridor->getPath(), sizeof(dtPolyRef) * count);

		return count;
	 }

	 EXPORT_API int dtpcGetPathCount(dtPathCorridor* corridor)
	 {
		if (corridor)
			return corridor->getPathCount();
		return 0;
	 }

	EXPORT_API bool dtpcGetData(dtPathCorridor* corridor
		, rcnPathCorridorData* result)
	{
		if (!corridor 
			|| !result 
			|| corridor->getPathCount() > MAX_RCN_PATH_CORRIDOR_SIZE)
		{
			return false;
		}

		int count = corridor->getPathCount();

		result->pathCount = count;
		dtVcopy(result->position, corridor->getPos());
		dtVcopy(result->target, corridor->getTarget());
		memcpy(&result->path[0]
			, corridor->getPath()
			, sizeof(dtPolyRef) * count);

		return true;
	}

	EXPORT_API bool dtpcIsValid(dtPathCorridor* corridor
		, const int maxLookAhead
		, dtNavMeshQuery* navquery
		, const dtQueryFilter* filter)
	{
		if (corridor)
			return corridor->isValid(maxLookAhead, navquery, filter);
		return false;
	}
}