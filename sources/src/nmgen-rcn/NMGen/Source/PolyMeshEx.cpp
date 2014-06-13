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

// Used for versioning related to serialization.
static const long NMG_POLYMESH_VERSION = 1;

struct nmgPolyMeshHeader
{
    // Same layout as non-pointer fields in rcPolyMesh except last field.
	int nverts;
	int npolys;
	int maxpolys;
	int nvp;
	float bmin[3], bmax[3];
	float cs, ch;
	int borderSize;
    int maxverts;
    float walkableHeight;
    float walkableRadius;
    float walkableStep;
    long version;
};

int getMaxVerts(rcPolyMesh& mesh)
{
    int maxIndex = 0;
    for (int i = 0; i < mesh.npolys; i++)
    {
        int p = i * 2 * mesh.nvp;
        for (int j = 0; j < mesh.nvp; j++)
        {
            int index = mesh.polys[p + j];
            if (index == RC_MESH_NULL_IDX)
                break;
            maxIndex = rcMax(index, maxIndex);
        }
    }

    return maxIndex + 1;
}

extern "C"
{
    // Not meant to be used with externally allocated meshes.
    EXPORT_API bool rcpmFreeMeshData(rcPolyMesh* mesh)
    {
        // Dev Note: Expect that the structure was allocated externally.
        // So only free the fields expected to have been allocated internally.

        if (!mesh)
            return false;

        rcFree(mesh->polys);
        rcFree(mesh->verts);
        rcFree(mesh->regs);
        rcFree(mesh->areas);
        rcFree(mesh->flags);

        mesh->polys = 0;
        mesh->verts = 0;
        mesh->regs = 0;
        mesh->areas = 0;
        mesh->flags = 0;
        mesh->borderSize = 0;
        mesh->ch = 0;
        mesh->cs = 0;
        mesh->flags = 0;
        mesh->maxpolys = 0;
        mesh->npolys = 0;
        mesh->nverts = 0;
        mesh->nvp = 0;
        
        memset(&mesh->bmin[0], 0, sizeof(float) * 6);

        return true;
    }

    EXPORT_API bool rcpmGetSerializedData(const rcPolyMesh* mesh
        , const int maxVerts
        , const float walkableHeight
        , const float walkableRadius
        , const float walkableStep
        , const bool includeBuffer
        , unsigned char** resultData
        , int* dataSize)
    {
        if (!mesh 
            || !resultData 
            || !dataSize  
            || mesh->maxpolys == 0)
        {
            return false;
        }

        int cap = sizeof(int) + sizeof(float) * 2 + sizeof(long);

        nmgPolyMeshHeader header;
        header.version = NMG_POLYMESH_VERSION;
        memcpy(&header
            , &mesh->nverts
            , sizeof(nmgPolyMeshHeader) - cap);

        int polyCount = (includeBuffer ? mesh->maxpolys : mesh->npolys);
        int vertCount = (includeBuffer ? maxVerts : mesh->nverts);

        header.maxpolys = polyCount;
        header.maxverts = vertCount;
        header.walkableHeight = walkableHeight;
        header.walkableRadius = walkableRadius;
        header.walkableStep = walkableStep;

        int headerSize = sizeof(nmgPolyMeshHeader);
        int vertSize = sizeof(unsigned short) * (vertCount * 3);
        int polySize = sizeof(unsigned short) * (polyCount * 2 * mesh->nvp);
        int regionFlagSize = sizeof(unsigned short) * polyCount;
        int areaSize = sizeof(unsigned char) * polyCount;

        int totalDataSize = headerSize + vertSize + polySize
            + 2 * regionFlagSize + areaSize;

        unsigned char* data = 
            (unsigned char*)rcAlloc(totalDataSize, RC_ALLOC_PERM);

        if (!data)
            return false;
        
        int pos = 0;
        memcpy(&data[pos], &header, headerSize);
        pos += headerSize;

        memcpy(&data[pos], mesh->verts, vertSize);
        pos += vertSize;

        memcpy(&data[pos], mesh->polys, polySize);
        pos += polySize;

        memcpy(&data[pos], mesh->regs, regionFlagSize);
        pos += regionFlagSize;

        memcpy(&data[pos], mesh->flags, regionFlagSize);
        pos += regionFlagSize;

        memcpy(&data[pos], mesh->areas, areaSize);

        *resultData = data;
        *dataSize = totalDataSize;

        return true;
    }

    EXPORT_API bool rcpmBuildSerializedData(const unsigned char* meshData
        , const int dataSize
        , rcPolyMesh* resultMesh
        , int* maxVerts
        , float* walkableHeight
        , float* walkableRadius
        , float* walkableStep)
    {
        int headerSize = sizeof(nmgPolyMeshHeader);

        if (!meshData 
            || !resultMesh
            || resultMesh->polys // Buffers should not be allocated.
            || dataSize < headerSize
            || !maxVerts
            || !walkableStep
            || !walkableRadius
            || !walkableHeight)
            return false;

        nmgPolyMeshHeader header;

        memcpy(&header, meshData, headerSize);

        if (header.version != NMG_POLYMESH_VERSION)
            return false;

        int vertSize = sizeof(unsigned short) * (header.maxverts * 3);
        int polySize = 
            sizeof(unsigned short) * (header.maxpolys * 2 * header.nvp);
        int regionFlagSize = sizeof(unsigned short) * header.maxpolys;
        int areaSize = sizeof(unsigned char) * header.maxpolys;

        int totalDataSize = headerSize + vertSize + polySize
            + 2 * regionFlagSize + areaSize;

        if (dataSize < totalDataSize)
            return false;

        resultMesh->verts = (unsigned short*)rcAlloc(vertSize, RC_ALLOC_PERM);
        if (!resultMesh->verts)
        {
           rcpmFreeMeshData(resultMesh);
           return false;
        }

        resultMesh->polys = (unsigned short*)rcAlloc(polySize, RC_ALLOC_PERM);
        if (!resultMesh->polys)
        {
           rcpmFreeMeshData(resultMesh);
           return false;
        }

        resultMesh->regs = 
            (unsigned short*)rcAlloc(regionFlagSize, RC_ALLOC_PERM);
        if (!resultMesh->regs)
        {
            rcpmFreeMeshData(resultMesh);
            return false;
        }

        resultMesh->flags = 
            (unsigned short*)rcAlloc(regionFlagSize, RC_ALLOC_PERM);
        if (!resultMesh->flags)
        {
            rcpmFreeMeshData(resultMesh);
            return false;
        }

        resultMesh->areas = (unsigned char*)rcAlloc(areaSize, RC_ALLOC_PERM);
        if (!resultMesh->areas)
        {
            rcpmFreeMeshData(resultMesh);
            return false;
        }

        // Populate the mesh.

        int cap = sizeof(int) + sizeof(float) * 2 + sizeof(long);
        memcpy(&resultMesh->nverts, &header, headerSize - cap);

        int pos = headerSize;
        memcpy(resultMesh->verts, &meshData[pos], vertSize);
        pos += vertSize;

        memcpy(resultMesh->polys, &meshData[pos], polySize);
        pos += polySize;

        memcpy(resultMesh->regs, &meshData[pos], regionFlagSize);
        pos += regionFlagSize;

        memcpy(resultMesh->flags, &meshData[pos], regionFlagSize);
        pos += regionFlagSize;

        memcpy(resultMesh->areas, &meshData[pos], areaSize);

        *maxVerts = header.maxverts;
        *walkableHeight = header.walkableHeight;
        *walkableRadius = header.walkableRadius;
        *walkableStep = header.walkableStep;

        return true;
    }

    EXPORT_API bool rcpmBuildFromContourSet(nmgBuildContext* ctx
        , rcContourSet* cset
        , const int nvp
        , rcPolyMesh* mesh
        , int* maxVerts)
    {
        if (!ctx || !cset || !mesh || !maxVerts)
            return false;

        if (!rcBuildPolyMesh(ctx, *cset, nvp, *mesh))
            return false;

        *maxVerts = getMaxVerts(*mesh);

        return true;
    }

    EXPORT_API bool rcmpMergePolyMeshes(nmgBuildContext* ctx
        , rcPolyMesh* meshes
        , const int nmeshes
        , rcPolyMesh* mesh
        , int* maxVerts)
    {
        if (!ctx || !meshes || !mesh || !maxVerts)
            return false;

        rcPolyMesh** m = (rcPolyMesh**)
            rcAlloc(sizeof(rcPolyMesh*) * nmeshes, RC_ALLOC_PERM);

        for (int i = 0; i < nmeshes; i++)
        {
            m[i] = &meshes[i];
        }

        bool result = rcMergePolyMeshes(ctx, m, nmeshes, *mesh);

        rcFree(m);

        if (!result)
            return false;

        *maxVerts = getMaxVerts(*mesh);

        return true;
    }
}