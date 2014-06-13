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

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Hashtable;

import org.critterai.nmgen.OpenHeightfield.OpenHeightFieldIterator;

/**
 * Builds an open heightfield from the solid data contained by an
 * {@link SolidHeightfield}.  It does this by locating and creating spans
 * representing the area above spans within the source field that have a
 * specified flag.
 * <p>Options are provided to generate neighbor, distance field, and
 * region information for the open span data</p>
 * <p>Example of a fully formed open heightfield.  (I.e. With region
 * information.) Only the floor of the spans is shown.</p>
 * <p>
 * <a
 * href="http://www.critterai.org/projects/nmgen/images/stage_regions.png"
 * target="_parent">
 * <img alt=""
 * src="http://www.critterai.org/projects/nmgen/images/stage_regions.gif"
 * style="width: 620px; height: 465px;" /></a></p>
 * @see <a href="http://www.critterai.org/?q=nmgen_hfintro"
 * target="_parent">Introduction to Height Fields</a>
 * @see <a href="http://www.critterai.org/nmgen_regiongen"
 * target="_parent">Region Generation</a>
 * @see OpenHeightfield
 * @see OpenHeightSpan
 */
public final class OpenHeightfieldBuilder
{

    /*
     * Design notes:
     * 
     * Recast references:
     *         rcBuildCompactHeightfield in Recast.cpp
     *         rcBuildDistanceField in RecastRegion.cpp
     *         rcBuildRegions in RecastRegion.cpp
     * 
     * Configuration getters won't be added until they are needed.
     * Never add setters.  Configuration should remain immutable to keep
     * the class thread friendly.
     */
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
    
    private final int mMinTraversableHeight;
    private final int mMaxTraversableStep;
    private final int mSmoothingThreshold;
    private final int mTraversableAreaBorderSize;
    private final int mFilterFlags;
    private final boolean mUseConservativeExpansion;
    
    private final ArrayList<IOpenHeightFieldAlgorithm> mRegionAlgorithms
        = new ArrayList<IOpenHeightFieldAlgorithm>();
    
    /**
     * Constructor
     * 
     * @param minTraversableHeight Represents the minimum floor to ceiling
     * height that will still allow the floor area to be considered walkable.
     * <p>Permits detection of overhangs in the geometry which make the
     * geometry below become unwalkable.</p>
     * <p>Constraints:  > 0</p>
     * 
     * @param maxTraversableStep Represents the maximum ledge height that
     * is considered to still be walkable.
     * <p>Prevents minor deviations in height from improperly showing as
     * obstructions. Permits detection of stair-like structures, curbs, etc.
     * </p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param traversableAreaBorderSize Represents the closest any part
     * of the navmesh can get to an obstruction in the source mesh.
     * <p>Usually set to the maximum bounding radius of entities utilizing
     * the navmesh for navigation decisions.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @param smoothingThreshold The amount of smoothing to be performed
     * when generating the distance field.
     * <p>This value impacts region formation and border detection.  A higher
     * value results in generally larger regions and larger border sizes.
     * A value of zero will disable smoothing.</p>
     * <p>Constraints:  0 <= value <= 4</p>
     * 
     * @param filterFlags The flags used to determine which spans from the
     * source {@link SolidHeightfield} should be used to build the
     * {@link OpenHeightfield}.  Only those spans which whose flags
     * exactly match the filter flag will be considered for inclusion in
     * the generated open field.
     * <p>Note: Spans from the source field which do not match the filter
     * flags are still taken into account as height obstructions.<p>
     * 
     * @param useConservativeExpansion Applies extra algorithms to regions
     * to help prevent poorly formed regions from forming.
     * <p>If the navigation mesh is missing sections that should be present,
     * then enabling this feature will likely fix the problem</p>
     * <p>Enabling this feature significantly increased processing cost.</p>
     * 
     * @param regionAlgorithms  A list of the algorithms to run after
     * initial region generation is complete. The algorithms will be run
     * in the order of the list.
     */
    public OpenHeightfieldBuilder(int minTraversableHeight
            , int maxTraversableStep
            , int traversableAreaBorderSize
            , int smoothingThreshold
            , int filterFlags
            , boolean useConservativeExpansion
            , ArrayList<IOpenHeightFieldAlgorithm> regionAlgorithms)
    {
        mMaxTraversableStep = Math.max(0, maxTraversableStep);
        mMinTraversableHeight = Math.max(1, minTraversableHeight);
        mTraversableAreaBorderSize = Math.max(0, traversableAreaBorderSize);
        mFilterFlags = filterFlags;
        mSmoothingThreshold = Math.min(4, Math.max(0, smoothingThreshold));
        mUseConservativeExpansion = useConservativeExpansion;
        if (regionAlgorithms != null)
            mRegionAlgorithms.addAll(regionAlgorithms);
    }
    
