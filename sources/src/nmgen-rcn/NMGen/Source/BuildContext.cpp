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

void nmgTransferMessages(const nmgBuildContext* context
    , unsigned char* messageBuffer
    , int messageBufferSize)
{
    if (context && context->getMessageCount() > 0)
    {
        int size = (messageBufferSize < context->getMessagePoolLength()
            ? messageBufferSize : context->getMessagePoolLength());
        memcpy(messageBuffer, context->getMessagePool(), size);
    }
}

nmgBuildContext::nmgBuildContext()
    : rcContext(false), mMessageCount(0), mTextPoolSize(0)
{
    m_logEnabled = true;
}

nmgBuildContext::~nmgBuildContext()
{
}

void nmgBuildContext::doResetLog()
{
    mMessageCount = 0;
    mTextPoolSize = 0;
}

void nmgBuildContext::doLog(const rcLogCategory category
    , const char* message
    , const int messageLength)
{
    // Design Note: The category is ignored.

    // Process early exits.
    if (!getLogEnabled()
        ||  messageLength == 0
        || mMessageCount >= MAX_MESSAGES)
        return;
    int remainingSpace = MESSAGE_POOL_SIZE - mTextPoolSize;
    if (remainingSpace < 1)
	    return;

    // Store message
    const int realLength = rcMin(messageLength+1, remainingSpace-1);
    char* pEntry = &mTextPool[mTextPoolSize];
    memcpy(pEntry, message, realLength);
    pEntry[realLength-1] = '\0';
    mTextPoolSize += realLength;
    mMessages[mMessageCount++] = pEntry;
}

int nmgBuildContext::getMessageCount() const
{
    return mMessageCount;
}

const char* nmgBuildContext::getMessage(const int i) const
{
    return mMessages[i];
}

int nmgBuildContext::getMessagePoolLength() const { return mTextPoolSize; }
const char* nmgBuildContext::getMessagePool() const { return mTextPool; }

extern "C"
{
    EXPORT_API nmgBuildContext* nmbcAllocateContext(bool logEnabled)
    {
        nmgBuildContext* context = new nmgBuildContext();
        context->enableLog(logEnabled);
        return context;
    }

    EXPORT_API void nmbcFreeContext(nmgBuildContext* context)
    {
        if (context)
            delete context;
    }

    EXPORT_API void nmbcEnableLog(nmgBuildContext* context, bool state)
    {
        if (context)
            context->enableLog(state);
    }

    EXPORT_API bool nmbcGetLogEnabled(nmgBuildContext* context)
    {
        if (context)
            return context->getLogEnabled();
        return false;
    }

    EXPORT_API void nmbcResetLog(nmgBuildContext* context)
    {
        if (context)
            context->resetLog();
    }

    EXPORT_API int nmbcGetMessageCount(const nmgBuildContext* context)
    {
        if (!context)
            return 0;

        return context->getMessageCount();
    }

    EXPORT_API int nmbcGetMessagePool(nmgBuildContext* context
        , unsigned char* messageBuffer
        , const int bufferSize)
    {
        if (!context)
            return 0;
        nmgTransferMessages(context, messageBuffer, bufferSize);
        return context->getMessageCount();
    }

    EXPORT_API void nmbcLog(nmgBuildContext* context
        , const char* message)
    {
        if (context && message)
            context->log(RC_LOG_PROGRESS, message);
    }
}