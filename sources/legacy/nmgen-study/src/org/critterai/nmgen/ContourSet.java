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

import java.util.ArrayList;

/**
 * Represents a set of related contours (simple polygons)
 * within a bounded field.
 * <p>The contours may be connected (share edges), but are expected
 * to not intersect.</p>
 * @see Contour
 */
public final class ContourSet
    extends BoundedField
{
    
    /*
     * Design notes:
     * 
     * Not adding the ability to remove contours until it is needed.
     * 
     * Recast Reference: rcContourSet in Recast.h
     */
    
    private final ArrayList<Contour> mContours;
    
    /**
     * Constructor
     * @param gridBoundsMin The minimum bounds of the field in the form
     * (minX, minY, minZ).
     * @param gridBoundsMax The maximum bounds of the field in the form
     * (maxX, maxY, maxZ).
     * @param cellSize The size of the cells.  (The grid that forms the
     * base of the field.)
     * @param cellHeight The height increment of the field.
     * @param initialSize  The initial size of the set.  Effects performance
     * and memory consumption.  The actual size will dynamically resize
     * as needed.
     */
    ContourSet(float[] gridBoundsMin
            , float[] gridBoundsMax
            , float cellSize
            , float cellHeight
            , int initialSize)
    {
        super(gridBoundsMin, gridBoundsMax, cellSize, cellHeight);
        mContours = new ArrayList<Contour>(initialSize);
    }
    
    /**
     * Add a contour to the set.
     * <p>Behavior is undefined if the contour argument is null.</p>
     * @param contour The contour to add to the set.
     */
    public void add(Contour contour) { mContours.add(contour);}
    
    /**
     * Gets the contour specified by the index.
     * @param index The index of the contour to retrieve.
     * @return The contour for the specified index, or null if the index
     * is invalid.
     */
    public Contour get(int index)
    {
        if (index < 0 || index >= mContours.size())
            return null;
        return mContours.get(index);
    }
    
    /**
     * The number of contours in the set.
     * @return The number of contours in the set.
     */
    public int size() {  return mContours.size(); }
    
}