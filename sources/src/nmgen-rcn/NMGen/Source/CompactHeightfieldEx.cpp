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
#include "NMGen.h"
#include "RecastAlloc.h"

extern "C"
{
    EXPORT_API bool nmcfBuildField(nmgBuildContext* ctx
        , const int walkableHeight
        , const int walkableClimb
        , rcHeightfield* hf
        , rcCompactHeightfield* chf)
    {
        if (!ctx || !hf || !chf)
            return false;

        return rcBuildCompactHeightfield(ctx
            , walkableHeight
            , walkableClimb
            , *hf
            , *chf);
    }

    EXPORT_API void nmcfFreeFieldData(rcCompactHeightfield* chf)
    {
        if (chf)
        {
            rcFree(chf->cells);
	        rcFree(chf->spans);
	        rcFree(chf->dist);
	        rcFree(chf->areas);
            chf->cells = 0;
            chf->spans = 0;
            chf->dist = 0;
            chf->areas = 0;
        }
    }

    EXPORT_API bool nmcfGetCellData(rcCompactHeightfield* chf
        , rcCompactCell* cells
        , const int cellsSize)
    {
        if (!chf || !cells || cellsSize < chf->width * chf->height)
            return false;

        memcpy(cells
            , chf->cells
            , sizeof(rcCompactCell) * chf->width * chf->height);

        return true;
    }

    EXPORT_API bool nmcfGetSpanData(rcCompactHeightfield* chf
        , rcCompactSpan* spans
        , const int spansSize)
    {
        if (!chf || !spans || spansSize < chf->spanCount)
            return false;

        memcpy(spans
            , chf->spans
            , sizeof(rcCompactSpan) * chf->spanCount);

        return true;
    }

    EXPORT_API bool nmcfErodeWalkableArea(nmgBuildContext* ctx
        , const int radius
        , rcCompactHeightfield* chf)
    {
        if (ctx && chf)
            return rcErodeWalkableArea(ctx, radius, *chf);
        return false;
    }

    EXPORT_API bool nmcfMedianFilterWalkableArea(nmgBuildContext* ctx
        , rcCompactHeightfield* chf)
    {
        if (ctx && chf)
            return rcMedianFilterWalkableArea(ctx, *chf);
        return false;
    }

    EXPORT_API bool nmcfMarkBoxArea(nmgBuildContext* ctx
        , const float* bmin
        , const float* bmax
        , unsigned char areaId
        , rcCompactHeightfield* chf)
    {
        if (ctx && bmin && bmax && chf)
        {
            rcMarkBoxArea(ctx, bmin, bmax, areaId, *chf);
            return true;
        }
        return false;
    }

    EXPORT_API bool nmcfMarkConvexPolyArea(nmgBuildContext* ctx
        , const float* verts
        , const int nverts
        , const float hmin
        , const float hmax
        , unsigned char areaId
        , rcCompactHeightfield* chf)
    {
        if (ctx && verts && chf)
        {
            rcMarkConvexPolyArea(ctx, verts, nverts, hmin, hmax, areaId, *chf);
            return true;
        }
        return false;
    }

    EXPORT_API bool nmcfMarkCylinderArea(nmgBuildContext* ctx
        , const float* pos
        , const float r
        , const float h
        , unsigned char areaId
        , rcCompactHeightfield* chf)
    {
        if (ctx && pos && chf)
        {
            rcMarkCylinderArea(ctx, pos, r, h, areaId, *chf);
            return true;
        }
        return false;
    }

    EXPORT_API bool nmcfBuildDistanceField(nmgBuildContext* ctx
        , rcCompactHeightfield* chf)
    {
        if (ctx && chf)
            return rcBuildDistanceField(ctx, *chf);
        return false;
    }

    EXPORT_API bool nmcfBuildRegions(nmgBuildContext* ctx
        , rcCompactHeightfield* chf
        , const int borderSize
        , const int minRegionArea
        , const int mergeRegionArea)
    {
        if (ctx && chf)
            return rcBuildRegions(ctx
                , *chf
                , borderSize
                , minRegionArea
                , mergeRegionArea);
        return false;
    }

    EXPORT_API bool nmcfBuildRegionsMonotone(nmgBuildContext* ctx
        , rcCompactHeightfield* chf
        , const int borderSize
        , const int minRegionArea
        , const int mergeRegionArea)
    {
        if (ctx && chf)
            return rcBuildRegions(ctx
                , *chf
                , borderSize
                , minRegionArea
                , mergeRegionArea);
        return false;
    }
}