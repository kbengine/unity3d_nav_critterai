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
 * Defines an axis aligned bounding box containing a grid based field.
 * <p>There are no enforced usage models for this class.  But within this
 * package the following standards apply:</p>
 * <ul>
 * <li>The base (width/depth) of the field is defined by a grid of cells
 * with a size of {@link #cellSize()}.</li>
 * <li>Width of the field is associated with the x-axis and depth with
 * the z-axis.  So the width and depth of the field lies on the xz-plane.</p>
 * <li>The position of a cell is defined by an aggregate width/depth index
 * obtained from  {@link #gridIndex(int, int) gridIndex()}.</li>
 * <li>The grid indices originate at the minimum bounds of the field.</li>
 * <li>The height increment within each grid location is defined by the
 * value of {@link #cellHeight()}
 * and also originates at the field's minimum bounds.</li>
 * <li>Sizing of the bounds and the values of cell size/height will be
 * such that the bounds will not contain partial cells.</li>
 * </ul>
 * <p>Warning: Behavior is undefined if the minimum bounds is set to be
 * greater than the maximum bounds.</p>
 * @see <a href="http://www.critterai.org/nmgen_hfintro"
 * target="_parent">Introduction to Height Fields</a>
 */
public class BoundedField
{

    /*
     * Design notes:
     * 
     * In most cases, performance trumps object safety.  For example, the
     * bounds getters return a reference to the internal bounds arrays in
     * order to save on object creation costs.
     * 
     * Setters are protected rather than public in order to allow implementing
     * classes to control mutability.
     * 
     * Recast Reference: None
     */
    
    private int mWidth;
    private int mDepth;
    private final float[] mBoundsMin = new float[3];
    private final float[] mBoundsMax = new float[3];
    private float mCellSize;
    private float mCellHeight;
    
    /**
     * Constructor - Default
     * <p>The bounds of the field will default to min(0, 0, 0) and
     * max(1, 1, 1).<p>
     * <p>The cell size and height will default to 0.1.</p>
     */
    public BoundedField()
    {
        resetBounds();
        resetCellInfo();  // Must call this after the bounds call.
    }
    
    /**
     * Constructor - Partial
     * <p>The bounds of the field will default to min(0, 0, 0) and
     * max(1, 1, 1).</p>
     * <p>The cell size and height values are auto-clamped to
     * {@link Float#MIN_VALUE}.</p>
     * @param cellSize The size of the cells.  (The grid that forms the
     * base of the field.)
     * @param cellHeight The height increment of the field.
     */
    public BoundedField(float cellSize, float cellHeight)
    {
        mCellSize = Math.max(cellSize, Float.MIN_VALUE);
        mCellHeight = Math.max(cellHeight, Float.MIN_VALUE);
        calculateWidthDepth();
    }
    
    /**
     * Constructor - Full
     * <p>The cell size and height values are auto-clamped to
     * {@link Float#MIN_VALUE}.</p>
     * <p>Warning: Behavior is undefined if the minimum bounds is set to
     * be greater  than the maximum bounds.</p>
     * @param gridBoundsMin The minimum bounds of the field in the form
     * (minX, minY, minZ).
     * @param gridBoundsMax The maximum bounds of the field in the form
     * (maxX, maxY, maxZ).
     * @param cellSize The size of the cells.  (The grid that forms the
     * base of the field.)
     * @param cellHeight The height increment of the field.
     * @throws IllegalArgumentException If the bounds are null or the
     * wrong size.
     */
    public BoundedField(float[] gridBoundsMin
                    , float[] gridBoundsMax
                    , float cellSize
                    , float cellHeight)
        throws IllegalArgumentException
    {
        if (gridBoundsMax == null
                || gridBoundsMin == null
                || gridBoundsMax.length != 3
                || gridBoundsMin.length != 3)
            throw new IllegalArgumentException(
                            "One or both bounds are invalid.");
        
        System.arraycopy(gridBoundsMin, 0, mBoundsMin, 0, 3);
        System.arraycopy(gridBoundsMax, 0, mBoundsMax, 0, 3);
        
        mCellSize = Math.max(cellSize, Float.MIN_VALUE);
        mCellHeight = Math.max(cellHeight, Float.MIN_VALUE);
        
        calculateWidthDepth();
    }
    
    /**
     * The maximum bounds of the field in world units.
     * <p>Warning: A reference to the internal array is being returned,
     * not a new array.</p>
     * <p>Form: (maxX, maxY, maxZ)</p>
     */
    public final float[] boundsMax() { return mBoundsMax; }
    
    /**
     * The minimum bounds of the field in world units.
     * <p>Warning: A reference to the internal array is being returned,
     * not a new array.</p>
     * <p>Form: (minX, minY, minZ)</p>
     * <p>Considered the origin of the field.<p>
     */
    public final float[] boundsMin() {  return mBoundsMin; }
    
    /**
     * The height increment of the field.
     */
    public final float cellHeight() { return mCellHeight; }
    
    /**
     * The size of the cells.  (The grid that forms the base of the field.)
     */
    public final float cellSize() { return mCellSize; }
    
    /**
     * Depth of the field in voxels.
     * <p>The maximum depth index for the field is equal to(depth - 1).
     */
    public final int depth() { return mDepth; }
    
    /**
     * Indicates whether or not the provided index values represent a
     * valid cell location within
     * the field.
     * @param widthIndex The width index.
     * @param depthIndex The depth index.
     * @return TRUE if the width and depth indices are valid for the field.
     * Otherwise FALSE.
     */
    public final boolean isInBounds(int widthIndex, int depthIndex)
    {
        return (widthIndex >= 0
                && depthIndex >= 0
                && widthIndex < mWidth
                && depthIndex < mDepth);
    }
    
    /**
     * Indicates whether or not the provided bounds overlaps the bounds of
     * the current field.
     * <p>All tests are inclusive.  So if there is an edge match, then the
     * bounds overlap.</p>
     * @param boundsMin The minimum bounds of the field to test in the
     * form (minX, minY, minZ).
     * @param boundsMax The maximum bounds of the field to test in the
     * form (maxX, maxY, maxZ).
     * @return TRUE if the provided bounds overlaps the bounds of the
     * current field.  Otherwise FALSE.
     */
    public final boolean overlaps(float[] boundsMin, float[] boundsMax)
    {
        boolean overlaps = true;
        if (boundsMin == null
                        || boundsMax == null
                        || boundsMin.length != 3
                        || boundsMax.length != 3)
            return false;
        // Keep the value of TRUE unless a non-overlap condition is found.
        overlaps = (mBoundsMin[0] > boundsMax[0]
                           || mBoundsMax[0] < boundsMin[0]) ? false : overlaps;
        overlaps = (mBoundsMin[1] > boundsMax[1]
                           || mBoundsMax[1] < boundsMin[1]) ? false : overlaps;
        overlaps = (mBoundsMin[2] > boundsMax[2]
                           || mBoundsMax[2] < boundsMin[2]) ? false : overlaps;
        return overlaps;
    }
    
    /**
     * Width of the field in voxels.
     * <p>The maximum width index for the field is equal to (width - 1).<p>
     */
    public final int width() { return mWidth; }
    

    /**
     * Generates a standardized grid index suitable for use in flattened
     * storage arrays.
     * <p>Results in depth adjacent storage.  (Cells at depth 0, Cells at
     * depth 1, etc.)</p>
     * @param widthIndex The width index.
     * @param depthIndex The depth index.
     * @return A standardized grid index for the cell identified by the
     * width and depth
     * indices.  If the width and depth combination is invalid for the
     * field, a value of -1 will be returned.
     */
    protected final int gridIndex(int widthIndex, int depthIndex)
    {
        /*
         * Design notes:
         * 
         * It is not uncommon during iteration processes, especially
         * processes which are iterating on the edge of the bounds, that the
         * width and depth indices may go out of range. While there would be
         * a slight performance gain by leaving out the below argument
         * validation, and require that callers be responsible for passing
         * good values, experience indicates that this causes hard to find
         * bugs.  So instead, the validation is being performed here for
         * all calls.
         * 
         * Compare this algorithm to flattened vertex storage.
         * This only involves different naming conventions.
         * vertPointer = vertIndex * 3 + offset.
         * Where: 3 is the number of values per vertex (the dimension)
         * and 0 <= offset < vertex dimensions.
         */
        if (widthIndex < 0
                        || depthIndex < 0
                        || widthIndex >= mWidth
                        || depthIndex >= mDepth)
            return -1;
        return  widthIndex * mDepth + depthIndex ;
    }
    
    /**
     * Resets the bounds to min(0, 0, 0) and max(1, 1, 1).
     */
    protected final void resetBounds()
    {
        mBoundsMin[0] = 0;
        mBoundsMin[1] = 0;
        mBoundsMin[2] = 0;
        mBoundsMax[0] = 1;
        mBoundsMax[1] = 1;
        mBoundsMax[2] = 1;
        calculateWidthDepth();
    }

    /**
     * Reset the cell size and height values to 0.1.
     */
    protected final void resetCellInfo()
    {
        mCellSize = 0.1f;
        mCellHeight = 0.1f;
        calculateWidthDepth();
    }

    /**
     * Sets the bounds of the field.
     * <p>Warning: Behavior is undefined if the minimum bounds is greater
     * than the maximum bounds.</p>
     * @param xmin The x-value for the minimum bounds.
     * @param ymin The y-value for the minimum bounds.
     * @param zmin The z-value for the minimum bounds.
     * @param xmax The x-value for the maximum bounds.
     * @param ymax The y-value for the maximum bounds.
     * @param zmax The z-value for the maximum bounds.
     */
    protected final void setBounds(float xmin, float ymin, float zmin
                    , float xmax, float ymax, float zmax)
    {
        mBoundsMin[0] = xmin;
        mBoundsMin[1] = ymin;
        mBoundsMin[2] = zmin;
        mBoundsMax[0] = xmax;
        mBoundsMax[1] = ymax;
        mBoundsMax[2] = zmax;
        calculateWidthDepth();
    }

    /**
     * Sets the bounds of the field.
     * <p>Null values and arrays of length other than 3 are ignored.</p>
     * <p>Warning: Behavior is undefined if the minimum bounds is greater
     * than the maximum bounds.</p>
     * @param min The minimum bounds in the form (minX, minY, minZ).
     * @param max The maximum bounds in the form (maxX, maxY, maxZ).
     */
    protected final void setBounds(float[] min, float[] max)
    {
        if (min == null || max == null || min.length != 3 || max.length != 3)
            return;
        System.arraycopy(min, 0, mBoundsMin, 0, 3);
        System.arraycopy(max, 0, mBoundsMax, 0, 3);
        calculateWidthDepth();
    }
    
    /**
     * Set the maximum bounds of the field.
     * <p>Null values and arrays of length other than 3 are ignored.</p>
     * <p>Warning: Behavior is undefined if the new maximum bounds is less
     * than the current minimum bounds.</p>
     * @param value The maximum bounds in the form (maxX, maxY, maxZ).
     */
    protected final void setBoundsMax(float[] value)
    {
        if (value == null || value.length != 3)
            return;
        System.arraycopy(value, 0, mBoundsMax, 0, 3);;
        calculateWidthDepth();
    }

    /**
     * Set the minimum bounds of the field.
     * <p>Null values and arrays of length other than 3 are ignored.</p>
     * <p>Warning: Behavior is undefined if the new minimum bounds is
     * greater than the current maximum bounds.</p>
     * @param value The minimum bounds in the form (minX, minY, minZ).
     */
    protected final void setBoundsMin(float[] value)
    {
        if (value == null || value.length != 3)
            return;
        System.arraycopy(value, 0, mBoundsMin, 0, 3);
        calculateWidthDepth();
    }

    /**
     * Set the cell height.
     * <p>The cell height value is clamped to {@link Float#MIN_VALUE}.</p>
     * @param value The new cell height.
     */
    protected final void setCellHeight(float value)
    {
        mCellHeight = Math.max(value, Float.MIN_VALUE);
    }

    /**
     * Set the cell size.
     * <p>The cell size value is clamped to {@link Float#MIN_VALUE}.</p>
     * @param value The new cell size.
     */
    protected final void setCellSize(float value)
    {
        mCellSize = Math.max(value, Float.MIN_VALUE);
        calculateWidthDepth();
    }
    
    /**
     * Sets the width and depth fields based on the current
     * bounds and cell size.
     */
    private void calculateWidthDepth()
    {
        mWidth = (int)((mBoundsMax[0] - mBoundsMin[0]) / mCellSize + 0.5f);
        mDepth = (int)((mBoundsMax[2] - mBoundsMin[2]) / mCellSize + 0.5f);
    }
    
    /**
     * When used in conjunction with {@link #getDirOffsetWidth(int)}, gets a
     * standard axis-neighbor direction offset that can be used for
     * searching adjacent grid locations.
     * <p>The combined offset will be in the clockwise direction for
     * direction 0 to 3, starting at (-1, 0).</p>
     * <p>For example, if a direction value of 3 is passed to both this
     * operation and {@link #getDirOffsetWidth(int)},
     * then the combined width/depth offset will be (0, -1).</p>
     * @param dir A value representing the direction to get the offset for.
     * The value will be automatically constrained to a valid value between
     * 0 and 3 inclusive using wrapping.  E.g. A value of 4 will be
     * automatically wrapped to 0, a value of 9 will be automatically
     * wrapped to 1, etc.
     * @return The standard offset for the provided direction.
     * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch"
     * target="_parent">Neighbor Searches</a>
     */
    public static int getDirOffsetDepth(int dir)
    {
        final int offset[] = { 0, 1, 0, -1 };
        return offset[dir&0x03];
    }

    /**
     * When used in conjunction with {@link #getDirOffsetDepth(int)}, gets a
     * standard axis-neighbor direction offset that can be used for
     * searching adjacent grid locations within the field.
     * <p>The combined offset will be in the clockwise direction for
     * direction 0 to 3, starting at (-1, 0).</p>
     * <p>For example, if a direction value of 3 is passed to both this
     * operation and {@link #getDirOffsetDepth(int)}, then the combined
     * width/depth offset will be (0, -1).</p>
     * @param dir A value representing the direction to get the offset for.
     * The value will be automatically constrained to a valid value between
     * 0 and 3 inclusive, using wrapping.  E.g. A value of 4 will be
     * automatically wrapped to 0, a value of 9 will be automatically wrapped
     * to 1, etc.
     * @return The standard offset for the provided direction.
     * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch"
     * target="_parent">Neighbor Searches</a>
     */
    public static int getDirOffsetWidth(int dir)
    {
        final int offset[] = { -1, 0, 1, 0 };
        // All bits above 3 are discarded, constraining argument to 0 - 3;
        return offset[dir&0x03];
    }
    
}
