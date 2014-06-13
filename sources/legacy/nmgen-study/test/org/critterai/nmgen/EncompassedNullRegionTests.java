package org.critterai.nmgen;

import static org.junit.Assert.*;
import static org.critterai.nmgen.OpenHeightFieldUtil.*;

import org.critterai.nmgen.OpenHeightfield.OpenHeightFieldIterator;
import org.junit.Test;

/**
 * Tests the ability of {@link CleanNullRegionBorders} to detect
 * and fix null regions that are fully encompassed by a single
 * non-null region.
 * <p>This is a partial test of {@link CleanNullRegionBorders}
 * functionality.</p>
 */
public class EncompassedNullRegionTests
{

    /*
     * Design notes:
     * 
     * These tests make assumptions on the way the OpenHeightField
     * iterates.  Specifically that it is a width first search.
     * 
     * The "all span" searches inherently validate the detection of
     * outer encompassing null regions.
     */
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;

    /**
     * Checks for proper region re-assignment for a fully
     * encompassed null region. Search restricted to null spans. (A)
     */
    @Test
    public void testEncompassedNullARestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionA();
        
        algo.apply(field);
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 0).regionID() == 2);
        assertTrue(field.getData(0, 1).regionID() == 2);
        assertTrue(field.getData(0, 2).regionID() == 2);
        assertTrue(field.getData(0, 3).regionID() == 2);
        assertTrue(field.getData(0, 4).regionID() == 2);
        assertTrue(field.getData(1, 0).regionID() == 2);
        assertTrue(field.getData(1, 1).regionID() == 2);
        assertTrue(field.getData(1, 2).regionID() == 2);
        assertTrue(field.getData(1, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(1, 4).regionID() == 2);
        assertTrue(field.getData(2, 0).regionID() == 2);
        assertTrue(field.getData(2, 1).regionID() == 2);
        assertTrue(field.getData(2, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 4).regionID() == 2);
        assertTrue(field.getData(3, 0).regionID() == 1);
        assertTrue(field.getData(3, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 4).regionID() == 1);
        assertTrue(field.getData(4, 0).regionID() == 1);
        assertTrue(field.getData(4, 1).regionID() == 1);
        assertTrue(field.getData(4, 2).regionID() == 1);
        assertTrue(field.getData(4, 3).regionID() == 1);
        assertTrue(field.getData(4, 4).regionID() == 1);
    }
    
    /**
     * Checks for proper region re-assignment for a fully
     * encompassed null region. Search all spans. (A)
     */
    @Test
    public void testEncompassedNullA()
    {
        
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionA();
        
        algo.apply(field);
        
        // field.printRegionField();
        
        assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 0).regionID() == 2);
        assertTrue(field.getData(0, 1).regionID() == 2);
        assertTrue(field.getData(0, 2).regionID() == 1);
        assertTrue(field.getData(0, 3).regionID() == 1);
        assertTrue(field.getData(0, 4).regionID() == 1);
        assertTrue(field.getData(1, 0).regionID() == 2);
        assertTrue(field.getData(1, 1).regionID() == 2);
        assertTrue(field.getData(1, 2).regionID() == 1);
        assertTrue(field.getData(1, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(1, 4).regionID() == 1);
        assertTrue(field.getData(2, 0).regionID() == 2);
        assertTrue(field.getData(2, 1).regionID() == 2);
        assertTrue(field.getData(2, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 4).regionID() == 1);
        assertTrue(field.getData(3, 0).regionID() == 2);
        assertTrue(field.getData(3, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 4).regionID() == 1);
        assertTrue(field.getData(4, 0).regionID() == 2);
        assertTrue(field.getData(4, 1).regionID() == 2);
        assertTrue(field.getData(4, 2).regionID() == 1);
        assertTrue(field.getData(4, 3).regionID() == 1);
        assertTrue(field.getData(4, 4).regionID() == 1);
    }
    
    /**
     * Tests proper detection of a null region that encompasses
     * a single region. Search restricted to null spans. (A)
     */
    @Test
    public void testExternalNullRegionARestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionA();
        OpenHeightfield control = getEncompassedNullRegionA();
        
        invertRegion(field);
        invertRegion(control);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that encompasses
     * a single region. Search all spans. (A)
     */
    @Test
    public void testExternalNullRegionA()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionA();
        OpenHeightfield control = getEncompassedNullRegionA();
        
        invertRegion(field);
        invertRegion(control);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that is
     * encompassed by more than one non-null region. 
     * Search restricted to null spans. (A)
     */
    @Test
    public void testMultiRegionARestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionA();
        OpenHeightfield control = getEncompassedNullRegionA();
        
        // On an inner corner.
        field.getData(1, 2).setRegionID(2);
        control.getData(1, 2).setRegionID(2);
        field.setRegionCount(3);
        control.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that is
     * encompassed by more than one non-null region. 
     * Search all spans. (A)
     */
    @Test
    public void testMultiRegionA()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionA();
        OpenHeightfield control = getEncompassedNullRegionA();
        
        // On an inner corner.
        field.getData(1, 2).setRegionID(2);
        control.getData(1, 2).setRegionID(2);
        field.setRegionCount(3);
        control.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Checks for proper region re-assignment for a fully
     * encompassed null region. 
     * Search restricted to null spans. (B)
     */
    @Test
    public void testEncompassedNullBRestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionB();
        
        algo.apply(field);
        
       assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 0).regionID() == 2);
        assertTrue(field.getData(0, 1).regionID() == 2);
        assertTrue(field.getData(0, 2).regionID() == 2);
        assertTrue(field.getData(0, 3).regionID() == 2);
        assertTrue(field.getData(0, 4).regionID() == 2);
        assertTrue(field.getData(1, 0).regionID() == 2);
        assertTrue(field.getData(1, 1).regionID() == 2);
        assertTrue(field.getData(1, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(1, 3).regionID() == 2);
        assertTrue(field.getData(1, 4).regionID() == 2);
        assertTrue(field.getData(2, 0).regionID() == 1);
        assertTrue(field.getData(2, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 2) == null);
        assertTrue(field.getData(2, 3).regionID() == 1);
        assertTrue(field.getData(2, 4).regionID() == 1);
        assertTrue(field.getData(3, 0).regionID() == 1);
        assertTrue(field.getData(3, 1).regionID() == 1);
        assertTrue(field.getData(3, 2) == null);
        assertTrue(field.getData(3, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 4).regionID() == 1);
        assertTrue(field.getData(4, 0).regionID() == 1);
        assertTrue(field.getData(4, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(4, 2) == null);
        assertTrue(field.getData(4, 3).regionID() == 1);
        assertTrue(field.getData(4, 4).regionID() == 1);
        assertTrue(field.getData(5, 0).regionID() == 1);
        assertTrue(field.getData(5, 1).regionID() == 1);
        assertTrue(field.getData(5, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(5, 3).regionID() == 1);
        assertTrue(field.getData(5, 4).regionID() == 1);
        assertTrue(field.getData(6, 0).regionID() == 1);
        assertTrue(field.getData(6, 1).regionID() == 1);
        assertTrue(field.getData(6, 2).regionID() == 1);
        assertTrue(field.getData(6, 3).regionID() == 1);
        assertTrue(field.getData(6, 4).regionID() == 1);
    }
    
    /**
     * Checks for proper region re-assignment for a fully
     * encompassed null region. 
     * Search all spans. (B)
     */
    @Test
    public void testEncompassedNullB()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionB();
        
        algo.apply(field);
        
       assertTrue(field.regionCount() == 3);
        
        assertTrue(field.getData(0, 0).regionID() == 2);
        assertTrue(field.getData(0, 1).regionID() == 2);
        assertTrue(field.getData(0, 2).regionID() == 1);
        assertTrue(field.getData(0, 3).regionID() == 1);
        assertTrue(field.getData(0, 4).regionID() == 1);
        assertTrue(field.getData(1, 0).regionID() == 2);
        assertTrue(field.getData(1, 1).regionID() == 2);
        assertTrue(field.getData(1, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(1, 3).regionID() == 1);
        assertTrue(field.getData(1, 4).regionID() == 1);
        assertTrue(field.getData(2, 0).regionID() == 2);
        assertTrue(field.getData(2, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(2, 2) == null);
        assertTrue(field.getData(2, 3).regionID() == 1);
        assertTrue(field.getData(2, 4).regionID() == 1);
        assertTrue(field.getData(3, 0).regionID() == 2);
        assertTrue(field.getData(3, 1).regionID() == 2);
        assertTrue(field.getData(3, 2) == null);
        assertTrue(field.getData(3, 3).regionID() == NULL_REGION);
        assertTrue(field.getData(3, 4).regionID() == 1);
        assertTrue(field.getData(4, 0).regionID() == 2);
        assertTrue(field.getData(4, 1).regionID() == NULL_REGION);
        assertTrue(field.getData(4, 2) == null);
        assertTrue(field.getData(4, 3).regionID() == 1);
        assertTrue(field.getData(4, 4).regionID() == 1);
        assertTrue(field.getData(5, 0).regionID() == 2);
        assertTrue(field.getData(5, 1).regionID() == 2);
        assertTrue(field.getData(5, 2).regionID() == NULL_REGION);
        assertTrue(field.getData(5, 3).regionID() == 1);
        assertTrue(field.getData(5, 4).regionID() == 1);
        assertTrue(field.getData(6, 0).regionID() == 2);
        assertTrue(field.getData(6, 1).regionID() == 2);
        assertTrue(field.getData(6, 2).regionID() == 1);
        assertTrue(field.getData(6, 3).regionID() == 1);
        assertTrue(field.getData(6, 4).regionID() == 1);
    }
    
    /**
     * Tests proper detection of a null region that encompasses
     * a single region. Search restricted to null spans.
     * (B)
     */
    @Test
    public void testIntertedBRestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionB();
        OpenHeightfield control = getEncompassedNullRegionB();
        
        invertRegion(field);
        invertRegion(control);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that encompasses
     * a single region. Search all spans.
     * (B)
     */
    @Test
    public void testIntertedB()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionB();
        OpenHeightfield control = getEncompassedNullRegionB();
        
        invertRegion(field);
        invertRegion(control);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that is
     * encompassed by more than one non-null region. 
     * Search restricted to null regions. (B)
     */
    @Test
    public void testMultiRegionBRestricted()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(true);
        OpenHeightfield field = getEncompassedNullRegionB();
        OpenHeightfield control = getEncompassedNullRegionB();
        
        // One an outer corner.
        field.getData(5, 3).setRegionID(2);
        control.getData(5, 3).setRegionID(2);
        field.setRegionCount(3);
        control.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * Tests proper detection of a null region that is
     * encompassed by more than one non-null region. 
     * Search all spans. (B)
     */
    @Test
    public void testMultiRegionB()
    {
        CleanNullRegionBorders algo = new CleanNullRegionBorders(false);
        OpenHeightfield field = getEncompassedNullRegionB();
        OpenHeightfield control = getEncompassedNullRegionB();
        
        // One an outer corner.
        field.getData(5, 3).setRegionID(2);
        control.getData(5, 3).setRegionID(2);
        field.setRegionCount(3);
        control.setRegionCount(3);
        
        algo.apply(field);
        
        assertTrue(isSameRegionLayout(field, control));
    }
    
    /**
     * A null region fully encompassed by a single region. (RegionID = 1)
     */
    private static OpenHeightfield getEncompassedNullRegionA()
    {
        /*
         *            W
         *        0 1 2 3 4
         *        ---------
         *    4 | a a a a a
         *    3 | a x x x a    x - null region span
         *  D 2 | a a x x a    a - region 1 span
         *    1 | a a a x a    All linked.
         *    0 | a a a a a
         * 
         */
        
        float[] gridBoundsMin = { -1, -2, -3 };
        float[] gridBoundsMax = { 11, 12, 13 };
        
        OpenHeightfield field = new OpenHeightfield(gridBoundsMin
                , gridBoundsMax
                , 0.2f
                , 0.1f);
        
        for (int w = 0; w < 5; w++)
        {
            for (int d = 0; d < 5; d++)
            {
                OpenHeightSpan span = new OpenHeightSpan(w, d + 1);
                span.setRegionID(1);
                field.addData(w, d, span);
            }
        }
        linkAllBaseSpans(field);
        
        field.getData(1, 3).setRegionID(NULL_REGION);
        field.getData(2, 3).setRegionID(NULL_REGION);
        field.getData(3, 3).setRegionID(NULL_REGION);
        field.getData(2, 2).setRegionID(NULL_REGION);
        field.getData(3, 2).setRegionID(NULL_REGION);
        field.getData(3, 1).setRegionID(NULL_REGION);
        
        field.setRegionCount(2);
        
        return field;
    }
    
    /**
     * A null region fully encompassed by a single region. (RegionID = 1)
     */
    private static OpenHeightfield getEncompassedNullRegionB()
    {
        /*
         *              W
         *        0 1 2 3 4 5 6 7
         *        ---------------
         *    4 | a a a a a a a a
         *    3 | a a a x a a a a    x - null region span
         *  D 2 | a x v v v x a a    v - null region, no span.
         *    1 | a a x a x a a a    a - region 1 span
         *    0 | a a a a a a a a    All linked.
         * 
         */
        
        float[] gridBoundsMin = { -1, -2, -3 };
        float[] gridBoundsMax = { 11, 12, 13 };
        
        OpenHeightfield field = new OpenHeightfield(gridBoundsMin
                , gridBoundsMax
                , 0.2f
                , 0.1f);
        
        for (int w = 0; w < 8; w++)
        {
            for (int d = 0; d < 5; d++)
            {
                if (w == 2 && d == 2
                        || w == 3 && d == 2
                        || w == 4 && d == 2)
                    continue;
                OpenHeightSpan span = new OpenHeightSpan(w, d + 1);
                span.setRegionID(1);
                field.addData(w, d, span);
            }
        }
        linkAllBaseSpans(field);
        
        field.getData(1, 2).setRegionID(NULL_REGION);
        field.getData(2, 1).setRegionID(NULL_REGION);
        field.getData(3, 3).setRegionID(NULL_REGION);
        field.getData(4, 1).setRegionID(NULL_REGION);
        field.getData(5, 2).setRegionID(NULL_REGION);
        
        field.setRegionCount(2);
        
        return field;
    }
    
    /**
     * Changes all {@link OpenHeightSpan#NULL_REGION} spans
     * to "1" and all spans in region "1" to {@link OpenHeightSpan#NULL_REGION}.
     */
    private static void invertRegion(OpenHeightfield field)
    {
        OpenHeightFieldIterator iter = field.dataIterator();

        while (iter.hasNext())
        {
            OpenHeightSpan span = iter.next();
            if (span.regionID() == NULL_REGION)
                span.setRegionID(1);
            else
                span.setRegionID(NULL_REGION);
        }
        
    }

}
