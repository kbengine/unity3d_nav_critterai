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
static const long NMG_POLYMESHDETAIL_VERSION = 1;

struct nmgPolyMeshDetailHeader
{
	int nmeshes;
	int nverts;
	int ntris;	
    int maxmeshes;
    int maxverts;
    int maxtris;
    long version;
};

struct nmgPolyMeshDetail
    : rcPolyMeshDetail
{
    int maxmeshes;
    int maxverts;
    int maxtris;
    unsigned char resourcetype;
};

// Iterates an array of vertices and copies the unique vertices to
// another array.
// vertCount - The number of vertices in sourceVerts.
// sourceVerts - The source vertices in the form (x, y, z).  Length equals
// vertCount * 3.
// resultVerts - An initialized array to load unique vertices into.  
// Values will be in the form (x, y, z).  It must be the same length as 
// sourceVerts.
// indicesMap - An initialized array of length vertCount which will hold
// the map of indices from sourceVerts to resultVerts.  E.g. If the value
// at index 5 is 2, then sourceVerts[5*3] is located at resultVerts[2*3].
// Returns: The number of unique vertices found.
// Notes:
//    If there are no duplicate vertices, the content of the source and
//    result arrays will be identical and vertCount will equal 
//    resultVertCount.
int removeDuplicateVerts(int vertCount
    , const float* sourceVerts 
    , float* resultVerts
    , int* indicesMap)
{
    int resultCount = 0;

    for (int i = 0; i < vertCount; i++)
    {
        int index = resultCount;
        int pi = i*3;
        // Check to see if this vertex has already been seen.
        for (int j = 0; j < resultCount; j++)
        {
            int pj = j*3;
            if (nmgSloppyEquals(sourceVerts[pi+0], resultVerts[pj+0])
                && nmgSloppyEquals(sourceVerts[pi+1], resultVerts[pj+1])
                && nmgSloppyEquals(sourceVerts[pi+2], resultVerts[pj+2]))
            {
                // This vertex already exists.
                index = j;
                break;
            }
        }
        indicesMap[i] = index;
        if (index == resultCount)
        {
            // This is a new vertex.
            resultVerts[resultCount*3+0] = sourceVerts[pi+0];
            resultVerts[resultCount*3+1] = sourceVerts[pi+1];
            resultVerts[resultCount*3+2] = sourceVerts[pi+2];
            resultCount++;
        }
    }

    return resultCount;

};

