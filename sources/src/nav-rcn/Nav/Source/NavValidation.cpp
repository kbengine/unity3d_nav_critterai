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
#include "DetourEx.h"
#include "DetourCommon.h"

extern "C"
{
	// The purpose of these functions is to allow checking that
	// the Vector3 structure can be auto-cast by .NET interop to a 
	// float[3] pointer. The tests are needed to allow validation across 
	// various OS's and platforms.

	EXPORT_API void dtvlVectorTest(const float* vector3in, float* vector3out)
	{
		dtVcopy(vector3out, vector3in);
	}

	EXPORT_API void dtvlVectorArrayTest(const float* vector3in
		, const int vectorCount
		, float* vector3out)
	{
		for (int i = 0; i < vectorCount; i++)
		{
			dtVcopy(&vector3out[i * 3], &vector3in[i * 3]);
		}
	}
}