    /**
     * Performs a smoothing pass on the distance field data.
     * <p>This operation depends on distance field information.  So the
     * {@link #generateDistanceField(OpenHeightfield)} operation must be
     * run before this operation.</p>
     * <p>This operation does not need to be run if the
     * {@link #build(SolidHeightfield, boolean) build} operation
     * was run with performFullGeneration set to TRUE.</p>
     * @param field A populated open height field with distance field data
     * already generated.
     */
    public void blurDistanceField(OpenHeightfield field)
    {
        // TODO: DOC: Need to find source documentation.
        // The basic process is to combine a span's original distance with
        // that of its neighbors.
        
        if (field == null)
            return;
        
        // Reference: Neighbor searches and nomenclature.
        // http://www.critterai.org/?q=nmgen_hfintro#nsearch
        
        if (mSmoothingThreshold <= 0)
            // Not configured to perform smoothing.  Exit early.
            return;
        
        // TODO: EVAL: Try to optimize out this hash table.
        /*
         * Holds information on the final blurred distance for each span.
         * Key = span
         * Value = new blurred distance.
         */
        final Hashtable<OpenHeightSpan, Integer> blurResults
            = new Hashtable<OpenHeightSpan, Integer>(field.spanCount());
        
        // Loop through all spans.
        final OpenHeightFieldIterator iter = field.dataIterator();
        while (iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            final int origDist = span.distanceToBorder();
            if (origDist <= mSmoothingThreshold)
            {
                // This span is at the minimum threshold.
                // Add it to the results and continue to next span.
                blurResults.put(span, mSmoothingThreshold);
                continue;
            }
            
            int workingDist = origDist;
            // Loop through neighbors.
            for (int dir = 0; dir < 4; dir++)
            {
                OpenHeightSpan nSpan = span.getNeighbor(dir);  // axis-neighbor.
                if (nSpan == null)
                    // No neighbor on this side.  Self buff using own
                    // original distance.
                    workingDist += origDist * 2;
                else
                {
                    // Neighbor on this side.  Add its distance to the
                    // current span.
                    workingDist += nSpan.distanceToBorder();
                    // Get diagonal neighbor.
                    nSpan = nSpan.getNeighbor((dir+1) & 0x3);
                    if (nSpan == null)
                        // No diagonal neighbor.  Self buff using own
                        // original distance.
                        workingDist += origDist;
                    else
                        // Has diagonal neighbor.  Add its distance to
                        // the current span.
                        workingDist += nSpan.distanceToBorder();
                }
            }
            // Adjust and store the result.
            // Don't know the why behind this specific formula.
            blurResults.put(span, ((workingDist + 5) / 9));
        }
        
        // Replace the original distance information with the new
        // distance information.
        for (OpenHeightSpan span : blurResults.keySet())
        {
            span.setDistanceToBorder(blurResults.get(span));
        }
        
        // Reset the known min/max border distance.  This will force a
        // recalculation if the values are needed later.
        field.clearBorderDistanceBounds();
    }
    
