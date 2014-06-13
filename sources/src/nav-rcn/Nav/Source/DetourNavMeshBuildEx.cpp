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
#include "DetourNavMeshBuilder.h"
#include "DetourCommon.h"
#include "DetourNavMeshEx.h"

const long RCN_NAVMESH_VERSION = 1;

struct rcnNavMeshCreateParams
    : dtNavMeshCreateParams
{
    // Design note: This exists for marshalling
    // purposes only.  Associated with the TileBuildData class
    // on the other side of the interop boundary.
    bool isDisposed;
    int maxPolyVerts;
    int maxPolys;
    int maxDetailVerts;
    int maxDetailTris;
    int maxConns;
};

struct rcnNavMeshSetHeader
{
    long version;
    int tileCount;
    dtNavMeshParams params;
};

struct rcnNavMeshTileHeader
{
	dtTileRef tileRef;
	int dataSize;
};

extern "C"
{

    EXPORT_API bool dtnmBuildTileData(rcnNavMeshCreateParams* params
        , rcnTileData* resultData)
    {
        if (!params 
            || !resultData 
            || resultData->data) // Already has data in it.  Not allowed.
        {
            return false;
        }
        resultData->isOwned = false;
        return dtCreateNavMeshData((rcnNavMeshCreateParams*)params
            , &resultData->data
            , &resultData->dataSize);
    }

    EXPORT_API bool dtnmBuildTileDataRaw(unsigned char* data
        , int dataSize
        , rcnTileData* resultData)
    {
        if (!data 
            || !resultData
            || resultData->data) // Already has data in it.  Not allowed.
        {
            return false;
        }

        resultData->data = (unsigned char*)dtAlloc(
            sizeof(unsigned char)*dataSize, DT_ALLOC_PERM);

        if (!resultData->data)
            return false;

        memcpy(resultData->data, data, dataSize);
        resultData->dataSize = dataSize;
        resultData->isOwned = false;
        
        return true;
    }

    EXPORT_API void dtnmFreeTileData(rcnTileData* tileData)
    {
        if (!tileData || !tileData->data || tileData->isOwned)
            return;

        dtFree(tileData->data);
        tileData->data = 0;
        tileData->dataSize = 0;
    }

    EXPORT_API dtStatus dtnmGetTileDataHeader(const unsigned char* data
		, const int dataSize
		, dtMeshHeader* resultHeader)
    {
		if (!data || !dataSize || !resultHeader)
			return DT_FAILURE | DT_INVALID_PARAM;

		dtMeshHeader* header = (dtMeshHeader*)data;

		if (header->magic != DT_NAVMESH_MAGIC)
			return DT_FAILURE | DT_WRONG_MAGIC;
		if (header->version != DT_NAVMESH_VERSION)
			return DT_FAILURE | DT_WRONG_VERSION;

		memcpy(resultHeader, header, sizeof(dtMeshHeader));

		return DT_SUCCESS;
    }


    EXPORT_API dtStatus dtnmGetTileDataHeaderAlt(const unsigned char* data
		, const int dataSize
		, dtMeshHeader* resultHeader)
    {
		// The reason for this alternate method is Unity iOS capatibility.
		// One signature supports an IntPtr, the other a byte[] array.
		return dtnmGetTileDataHeader(data, dataSize, resultHeader);
    }

    EXPORT_API void dtnmGetNavMeshRawData(const dtNavMesh* navMesh
        , unsigned char** resultData
        , int* dataSize)
    {
        if (!navMesh || !resultData || !dataSize)
        {
            *resultData = 0;
            *dataSize = 0;
            return;
        }

        rcnNavMeshSetHeader header;
        header.version = RCN_NAVMESH_VERSION;
        header.tileCount = 0;

	    for (int i = 0; i < navMesh->getMaxTiles(); ++i)
	    {
		    const dtMeshTile* tile = navMesh->getTile(i);
		    if (!tile || !tile->header || !tile->dataSize) continue;
		    header.tileCount++;
	    }
        memcpy(&header.params, navMesh->getParams(), sizeof(dtNavMeshParams));

        int totalDataSize = 0;
        rcnNavMeshTileHeader* tileHeaders = 
            new rcnNavMeshTileHeader[header.tileCount];

	    for (int i = 0; i < navMesh->getMaxTiles(); ++i)
	    {
		    const dtMeshTile* tile = navMesh->getTile(i);
		    if (!tile || !tile->header || !tile->dataSize) continue;

            tileHeaders[i].tileRef = navMesh->getTileRef(tile);
		    tileHeaders[i].dataSize = tile->dataSize;
            totalDataSize += tile->dataSize;
	    }
        
        totalDataSize += sizeof(rcnNavMeshSetHeader);
        totalDataSize += sizeof(rcnNavMeshTileHeader) * header.tileCount;

        unsigned char* data = 
            (unsigned char*)dtAlloc(totalDataSize, DT_ALLOC_PERM);

        int pos = 0;
        int size = sizeof(rcnNavMeshSetHeader);
        memcpy(&data[pos], &header, size);
        pos += size;

        for (int i = 0; i < header.tileCount; ++i)
        {
            size = sizeof(rcnNavMeshTileHeader);
            memcpy(&data[pos], &tileHeaders[i], size);
            pos += size;

            size = tileHeaders[i].dataSize;
            const dtMeshTile* tile = 
                navMesh->getTileByRef(tileHeaders[i].tileRef);
            memcpy(&data[pos], tile->data, size);
            pos += size;
        }
        
        delete tileHeaders;
        tileHeaders = 0;

        *resultData = data;
        *dataSize = totalDataSize;
    }

    EXPORT_API void dtnmFreeBytes(unsigned char** data)
    {
        dtFree(*data);
        *data = 0;
    }

    EXPORT_API dtStatus dtnmBuildDTNavMeshFromRaw(const unsigned char* data
        , int dataSize
		, bool safeStorage
        , dtNavMesh** ppNavMesh)
    {
        if (!data || dataSize < sizeof(rcnNavMeshSetHeader) || !ppNavMesh)
            return DT_FAILURE + DT_INVALID_PARAM;

        int pos = 0;
        int size = sizeof(rcnNavMeshSetHeader);

        rcnNavMeshSetHeader header;
        memcpy(&header, data, size);
        pos += size;

        if (header.version != RCN_NAVMESH_VERSION)
        {
            *ppNavMesh = 0;
            return DT_FAILURE + DT_WRONG_VERSION;
        }

        dtNavMesh* mesh = dtAllocNavMesh();
        if (!mesh)
        {
            *ppNavMesh = 0;
            return DT_FAILURE + DT_OUT_OF_MEMORY;
        }

	    dtStatus status = mesh->init(&header.params);
	    if (dtStatusFailed(status))
	    {
		    *ppNavMesh = 0;
		    return status;
	    }

	    // Read tiles.
        bool success = true;
	    for (int i = 0; i < header.tileCount; ++i)
	    {
		    rcnNavMeshTileHeader tileHeader;
            size = sizeof(rcnNavMeshTileHeader);
		    memcpy(&tileHeader, &data[pos], size);
            pos += size;

            size = tileHeader.dataSize;
			if (!tileHeader.tileRef || !tileHeader.dataSize)
            {
                success = false;
                status = DT_FAILURE + DT_INVALID_PARAM;
			    break;
            }
    		
		    unsigned char* tileData = 
                (unsigned char*)dtAlloc(size, DT_ALLOC_PERM);
		    if (!tileData)
            {
                success = false;
                status = DT_FAILURE + DT_OUT_OF_MEMORY;
                break;
            }
            memcpy(tileData, &data[pos], size);
            pos += size;

		    status = mesh->addTile(tileData
                , size
				, (safeStorage ? DT_TILE_FREE_DATA : 0)
                , tileHeader.tileRef
                , 0);

            if (dtStatusFailed(status))
            {
                success = false;
                break;
            }
	    }

        if (!success)
        {
            dtFreeNavMesh(mesh);
            *ppNavMesh = 0;
            return status;
        }

        *ppNavMesh = mesh;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmInitTiledNavMesh(dtNavMeshParams* params
        , dtNavMesh** ppNavMesh)
    {
        if (!params)
            return DT_FAILURE + DT_INVALID_PARAM;

        dtNavMesh* pNavMesh = dtAllocNavMesh();
		if (!pNavMesh)
            return DT_FAILURE + DT_OUT_OF_MEMORY;

		dtStatus status = 
            pNavMesh->init(params);
		if (dtStatusFailed(status))
		{
            dtFreeNavMesh(pNavMesh);
            return status;
		}

        *ppNavMesh = pNavMesh;

        return DT_SUCCESS;
    }

    EXPORT_API dtStatus dtnmBuildSingleTileMesh(dtNavMeshCreateParams* params
        , dtNavMesh** ppNavMesh)
    {
        if (!params)
            return DT_FAILURE + DT_INVALID_PARAM;

		unsigned char* navData = 0;
		int navDataSize = 0;

		if (!dtCreateNavMeshData(params, &navData, &navDataSize))
            return DT_FAILURE + DT_INVALID_PARAM;
		
        dtNavMesh* pNavMesh = dtAllocNavMesh();
		if (!pNavMesh)
		{
            dtFree(navData);
            return DT_FAILURE + DT_OUT_OF_MEMORY;
		}

		dtStatus status = 
            pNavMesh->init(navData, navDataSize, DT_TILE_FREE_DATA);
		if (dtStatusFailed(status))
		{
            dtFreeNavMesh(pNavMesh);
            dtFree(navData);
            return status;
		}

        *ppNavMesh = pNavMesh;

        return DT_SUCCESS;

    }

    EXPORT_API void dtnmFreeNavMesh(dtNavMesh** pNavMesh, bool freeTiles)
    {
		if (!pNavMesh && !(*pNavMesh))
			return;

		dtNavMesh* mesh = *pNavMesh;
		const dtNavMesh* cmesh = *pNavMesh;  // Cleaner code when calling getTile().

		if (freeTiles)
		{
			unsigned char* tData = 0;
			
			for (int i = 0; i < mesh->getMaxTiles(); ++i)
			{
				const dtMeshTile* tile = cmesh->getTile(i);

				if (!tile || !tile->header || !tile->dataSize) 
					continue;

				dtTileRef tref = mesh->getTileRef(tile);

				dtStatus status = mesh->removeTile(tref, &tData, 0);

				if (dtStatusSucceed(status) && tData)
				{
					dtFree(tData);
					tData = 0;
				}
			}
		}

        dtFreeNavMesh(mesh);
    }
}