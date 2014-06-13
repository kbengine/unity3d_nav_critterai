package org.critterai.nmgen;

import java.util.ArrayDeque;

import org.critterai.nmgen.OpenHeightfield.OpenHeightFieldIterator;

/**
 * Implements three algorithms that clean up issues that can
 * develop around null region boarders.
 * 
 * <p><strong>Detect and fix encompassed null regions:</strong></p>
 * <p>If a null region is found that is fully encompassed by a single
 * region, then the region will be split into two regions at the
 * null region border.</p>
 * 
 * <p><strong>Detect and fix "short wrapping" of null regions:</strong></p>
 * <p>Regions can sometimes wrap slightly around the corner of a null region
 * in a manner that eventually results in the formation of self-intersecting
 * polygons.</p>
 * <p>Example: Before the algorithm is applied:</p>
 * <p><a href=
 * "http://www.critterai.org/projects/nmgen/images/ohfg_08_cornerwrapbefore.png"
 * target="_parent">
 * <img alt="" src=
 * "http://www.critterai.org/projects/nmgen/images/ohfg_08_cornerwrapbefore.jpg"
 * style="width: 620px; height: 353px; " />
 * </a></p>
 * <p>Example: After the algorithm is applied:</p>
 * <p><a href=
 * "http://www.critterai.org/projects/nmgen/images/ohfg_09_cornerwrapafter.png"
 * target="_parent">
 * <img alt="" src=
 * "http://www.critterai.org/projects/nmgen/images/ohfg_09_cornerwrapafter.jpg"
 * style="width: 620px; height: 353px; " />
 * </a></p>
 * 
 * <p><strong>Detect and fix incomplete null region connections:</strong></p>
 * <p>If a region touches null region only diagonally, then contour detection
 * algorithms may not properly detect the null region connection.  This can
 * adversely effect other algorithms in the pipeline.<p>
 * <p>Example: Before algorithm is applied:</p>
 * <p><pre>
 *     b b a a a a
 *     b b a a a a
 *     a a x x x x
 *     a a x x x x
 * </pre></p>
 * <p>Example: After algorithm is applied:</p>
 * <p><pre>
 *     b b a a a a
 *     b b b a a a <-- Span transferred to region B.
 *     a a x x x x
 *     a a x x x x
 * </pre></p>
 * 
 * @see <a href="http://www.critterai.org/nmgen_regiongen"
 * target="_parent">Region Generation</a>
 */
