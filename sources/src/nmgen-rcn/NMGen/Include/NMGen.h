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
#ifndef CAI_NMG_EX_H
#define CAI_NMG_EX_H

#include "Recast.h"

#if _MSC_VER    // TRUE for Microsoft compiler.
#define EXPORT_API __declspec(dllexport) // Required for VC++
#else
#define EXPORT_API // Otherwise don't define.
#endif

static const float NMG_EPSILON = 0.00001f;
static const float NMG_TOLERANCE = 0.0001f;

// The memory was allocated externally and cannot
// be freed by this library.
static const unsigned char NMG_ALLOC_TYPE_EXTERN = 0;

// The memory was allocated locally and can be freed locally.
static const unsigned char NMG_ALLOC_TYPE_LOCAL = 1;

// The memory was allocated locally, but it is managed by an
// owner object.  It should only be freed by that object.
static const unsigned char NMG_ALLOC_TYPE_MANAGED_LOCAL = 2;

class nmgBuildContext 
    : public rcContext
{
public:
    static const int MAX_MESSAGES = 1024;
    static const int MESSAGE_POOL_SIZE = 65536;

    nmgBuildContext();
    virtual ~nmgBuildContext();
	
    int getMessageCount() const;
    const char* getMessage(const int i) const;

    int getMessagePoolLength() const;
    const char* getMessagePool() const;

    bool getLogEnabled() const { return m_logEnabled; }

protected:
    virtual void doResetLog();
    virtual void doLog(const rcLogCategory category
        , const char* msg
        , const int len);

private:
    const char* mMessages[MAX_MESSAGES];
    int mMessageCount;

    char mTextPool[MESSAGE_POOL_SIZE];
    int mTextPoolSize;
};

template<class T> inline bool nmgSloppyEquals(T a, T b) 
{ 
    return !(b < a - NMG_TOLERANCE || b > a + NMG_TOLERANCE);
};

#endif