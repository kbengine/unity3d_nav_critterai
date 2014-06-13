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
package org.critterai.math.geom;

import static org.critterai.math.geom.Rectangle2.contains;
import static org.critterai.math.geom.Rectangle2.intersectsAABB;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import org.critterai.math.MathUtil;
import org.critterai.math.geom.Rectangle2;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Rectangle2} class.
 */
public class Rectangle2Tests {

    private static final float XMIN = -3;
    private static final float YMIN = 2;
    private static final float XMAX = 2;
    private static final float YMAX = 6;
    
    private static final float TOLERANCE = MathUtil.TOLERANCE_STD;
    private static final float OFFSET = 1.5f;
    
    @Before
    public void setUp() 
        throws Exception 
    {
        
    }

    @Test
    public void testContainsPoint() 
    {
        
        // Wall tests.
        
        // On x min bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN, YMIN + OFFSET));
        // On y min bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + OFFSET, YMIN));
        // On x max bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX , YMIN + OFFSET));
        // On y max bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN +  OFFSET, YMAX));
        
        // Inside x min bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + TOLERANCE, YMIN + OFFSET));
        // Inside y min bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + OFFSET, YMIN + TOLERANCE));
        // Inside x max bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX - TOLERANCE , YMIN + OFFSET));
        // Inside y max bounds.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + OFFSET, YMAX - TOLERANCE));
        
        // Outside x min bounds.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN - TOLERANCE, YMIN + OFFSET));
        // Outside y min bounds.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + OFFSET, YMIN - TOLERANCE));
        // Outside x max bounds.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX + TOLERANCE , YMIN + OFFSET));
        // Outside y max bounds.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN + OFFSET, YMAX + TOLERANCE));
        
        // Corner tests.
        
        // On minX/minY corner
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN, YMIN));
        // On minX/maxY corner
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN, YMAX));
        // On maxX/maxY corner.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX , YMAX));
        // On maxX/minY corner.
        assertTrue(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX, YMIN));
        
        // Outside minX/minY corner
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN - TOLERANCE, YMIN - TOLERANCE));
        // Outside minX/maxY corner
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMIN - TOLERANCE, YMAX + TOLERANCE));
        // Outside maxX/maxY corner.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX + TOLERANCE , YMAX + TOLERANCE));
        // Outside maxX/minY corner.
        assertFalse(contains(XMIN, YMIN
                , XMAX, YMAX
                , XMAX + TOLERANCE, YMIN - TOLERANCE));
        
    }

    @Test
    public void testContainsAABB() 
    {
        
        // A == B
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN, YMIN, XMAX, YMAX));
        // B contains A
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN - TOLERANCE, YMIN - TOLERANCE, XMAX + TOLERANCE, YMAX + TOLERANCE));
        // B slightly smaller than A.
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + TOLERANCE, YMIN + TOLERANCE, XMAX - TOLERANCE, YMAX - TOLERANCE));
        
        // X-axis wall tests
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN, YMIN + OFFSET, XMAX - TOLERANCE, YMAX - OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN, YMIN + OFFSET, XMAX + TOLERANCE, YMAX - OFFSET));
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + TOLERANCE, YMIN + OFFSET, XMAX, YMAX - OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN - TOLERANCE, YMIN + OFFSET, XMAX, YMAX - OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMIN + OFFSET, XMIN + TOLERANCE, YMAX - OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMAX + TOLERANCE, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));
        
        // Y-axis wall tests
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN, XMAX - OFFSET, YMAX - TOLERANCE));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN, XMAX - OFFSET, YMAX + TOLERANCE));
        assertTrue(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN + TOLERANCE, XMAX - OFFSET, YMAX));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN - TOLERANCE, XMAX - OFFSET, YMAX));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN + TOLERANCE));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMAX + TOLERANCE, XMAX - OFFSET, YMAX + OFFSET));
        
        // Corner tests
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMIN - OFFSET, XMIN + TOLERANCE, YMIN + TOLERANCE));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMAX - TOLERANCE, XMIN + TOLERANCE, YMAX + OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMAX - TOLERANCE, YMAX - TOLERANCE, XMAX + OFFSET, YMAX + OFFSET));
        assertFalse(contains(XMIN, YMIN, XMAX, YMAX
                , XMAX - TOLERANCE, YMIN - OFFSET, XMAX + OFFSET, YMIN + TOLERANCE));
        
    }
    
    // Leave this test in place until the associated code is permanently
    // removed from the rectangle class.
