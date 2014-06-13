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
 * Represents the open space above a solid span within the cell column of
 * a heightfield.
 * <p><a href=
 * "http://www.critterai.org/projects/nmgen/images/hf_07_openfield.png"
 * target="_blank">
 * <img alt="" height="449" src=
 * "http://www.critterai.org/projects/nmgen/images/hf_07_openfield.jpg"
 * width="620" />
 * </a></p>
 * <p></p>
 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro"
 * target="_parent">Introduction to Heightfields</a>
 * @see HeightSpan
 */
public final class OpenHeightSpan
{
    /*
     * Design Notes:
     * 
     * Structurally this class is so similar with HeightSpan that it can
     * extend HeightSpan.  But it is being kept separate for clarity.
     * As an independent class it can use the floor and ceiling nomenclature
     * that makes reading code much easier.
     * 
     */
    
    /**
     * A value representing a span in the null region.  Spans in the null
     * are considered not traversable.
     * <p>Spans in the null-region are often skipped during processing.
     * Other processing is only applied when the null-region is involved.</p>
     */
    public static final int NULL_REGION = 0;
    
    /**
     * Temporary flags associated with the span.
     * <p>The value is meaningless outside the operation in which the flags
     * are needed. Since various operations may use this flag for their own
     * purpose, always reset the flag after use.</p>
     * <p>The contract is that an operation expects the flag to be zero when
     * it receives the span data.</p>
     */
    public int flags = 0;
    
    private int mRegionID = 0;
    private int mDistanceToRegionCore = 0;
    private int mDistanceToBorder = 0;
    
    private int mFloor;
    private int mHeight;
    
    private OpenHeightSpan mNext = null;
    private OpenHeightSpan mNeighborConnection0 = null;
    private OpenHeightSpan mNeighborConnection1 = null;
    private OpenHeightSpan mNeighborConnection2 = null;
    private OpenHeightSpan mNeighborConnection3 = null;
    
    /**
     * Constructor
     * @param floor The base height of the span.
     * @param height The height of the unobstructed space above the floor.
     * {@link Integer#MAX_VALUE} is generally used to indicate no obstructions
     * exist above the floor.
     * @throws IllegalArgumentException If the floor is below zero or the
     * height is less than 1.
     */
    public OpenHeightSpan(int floor, int height)
        throws IllegalArgumentException
    {
        if (floor < 0)
            throw new IllegalArgumentException("Floor is less than zero.");
        if (height < 1)
            throw new IllegalArgumentException("Height is less than one.");
        this.mFloor = floor;
        this.mHeight = height;
    }
    
    /**
     * The height of the ceiling.
     * @return The height of the ceiling.
     */
    public int ceiling() { return mFloor + mHeight; }
    
    /**
     * The distance this span is from the nearest border of the heightfield
     * it belongs to.
     * @return The distance this span is from the nearest heightfield border.
     */
    public int distanceToBorder() { return mDistanceToBorder; }
    
    /**
     * The distance this span is from the core of the heightfield region
     * it belongs to.
     * @return The distance this span is from the core of the heightfield
     * region it belongs to.
     */
    public int distanceToRegionCore() { return mDistanceToRegionCore; }
    
    /**
     * The base height of the span.
     * @return The base height of the span.
     */
    public int floor() { return mFloor; }
    
    /**
     * Populates an array with information on the regions a span's
     * 8-neighbors are assigned to.
     * <p>If necessary, both of a diagonal neighbor's associated
     * axis-neighbors will be used to detect the diagonal neighbor.</p>
     * <p>Special case: Since, diagonal neighbors are detected through
     * axis-neighbors, if the span has no axis-neighbors in the
     * direction of the diagonal-neighbor, then the diagonal-neighbor
     * will not be detected.</p>
     * <p>Neighbor order:</br>
     * 0 - 3 : Standard axis-neighbor order.
     *         (E.g. Starting at standard zero direction.)</br>
     * 4 - 7 : Standard diagonal neighbors.
     *         (E.g. Clockwise of associated axis-neighbor.)</br>
     * So the standard diagonal neigbor of an axis-neighbor can be
     * found at "axis-neighbor index + 4".
     * </p>
     * @param out  An array of at least size 8.
     * @see <a href="http://critterai.org/nmgen_hfintro#nsearch"
     * target="_blank">Neighbor Searches</a>
     */
    public void getDetailedRegionMap(int[] out, int insertIndex)
    {
        for (int i = 0; i < 8; i++)
            out[insertIndex+i] = NULL_REGION;
        OpenHeightSpan nSpan = null;
        OpenHeightSpan nnSpan = null;
        for (int dir = 0; dir < 4; dir++)
        {
            nSpan = getNeighbor(dir);
            if (nSpan != null)
            {
                out[insertIndex+dir] = nSpan.regionID();
                nnSpan = nSpan.getNeighbor((dir+1) & 0x3);
                if (nnSpan != null)
                    out[insertIndex+dir+4] = nnSpan.regionID();
                nnSpan = nSpan.getNeighbor((dir+3) & 0x3);
                if (nnSpan != null)
                    out[insertIndex+((dir+3)&0x3)+4] = nnSpan.regionID();
            }
        }
    }
    
