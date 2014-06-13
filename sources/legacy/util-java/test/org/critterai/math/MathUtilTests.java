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
package org.critterai.math;

import static org.junit.Assert.*;
import static org.critterai.math.MathUtil.*;

import org.critterai.math.MathUtil;
import org.junit.Test;

/**
 * Unit tests for the {@link MathUtil} class.
 */
public class MathUtilTests {

    @Test
    public void testSloppyEquals() 
    {
        float tol = 0.1f;
        assertTrue(sloppyEquals(5, 5.09f, tol));
        assertTrue(sloppyEquals(5, 4.91f, tol));
        assertTrue(sloppyEquals(5, 5.10f, tol));
        assertTrue(sloppyEquals(5, 4.90f, tol));
        assertFalse(sloppyEquals(5, 5.101f, tol));
        assertFalse(sloppyEquals(5, 4.899f, tol));
    }

    @Test
    public void testClampToPositiveNonZero() 
    {
        assertTrue(clampToPositiveNonZero(0.1f) == 0.1f);
        assertTrue(clampToPositiveNonZero(Float.MIN_VALUE) == Float.MIN_VALUE);
        assertTrue(clampToPositiveNonZero(0) == Float.MIN_VALUE);
        assertTrue(clampToPositiveNonZero(-Float.MIN_VALUE) == Float.MIN_VALUE);
    }

    @Test
    public void testClampFloat() 
    {
        assertTrue(clamp(5.0f, 4.99f, 5.01f) == 5.0f);
        assertTrue(clamp(4.99f, 4.99f, 5.01f) == 4.99f);
        assertTrue(clamp(4.9899f, 4.99f, 5.01f) == 4.99f);
        assertTrue(clamp(5.01f, 4.99f, 5.01f) == 5.01f);
        assertTrue(clamp(5.01001f, 4.99f, 5.01f) == 5.01f);
    }

    @Test
    public void testClampInt() 
    {
        assertTrue(clamp(5, 4, 6) == 5);
        assertTrue(clamp(4, 4, 6) == 4);
        assertTrue(clamp(3,  4, 6) == 4);
        assertTrue(clamp(6,  4, 6) == 6);
        assertTrue(clamp(7,  4, 6) == 6);
    }

    @Test
    public void testClampShort() 
    {
        assertTrue(clamp((short)5, (short)4, (short)6) == 5);
        assertTrue(clamp((short)4, (short)4, (short)6) == 4);
        assertTrue(clamp((short)3,  (short)4, (short)6) == 4);
        assertTrue(clamp((short)6,  (short)4, (short)6) == 6);
        assertTrue(clamp((short)7,  (short)4, (short)6) == 6);
    }
    
    @Test
    public void testMax()
    {
        assertTrue(max(2) == 2);
        assertTrue(max(-1, 0, 1, 2) == 2);
        assertTrue(max(-1, 2, -1, 0) == 2);
    }
    
    @Test
    public void testMin()
    {
        assertTrue(min(2) == 2);
        assertTrue(min(-1, 0, 1, 2) == -1);
        assertTrue(min(2, 2, -1, 0) == -1);
    }

}
