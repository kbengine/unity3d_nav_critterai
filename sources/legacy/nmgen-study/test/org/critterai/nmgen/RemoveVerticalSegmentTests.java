package org.critterai.nmgen;

import static org.junit.Assert.assertTrue;

import java.util.ArrayList;

import org.junit.Test;
import org.junit.Before;

/**
 * Tests related to the ContourSetBuilder.removeVerticalSegments()
 * operation.
 */
public class RemoveVerticalSegmentTests
{
    /*
     * Design notes:
     * 
     * These tests are simplified to assume that the second
     * vertex in a vertical segment is removed.
     */
    private static final int REGION_S = 58;
    
    private final ArrayList<Integer> mBadQuad = new ArrayList<Integer>(16);
    
    @Before
    public void setup()
    {
        
        /*
         * Base Quad
         * 
         *     1/2 -  3
         *      |     |
         *      0  - 4/5
         */
        
        mBadQuad.add(1);
        mBadQuad.add(5);
        mBadQuad.add(2);
        mBadQuad.add(0);
        
        mBadQuad.add(1);
        mBadQuad.add(5);
        mBadQuad.add(3);
        mBadQuad.add(0);
        
        mBadQuad.add(1);
        mBadQuad.add(6);
        mBadQuad.add(3);
        mBadQuad.add(0);
        
        mBadQuad.add(2);
        mBadQuad.add(5);
        mBadQuad.add(3);
        mBadQuad.add(0);
        
        mBadQuad.add(2);
        mBadQuad.add(5);
        mBadQuad.add(2);
        mBadQuad.add(0);
        
        mBadQuad.add(2);
        mBadQuad.add(4);
        mBadQuad.add(2);
        mBadQuad.add(0);
    }
    
    @Test
    public void testQuadOffset0()
    {
        
        ContourSetBuilder.removeVerticalSegments(REGION_S, mBadQuad);
        assertTrue(mBadQuad.size() == 16);
        
        assertTrue(mBadQuad.get(0) == 1);
        assertTrue(mBadQuad.get(2) == 2);
        
        assertTrue(mBadQuad.get(4) == 1);
        assertTrue(mBadQuad.get(6) == 3);
        
        assertTrue(mBadQuad.get(8) == 2);
        assertTrue(mBadQuad.get(10) == 3);
        
        assertTrue(mBadQuad.get(12) == 2);
        assertTrue(mBadQuad.get(14) == 2);
    }
    
    @Test
    public void testQuadOffset1()
    {
        // This test assumes that the second vertex is removed.
        
        ContourUtil.shiftContour(mBadQuad);
        ContourSetBuilder.removeVerticalSegments(REGION_S, mBadQuad);
        assertTrue(mBadQuad.size() == 16);
        
        assertTrue(mBadQuad.get(0) == 1);
        assertTrue(mBadQuad.get(2) == 2);
        
        assertTrue(mBadQuad.get(4) == 1);
        assertTrue(mBadQuad.get(6) == 3);
        
        assertTrue(mBadQuad.get(8) == 2);
        assertTrue(mBadQuad.get(10) == 3);
        
        assertTrue(mBadQuad.get(12) == 2);
        assertTrue(mBadQuad.get(14) == 2);
    }
    
    @Test
    public void testQuadOffset2()
    {
        // This test assumes that the second vertex is removed.
        
        ContourUtil.shiftContour(mBadQuad);
        ContourUtil.shiftContour(mBadQuad);
        ContourSetBuilder.removeVerticalSegments(REGION_S, mBadQuad);
        assertTrue(mBadQuad.size() == 16);
        
        assertTrue(mBadQuad.get(0) == 2);
        assertTrue(mBadQuad.get(2) == 2);
        
        assertTrue(mBadQuad.get(4) == 1);
        assertTrue(mBadQuad.get(6) == 2);
        
        assertTrue(mBadQuad.get(8) == 1);
        assertTrue(mBadQuad.get(10) == 3);
        
        assertTrue(mBadQuad.get(12) == 2);
        assertTrue(mBadQuad.get(14) == 3);
    }
    
}