    /**
     * Builds an {@link OpenHeightfield} from the provided
     * {@link SolidHeightfield} based on the configuration settings.
     * @param sourceField The solid field to derive the open field from.
     * @param performFullGeneration If TRUE, neighbor link, distance field
     * (including blurring),
     * and region information will be generated.  If FALSE, only the spans
     * will be generated.
     */
    public OpenHeightfield build(SolidHeightfield sourceField
            , boolean performFullGeneration)
    {
        if (sourceField == null)
            return null;
        
        // Construct the open field object.
        OpenHeightfield result = new OpenHeightfield(sourceField.boundsMin()
                , sourceField.boundsMax()
                , sourceField.cellSize()
                , sourceField.cellHeight());
        
        // Loop through all solid field grid locations.
        for (int depthIndex = 0
                ; depthIndex < sourceField.depth()
                ; depthIndex++)
        {
            for (int widthIndex = 0
                    ; widthIndex < sourceField.width()
                    ; widthIndex++)
            {
                // The lowest span in this column.
                OpenHeightSpan baseSpan = null;
                // The last span processed in this column.
                OpenHeightSpan previousSpan = null;
                // Climb up the list of spans at this grid location.
                // A loop will only occur if the grid location has at least
                // one span.
                for (HeightSpan span =
                    sourceField.getData(widthIndex, depthIndex)
                        ; span != null
                        ; span = span.next())
                {
                    // Ignore spans that do not match the filter flags.
                    if (span.flags() != mFilterFlags)
                        continue;
                    
                    /*
                     * Determine the open space between this span and the next
                     * higher span. Note that the flag of the ceiling span
                     * does not matter.  All spans matter when it comes to
                     * this step.
                     */
                    int floor = span.max();
                    int ceiling = (span.next() != null ?
                            span.next().max() : Integer.MAX_VALUE);
                    
                    // Add the span.
                    // Note that the original span flags are being discarded.
                    OpenHeightSpan oSpan = new OpenHeightSpan(floor
                            , (ceiling - floor));
                    if (baseSpan == null)
                        // This is the first span created at this grid location.
                        baseSpan = oSpan;
                    if (previousSpan != null)
                        // There is a span at a lower location in this grid
                        // location. Link the lower (previous) span to this
                        // span.
                        previousSpan.setNext(oSpan);
                    previousSpan = oSpan;
                    result.incrementSpanCount();
                }
                if (baseSpan != null)
                {
                    // There is one or more spans at this grid location.
                    // Add the first span in the chain to the spans hash.
                    result.addData(widthIndex, depthIndex, baseSpan);
                }
            }
        }
        
        if (performFullGeneration)
        {
            // Need to perform a full generation.
            generateNeighborLinks(result);
            generateDistanceField(result);
            blurDistanceField(result);
            generateRegions(result);
        }
        
        return result;

    }
    
    /**
     * Generates distance field information.
     * The {@link OpenHeightSpan#distanceToBorder()} information is generated
     * for all spans in the field.
     * <p>A boundary is a span with a missing neighbor.  It will always have
     * a distance value of zero. A span is not a boundary if is has 4 neighbors.
     * Its boundary distance value will be higher the further it is from a
     * boundary span.</p>
     * <p>All distance values are relative and do not represent explicit
     * distance values  (such as grid unit distance). The algorithm which is
     * used results in an approximation only.  It is not exhaustive.</p>
     * <p>This operation depends on neighbor information.  So the
     * {@link #generateNeighborLinks(OpenHeightfield)} operation must be
     * run before this operation.</p>
     * <p>This operation does not need to be run if the
     * {@link #build(SolidHeightfield, boolean) build} operation
     * was run with performFullGeneration set to TRUE.</p>
     * <p>The data generated by this operation is required by
     * {@link #blurDistanceField(OpenHeightfield)} and
     * {@link #generateRegions(OpenHeightfield)}</p>
     * @param field A field with spans and neighbor information already
     * generated.
     */
    public void generateDistanceField(OpenHeightfield field)
    {
        
        // TODO: DOC: Need to find source documentation for this algorithm.
        
        if (field == null)
            return;
        
        // Reference: Neighbor searches and nomenclature.
        // http://www.critterai.org/?q=nmgen_hfintro#nsearch
        
        // Enumerated values for border distance:
        
        // Represents a border span.  The value for a border span will never be
        // changed during any stage.
        final int BORDER = 0;
        
        // Represents an uninitialized non-border span.
        // All non-border spans will have this value going into pass 1.
        // No span will have this value by the end of pass 1.
        final int NEEDS_INIT = Integer.MAX_VALUE;
        
        /*
         * Initialization pass.
         * Loop through each span.
         * Set distance to BORDER for boundary spans. (Spans with less than
         * 4 known neighbors.)
         * Set distance to NEEDS_INIT for non-boundary spans.
         */
        final OpenHeightFieldIterator iter = field.dataIterator();
        while (iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            // Perform 8-neighbor search.  If any neighbor is missing, this
            // is a border.
            boolean isBorder = false;
            for (int dir = 0; dir < 4; dir++)
            {
                final OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan == null
                        || nSpan.getNeighbor(dir == 3 ? 0 : dir + 1) == null)
                {
                    // Either this axis-neighbor or the diagonal-neighbor
                    // associated with it is missing.  This is a border span.
                    isBorder = true;
                    break;
                }
            }
            if (isBorder)
                // Mark as a border span.
                span.setDistanceToBorder(BORDER);
            else
                // Marks as a non-border span that needs initialization.
                span.setDistanceToBorder(NEEDS_INIT);
        }

        /*
         * The next two phases basically check the neighbors of a span and
         * set the span's distance field to be slightly greater than the
         * neighbor with the lowest border distance. Distance is increased
         * slightly more for diagonal-neighbors than for axis-neighbors.
         * 
         * Example:
         *         Span.dist = 5 // Current estimated distance from border.
         *         Neighor1.dist = 3
         *         Neighor2.dist = 1  <- Neighbor with lowest border distance.
         *         Neighor3.dist = 2
         *         Neighor4.dist = 4
         *         Then set Span.dist = Neighbor2.dist + 2
         * 
         * See the first pass (below) for comments that detail the algorithm.
         */
        