    /**
     * Gets a reference to the span that is considered an axis-neighbor to
     * this span for the specified direction.  Uses the standard direction
     * indices (0 through 3) where zero is the neighbor offset at (-1, 0)
     * and the search proceeds clockwise.
     * @param direction  The direction to search.
     * @return A reference to the axis-neighbor in the specified direction.
     * Or null if there is no neighbor in the direction or the direction
     * index is invalid.
     * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch"
     * target="_parent">Neighbor Searches</a>
     */
    public OpenHeightSpan getNeighbor(int direction)
    {
        switch (direction)
        {
        case 0: return mNeighborConnection0;
        case 1: return mNeighborConnection1;
        case 2: return mNeighborConnection2;
        case 3: return mNeighborConnection3;
        default: return null;
        }
    }
    
    /**
     * The height of the unobstructed space above the floor.
     * <p>{@link Integer#MAX_VALUE} is generally used to indicate no
     * obstructions exist above the floor.</p>
     * @return The height of the unobstructed space above the floor.
     */
    public int height() { return mHeight; }
    
    /**
     * The next span higher in the span's heightfield column.
     * <p>The space between this span's ceiling and the next span's floor is
     * considered to be obstructed space.</p>
     * @return The next higher span in the span's heightfield column.
     * Or null if there is no heigher span.
     */
    public OpenHeightSpan next() { return mNext; }
    
    /**
     * The heightfield region this span belongs to.
     * <p>This value will never be less than {@link #NULL_REGION}
     * for a finished, properly constructed heightfield.</p>
     * <p>For a partially constructed heightfield the contract
     * is that any region ID less than or equal to {@link #NULL_REGION}
     * belongs to the null region.</p>
     * @return The heightfield region this span belongs to.
     */
    public int regionID() { return mRegionID; }
    
    /**
     * Set the distance this span is from the nearest border of the
     * heightfield it belongs to.
     * @param value The new distance.  Auto-clamped at a minimum of zero.
     */
    public void setDistanceToBorder(int value)
    {
        mDistanceToBorder = Math.max(value, 0);
    }
    
    /**
     * Set the distance this span is from the core of the heightfield region
     * it belongs to.
     * @param value The new distance.  Auto-clamped at a minimum of zero.
     */
    public void setDistanceToRegionCore(int value)
    {
        mDistanceToRegionCore = Math.max(value, 0);
    }
    
    /**
     * Sets the specified span at the neighbor of the current span.
     * <p>Uses the standard direction indices (0 through 3) where
     * Zero is the neighbor offset at (-1, 0) and the search proceeds
     * clockwise.</p>
     * @param direction The direction of the neighbor.
     * @param neighbor The neighbor of this span.
     * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch"
     * target="_parent">Neighbor Searches</a>
     */
    public void setNeighbor(int direction, OpenHeightSpan neighbor)
    {
        switch (direction)
        {
        case 0: mNeighborConnection0 = neighbor; break;
        case 1: mNeighborConnection1 = neighbor; break;
        case 2: mNeighborConnection2 = neighbor; break;
        case 3: mNeighborConnection3 = neighbor; break;
        }
    }
    
    /**
     * Set the next heigher span in the span's heightfield column.
     * @param value The new value.  null is an acceptable value.
     */
    public void setNext(OpenHeightSpan value) { mNext = value; }
    
    /**
     * The heightfield region this span belongs to.
     * <p>See {@link #regionID()} for important contract information.</p>
     * @param value The new value.
     */
    public void setRegionID(int value) { mRegionID = value; }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public String toString()
    {
        return "Floor: " + mFloor
        + ", Ceiling: " + mHeight
        + ", Region: " + mRegionID;
    }
    
}
