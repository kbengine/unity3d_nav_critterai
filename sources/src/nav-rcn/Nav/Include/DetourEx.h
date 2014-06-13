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
#ifndef CAI_DETOUREX_H
#define CAI_DETOUREX_H

#include "DetourNavMesh.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

static const int MAX_RCN_PATH_CORRIDOR_SIZE = 256;

struct rcnPathCorridorData
{
    float position[3];
    float target[3];

    dtPolyRef path[MAX_RCN_PATH_CORRIDOR_SIZE];
    int pathCount;
};

struct rcnNavmeshPoint
{
	dtPolyRef polyRef;
	float point[3];
};

#endif
