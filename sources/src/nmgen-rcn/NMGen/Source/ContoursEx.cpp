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
    EXPORT_API bool nmcsBuildSet(nmgBuildContext* ctx
        , rcCompactHeightfield* chf
        , const float maxError
        , const int maxEdgeLen
        , rcContourSet* cset
        , const int flags)
    {
        if (!ctx || !chf || !cset)
            return false;

        return rcBuildContours(ctx
            , *chf
            , maxError
            , maxEdgeLen
            , *cset
            , flags);
    }

    EXPORT_API void nmcsFreeSetData(rcContourSet* cset)
    {
        if (!cset || !cset->conts)
            return;

	    for (int i = 0; i < cset->nconts; ++i)
	    {
		    rcFree(cset->conts[i].verts);
		    rcFree(cset->conts[i].rverts);
	    }

	    rcFree(cset->conts);

        cset->ch = 0;
        cset->borderSize = 0;
        cset->conts = 0;
        cset->cs = 0;
        cset->height = 0;
        cset->nconts = 0;
        cset->width = 0;

        memset(&cset->bmin, 0, sizeof(float) * 6);
    }

    EXPORT_API bool nmcsGetContour(const rcContourSet* cset
        , const int index
        , rcContour* result)
    {
        if (!cset
            || !result
            || index < 0 
            || index >= cset->nconts)
            return false;

        memcpy(result, &cset->conts[index], sizeof(rcContour));

        return true;
    }
}