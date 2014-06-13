package org.critterai.nmgen;

import static org.junit.Assert.*;
import static org.critterai.nmgen.OpenHeightFieldUtil.*;
import org.junit.Test;

/**
 * Test the detection and resolution of a special case
 * where a region only touches a null region at the tip
 * of an outer corner.  This can result in failure to detect
 * a contour connection to the null region.
 * 
 * Governing pattern:
 * 
 *     b a
 *     a x
 */
public class NullRegionOuterCornerTipTests
{

    /**
     * Tests a outer corner tip scenario:
     * 
     *     b b a a
     *     b b a a
     *     a a x x
     *     a a x x
     */
    @Test
    public void testQuadrantA()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(0, 4).setRegionID(2);
        field.getData(0, 5).setRegionID(2);
        field.getData(1, 4).setRegionID(2);
        field.getData(1, 5).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        // Only checking in vicinity of expected change.
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 4).regionID() == 2);
        assertTrue(field.getData(0, 5).regionID() == 2);
        assertTrue(field.getData(1, 4).regionID() == 2);
        assertTrue(field.getData(1, 5).regionID() == 2);
        assertTrue(field.getData(0, 3).regionID() == 1);
        assertTrue(field.getData(2, 5).regionID() == 1);
        
        assertTrue((field.getData(1, 3).regionID() == 1
                        && field.getData(2, 4).regionID() == 2)
                || (field.getData(1, 3).regionID() == 2
                        && field.getData(2, 4).regionID() == 1));
    }
    
    /**
     * Tests a outer corner tip scenario:
     * 
     *     a b b b
     *     a a b a
     *     x x a a
     *     x x a a
     */
    @Test
    public void testQuadrantB()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(3, 5).setRegionID(2);
        field.getData(4, 4).setRegionID(2);
        field.getData(4, 5).setRegionID(2);
        field.getData(5, 5).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        // Only checking in vicinity of expected change.
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(3, 5).regionID() == 2);
        assertTrue(field.getData(4, 4).regionID() == 2);
        assertTrue(field.getData(4, 5).regionID() == 2);
        assertTrue(field.getData(5, 5).regionID() == 2);
        assertTrue(field.getData(5, 4).regionID() == 1);
        assertTrue(field.getData(4, 3).regionID() == 1);
        assertTrue(field.getData(5, 3).regionID() == 1);
        assertTrue(field.getData(3, 4).regionID() == 2); 
    }
    
    /**
     * Tests a outer corner tip scenario:
     * 
     *     x x a a
     *     x x a b
     *     a a b b
     *     a b b b
     */
    @Test
    public void testQuadrantC()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(3, 0).setRegionID(2);
        field.getData(4, 0).setRegionID(2);
        field.getData(4, 1).setRegionID(2);
        field.getData(5, 0).setRegionID(2);
        field.getData(5, 1).setRegionID(2);
        field.getData(5, 2).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        // Only checking in vicinity of expected change.
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(3, 0).regionID() == 2);
        assertTrue(field.getData(4, 0).regionID() == 2);
        assertTrue(field.getData(4, 1).regionID() == 2);
        assertTrue(field.getData(5, 0).regionID() == 2);
        assertTrue(field.getData(5, 1).regionID() == 2);
        assertTrue(field.getData(5, 2).regionID() == 2);
        
        assertTrue((field.getData(3, 1).regionID() == 1
                        && field.getData(4, 2).regionID() == 2)
                || (field.getData(3, 1).regionID() == 2
                        && field.getData(4, 2).regionID() == 1));
    }
    
    /**
     * Tests a outer corner tip scenario:
     * 
     *     a a x x
     *     b a x x
     *     b b a a
     *     b a a a
     */
    @Test
    public void testQuadrantD()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionPatch();
        
        field.getData(0, 0).setRegionID(2);
        field.getData(0, 1).setRegionID(2);
        field.getData(0, 2).setRegionID(2);
        field.getData(1, 1).setRegionID(2);
        field.setRegionCount(3);
        
        algo.apply(field);
        
        // Only checking in vicinity of expected change.
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 0).regionID() == 2);
        assertTrue(field.getData(0, 1).regionID() == 2);
        assertTrue(field.getData(0, 2).regionID() == 2);
        assertTrue(field.getData(1, 1).regionID() == 2);
        assertTrue(field.getData(1, 0).regionID() == 1);
        assertTrue(field.getData(2, 0).regionID() == 1);
        assertTrue(field.getData(2, 1).regionID() == 1);
        assertTrue(field.getData(1, 2).regionID() == 2);
    }

}
