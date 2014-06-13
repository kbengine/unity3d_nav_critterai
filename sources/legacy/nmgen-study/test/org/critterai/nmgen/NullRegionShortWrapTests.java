package org.critterai.nmgen;

import static org.junit.Assert.assertTrue;
import static org.critterai.nmgen.OpenHeightFieldUtil.*;

import org.junit.Test;

/**
 * Test the detection and resolution of a special case
 * where a region wraps an outer corner of a null region
 * This can result in self-intersecting polygons during polygon
 * generation.
 * 
 * Governing pattern:
 * 
 *     a a
 *     a x
 */
public class NullRegionShortWrapTests
{
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;

    @Test
    public void testNoPreferenceSelection()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(0, 4).setRegionID(2);
        field.getData(0, 5).setRegionID(2);
        field.getData(1, 3).setRegionID(2);
        field.getData(1, 4).setRegionID(2);
        field.getData(1, 5).setRegionID(2);
        field.getData(2, 4).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        // Only checking in vicinity of expected change.
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 4).regionID() == 2);
        assertTrue(field.getData(0, 5).regionID() == 2);
        assertTrue(field.getData(1, 4).regionID() == 2);
        assertTrue(field.getData(1, 5).regionID() == 2);
        
        assertTrue((field.getData(1, 3).regionID() == 1
                        && field.getData(2, 4).regionID() == 2)
                || (field.getData(1, 3).regionID() == 2
                        && field.getData(2, 4).regionID() == 1));
        
    }
    
    @Test
    public void testNoAlternative()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(3, 4).setRegionID(2);
        field.getData(3, 5).setRegionID(NULL_REGION);
        field.getData(4, 3).setRegionID(2);
        field.getData(4, 4).setRegionID(2);
        field.getData(5, 3).setRegionID(2);
        field.getData(5, 4).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(field.regionCount() == 3);
       
        // No change expected.
        assertTrue(field.getData(3, 4).regionID() == 2);
        assertTrue(field.getData(3, 5).regionID() == NULL_REGION);
        assertTrue(field.getData(4, 3).regionID() == 2);
        assertTrue(field.getData(4, 4).regionID() == 2);
        assertTrue(field.getData(5, 3).regionID() == 2);
        assertTrue(field.getData(5, 4).regionID() == 2);
        
    }
    
    @Test
    public void testNoBreakAllowed()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
       OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(2, 1).setRegionID(2);
        field.getData(3, 1).setRegionID(2);
        field.getData(4, 1).setRegionID(2);
        field.getData(4, 2).setRegionID(2);
        field.getData(4, 3).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(field.regionCount() == 3);
       
        // No change expected.
        assertTrue(field.getData(2, 1).regionID() == 2);
        assertTrue(field.getData(3, 1).regionID() == 2);
        assertTrue(field.getData(4, 1).regionID() == 2);
        assertTrue(field.getData(4, 2).regionID() == 2);
        assertTrue(field.getData(4, 3).regionID() == 2);
    }
    
    
}
