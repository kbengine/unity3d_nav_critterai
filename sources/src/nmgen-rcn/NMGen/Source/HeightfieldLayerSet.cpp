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
    EXPORT_API int nmlsBuildLayers(nmgBuildContext* ctx
        , rcCompactHeightfield* chf
        , const int borderSize
        , const int walkableHeight
        , rcHeightfieldLayerSet** resultSet)
    {
        if (!ctx || !chf)
            return -1;

        rcHeightfieldLayerSet* lset = rcAllocHeightfieldLayerSet();
        if (!lset)
            return -1;

        if (!rcBuildHeightfieldLayers(ctx
            , *chf
            , borderSize
            , walkableHeight
            , *lset))
        {
            rcFreeHeightfieldLayerSet(lset);
            return -1;
        }

        if (lset->nlayers == 0)
        {
            rcFreeHeightfieldLayerSet(lset);
            return -1;
        }

        *resultSet = lset;
        return lset->nlayers;
    }

    EXPORT_API void nmlsFreeLayers(rcHeightfieldLayerSet* lset)
    {
        rcFreeHeightfieldLayerSet(lset);
    }

    EXPORT_API bool nmlsGetLayer(rcHeightfieldLayerSet* lset
        , int index
        , rcHeightfieldLayer* layer)
    {
        if (!lset || !layer || index < 0 || index >= lset->nlayers)
            return false;

        memcpy(layer, &lset->layers[index], sizeof(rcHeightfieldLayer));

        return true;
    }

}