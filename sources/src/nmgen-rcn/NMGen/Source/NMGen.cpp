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
    EXPORT_API void nmgFreeSerializationData(unsigned char** data)
    {
        rcFree(*data);
        *data = 0;
    }

    // The only purpose for this function is to allow testing
    // the context interop.
    EXPORT_API void nmgTestContext(rcContext* ctx, const int count)
    {
        for (int i = 0; i < count; i++)
        {
            ctx->log(RC_LOG_PROGRESS, "MSG: %d", i);
        }
    }

    EXPORT_API void nmgMarkWalkableTriangles(rcContext* ctx
        , const float walkableSlopeAngle
        , const float* verts
        , int nv
        , const int* tris
        , int nt
        , unsigned char* areas)
    {
        rcMarkWalkableTriangles(ctx
            , walkableSlopeAngle
            , verts
            , nv
            , tris
            , nt
            , areas);
    }

    EXPORT_API void nmgClearUnwalkableTriangles(rcContext* ctx
        , const float walkableSlopeAngle
        , const float* verts
        , int nv
        , const int* tris
        , int nt
        , unsigned char* areas)
    {
        rcClearUnwalkableTriangles(ctx
            , walkableSlopeAngle
            , verts
            , nv
            , tris
            , nt
            , areas);
    }
}