        /*
         * Pass 1
         * During this pass, the following neighbors are checked:
         * (-1, 0) (-1, -1) (0, -1) (1, -1)
         * 
         * There is a special case during this pass since non-border spans may
         * have a value of NEEDS_INIT: If a neighbor's border distance is not
         * known, then it is treated at if it is a border span.
         * 
         * By the end of this pass, no spans will have the value NEEDS_INIT
         * since all non-border spans will have a neighbor that forces
         * its value to change.
         */
        
        // Loop through all spans.
        iter.reset();
        while (iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            int dist = span.distanceToBorder();
            if (dist == BORDER)
                // This is a border cell.  Skip it.
                continue;
            // This span is guaranteed to have 4 axis neighbors.
            // (-1, 0) Guaranteed to exist.
            OpenHeightSpan nSpan = span.getNeighbor(0);
            int ndist = nSpan.distanceToBorder();
            /*
             * At this point, dist is guaranteed to equal NEEDS_INIT.
             * (This is the first time this span has been selected for
             * change.) So the value of dist will always have its value set
             * to slightly higher than this  first neighbor.
             */
            if (ndist == NEEDS_INIT)
                // Don't know how far from border this neighbor is.
                // Default to slightly away from the border.
                dist = 1;
            else
                // Set to slightly further from the border than this neighbor.
                dist = ndist + 2;
            // (-1, -1) Diagonal. Not guaranteed to exist.
            nSpan = nSpan.getNeighbor(3);
            if (nSpan != null)
            {
                // There is a diagonal neighbor.
                ndist = nSpan.distanceToBorder();
                if (ndist == NEEDS_INIT)
                    // Don't know how far from border this neighbor is.
                    // Default to slightly away from the border.
                    ndist = 2;
                else
                    // Set to slightly further from the border than
                    // this neighbor.
                    ndist += 3;
                if (ndist < dist)
                    /*
                     * This neighbor is closer to the border than
                     * previously detected neighbors.  Use this neighbor's
                     * increment. (I.e. Slightly further from border than
                     * this neighbor.)
                     */
                    dist = ndist;
            }
            nSpan = span.getNeighbor(3);  // (0, -1) Guaranteed to exist.
            ndist = nSpan.distanceToBorder();
            if (ndist == NEEDS_INIT)
                // Don't know how far from border this neighbor is.
                // Default to slightly away from the border.
                ndist = 1;
            else
                // Set to slightly further from the border than this neighbor.
                ndist += 2;
            if (ndist < dist)
                // This neighbor is closer to the border than previously
                // detected neighbors. Use this neighbor's increment.
                // (I.e. Slightly further from border than this neighbor.)
                dist = ndist;
            // (1, -1) Diagonal. Not guaranteed to exist.
            nSpan = nSpan.getNeighbor(2);
            // More of the same.  So no new comments.
            if (nSpan != null)
            {
                ndist = nSpan.distanceToBorder();
                if (ndist == NEEDS_INIT)
                    ndist = 2;
                else
                    ndist += 3;
                if (ndist < dist)
                    dist = ndist;
            }
            // At this point, dist will contain this shortest estimated
            // distance. Set the span to this value.
            span.setDistanceToBorder(dist);
        }
        
        /*
         * Pass 2
         * During this pass, the following neighbors are checked:
         *   (1, 0) (1, 1) (0, 1) (-1, 1)
         * 
         * Besides checking different neighbors, this pass performs its
         * grid search in reverse order.
         * 
         * This pass does the same thing as the first pass, but is much
         * simpler because it doesn't need to handle the NEEDS_INIT special
         * case.
         * 
         * Minimal commenting is provided since nothing new is happening
         * here that isn't already described in the the previous pass.
         */
        
