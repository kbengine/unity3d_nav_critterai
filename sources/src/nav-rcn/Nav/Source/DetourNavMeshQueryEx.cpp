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
#include <stdlib.h>
#include <string.h>
#include "DetourNavMeshQuery.h"
#include "DetourEx.h"

// Returns a random number [0..1)
static float frand()
{
	return (float)rand()/(float)RAND_MAX;
}

extern "C"
{
    EXPORT_API dtStatus dtnqBuildDTNavQuery(dtNavMesh* pNavMesh
        , const int maxNodes
        , dtNavMeshQuery** ppNavQuery)
    {
         // Initialize the build context.

        if (!pNavMesh)
            return DT_FAILURE + DT_INVALID_PARAM;
		
        dtNavMeshQuery* pNavQuery = dtAllocNavMeshQuery();
        if (!pNavQuery)
            return DT_FAILURE + DT_OUT_OF_MEMORY;

		dtStatus status = pNavQuery->init(pNavMesh, maxNodes);
		if (dtStatusFailed(status))
		{
            dtFreeNavMeshQuery(pNavQuery);
            return status;
		}
        
        *ppNavQuery = pNavQuery;

        return DT_SUCCESS;
    }

    EXPORT_API void dtnqFree(dtNavMeshQuery** pNavQuery)
    {
        dtFreeNavMeshQuery(*pNavQuery);
    }

    EXPORT_API dtStatus dtqGetPolyWallSegments(dtNavMeshQuery* query
        , dtPolyRef ref
        , const dtQueryFilter* filter
        , float* segmentVerts
        , dtPolyRef* segmentRefs
        , int* segmentCount
        , const int maxSegments)
    {
        return query->getPolyWallSegments(ref
            , filter
            , segmentVerts
            , segmentRefs
            , segmentCount
            , maxSegments);
    }

	EXPORT_API dtStatus dtqFindNearestPoly(dtNavMeshQuery* query
        , const float* center
        , const float* extents
		, const dtQueryFilter* filter
		, rcnNavmeshPoint* nearest)
    {
        return query->findNearestPoly(center
            , extents
            , filter
            , &nearest->polyRef
            , &nearest->point[0]);
    }

    EXPORT_API dtStatus dtqQueryPolygons(dtNavMeshQuery* query 
        , const float* center
        , const float* extents
		, const dtQueryFilter* filter
		, dtPolyRef* polyIds
        , int* polyCount
        , const int maxPolys)
    {
        return query->queryPolygons(center
            , extents
            , filter
            , polyIds
            , polyCount
            , maxPolys);
    }

	EXPORT_API dtStatus dtqFindPolysAroundCircle(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* centerPos
        , const float radius
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultPolyRefs
        , dtPolyRef* resultParentRefs
        , float* resultCosts
	    , int* resultCount
        , const int maxResult)
    {
        return query->findPolysAroundCircle(startRef
            , centerPos
            , radius
            , filter
            , resultPolyRefs
            , resultParentRefs
            , resultCosts
            , resultCount
            , maxResult);
    }

	EXPORT_API dtStatus dtqFindPolysAroundShape(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* verts
        , const int nverts
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultRef
        , dtPolyRef* resultParent
        , float* resultCost
	    , int* resultCount
        , const int maxResult)
    {
        return query->findPolysAroundShape(startRef
            , verts
            , nverts
            , filter
            , resultRef
            , resultParent
            , resultCost
            , resultCount
            , maxResult);
    }

	EXPORT_API dtStatus dtqFindLocalNeighbourhood(dtNavMeshQuery* query 
        , dtPolyRef startRef
        , const float* centerPos
        , const float radius
	    , const dtQueryFilter* filter
	    , dtPolyRef* resultRef
        , dtPolyRef* resultParent
	    , int* resultCount
        , const int maxResult)
    {
        return query->findLocalNeighbourhood(startRef
            , centerPos
            , radius
            , filter
            , resultRef
            , resultParent
            , resultCount
            , maxResult);
    }

    EXPORT_API dtStatus dtqClosestPointOnPoly(dtNavMeshQuery* query 
        , dtPolyRef ref
        , const float* pos
        , float* closest)
    {
        return query->closestPointOnPoly(ref, pos, closest, 0);
    }

	EXPORT_API dtStatus dtqClosestPointOnPolyBoundary(dtNavMeshQuery* query 
        , dtPolyRef ref
        , const float* pos
        , float* closest)
    {
        return query->closestPointOnPolyBoundary(ref, pos, closest);
    }

    EXPORT_API dtStatus dtqGetPolyHeight(dtNavMeshQuery* query
        , rcnNavmeshPoint pos
        , float* height)
    {
		return query->getPolyHeight(pos.polyRef, &pos.point[0], height);
    }

	EXPORT_API dtStatus dtqFindDistanceToWall(dtNavMeshQuery* query
        , rcnNavmeshPoint centerPos
        , const float maxRadius
	    , const dtQueryFilter* filter
	    , float* hitDist
        , float* hitPos
        , float* hitNormal)
    {
		return query->findDistanceToWall(centerPos.polyRef
			, &centerPos.point[0]
            , maxRadius
            , filter
            , hitDist
            , hitPos
            , hitNormal);
    }

    EXPORT_API dtStatus dtqFindPath(dtNavMeshQuery* query 
		, rcnNavmeshPoint startPos
        , rcnNavmeshPoint endPos
		, const dtQueryFilter* filter
		, dtPolyRef* path
        , int* pathCount
        , const int maxPath)
    {
		return query->findPath(startPos.polyRef
			, endPos.polyRef
			, &startPos.point[0]
			, &endPos.point[0]
            , filter
            , path
            , pathCount
            , maxPath);
    }

    EXPORT_API dtStatus dtqFindPathExt(dtNavMeshQuery* query 
		, rcnNavmeshPoint* startPos
        , rcnNavmeshPoint* endPos
        , const float* extents
		, const dtQueryFilter* filter
		, dtPolyRef* path
        , int* pathCount
        , const int maxPath)
    {
		if (startPos->polyRef == 0)
        {
			dtStatus status = query->findNearestPoly(&startPos->point[0]
                , extents
                , filter
				, &startPos->polyRef
				, &startPos->point[0]);
            if (dtStatusFailed(status))
                return status;
        }

		if (endPos->polyRef == 0)
        {
			dtStatus status = query->findNearestPoly(&endPos->point[0]
                , extents
                , filter
				, &endPos->polyRef
				, &endPos->point[0]);
            if (dtStatusFailed(status))
                return status;
        }

		if (startPos->polyRef == 0 || endPos->polyRef == 0)
            // One of the searches failed. 
            return DT_FAILURE | DT_INVALID_PARAM;

		return query->findPath(startPos->polyRef
			, endPos->polyRef
			, &startPos->point[0]
            , &endPos->point[0]
            , filter
            , path
            , pathCount
            , maxPath);
    }

	EXPORT_API bool dtqIsInClosedList(dtNavMeshQuery* query
        , dtPolyRef ref)
    {
        return query->isInClosedList(ref);
    }

    EXPORT_API bool dtqIsValidPolyRef(dtNavMeshQuery* query
        , dtPolyRef ref
        , const dtQueryFilter* filter)
    {
        return query->isValidPolyRef(ref, filter);
    }

	EXPORT_API dtStatus dtqRaycast(dtNavMeshQuery* query
        , rcnNavmeshPoint startPos
        , const float* endPos
	    , const dtQueryFilter* filter
	    , float* t
        , float* hitNormal
        , dtPolyRef* path
        , int* pathCount
        , const int maxPath)
    {
		return query->raycast(startPos.polyRef
			, &startPos.point[0]
            , endPos
            , filter
            , t
            , hitNormal
            , path
            , pathCount
            , maxPath);
    }

    EXPORT_API dtStatus dtqFindStraightPath(dtNavMeshQuery* query
        , const float* startPos
        , const float* endPos
		, const dtPolyRef* path
        , const int pathStart
        , const int pathSize
	    , float* straightPath
        , unsigned char* straightPathFlags
        , dtPolyRef* straightPathRefs
	    , int* straightPathCount
        , const int maxStraightPath)
    {
        return query->findStraightPath(startPos
            , endPos
            , &path[pathStart]
            , pathSize
            , straightPath
            , straightPathFlags
            , straightPathRefs
            , straightPathCount
            , maxStraightPath);
    }

    EXPORT_API dtStatus dtqMoveAlongSurface(dtNavMeshQuery* query
        , rcnNavmeshPoint startPos
        , const float* endPos
	    , const dtQueryFilter* filter
	    , float* resultPos
        , dtPolyRef* visited
        , int* visitedCount
        , const int maxVisitedSize)
    {
		return query->moveAlongSurface(startPos.polyRef
			, &startPos.point[0]
            , endPos
            , filter
            , resultPos
            , visited
            , visitedCount
            , maxVisitedSize);
    }

	EXPORT_API dtStatus dtqInitSlicedFindPath(dtNavMeshQuery* query
        , rcnNavmeshPoint startPos
        , rcnNavmeshPoint endPos
        , const dtQueryFilter* filter)
    {
		return query->initSlicedFindPath(startPos.polyRef
			, endPos.polyRef
			, &startPos.point[0]
			, &endPos.point[0]
            , filter);
    }

	EXPORT_API dtStatus dtqUpdateSlicedFindPath(dtNavMeshQuery* query
        , const int maxIter
        , int* doneIters)
    {
        return query->updateSlicedFindPath(maxIter, doneIters);
    }

	EXPORT_API dtStatus dtqFinalizeSlicedFindPath(dtNavMeshQuery* query
        , dtPolyRef* path
        , int* pathCount
        , const int maxPath)
    {
        return query->finalizeSlicedFindPath(path, pathCount, maxPath);
    }

	EXPORT_API dtStatus dtqFindRandomPoint(dtNavMeshQuery* query
		, const dtQueryFilter* filter
		, rcnNavmeshPoint* randomPt)
	{
		return query->findRandomPoint(filter, frand, &randomPt->polyRef, &randomPt->point[0]);
	}

	EXPORT_API dtStatus dtqFindRandomPointCircle(dtNavMeshQuery* query
		, rcnNavmeshPoint start
        , const float radius
		, const dtQueryFilter* filter
		, rcnNavmeshPoint* randomPt)
	{
		return query->findRandomPointAroundCircle(start.polyRef, &start.point[0], radius
			, filter, frand
			, &randomPt->polyRef, &randomPt->point[0]);
	}

}