extern "C"
{
    // Won't free managed local resources.
    EXPORT_API bool rcpdFreeMeshData(nmgPolyMeshDetail* mesh)
    {
        // Dev Note: Expect that the structure was allocated externally.
        // So only free the fields expected to have been allocated internally.

        if (mesh && mesh->resourcetype == NMG_ALLOC_TYPE_LOCAL)
        {
            rcFree(mesh->meshes);
            rcFree(mesh->verts);
            rcFree(mesh->tris);
            return true;
        }

        return false;
    }

    EXPORT_API bool rcpdGetSerializedData(const nmgPolyMeshDetail* mesh
        , bool includeBuffer
        , unsigned char** resultData
        , int* dataSize)
    {
        if (!mesh 
            || !resultData 
            || !dataSize  
            || mesh->maxmeshes == 0)
        {
            return false;
        }

        nmgPolyMeshDetailHeader header;
        header.version = NMG_POLYMESHDETAIL_VERSION;
        memcpy(&header
            , &mesh->nmeshes
            , sizeof(nmgPolyMeshDetailHeader) - sizeof(long));

        int meshCount = (includeBuffer ? mesh->maxmeshes : mesh->nmeshes);
        int vertCount = (includeBuffer ? mesh->maxverts : mesh->nverts);
        int triCount = (includeBuffer ? mesh->maxtris : mesh->ntris);

        header.maxmeshes = meshCount;
        header.maxverts = vertCount;
        header.maxtris = triCount;

        int headerSize = sizeof(nmgPolyMeshDetailHeader);
        int vertSize = sizeof(float) * (vertCount * 3);
        int meshSize = sizeof(unsigned int) * (meshCount * 4);
        int trisSize = sizeof(unsigned char) * (triCount * 4);

        int totalDataSize = headerSize + vertSize + meshSize + trisSize;

        unsigned char* data = 
            (unsigned char*)rcAlloc(totalDataSize, RC_ALLOC_PERM);

        if (!data)
            return false;
        
        int pos = 0;
        memcpy(&data[pos], &header, headerSize);
        pos += headerSize;

        memcpy(&data[pos], mesh->meshes, meshSize);
        pos += meshSize;

        memcpy(&data[pos], mesh->tris, trisSize);
        pos += trisSize;

        memcpy(&data[pos], mesh->verts, vertSize);
        pos += vertSize;

        *resultData = data;
        *dataSize = totalDataSize;

        return true;
    }

    EXPORT_API bool rcpdBuildFromMeshData(const unsigned char* meshData
        , const int dataSize
        , nmgPolyMeshDetail* resultMesh)
    {
        int headerSize = sizeof(nmgPolyMeshDetailHeader);

        if (!meshData 
            || !resultMesh
            || resultMesh->maxmeshes // Buffers should not be allocated.
            || dataSize < headerSize)
            return false;

        nmgPolyMeshDetailHeader header;

        memcpy(&header, meshData, headerSize);

        if (header.version != NMG_POLYMESHDETAIL_VERSION)
            return false;

        int vertSize = sizeof(float) * (header.maxverts * 3);
        int meshSize = sizeof(unsigned int) * (header.maxmeshes * 4);
        int trisSize = sizeof(unsigned char) * (header.maxtris * 4);

        int totalDataSize = headerSize + vertSize + meshSize + trisSize;

        if (dataSize < totalDataSize)
            return false;

        // This needs to be set early or the error handling won't work.
        resultMesh->resourcetype = NMG_ALLOC_TYPE_LOCAL;

        resultMesh->meshes = (unsigned int*)rcAlloc(meshSize, RC_ALLOC_PERM);
        if (!resultMesh->meshes)
        {
           rcpdFreeMeshData(resultMesh);
           return false;
        }

        resultMesh->tris = (unsigned char*)rcAlloc(trisSize, RC_ALLOC_PERM);
        if (!resultMesh->tris)
        {
           rcpdFreeMeshData(resultMesh);
           return false;
        }

        resultMesh->verts = (float*)rcAlloc(vertSize, RC_ALLOC_PERM);
        if (!resultMesh->verts)
        {
           rcpdFreeMeshData(resultMesh);
           return false;
        }

        // Populate the mesh.

        memcpy(&resultMesh->nmeshes, &header, headerSize - sizeof(long));

        int pos = headerSize;
        memcpy(resultMesh->meshes, &meshData[pos], meshSize);
        pos += meshSize;

        memcpy(resultMesh->tris, &meshData[pos], trisSize);
        pos += trisSize;

        memcpy(resultMesh->verts, &meshData[pos], vertSize);

        return true;
    }

    EXPORT_API bool rcpdFlattenMesh(rcPolyMeshDetail* detailMesh
        , float* verts
        , int* vertCount
        , int vertsSize
        , int* tris
        , int* triCount
        , int trisSize)
    {
        /*
         * Remember: The detailMesh->tris array has a stride of 4
         * (3 indices + flags)
         *
         * The detail meshes are completely independent, which results
         * in duplicate verts.  The flattening process will remove
         * the duplicates.
         */

        if (!detailMesh 
            || !verts 
            || !vertCount 
            || !tris 
            || !triCount
            || trisSize < detailMesh->ntris)
        {
            return false;
        }
        
        float* uniqueVerts = new float[(detailMesh->nverts)*3];
        int* vertMap = new int[detailMesh->nverts];

        int resultVertCount = removeDuplicateVerts(detailMesh->nverts
            , detailMesh->verts
            , uniqueVerts
            , vertMap);

        if (vertsSize < resultVertCount)
            return false;

        memcpy(verts, &uniqueVerts[0], sizeof(float) * resultVertCount * 3);

        delete [] uniqueVerts;

        // Flatten and re-map the indices.
        int pCurrentTri = 0;
        for (int iMesh = 0; iMesh < detailMesh->nmeshes; iMesh++)
        {
            int vBase = detailMesh->meshes[iMesh*4+0];
            int tBase = detailMesh->meshes[iMesh*4+2];
            int tCount =  detailMesh->meshes[iMesh*4+3];
            for (int iTri = 0; iTri < tCount; iTri++)
            {
                const unsigned char* tri = &detailMesh->tris[(tBase+iTri)*4];
                for (int i = 0; i < 3; i++)
                {
                    tris[pCurrentTri] = vertMap[vBase+tri[i]];
                    pCurrentTri++;
                }
            }
        }

        delete [] vertMap;

        *vertCount = resultVertCount;
        *triCount = detailMesh->ntris;

        return true;
    }

    EXPORT_API bool rcpdBuildPolyMeshDetail(nmgBuildContext* ctx
        , const rcPolyMesh* mesh
        , const rcCompactHeightfield* chf
        , const float sampleDist
        , const float sampleMaxError
        , nmgPolyMeshDetail* dmesh)
    {
        if (!ctx || !mesh || !chf || !dmesh)
            return false;

        if (rcBuildPolyMeshDetail(ctx
            , *mesh
            , *chf
            , sampleDist
            , sampleMaxError
            , *dmesh))
        {
            dmesh->maxverts = dmesh->nverts;
            dmesh->maxtris = dmesh->ntris;
            dmesh->maxmeshes = dmesh->nmeshes;
            dmesh->resourcetype = NMG_ALLOC_TYPE_LOCAL;
            return true;
        }

        return false;
    }
}