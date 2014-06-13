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

import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import static org.critterai.math.geom.Triangle2.*;

import org.critterai.math.geom.Triangle2;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Triangle2} class.
 */
public class Triangle2Test {
    
    // Clockwise wrapped
    private static final float AX = 3;
    private static final float AY = 2;
    private static final float BX = 2;
    private static final float BY = -1;
    private static final float CX = 0;
    private static final float CY = -1;
    
    // Clockwise Wrapped
    private static final float AXI = 3;
    private static final float AYI = 2;
    private static final float BXI = 2;
    private static final float BYI = -1;
    private static final float CXI = 0;
    private static final float CYI = -1;
    
    public static final float TOLERANCE = 0.0001f;
    
    @Before
    public void setUp() throws Exception 
    {
    }
    
    @Test
    public void testStaticContains()
    {
        
        // Vertex inclusion tests
        
        assertTrue(contains(AX, AY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(BX, BY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(BX - TOLERANCE, BY + TOLERANCE, AX, AY, BX, BY, CX, CY));
        assertFalse(contains(BX + TOLERANCE, BY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(CX, CY, AX, AY, BX, BY, CX, CY));
    
        // Wall inclusion tests
        
        float midpointX = AX + (BX - AX) / 2;
        float midpointY = AY + (BY - AY) / 2;
        assertTrue(contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        assertFalse(contains(midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        midpointX = BX + (CX - BX) / 2;
        midpointY = BY + (CY - BY) / 2;
        assertTrue(contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(midpointX, midpointY + TOLERANCE, AX, AY, BX, BY, CX, CY));
        assertFalse(contains(midpointX, midpointY - TOLERANCE, AX, AY, BX, BY, CX, CY));
        midpointX = CX + (AX - CX) / 2;
        midpointY = CY + (AY - CY) / 2;
        assertTrue(contains(midpointX, midpointY, AX, AY, BX, BY, CX, CY));
        assertTrue(contains(midpointX + TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        assertFalse(contains(midpointX - TOLERANCE, midpointY, AX, AY, BX, BY, CX, CY));
        
    }
    

    
    @Test
    public void testAreaFloat()
    {
        float result = getSignedAreaX2(AX, AY, BX, BY, CX, CY);
        assertTrue(result == -6);
        result = getSignedAreaX2(AX, AY, CX, CY, BX, BY);
        assertTrue(result == 6);
    }
    
    @Test
    public void testAreaInt()
    {
        float result = getSignedAreaX2(AXI, AYI, BXI, BYI, CXI, CYI);
        assertTrue(result == -6);
        result = getSignedAreaX2(AXI, AYI, CXI, CYI, BXI, BYI);
        assertTrue(result == 6);
    }

}
