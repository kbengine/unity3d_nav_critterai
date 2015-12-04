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
#include "DetourCrowd.h"
#include "DetourCommon.h"
#include "DetourEx.h"

static const int MAX_LOCAL_BOUNDARY_SEGS = 8;

struct rcnLocalBoundary
{
    float center[3];
    float segs[6 * MAX_LOCAL_BOUNDARY_SEGS];
    int segmentCount;
};

struct rcnCrowdCornerData
{
	float cornerVerts[DT_CROWDAGENT_MAX_CORNERS*3];
	unsigned char cornerFlags[DT_CROWDAGENT_MAX_CORNERS];
	dtPolyRef cornerPolys[DT_CROWDAGENT_MAX_CORNERS];
	int ncorners;
};

struct rcnCrowdAgentCoreData
{
	unsigned char state;
    dtPolyRef polyRef;
	dtPolyRef targetRef;
	dtPolyRef cornerRef;

    // Important: Keep this section in sync with the dtCrowdAgent layout.

	int nneis;

	float desiredSpeed;

	float npos[3];
	float disp[3];
	float dvel[3];
	float nvel[3];
	float vel[3];

	// End of sync section.

	float target[3];	// Corridor target.
	float corner[3];	// Next corner.
};

extern "C"
{
    EXPORT_API dtCrowd* dtcDetourCrowdAlloc(const int maxAgents
        , const float maxAgentRadius
        , dtNavMesh* nav)
    {
        dtCrowd* result = new dtCrowd();
        if (result)
            result->init(maxAgents, maxAgentRadius, nav);
        return result;
    }

    EXPORT_API void dtcDetourCrowdFree(dtCrowd* crowd)
    {
        if (crowd)
            crowd->~dtCrowd();
    }

	EXPORT_API void dtcSetObstacleAvoidanceParams(dtCrowd* crowd
        , const int idx
        , dtObstacleAvoidanceParams* params)
    {
        crowd->setObstacleAvoidanceParams(idx, params);
    }

    EXPORT_API const void dtcGetObstacleAvoidanceParams(
        dtCrowd* crowd
        , const int idx
        , dtObstacleAvoidanceParams* params)
    {
        memcpy(params
            , crowd->getObstacleAvoidanceParams(idx)
            , sizeof(dtObstacleAvoidanceParams));
    }
	
	EXPORT_API const dtCrowdAgent* dtcGetAgent(dtCrowd* crowd
        , const int idx)
    {
        return crowd->getAgent(idx);
    }

	EXPORT_API const int dtcGetAgentCount(dtCrowd* crowd)
    {
        return crowd->getAgentCount();
    }

	EXPORT_API void dtcUpdateAgentParameters(dtCrowd* crowd
        , const int idx, const dtCrowdAgentParams* params)
    {
        crowd->updateAgentParameters(idx, params);
    }

	EXPORT_API void dtcRemoveAgent(dtCrowd* crowd
        , const int idx)
    {
        crowd->removeAgent(idx);
    }
	
	EXPORT_API bool dtcRequestMoveTarget(dtCrowd* crowd
        , const int idx
		, rcnNavmeshPoint pos)
    {
		return crowd->requestMoveTarget(idx, pos.polyRef, &pos.point[0]);
    }

	EXPORT_API bool dtcAdjustMoveTarget(dtCrowd* crowd
        , const int idx
        , rcnNavmeshPoint pos)
    {
		return false;
    }
	
	EXPORT_API const dtQueryFilter* dtcGetFilter(dtCrowd* crowd)
    {
        return crowd->getFilter(0);
    }

	EXPORT_API void dtcGetQueryExtents(dtCrowd* crowd, float* extents)
    {
        const float* e = crowd->getQueryExtents();
        dtVcopy(extents, e);
    }
	
	EXPORT_API int dtcGetVelocitySampleCount(dtCrowd* crowd)
    {
        return crowd->getVelocitySampleCount();
    }
	
	EXPORT_API const dtProximityGrid* dtcGetGrid(dtCrowd* crowd)
    {
        return crowd->getGrid();
    }

    EXPORT_API const float dtpgGetCellSize(dtProximityGrid* grid)
    {
        return grid->getCellSize();
    }

    EXPORT_API void dtpgGetBounds(dtProximityGrid* grid, int* bounds)
    {
        if (bounds)
        {
            memcpy(bounds, grid->getBounds(), sizeof(int) * 6);
        }
    }

    EXPORT_API int dtpgGetItemCountAt(dtProximityGrid* grid
        , const int x
        , const int y)
    {
        return grid->getItemCountAt(x, y);
    }

	EXPORT_API const dtNavMeshQuery* dtcGetNavMeshQuery(dtCrowd* crowd)
    {
        return crowd->getNavMeshQuery();
    }

    EXPORT_API void dtcaGetAgentParams(const dtCrowdAgent* agent
        , dtCrowdAgentParams* params)
    {
        if (!agent || !params)
            return;
        memcpy(params, &agent->params, sizeof(dtCrowdAgentParams));
    }

    EXPORT_API void dtcaGetAgentCorners(const dtCrowdAgent* agent
        , rcnCrowdCornerData* resultData)
    {
        if (!agent || !resultData)
            return;
        memcpy(resultData, &agent->cornerVerts[0], sizeof(rcnCrowdCornerData));
    }

    EXPORT_API void dtcaGetAgentCoreData(const dtCrowdAgent* agent
        , rcnCrowdAgentCoreData* resultData)
    {
        // The active check is important to the design.
        if (!agent || !agent->active || !resultData)
            return;

        resultData->state = agent->state;
        resultData->polyRef = agent->corridor.getFirstPoly();
		resultData->targetRef = agent->corridor.getLastPoly();
		resultData->cornerRef = agent->cornerPolys[0];

        int size = sizeof(rcnCrowdAgentCoreData)
            - sizeof(unsigned char) - 3 * sizeof(dtPolyRef)
			- sizeof(float) * 6;

        memcpy(&resultData->nneis, &agent->nneis, size);

		dtVcopy(&resultData->target[0], agent->corridor.getTarget());
		dtVcopy(&resultData->corner[0], &agent->cornerVerts[0]);
    }

    EXPORT_API int dtcaGetAgentNeighbors(const dtCrowdAgent* agent
        , dtCrowdNeighbour* neighbors
        , const int neighborsSize)
    {
        if (!agent
            || !neighbors
            || neighborsSize < agent->nneis)
        {
            return -1;
        }

        int count = agent->nneis;

        memcpy(neighbors, agent->neis, sizeof(dtCrowdNeighbour) * count);

        return count;
    }

    EXPORT_API void dtcaGetPathCorridorData(const dtCrowdAgent* agent
        , rcnPathCorridorData* corridor)
    {
        if (!agent || !corridor)
            return;

        int count = agent->corridor.getPathCount();

        corridor->pathCount = count;
        dtVcopy(corridor->position, agent->corridor.getPos());
        dtVcopy(corridor->target, agent->corridor.getTarget());
        memcpy(&corridor->path[0]
        , agent->corridor.getPath()
            , sizeof(dtPolyRef) * count);
    }

    EXPORT_API void dtcaGetLocalBoundary(const dtCrowdAgent* agent
        , rcnLocalBoundary* boundary)
    {
        if (!agent || !boundary)
            return;

        int count = agent->boundary.getSegmentCount();

        boundary->segmentCount = count;
        dtVcopy(&boundary->center[0], agent->boundary.getCenter());

        for (int i = 0; i < count; i++)
        {
            memcpy(&boundary->segs[i*6]
                , agent->boundary.getSegment(i)
                , sizeof(float) * 6);
        }
    }

	EXPORT_API void dtcUpdate(dtCrowd* crowd
        , const float dt
        , rcnCrowdAgentCoreData* coreData)
    {
        crowd->update(dt, 0);

        for (int i = 0; i < crowd->getAgentCount(); i++)
        {
            // Note: The get function performs all necessary parameter
            // validations.
            dtcaGetAgentCoreData(crowd->getAgent(i), &coreData[i]);
        }
    }

    EXPORT_API int dtcAddAgent(dtCrowd* crowd
        , const float* pos
        , const dtCrowdAgentParams* params
        , const dtCrowdAgent** agent
        , rcnCrowdAgentCoreData* initialData)
    {
        int index = crowd->addAgent(pos, params);
        if (agent)
        {
            if (index == -1)
                *agent = 0;
            else
            {
                *agent = crowd->getAgent(index);
                dtcaGetAgentCoreData(*agent, initialData);
            }
        }
        return index;
    }
}