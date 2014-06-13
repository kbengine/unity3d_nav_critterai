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

import org.critterai.nmgen.OpenHeightfield.OpenHeightFieldIterator;

/**
 * Removes and merges small regions within a height field.
 * <p>Applies two algorithms:</p>
 * <ul>
 * <li>Searches for island regions (regions only connected to the null region)
 * and removes the ones that are smaller than the chosen threshold.</li>
 * <li>If a region is below a threshold size, attempts to merge the region
 * into the most appropriate larger neighbor region.</li>
 * </ul><a href=
 * "http://www.critterai.org/projects/nmgen/images/islandregion_filter.png"
 * target="_parent">
 * <img alt="" src=
 * "http://www.critterai.org/projects/nmgen/images/islandregion_filter.jpg"
 * style="width: 620px; height: 452px; " />
 * </a>
 * @see <a href="http://www.critterai.org/nmgen_regiongen"
 * target="_parent">Region Generation</a>
 */
public final class FilterOutSmallRegions
    implements IOpenHeightFieldAlgorithm
{

    /*
     * Recast Reference: filerSmallRegions() in RecastRegion.cpp
     */
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
    
    private final int mMinUnconnectedRegionSize;
    private final int mMergeRegionSize;
    
    /**
     * Constructor
     * @param minUnconnectedRegionSize The minimum region size for
     * unconnected (island) regions. (Voxels)
     * <p>Any generated regions that are not connected to any other region
     * and are smaller than this size will be culled before final navmesh
     * generation.  I.e. No longer considered traversable.<p>
     * <p>Constraints:  > 0</p>
     * 
     * @param mergeRegionSize Any regions smaller than this size will,
     * if possible, be merged with larger regions. (Voxels)
     * <p>Helps reduce the number of unnecessarily small regions that can
     * be formed.  This is especially an issue in diagonal path regions
     * where inherent faults in the region generation algorithm can result in
     * unnecessarily small regions.</p>
     * <p>If a region cannot be legally merged with a neighbor region, then
     * it will be left alone.</p>
     * <p>Constraints:  >= 0</p>
     * 
     * @see <a href=
     * "http://www.critterai.org/?q=nmgen_config#minUnconnectedRegionSize"
     * target="_parent">Detailed parameter information.</a>
     */
    public FilterOutSmallRegions(int minUnconnectedRegionSize
            , int mergeRegionSize)
    {
        mMinUnconnectedRegionSize = Math.max(1, minUnconnectedRegionSize);
        mMergeRegionSize = Math.max(0, mergeRegionSize);
    }
    
    /**
     * {@inheritDoc}
     * <p>The height field must contain valid region information in order
     * for this algorithm to be effective.</p>
     */
    @Override
    public void apply(OpenHeightfield field)
    {
    
        if (field.regionCount() < 2)
            // No spans or all spans are in the null region.
            return;
        
        /*
         *  Over the life of the region array, the ids of region objects
         *  contained within the array may be changed as follows:
         *      - To the null region.  This indicates the region has been
         *        abandoned.  All spans assigned to it will be merged into
         *        the null region.
         *      - To another region id.  This indicates that the spans
         *        assigned to the region will be re-assigned to the new
         *        region.  (This occurs when region merging occurs.)
         *  So the index of the array indicates the original region id.  The
         *  region objects within the array contain the current target
         *  region id.
         */
        final Region[] regions = new Region[field.regionCount()];
        
        for (int i = 0; i < field.regionCount(); i++)
        {
            regions[i] = new Region(i);
        }
        
        final OpenHeightFieldIterator iter = field.dataIterator();
        
        /*
         * Region object initialization.
         * For all non-null regions:
         * - Tally the number of spans that belong to the region.
         * - Record connections to other regions.  (Including connections
         *   to the null region.)
         * - Record regions that lie above (overlap) the region.
         * (Only non-null regions.)
         */
        while (iter.hasNext())
        {
            OpenHeightSpan span = iter.next();
            
            if (span.regionID() <= NULL_REGION)
                // Span is in the null region.  So skip it.
                continue;
            
            // Get current span's region object and increment its membership
            // count.
            Region region = regions[span.regionID()];
            region.spanCount++;
            
            // Step up the list of spans above the current span.
            for (OpenHeightSpan nextHigherSpan = span.next()
                    ; nextHigherSpan != null
                    ; nextHigherSpan = nextHigherSpan.next())
            {
                if (nextHigherSpan.regionID() <= NULL_REGION)
                    // Span is in the null region.  So ignore it.
                    continue;
                if (!region.overlappingRegions.contains(
                        nextHigherSpan.regionID()))
                    // This is a previously undetected region above the
                    // current region.
                    region.overlappingRegions.add(nextHigherSpan.regionID());
            }
            
            if (region.connections.size() > 0)
                // Have already found the connections for this span's region.
                // So move to the next span.
                continue;
            
            // Is this span on the edge of the its region?
            int edgeDirection = getRegionEdgeDirection(span);
            if (edgeDirection != -1)
                // This is the first span detected that lies on the edge of
                // its region. Can generate this region's the connection
                // information.
                findRegionConnections(span, edgeDirection, region.connections);
        }
        
        /*
         * Region information has been gathered.
         * 
         * Find unconnected (island) regions that are below the allowed
         * minimum size and convert them to null regions.
         * 
         * This will result in all spans assigned to these regions being
         * re-assigned to the null region.
         * 
         * Starting at region 1 since zero is the null region.
         */
        for (int regionID = 1; regionID < field.regionCount(); regionID++)
        {
            Region region = regions[regionID];
            if (region.spanCount == 0)
                // Skip empty regions.
                continue;
            if (region.connections.size() == 1
                    && region.connections.get(NULL_REGION) == NULL_REGION)
            {
                // This region is only connected to the null region.
                // (It is an island region.)
                if (region.spanCount < mMinUnconnectedRegionSize)
                {
                    // This region is too small to be allowed as an
                    // island region. Make it a null region.
                    region.resetWithID(0);
                }
            }
        }
        
        /*
         * Search for small regions to merge with other regions.
         * 
         * This is accomplished by searching for regions that are allowed to
         * be merged with other regions, then searching through the region's
         * connections for regions to be merged with.
         * 
         * A region only looks for a single other region to merge with
         * during each iteration of the loop. The loop will continue until
         * no successful merges are detected.
         * 
         * At the end of this process, regions that have been merged into
         * other regions will have had their IDs replaced by the region's
         * they were merged into.
         * 
         * Example:
         * 
         * Region 11 is at array index 11.  It is merged with region 18
         * at index 18.  After the merge, the region OBJECT at index 11
         * and will have an id of 18.
         */
        int mergeCount;
        do
        {
            mergeCount = 0;
            // Loop through all regions.
            for (Region region : regions)
            {
                if (region.id <= NULL_REGION || region.spanCount == 0)
                    // Skip null and empty regions.
                    continue;
                
                if (region.spanCount > mMergeRegionSize)
                    // Region is not a candidate for being merged into
                    // another region.
                    continue;
                
                // Region is either a small region, or a large region
                // not connected to a null region.
                // Find its smallest neighbor region.
                Region targetMergeRegion = null;
                int smallestSizeFound = Integer.MAX_VALUE;
                // Loop through all region's neighbor (connections).
                for (Integer nRegionID : region.connections)
                {
                    if (nRegionID <= 0)
                        // This neighbor is the null region.  So skip it.
                        continue;
                    final Region nRegion = regions[nRegionID];
                    if (nRegion.spanCount < smallestSizeFound
                            && canMerge(region, nRegion))
                    {
                        // This neighbor region is the smallest merge-able
                        // region found so far.
                        targetMergeRegion = nRegion;
                        smallestSizeFound = nRegion.spanCount;
                    }
                }
                // If a target was found, try to merge.
                if (targetMergeRegion != null
                        && mergeRegions(targetMergeRegion, region))
                {
                    // A successful merge took place.
                    // Discard the old region and replace it with the region
                    // it was merged into.
                    final int oldRegionID = region.id;
                    region.resetWithID(targetMergeRegion.id);
                    // Update region ID in all regions which reference the
                    // discarded region.
                    for (Region r : regions)
                    {
                        if (r.id <= 0)
                            // Ignore null region.
                            continue;
                        if (r.id == oldRegionID)
                            // This region was merged into the old
                            // outdated region. Re-point its id to the new
                            // target region.
                            r.id = targetMergeRegion.id;
                        else
                            // Fix connection and overlap information.
                            replaceNeighborRegionID(r
                                    , oldRegionID
                                    , targetMergeRegion.id);
                        
                    }
                    mergeCount++;
                }
            }
        // Continue looping as long as there were merges in the last iteration.
        } while (mergeCount > 0);
        
        /*
         *  At this point the region ids may no longer be sequential.
         *  This next section re-maps the region ids so they are again
         *  sequential.
         * 
         *  Flag regions that need to be considered for id re-mapping.
         *  This flagging is necessary since multiple regions may be sharing
         *  the same id. Only want to consider a single id for mapping at
         *  a time.
         */
        for (Region region : regions)
        {
            if (region.id > 0)
                // This is not a null region.  So it needs to be considered
                // for remapping.
                region.remap = true;
        }
        
         // Next variable will be incremented to 1 the first iteration
        // of the loop.
        int currRegionID = 0;
        for (Region region : regions)
        {
            if (!region.remap)
                // Region already considered for re-mapping.
                continue;
            // Will re-assign all regions that use the current region ID to
            // the next available sequential region ID.
            currRegionID++;
            final int oldID = region.id;
            // Search all regions for ones using the old region ID.
            for (Region r : regions)
            {
                if (r.id == oldID)
                {
                    // This region uses the old ID.
                    // Re-assign it to the new ID.
                    r.id = currRegionID;
                    r.remap = false;  // Don't consider this region again.
                }
            }
        }
        // Update the number of regions in the field.
        // Add one to account for null region.
        field.setRegionCount(currRegionID+1);
        
        // Finally, update the span region ID's to their final values.
        iter.reset();
        while (iter.hasNext())
        {
            final OpenHeightSpan span = iter.next();
            if (span.regionID() == 0)
                // Leave null regions alone.
                continue;
            else
                // Re-map by getting the region object at the old index
                // and assigning its current region id to the span.
                span.setRegionID(regions[span.regionID()].id);
        }

    }

    /**
     * Determines if two regions can be legally merged.
     * <p>Regions cannot be merged if:</p>
     * <ul>
     * <li>They do not connect.</li>
     * <li>They connect at more than one point.</li>
     * <li>They overlap vertically.</li>
     * </ul>
     * <p>Behavior is undefined if either region is a null region.</p>
     * @param regionA A region connected with regionB.
     * @param regionB A region connected with regionA.
     * @return TRUE if the two regions can be merged.  Otherwise FALSE.
     */
    private static boolean canMerge(Region regionA, Region regionB)
    {
        // Only checking connections from A to B since checking B to A would
        // be redundant.
        int connectionsAB = 0;
        for (Integer connectionID : regionA.connections)
        {
            if (connectionID == regionB.id)
                // Connection detected.
                connectionsAB++;
        }
        if (connectionsAB != 1)
            /*
             * Either the regions are not connected or there is a connection
             * in more than one location. So a valid simple polygon can't
             * be formed if the regions are merged.
             */
            return false;
        
        // Can't merge regions that overlap vertically.
        // This check needs to be checked in both directions due to the way
        // the data is built.
        if (regionA.overlappingRegions.contains(regionB.id))
            // Region B overlaps region A.  Can't merge.
            return false;
        if (regionB.overlappingRegions.contains(regionA.id))
            // Region A overlaps region B.  Can't merge.
            return false;
        
        // OK to merge.
        return true;
    }
    
    /**
     * Walks the edge of a region adding all neighbor region connections to
     * a connection array.
     * <p>WARNING: Only run this operation on spans that are already known
     * to be on a region edge. The direction must also be pointing to
     * a valid edge.</p>
     * @param startSpan  A span that is known to be on the edge of a region.
     * @param startDirection The direction of the edge of the span that is
     * known to point across the region edge.
     * @param outConnections  A reference to the region's connection object.
     * This object will be filled with connection data.
     */
    private static void findRegionConnections(OpenHeightSpan startSpan
            , int startDirection
            , ArrayList<Integer> outConnections)
    {
        
        /*
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
         */
        
        OpenHeightSpan span = startSpan;
        int dir = startDirection;
        // Default in case no neighbor exists.
        int lastEdgeRegionID = NULL_REGION;

        // Add the first known connection.
        OpenHeightSpan nSpan = span.getNeighbor(dir);
        if (nSpan != null)
            // Found a neighbor in this direction.  Use it's region ID.
            lastEdgeRegionID = nSpan.regionID();
        // Add the first region ID to the list.
        outConnections.add(lastEdgeRegionID);
        
        int loopCount = 0;
        /*
         * The loop limit is arbitrary.  It exists only to guarantee that bad
         * input data doesn't result in an infinite loop. The only down side
         * of this loop limit is that it limits the number  of detectable
         * region connections.  (The longer the region edge and the higher
         * the number of "turns"  in a region's edge, the less connections
         * can be detected for that region.)
         */
        while (++loopCount < 65534)
        {
            
            // NOTE: The design of this loop is such that the span variable
            // will always  reference a span from the same region as the
            // start span.
            
            nSpan = span.getNeighbor(dir);
            // Default in case no neighbor exists.
            int currEdgeRegionID = NULL_REGION;
            if (nSpan == null || nSpan.regionID() != span.regionID())
            {
                // The current direction points across a region edge.
                if (nSpan != null)
                    // There is a span in this direciton.  Get its region ID.
                    currEdgeRegionID = nSpan.regionID();
                if (currEdgeRegionID != lastEdgeRegionID)
                {
                    // The region across this edge is different from
                    // the last edge region detected.  (Is a transition to a
                    // new region connection.) Add it to the list.
                    outConnections.add(currEdgeRegionID);
                    lastEdgeRegionID = currEdgeRegionID;
                }
                
                dir = (dir+1) & 0x3; // Rotate in clockwise direction.

            }
            else
            {
                /*
                 * The current direction points to a neighbor in the same
                 * region as the current span.  (And therefore in the same
                 * region as the  start span.)
                 * Move to the neighbor and swing the search direction back
                 * one increment (counterclockwise).  By moving the direction
                 * back one increment we guarantee we don't miss any edges.
                 */
                span = nSpan;
                dir = (dir+3) & 0x3; // Rotate counterclockwise direction.
            }
            
            if (startSpan == span && startDirection == dir)
                // Have returned to the original span and direction.
                // The search is complete.
                break;
        }
        
        // Make sure the first and last regions are not the same.  This is
        // the only type of connection adjacency that has the possibility
        // of existing.
        if (outConnections.size() > 1
                && outConnections.get(0)
                    .equals(outConnections.get(outConnections.size() - 1)))
            // The fist and last connection is the same.
            // Remove the last connection.
            outConnections.remove(outConnections.size() - 1);

    }
    
    /**
     * Checks to see if a span is on a region edge.  If so, returns the
     * direction of the neighbor region.
     * <p>This is an axis-neighbor search.  Diagonal neighbors are not
     * considered.</p>
     * <p>The direction of the first detected neighbor region is
     * returned.</p>
     * @param span The span to check.
     * @return The direction of span edge that lies along a neighbor region,
     * or -1 if the span is not on a region edge.
     */
    private static int getRegionEdgeDirection(OpenHeightSpan span)
    {
        // Search axis-neighbors.
        for (int dir = 0; dir < 4; ++dir)
        {
            OpenHeightSpan nSpan = span.getNeighbor(dir);
            if (nSpan == null || nSpan.regionID() != span.regionID())
                // Doesn't have a neighbor or its neighbor is in a
                // different region.
                return dir;
        }
        // All neighbors are in the same region.
        return -1;
    }
    

    /**
     * Merges the candidate region into the target region.
     * <p>Only the target region is updated.  The candidate is not altered.
     * So if the candidate is to be discarded, it should be cleaned up after
     * this operation successfully completes.</p>
     * <p>IMPORTANT: The provided regions should already have been checked
     * to ensure they are valid for merging. E.g. Using
     * {@link #canMerge(Region, Region)}. Otherwise, behavior will be undefined.
     * The only time this operation will detect a failure and abort
     * (return false) is if the failure check is inherent to the merge
     * process.</p>
     * @param target The region to merge the candidate into.
     * @param candidate The region to merge into the target.
     * @return TRUE if the spans in the candidate region were successfully
     * added to the target region.
     */
    private static boolean mergeRegions(Region target, Region candidate)
    {
        
        // Get connection indices for target and candidate.
        // (Where the two regions connect.)
        final int connectionPointOnTarget =
            target.connections.indexOf(candidate.id);
        if (connectionPointOnTarget == -1)
            // The target knows of no connection between the regions.
            return false;
        final int connectionPointOnCandidate =
            candidate.connections.indexOf(target.id);
        if (connectionPointOnCandidate == -1)
            // The candidate knows of no connection between the regions.
            return false;
        
        // Merging candidate into target.  So need to save original target
        // connections prior to the rebuilding them.
        final ArrayList<Integer> targetConns =
            new ArrayList<Integer>(target.connections);
        
        /*
         *  Merge connection information.
         * 
         *  Step 1: Rebuild the target connections.
         * 
         *  Start from the point just past where the candidate connection
         *  exists and loop back until just before the candidate connection.
         *  Scenario:
         *      Original target connections are 0, 2, 3, 4, 5
         *      Merging with region 2.
         *  Then:
         *      Rebuild starting at index 2 and stop building at
         *      index 0 to get: 3, 4, 5, 0.
         */
        target.connections.clear();
        int workingSize = targetConns.size();
        for (int i = 0; i < workingSize - 1; i++)
        {
            // The modulus calculation is what results in the wrapping.
            target.connections.add(targetConns.get(
                    (connectionPointOnTarget + 1 + i) % workingSize));
        }
        /*
         * Step 2: Insert candidate connections into target connections at
         * their mutual connection point.
         * 
         * Same extraction process as for step one, but inserting data
         * into target connection data.
         * 
         * Scenario:
         *         Target connections after step 1 are 3, 4, 5, 0
         *      Candidate connections are 3, 1, 0
         *      Target region id is 1.
         *  Then:
         *      The loop will insert 0, 3 from the candidate at the end of
         *      the target connections.
         *      The final target connections:  3, 4, 5, 0, 0, 3
         *  Note that this process can result in adjacent duplicate
         *  connections which will be fixed later.
         */
        workingSize = candidate.connections.size();
        for (int i = 0; i < workingSize - 1; i++)
        {
            target.connections.add(candidate.connections.get(
                    (connectionPointOnCandidate + 1 + i) % workingSize));
        }
        /*
         * Step 3: Get rid of any adjacent duplicate connections that may
         * have been created.
         */
        removeAdjacentDuplicateConnections(target);
        
        // Add overlap data from the candidate to the target.
        for (Integer i : candidate.overlappingRegions)
        {
            if (!target.overlappingRegions.contains(i))
                // This is a new overlap.  Add it to the target.
                target.overlappingRegions.add(i);
        }

        // Merge span counts.
        target.spanCount += candidate.spanCount;
        
        return true;
        
    }
    
    /**
     * If a regions connection list contains adjacent duplicate connections,
     * this operation removes them. Example of a cleanup:
     * 3, 4, 5, 0, 0, 3 -> 4, 5, 0, 3
     * @param region The region to cleanup.
     */
    private static void removeAdjacentDuplicateConnections(Region region)
    {
        int iConnection = 0;
        // Loop through all adjacent connections.
        while (iConnection < region.connections.size()
                && region.connections.size() > 1)
        {
            int iNextConnection = iConnection+1;
            if (iNextConnection >= region.connections.size())
                // Need to loop back to zero.
                iNextConnection = 0;
            if (region.connections.get(iConnection)
                .equals(region.connections.get(iNextConnection)))
            {
                // Found duplicate.
                // Remove duplicate and stay at current index.
                region.connections.remove(iNextConnection);
            }
            else
                // Move to next connection.
                 iConnection++;
        }
    }
    
    /**
     * Searches through a region's connection and overlap information
     * for any references to an old region ID and replaces with the new ID.
     * @param region The region to perform the search and replace on.
     * @param oldID The old region ID to replace.
     * @param newID The ID to replace the old ID with.
     */
    private static void replaceNeighborRegionID(Region region
            , int oldID
            , int newID)
    {
        boolean connectionsChanged = false;
        // Perform the search and replace on the region connections.
        for (int i = 0; i < region.connections.size(); i++)
        {
            if (region.connections.get(i) == oldID)
            {
                // Found a match.  Replace it.
                region.connections.set(i, newID);
                connectionsChanged = true;
            }
        }
        // Perform a search and replace on the overlap regions.
        for (int i = 0; i < region.overlappingRegions.size(); i++)
        {
            if (region.overlappingRegions.get(i) == oldID)
                // Found a match.  Replace it.
                region.overlappingRegions.set(i, newID);
        }
        if (connectionsChanged)
            // Connections changed.  This might have resulted in
            // two connections being merged into one.  Search for and
            // fix as needed.
            removeAdjacentDuplicateConnections(region);
    }
    
}
