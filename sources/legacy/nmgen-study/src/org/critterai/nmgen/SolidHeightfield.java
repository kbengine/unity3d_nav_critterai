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

import java.util.Hashtable;
import java.util.Iterator;
import java.util.NoSuchElementException;

/**
 * Contains data that represents the obstructed (solid) area of a bounded
 * field of voxels.
 * <p>The data is stored within spans which represent a vertically contiguous
 * group of solid voxels.  A reference to the lowest span within a grid
 * location is stored at that grid location's width/depth index. Height spans
 * further up in the grid's column can be accessed via
 * {@link HeightSpan#next()} on the base span. (I.e. By climbing up the links.)
 * </p>
 * @see SolidHeightfieldBuilder
 * @see <a href="http://www.critterai.org/nmgen_voxel"
 * target="_parent">The Voxelization Process</a>
 * @see <a href="http://www.critterai.org/nmgen_hfintro"
 * target="_parent">Introduction to Height Fields</a>
 */
public final class SolidHeightfield
    extends BoundedField
{
    
    /*
     * Recast Reference: reHeightfield in Recast.h
     */
    
    /**
     * Implements an iterator that will iterate through all spans within a
     * height field. (Not just the base spans.)
     * <p>Behavior of the iterator is undefined if the interator's source
     * is changed during iteration.</p>
     */
    public class SolidHeightFieldIterator
        implements Iterator<HeightSpan>
    {
    
        private int mNextWidth = 0;
        private int mNextDepth = 0;
        private HeightSpan mNext = null;
        
        private int mLastWidth = 0;
        private int mLastDepth = 0;
        
        private SolidHeightFieldIterator()
        {
            moveToNext();
        }
        
        /**
         * The depth index of the last span returned by {@link #next()}
         * @return The depth index of the last span returned by {@link #next()}
         */
        public int depthIndex() { return mLastDepth; }
    
        /**
         * {@inheritDoc}
         */
        @Override
        public boolean hasNext()
        {
            return (mNext != null);
        }
    
        /**
         * {@inheritDoc}
         */
        @Override
        public HeightSpan next()
        {
            if (mNext == null) throw new NoSuchElementException();
            HeightSpan next = mNext;
            mLastWidth = mNextWidth;
            mLastDepth = mNextDepth;
            moveToNext();
            return next;
        }
    
        /**
         * {@inheritDoc}
         * This operation is not supported.
         */
        @Override
        public void remove()
        {
            throw new UnsupportedOperationException();
        }
    
        /**
         * Resets the iterator so that it can be re-used.
         */
        public void reset()
        {
            mNextWidth = 0;
            mNextDepth = 0;
            mNext = null;
            mLastWidth = 0;
            mLastDepth = 0;
            moveToNext();
        }
        
        /**
         * The width index of the last span returned by {@link #next()}
         * @return The width index of the last span returned by {@link #next()}
         */
        public int widthIndex() { return mLastWidth; }
        
        /**
         * Move to the next span in the data set.
         */
        private void moveToNext()
        {
            if (mNext != null)
            {
                // There is a current span selected.
                if (mNext.next() != null)
                {
                    // The current span has a next.
                    // Move to it.
                    mNext = mNext.next();
                    return;
                }
                else
                    // No more spans in the column of the current grid location.
                    // Move to next grid location.
                    mNextWidth++;
            }
            // Search through the grid until a new base span is found.
            for (int depthIndex = mNextDepth
                            ; depthIndex < depth()
                            ; depthIndex++)
            {
                for (int widthIndex = mNextWidth
                                ; widthIndex < width()
                                ; widthIndex++)
                {
                    HeightSpan span =
                        mSpans.get(gridIndex(widthIndex, depthIndex));
                    if (span != null)
                    {
                        // A new base span was found.  Select it.
                        mNext = span;
                        mNextWidth = widthIndex;
                        mNextDepth = depthIndex;
                        return;
                    }
                }
                mNextWidth = 0;
            }
            // If got here, then there are no more spans.
            mNext = null;
            mNextDepth = -1;
            mNextWidth = -1;
        }
    }
    
    /**
     * Contains the spans within the heightfield's grid.
     * <p>Key: Grid index obtained via {@link #gridIndex(int, int)}.<br/>
     * Value: The lowest span at the grid location, or null if there are no
     * spans at the grid location.</p>
     */
    private final Hashtable<Integer, HeightSpan> mSpans =
        new Hashtable<Integer, HeightSpan>();
    
    /**
     * Constructor
     * <p>The bounds of the field will default to min(0, 0, 0)
     * and max(1, 1, 1).</p>
     * @param cellSize The size of the cells.  (The grid that forms the
     * base of the field.)
     * @param cellHeight The height increment of the field.
     */
    public SolidHeightfield(float cellSize
            , float cellHeight)
    {
        super(cellSize, cellHeight);
    }
    
    /**
     * Adds span data to the heightfield.  New span data is either merged
     * into existing spans or a new span is created.
     * <p>Only the following validations are peformed:</p>
     * <ul>
     * <li>The bounds of the width and depth indices.</li>
     * <li>The lower bounds of the height indices. (>=0)</li>
     * <li>Height min <= max.</li>
     * </ul>
     * <p>No check that the height maximum is within bounds is performed.</p>
     * <p>Flags are set as follows:</p>
     * <ul>
     * <li>If the maximum of the new data coincides with the maximum of an
     * existing span, the old and new flags are merged.</li>
     * <li>If the new data represents a new maximum (new span or new maximum
     * for an existing span), the flags for the new data is used
     * exclusively.</li>
     * <li>Otherwise, the new data's flags are ignored.</li>
     * </ul>
     * <p>Basically, only the flags at the top of a span are considered
     * to matter.</p>
     * @param widthIndex The width index of the column that contains the
     * new data.
     * @param depthIndex The depth index of the column that contains the
     * new data.
     * @param heightIndexMin The solid span's minimum. The minimum of the
     * obstructed space. (In zero-based height increments based on
     * {@link #cellHeight()}.)
     * @param heightIndexMax The solid span's maximum. The maximum of the
     * obstructed space. (In zero-based height increments based on
     * {@link #cellHeight()}.)
     * @param flags  The flags for the new data.
     * @return TRUE if the data was successfully added.  Otherwise FALSE.
     * The only time this operation will fail is if the argument data is
     * invalid in some way.
     */
    public boolean addData(int widthIndex
                    , int depthIndex
                    , int heightIndexMin
                    , int heightIndexMax
                    , int flags)
    {
        if (widthIndex < 0
                        || widthIndex >= width()
                        || depthIndex < 0
                        || depthIndex >= depth())
            // Outside of grid bounds.
            return false;
        
        if (heightIndexMin < 0
                        || heightIndexMax < 0
                        || heightIndexMin > heightIndexMax)
            // Invalid height values.
            return false;
        
        // Find the grid location of the span and get existing data for the
        // location.
        int gridIndex = gridIndex(widthIndex, depthIndex);
        HeightSpan currentSpan = mSpans.get(gridIndex);
        
        if (currentSpan == null)
        {
            // This is the first span for this grid location.
            // Generate a new span.
            mSpans.put(gridIndex, new HeightSpan(heightIndexMin
                            , heightIndexMax
                            , flags));
            return true;
        }
        
        // Span data already exists at this location.  Search the spans in
        // this column to see which one should contain this span.  Or if a
        // new span should be created.
        HeightSpan previousSpan = null;
        while (currentSpan != null)
        {
            /*
             * Note: The way the spans are built, separate spans are always
             * guaranteed to have a gap between them.  The minimum gap will
             * be the cell height increment.
             */
            if (currentSpan.min() > heightIndexMax + 1)
            {
                /*
                 * The new span is below the current span and NOT adjacent.
                 * Due to the structure of the data, the new span is
                 * guaranteed to fit below the current span.
                 * 
                 * Create a new span.
                 */
                HeightSpan newSpan = new HeightSpan(heightIndexMin
                                , heightIndexMax
                                , flags);
                // Insert this span below the current span.
                newSpan.setNext(currentSpan);
                if (previousSpan == null)
                    // The new span is the new first span in this column.
                    // Insert it at the base of this column.
                    mSpans.put(gridIndex, newSpan);
                else
                    // The new span is between two spans.
                    // Link the previous span to the new span.
                    previousSpan.setNext(newSpan);
                return true;
            }
            else if (currentSpan.max() < heightIndexMin - 1)
            {
                // Current span is below the new span and NOT adjacent.
                if (currentSpan.next() == null)
                {
                    // The new span is the final span.
                    // Insert it above the current span.
                    currentSpan.setNext(new HeightSpan(heightIndexMin
                                    , heightIndexMax
                                    , flags));
                    return true;
                }
                // Continue searching up the span's in this column.
                previousSpan = currentSpan;
                currentSpan = currentSpan.next();
            }
            else
            {
                /*
                 * There is either overlap or adjacency between the current
                 * span and the new span.
                 * Need to perform a merge of some type.
                 * Will always return after the merge is complete.
                 * Get easy stuff out of the way first.
                 */
                if (heightIndexMin < currentSpan.min())
                    // This span will result in a new minimum for the current
                    // span. Adjust the current span's minimum.
                    currentSpan.setMin(heightIndexMin);
                if (heightIndexMax == currentSpan.max())
                {
                    // The new span ends at same height as current span.
                    // Merge flags.
                    currentSpan.setFlags((byte)(currentSpan.flags() | flags));
                    return true;
                }
                if (currentSpan.max() > heightIndexMax)
                    // The top of the current span is higher than the new span.
                    // So discard the new span's flag.
                    return true;
                // The new spans's maximum height is higher than the current
                // span's maximum height.
                // Need to search up the spans to find where the merge ends.
                HeightSpan nextSpan = currentSpan.next();
                while (true)
                {
                    if (nextSpan == null || nextSpan.min() > heightIndexMax + 1)
                    {
                        /*
                         * There are no spans above the current span, or the
                         * height increase caused by this span will not touch
                         * the next span. Can just expand the current span
                         * upward.
                         */
                        currentSpan.setMax(heightIndexMax);
                        // New span is new "top", so its flags replace current
                        // span's flags.
                        currentSpan.setFlags(flags);
                        if (nextSpan == null)
                            // The current span is at the top of the column.
                            // Get rid of any links it may have had.
                            currentSpan.setNext(null);
                        else
                            // Take care of re-pointing.  (Some spans may have
                            // been encompassed.)
                            currentSpan.setNext(nextSpan);
                        // Finished.
                        return true;
                    }
                    // The new height of the current span will touch the next
                    // span in some manner. Merging is needed.
                    if (nextSpan.min() == heightIndexMax + 1
                                    || heightIndexMax <= nextSpan.max())
                    {
                        // No gap between current and next spans, but no
                        // overlap with next span. (Spans abut each other.)
                        // Encompass the next span.
                        currentSpan.setMax(nextSpan.max());
                        // Set the current span to point the the encompassed
                        // span's next span.
                        currentSpan.setNext(nextSpan.next());
                        // Take the flags of the next span since we know the
                        // next span's max is higher than the current span.
                        currentSpan.setFlags(nextSpan.flags());
                        if (heightIndexMax == currentSpan.max())
                        {
                            // New span ends at same height as merged span.
                            // Merge flags.
                            currentSpan.setFlags(currentSpan.flags() | flags);
                            return true;
                        }
                        return true;
                    }
                    // The current span overlaps with the next span.
                    // Need to continue up the column to see if the next span
                    // will be fully engulfed.
                    nextSpan = nextSpan.next();
                }
            }
        }
        
        // Will only ever get here if there is a code logic error.
        return false;
        
    }

    /**
     * Provides an iterator that iterates all spans in the field.
     * <p>Unlike {@link #getData(int, int)}, this iterator will iterate
     * through all spans, not just the base spans.  So their is no need to
     * use {@link HeightSpan#next()} to climb  the span structure.</p>
     */
    public SolidHeightFieldIterator dataIterator()
    {
        return this.new SolidHeightFieldIterator();
    }

    /**
     * Gets the lowest span at the grid location, or null if there are no
     * spans at the location.
     * <p>The data will be the lowest span at the grid location.</p>
     * @return The lowest span at the grid location.
     */
    public HeightSpan getData(int widthIndex, int depthIndex)
    {
        return mSpans.get(gridIndex(widthIndex, depthIndex));
    }
    
    /**
     * Indicates whether or not the field contains any spans. If FALSE is
     * returned, then the field does not contain any obstructed  space.
     * @return TRUE if the field contains spans.  Otherwise FALSE.
     */
    public boolean hasSpans() { return (mSpans.size() > 0); }
    
}
