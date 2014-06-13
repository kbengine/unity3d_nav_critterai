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

import static org.junit.Assert.*;
import static org.critterai.math.geom.Triangle3.*;
import static org.critterai.math.MathUtil.*;
import static org.critterai.math.Vector3.*;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Triangle3;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Triangle3} class.
 */
public class Triangle3Tests {

    // Clockwise wrapped
    private static final float AX = 3;
    private static final float AY = 2;
    private static final float AZ = -1;
    private static final float BX = 2;
    private static final float BY = -1;
    private static final float BZ = 1;
    private static final float CX = 0;
    private static final float CY = -1;
    private static final float CZ = 0;
    
    @Before
    public void setUp() 
        throws Exception 
    {
    }

    @Test
    public void testGetArea() 
    {
        float expected = getHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
        float actual = getArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
        assertTrue(sloppyEquals(actual, expected, 0.0001f));
    }
    
    @Test
    public void testGetAreaComp() 
    {
        float expected = getHeronArea(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ);
        float actual = (float)Math.sqrt(getAreaComp(AX, AY, AZ, BX, BY, BZ, CX, CY, CZ))/2;
        assertTrue(sloppyEquals(actual, expected, 0.0001f));
    }

    @Test
    public void testGetNormalFloatVector3() 
    {
        Vector3 v = new Vector3();
        assertTrue(v == getNormal(AX, AY, 0, BX, BY, 0, CX, CY, 0, v));
        assertTrue(v.sloppyEquals(0, 0, -1, 0.0001f));
        assertTrue(v == getNormal(AX, AY, 0, CX, CY, 0, BX, BY, 0, v));
        assertTrue(v.sloppyEquals(0, 0, 1, 0.0001f));
        assertTrue(v == getNormal(AX, 0, AZ, BX, 0, BZ, CX, 0, CZ, v));
        assertTrue(v.sloppyEquals(0, -1, 0, 0.0001f));
        assertTrue(v == getNormal(0, AY, AZ, 0, BY, BZ, 0, CY, CZ, v));
        assertTrue(v.sloppyEquals(1, 0, 0, 0.0001f));
    }

    @Test
    public void testGetNormalArrayVector3() 
    {
        float[] vertices = {
                5, 5, 5
                , AX, 0, AZ
                , BX, 0, BZ
                , CX, 0, CZ
                , 9, 9, 9
        };
        Vector3 v = new Vector3();
        assertTrue(v == getNormal(vertices, 1, v));
        assertTrue(v.sloppyEquals(0, -1, 0, 0.0001f));
    }

    private float getHeronArea(float ax, float ay, float az
            , float bx, float by, float bz
            , float cx, float cy, float cz)
    {
        double a = Math.sqrt(getDistanceSq(AX, AY, AZ, BX, BY, BZ));
        double b = Math.sqrt(getDistanceSq(AX, AY, AZ, CX, CY, CZ));
        double c = Math.sqrt(getDistanceSq(CX, CY, CZ, BX, BY, BZ));
        double s = (a + b + c)/2;
        return (float)Math.sqrt(s * (s - a) * (s - b) * (s - c));
    }

}