        // Loop through all spans in reverse order. The looping method is
        // slightly more complex since the standard  iterator cannot be used.
        for (int depthIndex = field.depth() - 1; depthIndex >= 0; depthIndex--)
        {
            for (int widthIndex = field.width() - 1
                    ; widthIndex >= 0
                    ; widthIndex--)
            {
                // Loop through all spans in the current grid location.
                for (OpenHeightSpan span =
                    field.getData(widthIndex, depthIndex)
                        ; span != null
                        ; span = span.next())
                {
                    int dist = span.distanceToBorder();
                    if (dist == BORDER) continue;  // Border cells never change.
                    OpenHeightSpan nSpan = span.getNeighbor(2);  // (1, 0)
                    int ndist = nSpan.distanceToBorder();
                    ndist = nSpan.distanceToBorder() + 2;
                    if (ndist < dist)
                        dist = ndist;
                    nSpan = nSpan.getNeighbor(1); // (1, 1)
                    if (nSpan != null)
                    {
                        ndist = nSpan.distanceToBorder() + 3;
                        if (ndist < dist)
                            dist = ndist;
                    }
                    nSpan = span.getNeighbor(1);  // (0, 1)
                    ndist = nSpan.distanceToBorder();
                    ndist = nSpan.distanceToBorder() + 2;
                    if (ndist < dist)
                        dist = ndist;
                    nSpan = nSpan.getNeighbor(0); // (-1, 1)
                    if (nSpan != null)
                    {
                        ndist = nSpan.distanceToBorder() + 3;
                        if (ndist < dist)
                            dist = ndist;
                    }
                    span.setDistanceToBorder(dist);
                }
            }
        }
        
