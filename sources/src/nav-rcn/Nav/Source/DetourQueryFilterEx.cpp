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
#include "DetourNavMeshQuery.h"
#include "DetourEx.h"

extern "C"
{
    EXPORT_API dtQueryFilter* dtqfAlloc()
    {
        return new dtQueryFilter();
    }
    
    EXPORT_API void dtqfFree(dtQueryFilter* filter)
    {
        if (filter)
             filter->~dtQueryFilter();
    }
    
    EXPORT_API void dtqfSetAreaCost(dtQueryFilter* filter
        , const int index
        , const float cost)
    {
        filter->setAreaCost(index, cost);
    }

    EXPORT_API float dtqfGetAreaCost(dtQueryFilter* filter
        , const int index)
    {
        return filter->getAreaCost(index);
    }
    
    EXPORT_API void dtqfSetIncludeFlags(dtQueryFilter* filter
        , const unsigned short flags)
    {
        filter->setIncludeFlags(flags);
    }
    
    EXPORT_API unsigned short dtqfGetIncludeFlags(dtQueryFilter* filter)
    {
        return filter->getIncludeFlags();
    }

    EXPORT_API void dtqfSetExcludeFlags(dtQueryFilter* filter
        , const unsigned short flags)
    {
        filter->setExcludeFlags(flags);
    }

    EXPORT_API unsigned short dtqfGetExcludeFlags(dtQueryFilter* filter)
    {
        return filter->getExcludeFlags();
    }
}