//    @Test
//    public void testContainsStd() 
//    {
//        // Wall tests.
//        
//        // On x min bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN, YMIN + OFFSET));
//        // On y min bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + OFFSET, YMIN));
//        // On x max bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX , YMIN + OFFSET));
//        // On y max bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN +  OFFSET, YMAX));
//        
//        // Inside x min bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + TOLERANCE, YMIN + OFFSET));
//        // Inside y min bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + OFFSET, YMIN + TOLERANCE));
//        // Inside x max bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX - TOLERANCE , YMIN + OFFSET));
//        // Inside y max bounds.
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + OFFSET, YMAX - TOLERANCE));
//        
//        // Outside x min bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN - TOLERANCE, YMIN + OFFSET));
//        // Outside y min bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + OFFSET, YMIN - TOLERANCE));
//        // Outside x max bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX + TOLERANCE , YMIN + OFFSET));
//        // Outside y max bounds.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN + OFFSET, YMAX + TOLERANCE));
//        
//        // Corner tests.
//        
//        // On minX/minY corner
//        assertTrue(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN, YMIN));
//        // On minX/maxY corner
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN, YMAX));
//        // On maxX/maxY corner.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX , YMAX));
//        // On maxX/minY corner.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX, YMIN));
//        
//        // Outside minX/minY corner
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN - TOLERANCE, YMIN - TOLERANCE));
//        // Outside minX/maxY corner
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMIN - TOLERANCE, YMAX + TOLERANCE));
//        // Outside maxX/maxY corner.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX + TOLERANCE , YMAX + TOLERANCE));
//        // Outside maxX/minY corner.
//        assertFalse(containsStd(XMIN, YMIN
//                , XMAX, YMAX
//                , XMAX + TOLERANCE, YMIN - TOLERANCE));
//    }
    
    @Test
    public void testIntersects() 
    {
        // Complete overlap
        assertTrue(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN, YMIN, XMAX, YMAX));
        
        // A fully contains B
        assertTrue(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN + TOLERANCE, YMIN + TOLERANCE, XMAX - TOLERANCE, YMAX - TOLERANCE));
        
        // Wall tests
        
        // A xmin overlaps B xmax
        assertTrue(intersectsAABB(XMIN         , YMIN         , XMAX, YMAX
                            , XMIN - OFFSET, YMIN + OFFSET, XMIN, YMAX - OFFSET));
        // A xmax overlaps B xmin
        assertTrue(intersectsAABB(XMIN, YMIN         , XMAX         , YMAX
                            , XMAX, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));
        // A ymin overlaps B ymax
        assertTrue(intersectsAABB(XMIN         , YMIN         , XMAX         , YMAX
                            , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN));
        // A ymax overlaps B ymin
        assertTrue(intersectsAABB(XMIN         , YMIN, XMAX         , YMAX
                            , XMIN + OFFSET, YMAX, XMAX - OFFSET, YMAX + OFFSET));
        
        // A xmin above B xmax
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMIN + OFFSET, XMIN - TOLERANCE, YMAX - OFFSET));
        // A xmax below B xmin
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMAX + TOLERANCE, YMIN + OFFSET, XMAX + OFFSET, YMAX - OFFSET));
        // A ymin above B ymax
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMIN - OFFSET, XMAX - OFFSET, YMIN - TOLERANCE));
        // A ymax below B ymin
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN + OFFSET, YMAX + TOLERANCE, XMAX - OFFSET, YMAX + OFFSET));
        
        // Corner tests.
        
        // B fully below A xmin and A ymin
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMIN - OFFSET, XMIN - TOLERANCE, YMIN - TOLERANCE));
        // B fully above A xmax and A ymax
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMAX + TOLERANCE, YMAX + TOLERANCE, XMAX + OFFSET, YMAX + OFFSET));
        // B above and to right of A
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMIN - OFFSET, YMAX + TOLERANCE, XMIN - TOLERANCE, YMAX + OFFSET));
        // B below and to the left of A
        assertFalse(intersectsAABB(XMIN, YMIN, XMAX, YMAX
                , XMAX + TOLERANCE, YMIN - OFFSET, XMAX + OFFSET, YMIN - TOLERANCE));
        
    }

}