        // Reset the known min/max border distance.  This will force a
        // recalculation if the values are needed later.
        field.clearBorderDistanceBounds();
        
    }
    
    /**
     * Generates axis-neighbor link information for all spans in the field.
     * This information is required for algorithms which perform neighbor
     * searches.
     * <p>After this operation is run, the
     * {@link OpenHeightSpan#getNeighbor(int)} operation can be used for
     * neighbor searches.</p>
     * <p>This operation does not need to be run if the
     * {@link #build(SolidHeightfield, boolean) build} operation
     * was run with performFullGeneration set to TRUE.</p>
     * <p>The data generated by this operation is required by
     * {@link #generateDistanceField(OpenHeightfield)}</p>
     * @param field A field already loaded with span information.
     * @see <a href="http://www.critterai.org/?q=nmgen_hfintro#nsearch"
     * target="_parent">Neighbor Searches</a>
     */
    public void generateNeighborLinks(OpenHeightfield field)
    {
        if (field == null)
            return;
        
        // Loop through all spans and generate neighbor information.
        OpenHeightFieldIterator iter = field.dataIterator();
        while (iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            // Loop through all neighbor grid locations and check to see if
            // any of their spans are accessible from this span.
            for (int dir = 0; dir < 4; dir++)
            {
                // Get the neighbor offset for this direction.
                final int nWidthIndex =
                    (iter.widthIndex() + BoundedField.getDirOffsetWidth(dir));
                final int nDepthIndex =
                    (iter.depthIndex() + BoundedField.getDirOffsetDepth(dir));
                /*
                 * Loop through all spans in the neighbor grid location.
                 * Note: Because of the way solid heightfields are built, only
                 * one span in each neighbor column can ever meet the
                 * conditions to be a neighbor of the current span.
                 */
                for (OpenHeightSpan nSpan =
                    field.getData(nWidthIndex, nDepthIndex)
                        ; nSpan != null
                        ; nSpan = nSpan.next())
                {
                    // Select the floor, current or neighbor span, that is
                    // higher.
                    int maxFloor = Math.max(span.floor(), nSpan.floor());
                    // Select the ceiling, current or neighbor span, this is
                    // lower.
                    int minCeiling = Math.min(span.ceiling(), nSpan.ceiling());
                    /*
                     * The above values are used to determine if the the gap
                     * formed by the two spans is large enough for agents to
                     * walk through? E.g. Without bumping its head on anything.
                     */
                    if ((minCeiling - maxFloor) >= mMinTraversableHeight
                            && Math.abs(nSpan.floor() - span.floor())
                            <= mMaxTraversableStep)
                    {
                        // There is space to walk between current and neighbor
                        // span, and the step up/down between this and
                        // neighbor span is acceptable.
                        span.setNeighbor(dir, nSpan);
                        break;
                    }
                }
            }
        }

    }

    /**
     * Groups spans into contiguous regions using an watershed based algorithm.
     * <p>This operation depends on neighbor and distance field information.
     * So the  {@link #generateNeighborLinks(OpenHeightfield)} and
     * {@link #generateDistanceField(OpenHeightfield)} operations must be
     * run before this operation.</p>
     * <p>This operation does not need to be run if the
     * {@link #build(SolidHeightfield, boolean) build} operation
     * was run with performFullGeneration set to TRUE.</p>
     * @param field  A field with span, neighbor, and distance information
     * fully generated.
     */
    public void generateRegions(OpenHeightfield field)
    {
        if (field == null)
            return;
        /*
         * Watershed Algorithm
         * 
         * Reference: http://en.wikipedia.org/wiki/Watershed_%28algorithm%29
         * A good visualization:
         * http://artis.imag.fr/Publications/2003/HDS03/ (PDF)
         * 
         * Summary:
         * 
         * This algorithm utilizes the span.distanceToBorder() value, which
         * is generated by the generateDistanceField() operation.
         * 
         * Using the watershed analogy, the spans which are furthest from
         * a border (highest distance to border) represent the lowest points
         * in the watershed. A border span represents the highest possible
         * water level.
         * 
         * The main loop iterates, starting at the lowest point in the
         * watershed, then incrementing with each loop until the highest
         * allowed water level is reached.  This slowly "floods" the spans
         * starting at the lowest points.
         * 
         * (Remember: From this algorithm's point of view "lower" refers
         * to distance from a border, not height within the heightfield.)
         * 
         * During each iteration of the loop, spans that are below the
         * current water level are located and an attempt is made to either
         * add them to exiting regions or create new regions from them.
         * 
         * During the region expansion phase, if a newly flooded span
         * borders on an existing region, it is usually added to the region.
         * 
         * Any newly flooded span that survives the region expansion phase
         * is used as a seed for a new region.
         * 
         * At the end of the main loop, a final region expansion is
         * performed which should catch any stray spans that escaped region
         * assignment during the main loop.
         */
        
        /*
         * Represents the minimum distance to an obstacle that is considered
         * traversable. I.e. Can't traverse spans closer than this distance
         * to a border. This provides a way of artificially capping the
         * height to which watershed flooding can occur.
         * I.e. Don't let the algorithm flood all the way to the actual border.
         * 
         * We add the minimum border distance to take into account the
         * blurring algorithm which can result in a border span having a
         * border distance > 0.
         * 
         */
        final int minDist =
            mTraversableAreaBorderSize  + field.minBorderDistance();

        // TODO: EVAL: Figure out why this iteration limit is needed.
        final int expandIterations = 4 + (mTraversableAreaBorderSize * 2);
        
        /*
         * This value represents the current distance from the border which
         * is to be searched. The search starts at the maximum distance then
         * moves toward zero. (Toward borders.)
         * 
         * This number will always be divisible by 2.
         */
        int dist = (field.maxBorderDistance() - 1) & ~1;
        
        /*
         * Contains a list of spans that are considered to be flooded and
         * therefore are ready to be processed.  This list may contain nulls
         * at certain points in the process.  Nulls indicate spans that were
         * initially in the list but have been successfully added to a region.
         * The initial size is arbitrary.
         */
        final ArrayList<OpenHeightSpan> floodedSpans =
            new ArrayList<OpenHeightSpan>(1024);
        
        /*
         * A predefined stack for use in the flood operation.  Its content
         * has no meaning outside the new region flooding operation.
         * (Saves on object creation time.)
         */
        final ArrayDeque<OpenHeightSpan> workingStack =
            new ArrayDeque<OpenHeightSpan>(1024);
        
        final OpenHeightFieldIterator iter = field.dataIterator();
        
        // Zero is reserved for the null-region. So initializing to 1.
        int nextRegionID = 1;
        
        /*
         * Search until the current distance reaches the minimum allowed
         * distance.
         * 
         * Note: This loop will not necessarily complete all region
         * assignments.  This is OK since a final region assignment step
         * occurs after the loop iteration is complete.
         */
        while (dist > minDist)
        {
            
            // Find all spans that are at or below the current "water level"
            // and are not already assigned to a region. Add these spans to
            // the flooded span list for processing.
            iter.reset();
            floodedSpans.clear();
            while (iter.hasNext())
            {
                OpenHeightSpan span = iter.next();
                if (span.regionID() == NULL_REGION
                        && span.distanceToBorder() >= dist)
                    // The span is not already assigned a region and is
                    // below the current "water level". So the span can be
                    // considered for region assignment.
                    floodedSpans.add(span);
            }
            
            if (nextRegionID > 1)
            {
                // At least one region has already been created, so first
                // try to  put the newly flooded spans into existing regions.
                if (dist > 0)
                    expandRegions(floodedSpans, expandIterations);
                else
                    expandRegions(floodedSpans, -1);
            }

            // Create new regions for all spans that could not be added to
            // existing regions.
            for (OpenHeightSpan span : floodedSpans)
            {
                if (span == null || span.regionID() != 0)
                    // This span was assigned to a newly created region
                    // during an earlier iteration of this loop.
                    // So it can be skipped.
                    continue;
                // Fill to slightly more than the current "water level".
                // This improves efficiency of the algorithm.
                int fillTo = Math.max(dist - 2, minDist);
                if (floodNewRegion(span, fillTo, nextRegionID, workingStack))
                    // A new region was successfully generated.
                    nextRegionID++;
            }
            
            // Increment the "water level" by 2, clamping at 0.
            dist = Math.max(dist - 2, 0);
            
        }
        
        // Find all spans that haven't been assigned regions by the main loop.
        // (Up to the minimum distance.)
        iter.reset();
        floodedSpans.clear();
        while (iter.hasNext())
        {
            OpenHeightSpan span = iter.next();
            if (span.distanceToBorder() >=
                minDist && span.regionID() == NULL_REGION)
                // Not a border or null region span.  Should be in a region.
                floodedSpans.add(span);
        }
        
        // Perform a final expansion of existing regions.
        // Allow more iterations than normal for this last expansion.
        if (minDist > 0)
            expandRegions(floodedSpans, expandIterations * 8);
        else
            expandRegions(floodedSpans, -1);
        
        field.setRegionCount(nextRegionID);
        
        // Run the post processing algorithms.
        for (IOpenHeightFieldAlgorithm algorithm : mRegionAlgorithms)
        {
            algorithm.apply(field);
        }
        
    }
    
    /**
     * Attempts to find the most appropriate regions to attach spans to.
     * <p>Any spans successfully attached to a region will have their list
     * entry set to null. So any non-null entries in the list will be spans
     * for which a region could not be determined.</p>
     * @param inoutSpans As input, the list of spans available for formation
     * of new regions. As output, the spans that could not be assigned
     * to new regions.
     * @param maxIterations If set to -1, will iterate through completion.
     */
    private void expandRegions(ArrayList<OpenHeightSpan> inoutSpans
            , int maxIterations)
    {
        if (inoutSpans.size() == 0)
            return;  // No spans available to process.
        
        int iterCount = 0;
        while(true)
        {
            /*
             * The number of spans in the working list that have been
             * successfully processed or could not be processed successfully
             * for some reason.
             * This value controls when iteration ends.
             */
            int skipped = 0;
            
            // Loop through all spans in the working list.
            for (int iSpan = 0; iSpan < inoutSpans.size(); iSpan++)
            {
                OpenHeightSpan span = inoutSpans.get(iSpan);
                if (span == null)
                {
                    // The span originally at this index location has
                    // already been successfully assigned a region.  Nothing
                    // else to do with it.
                    skipped++;
                    continue;
                }
                // Default to unassigned.
                int spanRegion = NULL_REGION;
                // Default to highest possible distance.
                int regionCenterDist = Integer.MAX_VALUE;
                /*
                 * Search through this span's axis-neighbors.
                 * Reference: Neighbor searches
                 *  http://www.critterai.org/?q=nmgen_hfintro#nsearch
                 */
                for (int dir = 0; dir < 4; dir++)
                {
                    OpenHeightSpan nSpan = span.getNeighbor(dir);
                    if (nSpan == null)
                        // No neighbor at this location.
                        continue;
                    // There is a neighbor at this location.
                    if (nSpan.regionID() > NULL_REGION)
                    {
                        /*
                         * This neighbor span belongs to a region.
                         */
                        if (nSpan.distanceToRegionCore() + 2 < regionCenterDist)
                        {
                            /*
                             * This neighbor is closer to its region core
                             * than previously detected neighbors.
                             */
                            int sameRegionCount = 0;
                            if (mUseConservativeExpansion)
                            {
                                /*
                                 * Check to ensure that this neighbor has
                                 * at least two other neighbors in its region.
                                 * This makes sure that adding this span to
                                 * this neighbor's  region will not result
                                 * in a single width line of voxels.
                                 */
                                for (int ndir = 0; ndir < 4; ndir++)
                                {
                                    OpenHeightSpan nnSpan =
                                        nSpan.getNeighbor(ndir);
                                    if (nnSpan == null)
                                        // No diagonal-neighbor.
                                        continue;
                                    // There is a diagonal-neighbor
                                    if (nnSpan.regionID() == nSpan.regionID())
                                        // This neighbor has a neighbor in
                                        // the same region.
                                        sameRegionCount++;
                                }
                            }
                            if (!mUseConservativeExpansion
                                    || sameRegionCount > 1)
                            {
                                /*
                                 * Either conservative expansion is turned off,
                                 * or it is on and this neighbor's region is
                                 * acceptable for the current span.
                                 * Choose this neighbor's region.
                                 * Set the current distance to center as
                                 * slightly further than this neighbor.
                                 */
                                spanRegion = nSpan.regionID();
                                regionCenterDist =
                                    nSpan.distanceToRegionCore() + 2;
                            }
                        }
                    }
                }
                if (spanRegion != NULL_REGION)
                {
                    // Found a suitable region for this span to belong to.
                    // Mark this index as having been processed.
                    inoutSpans.set(iSpan, null);
                    span.setRegionID(spanRegion);
                    span.setDistanceToRegionCore(regionCenterDist);
                }
                else
                    // Could not find an existing region for this span.
                    skipped++;
            }
            
            if (skipped == inoutSpans.size())
                // All spans have either been processed or could not be
                // processed during the last cycle.
                break;
            
            if (maxIterations != -1)
            {
                iterCount++;
                if (iterCount > maxIterations)
                    // Reached the iteration limit.
                    break;
            }
            
        }
        
    }
    
    /**
     * Creates a new region surrounding a span, adding neighbor spans to the
     * new region as appropriate.
     * <p>The new region creation will fail if the root span is on the
     * border of an existing region.</p>
     * <p>All spans added to the new region as part of this process become
     * "core" spans with a distance to region core of zero.</p>
     * @param rootSpan The span used to seed the new region.
     * @param fillToDist The watershed distance to flood to.
     * @param regionID The region ID to use for the new region.
     * (If creation is successful.)
     * @param workingStack A stack used internally.  The content is
     * cleared before use.  Its content  has no meaning outside of
     * this operation.
     * @return TRUE if a new region was created.  Otherwise FALSE.
     */
    private static boolean floodNewRegion(OpenHeightSpan rootSpan
            , int fillToDist
            , int regionID
            , ArrayDeque<OpenHeightSpan> workingStack)
    {
        workingStack.clear();
        // TODO: EVAL: Change this into a working argument?
        // Don't want unneeded object creation.
        ArrayList<OpenHeightSpan> workingList =
            new ArrayList<OpenHeightSpan>();
        // See stack and list.
        workingStack.push(rootSpan);
        workingList.add(rootSpan);
        rootSpan.setRegionID(regionID);  // Seed with region id.
        rootSpan.setDistanceToRegionCore(0);  // Set as center of region.
        
        int regionSize = 0;
        
        while (workingStack.size() > 0)
        {
            
            OpenHeightSpan span = workingStack.pop();
            
            /*
             * Check regions of neighbor spans.
             * 
             * If any neighbor is found to have a region assigned, then
             * the current span can't be in the new region.
             * (Want standard flooding algorithm to handle deciding which
             * region this span should go in.)
             * 
             * Up to 8 neighbors are checked.
             * 
             * Reference: Neighbor searches.
             * http://www.critterai.org/?q=nmgen_hfintro#nsearch
             */
            boolean isOnRegionBorder = false;
            for (int dir = 0; dir < 4; dir++)
            {
                OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan == null)
                    // No neighbor in this direction.
                    continue;
                
                // Check this axis-neighbor.
                if (nSpan.regionID() != NULL_REGION
                        && nSpan.regionID() != regionID)
                {
                    // Current span borders the null region or another region.
                    // No need to check rest of neighbors.
                    isOnRegionBorder = true;
                    break;
                }
                
                // Check the diagonal-neighbor.
                nSpan = nSpan.getNeighbor((dir+1) & 0x3);
                if (nSpan != null
                        && nSpan.regionID() != NULL_REGION
                        && nSpan.regionID() != regionID)
                {
                    // Current span borders the null region or another region.
                    // No need to check rest of neighbors.
                    isOnRegionBorder = true;
                    break;
                }
            }
            if (isOnRegionBorder)
            {
                // Current span borders the null region or another region.
                // Can't be part of the new region.
                span.setRegionID(NULL_REGION);
                continue;
            }

            regionSize++;
            
            // If got this far, we know the current span is part of the new
            // region. Now check its neighbors to see if they should be
            // assigned to this new region.
            for (int dir = 0; dir < 4; dir++)
            {
                OpenHeightSpan nSpan = span.getNeighbor(dir);
                if (nSpan != null
                        && nSpan.distanceToBorder() >= fillToDist
                        && nSpan.regionID() == 0)
                {
                    // This neighbor does not have a region assignment and
                    // it is within the allowed fill range.  Set it as a
                    // candidate for this new region.
                    nSpan.setRegionID(regionID);
                    nSpan.setDistanceToRegionCore(0);
                    workingStack.push(nSpan);
                    workingList.add(nSpan);
                }
            }
        }
        
        return (regionSize > 0);
    }
    
}
