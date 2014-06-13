/*
 * Copyright (c) 2010 Stephen A. Pratt
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
package org.critterai.nmgen;

/**
 * Represents a span within the cell column of a heightfield.
 * Spans represent one or more contiguous voxels.
 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro"
 * target="_parent">Introduction to Heightfields</a>
 */
public final class HeightSpan
{

    /*
     * Recast Reference: rcSpan in Recast.h
     */
    
    private int mMinimum;
    private int mMaximum;
    private int mFlags = 0;
    private HeightSpan mNext = null;
    
    /**
     * Constructor
     * @param min The minimum increment of the span.
     * (Usually the height increment.)
     * @param max The maximum increment of the span.
     * (Usually the height increment.)
     * @param flags The span flags.
     * @throws IllegalArgumentException If the minimum is greater than or
     * equal to the maximum.
     */
    public HeightSpan(int min, int max, int flags)
        throws IllegalArgumentException
    {
        if (min > max)
            throw new IllegalArgumentException(
                            "Minimum is greater than or equal to the maximum.");
        mMinimum = min;
        mMaximum = max;
        mFlags = flags;
    }
    
    /**
     * The flags for the span.
     * @return The flags for the span.
     */
    public int flags() { return mFlags; }
    
    /**
     * The span maximum.
     * @return The span maximum.
     */
    public int max() { return mMaximum; }
    
    /**
     * The span minimum.
     * @return The span minimum.
     */
    public int min() { return mMinimum; }
    
    /**
     * The next span in the column.  (Usually above the current span.)
     * @return The next span in the column.  Or null if there is no next span.
     */
    public HeightSpan next() { return mNext; }
    
    /**
     * Set the flags for the span.
     * @param value The new flags for the span.
     */
    public void setFlags(int value) { mFlags = value; }
    
    /**
     * Sets the span maximum.
     * <p>Auto-clamps the value to ({@link #min()} + 1).</p>
     * @param value The new maximum.
     */
    public void setMax(int value)
    {
        if (value <= mMinimum)
            mMaximum = mMinimum + 1;
        else
            mMaximum = value;
    }
    
    /**
     * Sets the span minimum.
     * <p>Auto-clamps the value to ({@link #max()} - 1).</p>
     * @param value The new minimum.
     */
    public void setMin(int value)
    {
        if (value >= mMaximum)
            mMinimum = mMaximum - 1;
        else
            mMinimum = value;
    }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public String toString()
    {
        return mMinimum + "->" + mMaximum + ", Flags: " + mFlags;
    }

    /**
     * Set the next span value.
     * @param value The new next span.  (null is a valid value.)
     */
    void setNext(HeightSpan value) { mNext = value; }
    
}
