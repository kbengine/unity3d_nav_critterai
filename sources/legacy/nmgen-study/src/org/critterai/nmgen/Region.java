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
 * A utility class which holds meta data related to building regions.
 */
final class Region
{
    /**
     * The region ID.
     */
    public int id = 0;
    
    /**
     * The number of spans in this region.
     */
    public int spanCount = 0;
    
    /**
     * Used to indicate whether or not a re-mapping of the region ID is
     * required.
     */
    public boolean remap = false;
    
    /**
     * Represents an ordered list of connections between this and other regions.
     * All properly initialized regions will have at least one connection,
     * even if that connection is only to the null region.
     * <p>The start point, end point, and direction of the ordering is
     * arbitrary. If the current region connects at multiple non-adjacent
     * points t another region,  then the list will contain multiple references.
     * One entry for each non-adjacent connection point.</p>
     * <p>Example: 1, 5, 8, 1, 2 -> This region connects to region 1 at
     * two points.</p>
     * <p>The reason that multiple connection information is stored is that
     * it can be used to indicate whether two regions, if combined, would
     * result in a polygon with internal space, which is an invalid state.</p>
     */
    public final ArrayList<Integer> connections = new ArrayList<Integer>();
    
    /**
     * A list of non-null regions that overlap this region.
     * <p>An overlap is considered to have occurred
     * if a span in this region is below a span belonging to another region.</p>
     * <p>Note that if two spans in the same grid cell are in the same region,
     * then the region will show as overlapping itself.</p>
     */
    public final ArrayList<Integer> overlappingRegions =
        new ArrayList<Integer>();
    
    /**
     * Constructor
     * @param id The initial ID of the region.
     */
    public Region(int id)
    {
        this.id = id;
    }
    
    /**
     * Reinitializes (clears) all instance fields and sets the id to a
     * new value.
     * @param newRegionID The new region ID.
     */
    public void resetWithID(int newRegionID)
    {
        id = newRegionID;
        spanCount = 0;
        connections.clear();
        overlappingRegions.clear();
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String toString()
    {
        return "id: " + id + ", spans: " + spanCount
            + ", connections: " + connections + ", overlaps: "
            + overlappingRegions;
    }
    
}