public class CleanNullRegionBorders
    implements IOpenHeightFieldAlgorithm
{

    /*
     * Design Notes:
     * 
     * Three algorithms have been aggregated into this single class
     * for performance reasons.  Otherwise we would be stuck
     * performing three full contour searches rather than one.
     * 
     * The optimization method used in the search can result in missed
     * null region contours.  Consider the following pattern:
     * 
     *     x x x x x x
     *     x a a a a x   x - null region WITHOUT SPANS
     *     x a v v a x   a - region A
     *     x a v v a x   v - null region WITHOUT SPANS
     *     x a a a a x
     *     x x x x x x
     * 
     * If an all span search is performed, the outer null region (x) will
     * be detected, but during the process all a-region spans will be marked as
     * viewed. This will leave no spans available to detect the inner null
     * region (v).
     * 
     * I've not fixing this until it proves to be a problem or I figure out
     * a way of resolving the design issue without killing performance.
     */
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
    
    private final boolean mUseOnlyNullSpans;
    
    // Working variables.  Content is meaningless outside of
    // method they are used.
    private final ArrayDeque<OpenHeightSpan> mwOpenSpans
        = new ArrayDeque<OpenHeightSpan>(1024);
    private final ArrayDeque<Integer> mwBorderDistance
        = new ArrayDeque<Integer>(1024);
    private final int[] mwNeighborRegions = new int[8];
    
    /**
     * Constructor.
     * <p>Choosing a contour detection type:</p>
     * <p>This algorithm has to detect and walk null region contours. (Where
     * null regions border non-null regions.)  There are two options for
     * detection:  Search every single span looking for null region
     * neighbors.  Or search only null region spans looking for
     * non-null region neighbors. Since null region spans are only a tiny
     * fraction of total spans, the second option has better performance.</p>
     * <p>If a heightfield is constructed such that all null regions have
     * at least one null region span in each contour, then set
     * useOnlyNullRegionSpans to TRUE.</p>
     * @param useOnlyNullRegionSpans If TRUE, then only null region spans
     * will be used to initially detect null region borders.  This
     * improves performance.  If FALSE, all spans are searched to detect
     * borders.
     */
    public CleanNullRegionBorders(boolean useOnlyNullRegionSpans)
    {
        mUseOnlyNullSpans = useOnlyNullRegionSpans;
    }
    
    /**
     * {@inheritDoc}
     * <p>This operation utilizes {@link OpenHeightSpan#flags}.  It
     * expects the value to be zero on entry, and re-zero's the value
     * on exit.</p>
     * <p>Expects a heightfield with fully built regions.</p>
     */
    @Override
    public void apply(OpenHeightfield field)
    {
        
        int nextRegionID = field.regionCount();
        final OpenHeightFieldIterator iter = field.dataIterator();
        
        // Iterate over the spans, trying to find null region borders.
        while (iter.hasNext())
        {
            OpenHeightSpan span = iter.next();
            
            if (span.flags != 0)
                // Span was processed in a previous iteration.
                // Ignore it.
                continue;
            
            span.flags = 1;
            
            OpenHeightSpan workingSpan = null;
            int edgeDirection = -1;
            
            if (span.regionID() == NULL_REGION)
            {
                // This is a null region span.  See if it
                // connects to a span in a non-null region.
                edgeDirection = getNonNullBorderDrection(span);
                if (edgeDirection == -1)
                    // This span is not a border span.  Ignore it.
                    continue;
                
                // This is a border span.  Step into the non-null
                // region and swing the direction around 180 degrees.
                workingSpan = span.getNeighbor(edgeDirection);
                edgeDirection = (edgeDirection+2) & 0x3;
            }
            else if (!mUseOnlyNullSpans)
            {
                // This is a non-null region span and I'm allowed
                // to look at it.  See if it connects to a null region.
                edgeDirection = getNullBorderDrection(span);
                if (edgeDirection == -1)
                    // This span is not a null region border span.  Ignore it.
                    continue;
                workingSpan = span;
            }
            else
                // Not interested in this span.
                continue;
            
            // Process the null region contour.  Detect and fix
            // local issues.  Determine if the region is
            // fully encompassed by a single non-null region.
            boolean isEncompassedNullRegion = processNullRegion(
                    workingSpan
                    , edgeDirection);
            
            if (isEncompassedNullRegion)
            {
                // This span is part of a group of null region spans
                // that is encompassed within a single non-null region.
                // This is not permitted.  Need to fix it.
                partialFloodRegion(workingSpan
                        , edgeDirection
                        , nextRegionID);
                nextRegionID++;
            }
        }
        
        field.setRegionCount(nextRegionID);
         
        // Clear all flags.
        iter.reset();
        while (iter.hasNext())
        {
            iter.next().flags = 0;
        }
    }
    
    /**
     * Partially flood a region away from the specified direction.
     * <p>{@link OpenHeightSpan#distanceToRegionCore()}
     * is set to zero for all flooded spans.</p>
     * @param startSpan The span to start the flood from.
     * @param borderDirection  The hard border for flooding.  No
     * spans in this direction from the startSpan will be flooded.
     * @param newRegionID The region id to assign the flooded
     * spans to.
     */
    private void partialFloodRegion(OpenHeightSpan startSpan
            , int borderDirection
            , int newRegionID)
    {
        // Gather some information.
        final int antiBorderDirection = (borderDirection+2) & 0x3;
        final int regionID = startSpan.regionID();
        
        // Re-assign the start span and queue it for the neighbor search.
        startSpan.setRegionID(newRegionID);
        startSpan.setDistanceToRegionCore(0);  // This information is lost.
        mwOpenSpans.add(startSpan);
        mwBorderDistance.add(0);
        
        // Search for new spans that can be assigned the new region.
        while(!mwOpenSpans.isEmpty())
        {
            // Get the next span off the stack.
            final OpenHeightSpan span = mwOpenSpans.pollLast();
            final int distance = mwBorderDistance.pollLast();
            
            // Search in all directions for neighbors.
            for (int i = 0; i < 4; i++)
            {
                final OpenHeightSpan nSpan = span.getNeighbor(i);
                if (nSpan == null
                        || nSpan.regionID() != regionID)
                    // No span in this direction, or the span
                    // is not in the region being processed.
                    // Note: It may have already been transferred.
                    continue;
                int nDistance = distance;
                if (i == borderDirection)
                {
                    // This neighbor is back toward the border.
                    if (distance == 0)
                        // The span is at the border.  Can't go
                        // further in this direction.  Ignore
                        // this neighbor.
                        continue;
                    nDistance--;
                }
                else if (i == antiBorderDirection)
                    // This neighbor is further away from the border.
                    nDistance++;
                
                // Transfer the neighbor to the new region.
                nSpan.setRegionID(newRegionID);
                nSpan.setDistanceToRegionCore(0);  // This information is lost.
                
                // Add the span to the stack to be processed.
                mwOpenSpans.add(nSpan);
                mwBorderDistance.add(nDistance);
               
            }
            
        }
        
    }
    
    /**
     * Detects and fixes bad span configurations in the vicinity of a
     * null region contour.  (See class description for details.)
     * @param startSpan A span in a non-null region that borders a null
     * region.
     * @param startDirection The direction of the null region border.
     * @return TRUE if the start span's region completely encompasses
     * the null region.
     */
    private boolean processNullRegion(OpenHeightSpan startSpan
            , int startDirection)
    {
        
        /*
         * This algorithm traverses the contour.  As it does so, it detects
         * and fixes various known dangerous span configurations.
         * 
         * Traversing the contour:  A good way to  visualize it is to think
         * of a robot sitting on the floor facing  a known wall.  It then
         * does the following to skirt the wall:
         * 1. If there is a wall in front of it, turn clockwise in 90 degrees
         *    increments until it finds the wall is gone.
         * 2. Move forward one step.
         * 3. Turn counter-clockwise by 90 degrees.
         * 4. Repeat from step 1 until it finds itself at its original
         *    location facing its original direction.
         * 
         * See also: http://www.critterai.org/nmgen_contourgen#robotwalk
         * 
         * As the traversal occurs, the number of acute (90 degree) and
         * obtuse (270 degree) corners are monitored.  If a complete contour is
         * detected and (obtuse corners > acute corners), then the null
         * region is inside the contour.  Otherwise the null region is
         * outside the contour, which we don't care about.
         */
        
        int borderRegionID = startSpan.regionID();
        
        // Prepare for loop.
        OpenHeightSpan span = startSpan;
        OpenHeightSpan nSpan = null;
        int dir = startDirection;
       
        // Initialize monitoring variables.
        int loopCount = 0;
        int acuteCornerCount = 0;
        int obtuseCornerCount = 0;
        int stepsWithoutBorder = 0;
        boolean borderSeenLastLoop = false;
        boolean isBorder = true;  // Initial value doesn't matter.
        
        // Assume a single region is connected to the null region
        // until proven otherwise.
        boolean hasSingleConnection = true;
        
        /*
         * The loop limit exists for the sole reason of preventing
         * an infinite loop in case of bad input data.
         * It is set to a very high value because there is no way of
         * definitively determining a safe smaller value.  Setting
         * the value too low can result in rescanning a contour
         * multiple times, killing performance.
         */
        while (++loopCount < Integer.MAX_VALUE)
        {
            // Get the span across the border.
            nSpan = span.getNeighbor(dir);
            
            // Detect which type of edge this direction points across.
            if (nSpan == null)
            {
                // It points across a null region border edge.
                isBorder = true;
            }
            else
            {
                // We never need to perform contour detection
                // on this span again.  So mark it as processed.
                nSpan.flags = 1;
                if (nSpan.regionID() == NULL_REGION)
                {
                    // It points across a null region border edge.
                    isBorder = true;
                }
                else
                {
                    // This isn't a null region border.
                    isBorder = false;
                    if (nSpan.regionID() != borderRegionID)
                        // It points across a border to a non-null region.
                        // This means the current contour can't
                        // represent a fully encompassed null region.
                        hasSingleConnection = false;
                }
            }
            
            // Process the border.
            if (isBorder)
            {
                // It is a border edge.
                if (borderSeenLastLoop)
                {
                    /*
                     * A border was detected during the last loop as well.
                     * Two detections in a row indicates we passed an acute
                     * (inner) corner.
                     * 
                     *     a x
                     *     x x
                     */
                    acuteCornerCount++;
                }
                else if (stepsWithoutBorder > 1)
                {
                    /*
                     * We have moved at least two spans before detecting
                     * a border.  This indicates we passed an obtuse
                     * (outer) corner.
                     * 
                     *     a a
                     *     a x
                     */
                    obtuseCornerCount++;
                    stepsWithoutBorder = 0;
                    // Detect and fix span configuraiton issue around this
                    // corner.
                    if (processOuterCorner(span, dir))
                        // A change was made and it resulted in the
                        // corner area having multiple region connections.
                        hasSingleConnection = false;
                }
                dir = (dir+1) & 0x3; // Rotate in clockwise direction.
                borderSeenLastLoop = true;
                stepsWithoutBorder = 0;
            }
            else
            {
               /*
                * Not a null region border.
                * Move to the neighbor and swing the search direction back
                * one increment (counterclockwise).  By moving the direction
                * back one increment we guarantee we don't miss any edges.
                */
               span = nSpan;
               dir = (dir+3) & 0x3; // Rotate counterclockwise direction.
               borderSeenLastLoop = false;
               stepsWithoutBorder++;
           }
            
            if (startSpan == span && startDirection == dir)
                // Have returned to the original span and direction.
                // The search is complete.
                // Is the null region inside the contour?
                return (hasSingleConnection
                        && obtuseCornerCount > acuteCornerCount);
        }
 
        // If got here then the null region boarder is too large to be fully
        // explored.  So it can't be encompassed.
        return false;
    }
    
    /**
     * Detects and fixes span configuration issues in the vicinity
     * of obtuse (outer) null region corners.
     * @param referenceSpan The span in a non-null region that is
     * just past the outer corner.
     * @param borderDirection The direciton of the null region border.
     * @return TRUE if more than one region connects to the null region
     * in the vicinity of the corner. (This may or may not be due to
     * a change made by this operation.)
     */
    private boolean processOuterCorner(OpenHeightSpan referenceSpan
        , int borderDirection)
    {
        
        boolean hasMultiRegions = false;
        
        // Get the previous two spans along the border.
        OpenHeightSpan backOne =
            referenceSpan.getNeighbor((borderDirection+3) & 0x3);
        OpenHeightSpan backTwo = backOne.getNeighbor(borderDirection);
        OpenHeightSpan testSpan;
        
        if (backOne.regionID() != referenceSpan.regionID()
                && backTwo.regionID() == referenceSpan.regionID())
        {
            /*
             * Dangerous corner configuration.
             * 
             *     a x
             *     b a
             * 
             * Need to change to one of the following configurations:
             * 
             *     b x        a x
             *     b a        b b
             * 
             * Reason: During contour detection this type of configuration can
             * result in the region connection being detected as a
             * region-region portal, when it is not.  The region connection
             * is actually interrupted by the null region.
             * 
             * This configuration has been demonstrated to result in
             * two regions being improperly merged to encompass an
             * internal null region.
             * 
             * Example:
             * 
             *     a a x x x a
             *     a a x x a a
             *     b b a a a a
             *     b b a a a a
             * 
             * During contour and connection detection for region b, at no
             * point will the null region be detected.  It will appear
             * as if a clean a-b portal exists.
             * 
             * An investigation into fixing this issue via updates to the
             * watershed or contour detection algorithms did not turn
             * up a better way of resolving this issue.
             */
            hasMultiRegions = true;
            // Determine how many connections backTwo has to backOne's region.
            testSpan = backOne.getNeighbor((borderDirection+3) & 0x3);
            int backTwoConnections = 0;
            if (testSpan != null
                    && testSpan.regionID() == backOne.regionID())
            {
                backTwoConnections++;
                testSpan = testSpan.getNeighbor(borderDirection);
                if (testSpan != null
                        && testSpan.regionID() == backOne.regionID())
                    backTwoConnections++;
            }
            // Determine how many connections the reference span has
            // to backOne's region.
            int referenceConnections = 0;
            testSpan = backOne.getNeighbor((borderDirection+2) & 0x3);
            if (testSpan != null
                    && testSpan.regionID() == backOne.regionID())
            {
                referenceConnections++;
                testSpan = testSpan.getNeighbor((borderDirection+2) & 0x3);
                if (testSpan != null
                        && testSpan.regionID() == backOne.regionID())
                    backTwoConnections++;
            }
            // Change the region of the span that has the most connections
            // to the target region.
            if (referenceConnections > backTwoConnections)
                referenceSpan.setRegionID(backOne.regionID());
            else
                backTwo.setRegionID(backOne.regionID());
        }
        else if (backOne.regionID() == referenceSpan.regionID()
                && backTwo.regionID() == referenceSpan.regionID())
        {
            /*
             * Potential dangerous short wrap.
             * 
             *  a x
             *  a a
             * 
             *  Example of actual problem configuration:
             * 
             *  b b x x
             *  b a x x <- Short wrap.
             *  b a a a
             * 
             *  In the above case, the short wrap around the corner of the
             *  null region has been demonstrated to cause self-intersecting
             *  polygons during polygon formation.
             * 
             *  This algorithm detects whether or not one (and only one)
             *  of the axis neighbors of the corner should be re-assigned to
             *  a more appropriate region.
             * 
             *  In the above example, the following configuration is more
             *  appropriate:
             * 
             *  b b x x
             *  b b x x <- Change to this row.
             *  b a a a
             */
            // Check to see if backTwo should be in a different region.
            int selectedRegion = selectedRegionID(backTwo
                    , (borderDirection+1) & 0x3
                    , (borderDirection+2) & 0x3);
            if (selectedRegion == backTwo.regionID())
            {
                // backTwo should not be re-assigned.  How about
                // the reference span?
                selectedRegion = selectedRegionID(referenceSpan
                        , borderDirection
                        , (borderDirection+3) & 0x3);
                if (selectedRegion != referenceSpan.regionID())
                {
                    // The reference span should be reassigned
                    // to a new region.
                    referenceSpan.setRegionID(selectedRegion);
                    hasMultiRegions = true;
                }
            }
            else
            {
                // backTwo should be re-assigned to a new region.
                backTwo.setRegionID(selectedRegion);
                hasMultiRegions = true;
            }
        }
        else
            /*
             * No dangerous configurations detected.  But definitely
             * has a change in regions at the corner. (We know this
             * because one of the previous checks looked for a single
             * region for all wrap spans.)
             */
            hasMultiRegions = true;
        
        return hasMultiRegions;
    }
    
    /**
     * Checks the span to see if it should be reassigned to a new region.
     * @param referenceSpan A span on one side of an null region contour's
     * outer corner.  It is expected that the all spans that wrap the
     * corner are in the same region.
     * @param borderDirection  The direction of the null region border.
     * @param cornerDirection The direction of the outer corner from the
     * reference span.
     * @return The region the span should be a member of.  May be the
     * region the span is currently a member of.
     */
    private int selectedRegionID(OpenHeightSpan referenceSpan
            , int borderDirection
            , int cornerDirection)
    {
        
        // Get the regions of all neighbors.
        referenceSpan.getDetailedRegionMap(mwNeighborRegions, 0);
        
        /*
         * Initial example state:
         * 
         * a - Known region.
         * x - Null region.
         * u - Unknown, not checked yet.
         * 
         *     u u u
         *     u a x
         *     u a a
         */
        
        // The only possible alternate region id is from
        // the span that is opposite the border.  So check it first.
        int regionID = mwNeighborRegions[(borderDirection+2) & 0x3];
        if (regionID == referenceSpan.regionID()
                || regionID == NULL_REGION)
            /*
             * The region away from the border is either a null region
             * or the same region.  So we keep the current region.
             * 
             *     u u u      u u u
             *     a a x  or  x a x  <-- Potentially bad, but stuck with it.
             *     u a a      u a a
             */
            return referenceSpan.regionID();
        
        // Candidate region for re-assignment.
        int potentialRegion = regionID;
       
        // Next we check the region opposite from the corner direction.
        // If it is the current region, then we definitely can't
        // change the region id without risk of splitting the region.
        regionID = mwNeighborRegions[(cornerDirection+2) & 0x3];
        if (regionID == referenceSpan.regionID() || regionID == NULL_REGION)
            /*
             * The region opposite from the corner direction is
             * either a null region or the same region.  So we
             * keep the current region.
             * 
             *     u a u      u x u
             *     b a x  or  b a x
             *     u a a      u a a
             */
            return referenceSpan.regionID();

        /*
         * We have checked the early exit special cases.  Now a generalized
         * brute count is performed.
         * 
         * Priority is given to the potential region.  Here is why:
         * (Highly unlikely worst case scenario.)
         * 
         *     c c c    c c c
         *     b a x -> b b x  Select b even though b count == a count.
         *     b a a    b a a
         */
        
        // Neighbors in potential region.
        // We know this will have a minimum value of 1.
        int potentialCount = 0;
        
        // Neighbors in the span's current region.
        // We know this will have a minimum value of 2.
        int currentCount = 0;
        
        /*
         * Maximum edge case:
         * 
         *     b b b
         *     b a x
         *     b a a
         * 
         * The maximum edge case for region A can't exist.  It
         * is filtered out during one of the earlier special cases
         * handlers.
         * 
         * Other cases may exist if more regions are involved.
         * Such cases will tend to favor the current region.
         */
         
        for (int i = 0; i < 8; i++)
        {
            if (mwNeighborRegions[i] == referenceSpan.regionID())
                currentCount++;
            else if (mwNeighborRegions[i] == potentialRegion)
                potentialCount++;
        }

        return (potentialCount < currentCount
                ? referenceSpan.regionID() : potentialRegion);
    }
    
    /**
     * Returns the direction of the first neighbor in a non-null region.
     * @param span The span to check.
     * @return The direction of the first neighbor in a non-null region, or
     * -1 if all neighbors are in the null region.
     */
    private static int getNonNullBorderDrection(OpenHeightSpan span)
    {
        // Search axis-neighbors.
        for (int dir = 0; dir < 4; ++dir)
        {
            OpenHeightSpan nSpan = span.getNeighbor(dir);
            if (nSpan != null && nSpan.regionID() != NULL_REGION)
                // The neighbor is a non-null region.
                return dir;
        }
        // All neighbors are in the null region.
        return -1;
    }
    
    /**
     * Returns the direction of the first neighbor in the null region.
     * @param span The span to check.
     * @return The direction of the first neighbor that is in the null
     * region, or -1 if there are no null region neighbors.
     */
    private static int getNullBorderDrection(OpenHeightSpan span)
    {
        // Search axis-neighbors.
        for (int dir = 0; dir < 4; ++dir)
        {
            OpenHeightSpan nSpan = span.getNeighbor(dir);
            if (nSpan == null || nSpan.regionID() == NULL_REGION)
                // The neighbor is a null region.
                return dir;
        }
        // All neighbors are in a non-null region.
        return -1;
    }

}
