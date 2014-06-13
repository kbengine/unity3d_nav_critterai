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
#include "DetourNavMeshEx.h"
#include "DetourCommon.h"

extern "C"
{
    EXPORT_API void dtnmGetParams(const dtNavMesh* pNavMesh
        , dtNavMeshParams* params)
    {
        if (!pNavMesh)
            return;

        const dtNavMeshParams* lparams = pNavMesh->getParams();

        params->maxPolys = lparams->maxPolys;
        params->maxTiles = lparams->maxTiles;
        params->tileHeight = lparams->tileHeight;
        params->tileWidth = lparams->tileWidth;

        dtVcopy(params->orig, lparams->orig);
    }

    EXPORT_API dtStatus dtnmAddTile(dtNavMesh* navMesh
        , rcnTileData* tileData
        , dtTileRef lastRef
        , dtTileRef* resultRef)
    {
        if (!navMesh 
            || !tileData 
            || !tileData->data
            || tileData->dataSize < 1
            || tileData->isOwned)
            return DT_FAILURE + DT_INVALID_PARAM;

        dtStatus status = navMesh->addTile(tileData->data
            , tileData->dataSize
            , DT_TILE_FREE_DATA
            , lastRef
            , resultRef);

        if (dtStatusSucceed(status))
            tileData->isOwned = true;

        return status;
    }

    EXPORT_API dtStatus dtnmRemoveTile(dtNavMesh* navMesh
        , dtTileRef ref
		, unsigned char** data
		, int* dataSize)
    {
		unsigned char* tData = 0;
		int tDataSize = 0;

		if (!navMesh)
			return DT_FAILURE + DT_INVALID_PARAM;
		
		dtStatus status = navMesh->removeTile(ref, &tData, &tDataSize);

		if (data)
			*data = tData;
		if (dataSize)
			*dataSize = tDataSize;

		if (dtStatusFailed(status))
			return status;

		if (!data && tData)
		{
			// Data was returned, but the caller doesn't want it.
			// Need to free the memory.
			dtFree(tData);
			tData = 0;
		}

		return status;
    }

    EXPORT_API void dtnmCalcTileLoc(const dtNavMesh* navMesh
        , const float* pos, int* tx, int* ty)
    {
        if (navMesh)
            navMesh->calcTileLoc(pos, tx, ty);
    }

    EXPORT_API const dtMeshTile* dtnmGetTileAt(const dtNavMesh* navmesh
        , int x
        , int y
        , int layer)
    {
        if (!navmesh)
            return 0;
        return navmesh->getTileAt(x, y, layer);
    }

    EXPORT_API int dtnmGetTilesAt(const dtNavMesh* navmesh
        , int x
        , int y
        , const dtMeshTile** tiles
        , const int tilesSize)
    {
        if (navmesh)
            return navmesh->getTilesAt(x, y, tiles, tilesSize);
        return 0;
    }

    EXPORT_API dtTileRef dtnmGetTileRefAt(const dtNavMesh* navMesh
        , int x
        , int y
        , int layer)
    {
        if (navMesh)
            return navMesh->getTileRefAt(x, y, layer);
        return 0;
    }

    EXPORT_API dtTileRef dtnmGetTileRef(const dtNavMesh* navmesh
        , const dtMeshTile* tile)
    {
        if (navmesh)
            return navmesh->getTileRef(tile);
        return 0;
    }

    EXPORT_API const dtMeshTile* dtnmGetTileByRef(const dtNavMesh* navmesh
        , dtTileRef ref)
    {
        if (navmesh)
            return navmesh->getTileByRef(ref);
        return 0;
    }

    EXPORT_API int dtnmGetMaxTiles(const dtNavMesh* pNavMesh)
    {
        if (!pNavMesh)
            return -1;

        return pNavMesh->getMaxTiles();
    }

    EXPORT_API const dtMeshTile* dtnmGetTile(const dtNavMesh* navmesh
        , int index)
    {
        if (navmesh)
            return navmesh->getTile(index);
        return 0;
    }

    EXPORT_API dtStatus dtnmGetTileAndPolyByRef(const dtNavMesh* navmesh
        , const dtPolyRef ref
        , const dtMeshTile** tile
        , const dtPoly** poly)
    {
        if (navmesh)
            return navmesh->getTileAndPolyByRef(ref, tile, poly);
        return (DT_FAILURE + DT_INVALID_PARAM);
    }

    EXPORT_API bool dtnmIsValidPolyRef(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef)
    {
        if (!pNavMesh)
            return false;

        return pNavMesh->isValidPolyRef(polyRef);
    }

    EXPORT_API dtStatus dtnmGetConnectionEndPoints(
        const dtNavMesh* pNavMesh
        , const dtPolyRef prevRef
        , const dtPolyRef polyRef
        , float* startPos
        , float* endPos)
    {
        if (!pNavMesh)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getOffMeshConnectionPolyEndPoints(
            prevRef
            , polyRef
            , startPos
            , endPos);
    }

    EXPORT_API const dtOffMeshConnection* dtnmGetOffMeshConnectionByRef(
        const dtNavMesh* navmesh
        , dtPolyRef ref)
    {
        if (navmesh)
            return navmesh->getOffMeshConnectionByRef(ref);
        return 0;
    }

    EXPORT_API dtStatus dtnmGetPolyFlags(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned short* flags)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getPolyFlags(polyRef, flags);
    }

    EXPORT_API dtStatus dtnmSetPolyFlags(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned short flags)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->setPolyFlags(polyRef, flags);
    }

    EXPORT_API dtStatus dtnmGetPolyArea(const dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char* area)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->getPolyArea(polyRef, area);
    }

    EXPORT_API dtStatus dtnmSetPolyArea(dtNavMesh* pNavMesh
        , const dtPolyRef polyRef
        , unsigned char area)
    {
        if (!pNavMesh || !polyRef)
            return (DT_FAILURE | DT_INVALID_PARAM);

        return pNavMesh->setPolyArea(polyRef, area);
    }

     EXPORT_API int dtnmGetTileStateSize(const dtNavMesh* navmesh
        , const dtMeshTile* tile)
    {
        if (navmesh)
            return navmesh->getTileStateSize(tile);
        return 0;
    }

    EXPORT_API dtStatus dtnmStoreTileState(const dtNavMesh* navmesh
        , const dtMeshTile* tile
        , unsigned char* data
        , const int maxDataSize)
    {
        if (navmesh)
            return navmesh->storeTileState(tile, data, maxDataSize);
        return (DT_FAILURE + DT_INVALID_PARAM);
    }

    EXPORT_API dtStatus dtnmRestoreTileState(dtNavMesh* navmesh
        , dtMeshTile* tile
        , const unsigned char* data
        , const int maxDataSize)
    {
        if (navmesh)
            return navmesh->restoreTileState(tile, data, maxDataSize);
        return (DT_FAILURE + DT_INVALID_PARAM);
    }

    EXPORT_API const dtMeshHeader* dtnmGetTileHeader(const dtMeshTile* tile)
    {
        return tile->header;
    }

    EXPORT_API dtPolyRef dtnmGetPolyRefBase(const dtNavMesh* navmesh
        , const dtMeshTile* tile)
    {
        if (navmesh)
            return navmesh->getPolyRefBase(tile);
        return 0;
    }

    EXPORT_API int dtnmGetTileVerts(const dtMeshTile* tile
        , float* verts
        , const int vertsCount)
    {
        if (!tile
            || !verts
            || !tile->header
            || !tile->dataSize
            || vertsCount < tile->header->vertCount)
        {
            return 0;
        }

        int count = tile->header->vertCount;

        if (count > 0)
            memcpy(verts, tile->verts, sizeof(float) * count * 3);

        return count;
    }

    EXPORT_API int dtnmGetTilePolys(const dtMeshTile* tile
        , dtPoly* polys
        , const int polysSize)
    {
        if (!tile 
            || !polys 
            || !tile->header
            || !tile->dataSize
            || polysSize < tile->header->polyCount)
        {
            return 0;
        }

        int count = tile->header->polyCount;

        if (count > 0)
            memcpy(polys, tile->polys, sizeof(dtPoly) * count);

        return count;
    }

    EXPORT_API int dtnmGetTileDetailVerts(const dtMeshTile* tile
        , float* verts
        , const int vertsCount)
    {
        if (!tile
            || !verts
            || !tile->header
            || !tile->dataSize
            || vertsCount < tile->header->detailVertCount)
        {
            return 0;
        }

        int count = tile->header->detailVertCount;

        if (count > 0)
            memcpy(verts, tile->detailVerts, sizeof(float) * count * 3);

		return count;
    }

    EXPORT_API int dtnmGetTileDetailTris(const dtMeshTile* tile
        , unsigned char* tris
        , const int trisSize)
    {
        if (!tile
            || !tris
            || !tile->header
            || !tile->dataSize
            || trisSize < tile->header->detailTriCount * 4)
        {
            return 0;
        }

        int count = tile->header->detailTriCount;

        if (count > 0)
            memcpy(tris, tile->detailTris, sizeof(unsigned char) * count * 4);

        return count;
    }

    EXPORT_API int dtnmGetTileDetailMeshes(const dtMeshTile* tile
        , dtPolyDetail* detailMeshes
        , const int meshesSize)
    {
        if (!tile 
            || !detailMeshes 
            || !tile->header
            || !tile->dataSize
            || meshesSize < tile->header->detailMeshCount)
        {
            return 0;
        }

        int count = tile->header->detailMeshCount;

        if (count > 0)
            memcpy(detailMeshes, tile->detailMeshes, sizeof(dtPolyDetail) * count);

        return count;
    }

    EXPORT_API int dtnmGetTileLinks(const dtMeshTile* tile
        , dtLink* links
        , const int linksSize)
    {
        if (!tile 
            || !links
            || !tile->header
            || !tile->dataSize
            || linksSize < tile->header->maxLinkCount)
        {
            return 0;
        }

        int count = tile->header->maxLinkCount;

        if (count > 0)
            memcpy(links, tile->links, sizeof(dtLink) * count);

        return count;
    }

    EXPORT_API int dtnmGetTileBVTree(const dtMeshTile* tile
        , dtBVNode* nodes
        , const int nodesSize)
    {
        if (!tile 
            || !nodes
            || !tile->header
            || !tile->dataSize
            || nodesSize < tile->header->bvNodeCount)
        {
            return 0;
        }

        int count = tile->header->bvNodeCount;

        if (count > 0)
            memcpy(nodes, tile->bvTree, sizeof(dtBVNode) * count);

        return count;
    }

    EXPORT_API int dtnmGetTileConnections(const dtMeshTile* tile
        , dtOffMeshConnection* conns
        , const int connsSize)
    {
        if (!tile 
            || !conns
            || !tile->header
            || !tile->dataSize
            || connsSize < tile->header->offMeshConCount)
        {
            return 0;
        }

        int count = tile->header->offMeshConCount;

        if (count > 0)
        {
            memcpy(conns
                , tile->offMeshCons
                , sizeof(dtOffMeshConnection) * count);
        }

        return count;
    }
}