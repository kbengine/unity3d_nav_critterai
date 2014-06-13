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

import static org.critterai.math.Vector2.*;
import static org.junit.Assert.*;

import static org.critterai.math.MathUtil.*;
import org.critterai.math.Vector2;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Vector2} class.
 */
public class Vector2Tests {

    public static final float AX = 1.5f;
    public static final float AY = 8.0f;
    public static final float BX = -17.112f;
    public static final float BY = 77.5f;
    
    private Vector2 mA1;
    private Vector2 mA2;
    private Vector2 mB1;
    private Vector2 mC1;
    
    @Before
    public void setUp() 
        throws Exception 
    {
        mA1 = new Vector2(AX, AY);
        mA2 = new Vector2(AX, AY);
        mB1 = new Vector2(BX, BY);
        mC1 = new Vector2(AY, AX);
    }
    
    @Test
    public void testConstructorDefault() 
    {
        Vector2 v = new Vector2();
        assertTrue(v.x == 0 && v.y == 0);
    }
    
    @Test
    public void testConstructorFloatFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        assertTrue(v.x == AX && v.y == AY);
    }

    @Test
    public void testDotFloat() 
    {
        float result = mA1.dot(BX, BY);
        float expected = (AX * BX) + (AY * BY);
        assertTrue(result == expected);
    }

    @Test
    public void testDotVector2() 
    {
        float result = mA1.dot(mB1);
        float expected = (AX * BX) + (AY * BY);
        assertTrue(result == expected);
    }
    
    @Test
    public void testEqualsFloat() 
    {
        assertTrue(mA1.equals(AX, AY));
        assertFalse(mA1.equals(AY, AX));
    }
    
    @Test
    public void testEqualsObject() 
    {
        assertTrue(mA1.equals((Object)mA2));
        assertFalse(mA1.equals((Object)mB1));
        assertFalse(mA1.equals((Object)mC1));
    }

    @Test
    public void testEqualsVector2() 
    {
        assertTrue(mA1.equals(mA2));
        assertFalse(mA1.equals(mB1));
        assertFalse(mA1.equals(mC1));
    }

    @Test
    public void testFields()
    {
        assertTrue(mA1.x == AX);            
        assertTrue(mB1.x == BX);
        assertTrue(mA1.y == AY);            
        assertTrue(mB1.y == BY);
    }

    @Test
    public void testGetX() 
    {
        assertTrue(mA1.getX() == AX);
    }

    @Test
    public void testGetY() 
    {
        assertTrue(mA1.getY() == AY);
    }
    
    @Test
    public void testHashCode() 
    {
        assertTrue(mA1.hashCode() == mA2.hashCode());
        assertTrue(mA1.hashCode() != mB1.hashCode());
        assertTrue(mA1.hashCode() != mC1.hashCode());
    }

    @Test
    public void testIsZeroLength() 
    {
        assertTrue(new Vector2().isZeroLength());
        assertFalse(mA1.isZeroLength());
    }

    @Test
    public void testLengthSq() 
    {
        float len = (AX * AX) + (AY * AY);
        assertTrue(mA1.lengthSq() == len);
    }
    
    @Test
    public void testMutatorAddFloat() 
    {
        assertTrue(mA1.add(BX, BY) == mA1);
        assertTrue(mA1.x == AX + BX);
        assertTrue(mA1.y == AY + BY);
    }
    
    @Test
    public void testMutatorAddVector2() 
    {
        assertTrue(mA1.add(mB1) == mA1);
        assertTrue(mA1.x == AX + BX);
        assertTrue(mA1.y == AY + BY);
    }

    @Test
    public void testMutatorDivide() 
    {
        assertTrue(mA1.divide(5) == mA1);
        assertTrue(mA1.x == AX / 5);
        assertTrue(mA1.y == AY / 5);
    }
    
    @Test
    public void testMutatorMultiply() 
    {
        assertTrue(mA1.multiply(5) == mA1);
        assertTrue(mA1.x == AX * 5);
        assertTrue(mA1.y == AY * 5);
    }
    
    @Test
    public void testMutatorNormalize() 
    {
        float len = (float) Math.sqrt((AX * AX) + (AY * AY));
        float expectedX = AX / len;
        float expectedY = AY / len;
        
        assertTrue(mA1.normalize() == mA1);
        assertTrue(mA1.x == expectedX);
        assertTrue(mA1.y == expectedY);
    }

    @Test
    public void testMutatorReverse()
    {
        assertTrue(mA1.reverse() == mA1);
        assertTrue(mA1.x == -AX);
        assertTrue(mA1.y == -AY);
    }
    
    @Test
    public void testMutatorRotate()
    {
        // Only a simple test.  Rotate 90 degrees counter clockwise.
        Vector2 u = new Vector2(1, 0);
        assertTrue(u.rotate((float)(Math.PI / 2)) == u);
        assertTrue(u.x > -0.001
                && u.x < 0.001
                && u.y > 0.999
                && u.y < 1.001);
    }
    
    @Test
    public void testMutatorScaleTo() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        assertTrue(mA1.scaleTo(15.0f) == mA1);
        float len = (float)Math.sqrt(mA1.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        mA1 = new Vector2(AX, AY).scaleTo(0);
        assertTrue(mA1.x == 0 && mA1.y == 0);
    }
    
    @Test
    public void testMutatorSubtractFloat() 
    {
        assertTrue(mA1.subtract(BX, BY) == mA1);
        assertTrue(mA1.x == AX - BX);
        assertTrue(mA1.y == AY - BY);
    }
    
    @Test
    public void testMutatorSubtractVector() 
    {
        assertTrue(mA1.subtract(mB1) == mA1);
        assertTrue(mA1.x == AX - BX);
        assertTrue(mA1.y == AY - BY);
    }
    
    @Test
    public void testMutatorTruncateLength() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        assertTrue(mA1.truncateLength(15.0f) == mA1);
        assertTrue(mA1.x == AX && mA1.y == AY);
        
        mA1 = new Vector2(AX, AY).truncateLength(5);
        float len = (float)Math.sqrt(mA1.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        mA1 = new Vector2(AX, AY).truncateLength(0);
        assertTrue(mA1.x == 0 && mA1.y == 0);
    }

    @Test
    public void testMutatorXValue() 
    {
        mA1.setX(BX);
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == AY);
    }
    
    @Test
    public void testMutatorYValue() 
    {
        mA1.setY(BY);
        assertTrue(mA1.x == AX);
        assertTrue(mA1.y == BY);
    }
    
    @Test
    public void testSetValueFloat() 
    {
        assertTrue(mA1 == mA1.set(BX, BY));
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == BY);
    }
    
    @Test
    public void testSetValueVector2() 
    {
        assertTrue(mA1 == mA1.set(mB1));
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == BY);
    }
    
    @Test
    public void testSloppyEqualsFloat() 
    {
        assertTrue(mA1.sloppyEquals(AX, AY, 0.0f));

        mA1 = new Vector2(AX + 0.019f, AY);
        assertFalse(mA1.sloppyEquals(AX, AY, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.020f));
        
        mA1 = new Vector2(AX - 0.019f, AY);
        assertFalse(mA1.sloppyEquals(AX, AY, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.020f));

        mA1 = new Vector2(AX, AY + 0.019f);
        assertFalse(mA1.sloppyEquals(AX, AY, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.020f));
        
        mA1 = new Vector2(AX, AY - 0.019f);
        assertFalse(mA1.sloppyEquals(AX, AY, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, 0.020f));
    }
    
    @Test
    public void testSloppyEqualsVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        assertTrue(v.sloppyEquals(mA1, 0.0f));

        v = new Vector2(AX + 0.019f, AY);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector2(AX - 0.019f, AY);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));

        v = new Vector2(AX, AY + 0.019f);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector2(AX, AY - 0.019f);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
    }

    @Test
    public void testStaticAddValueFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = add(AX, AY, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BX);
    }

    @Test
    public void testStaticAddValueVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = add(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BX);
    }
    
    @Test
    public void testStaticDivideFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = divide(AX, AY, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX / BX);
        assertTrue(u.y == AY / BX);
    }

    @Test
    public void testStaticDivideVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = divide(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX / BX);
        assertTrue(u.y == AY / BX);
    }
    
    @Test
    public void testStaticDotFloat() 
    {
        float expected = (AX * BX) + (AY * BY); 
        assertTrue(dot(AX, AY, BX, BY) == expected);
    }

    @Test
    public void testStaticGetDirectionABArray() 
    {
        float sos = (float)Math.sqrt(0.5f); // 0.707
        float[] v = new float[2];
        float[] u = getDirectionAB(1, 1, 3, 3, v, 0);
        assertTrue(u == v);
        assertTrue(sloppyEquals(u[0], sos, 0.0001f)
                && sloppyEquals(u[1], sos, 0.0001f));
        
        v = new float[4]; // <<
        u = getDirectionAB(3, 3, 1, 1, v, 1);
        assertTrue(u == v);
        assertTrue(sloppyEquals(u[2], -sos, 0.0001f)
                && sloppyEquals(u[3], -sos, 0.0001f));
        
        u = getDirectionAB(1, 1, -1, 3, v, 0);
        assertTrue(sloppyEquals(u[0], -sos, 0.0001f)
                && sloppyEquals(u[1], sos, 0.0001f));
        
        u = getDirectionAB(1, 1, 3, -1, v, 0);
        assertTrue(sloppyEquals(u[0], sos, 0.0001f)
                && sloppyEquals(u[1], -sos, 0.0001f));
        
    }

    @Test
    public void testStaticGetDirectionABVector() 
    {
        float sos = (float)Math.sqrt(0.5f); // 0.707
        Vector2 v = new Vector2();
        Vector2 u = getDirectionAB(1, 1, 3, 3, v);
        assertTrue(u == v);
        assertTrue(sloppyEquals(u.x, sos, 0.0001f)
                && sloppyEquals(u.y, sos, 0.0001f));
        
        u = getDirectionAB(3, 3, 1, 1, v);
        assertTrue(sloppyEquals(u.x, -sos, 0.0001f)
                && sloppyEquals(u.y, -sos, 0.0001f));
        
        u = getDirectionAB(1, 1, -1, 3, v);
        assertTrue(sloppyEquals(u.x, -sos, 0.0001f)
                && sloppyEquals(u.y, sos, 0.0001f));
    }

    
    @Test
    public void testStaticGetDistanceSqFloat() 
    {
        float result = getDistanceSq(AX, AY, BX, BY);
        float deltaX = BX - AX;
        float deltaY = BY - AY;
        float expected = (deltaX * deltaX) + (deltaY * deltaY);
        assertTrue(result == expected);
    }
    
    @Test
    public void testStaticGetDistanceSqVector() 
    {
        float result = getDistanceSq(mA1, mB1);
        float deltaX = BX - AX;
        float deltaY = BY - AY;
        float expected = (deltaX * deltaX) + (deltaY * deltaY);
        assertTrue(result == expected);
    }

    @Test
    public void testStaticGetLengthSq() 
    {
        float len = (AX * AX) + (AY * AY);
        assertTrue(getLengthSq(AX, AY) == len);
    }

    @Test
    public void testStaticMultiplyFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = multiply(AX, AY, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX * BX);
        assertTrue(u.y == AY * BX);
    }
    
    @Test
    public void testStaticMultiplyVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = multiply(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX * BX);
        assertTrue(u.y == AY * BX);
    }
    
    @Test
    public void testStaticNormalizeFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = normalize(AX, AY, v);
        float len = (float) Math.sqrt((AX * AX) + (AY * AY));
        float x = AX / len;
        float y = AY / len;
        assertTrue(u == v);
        assertTrue(u.x == x && u.y == y);
    }

    @Test
    public void testStaticNormalizeVector() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = normalize(mA1, v);
        float len = (float) Math.sqrt((AX * AX) + (AY * AY));
        float x = AX / len;
        float y = AY / len;
        assertTrue(u == v);
        assertTrue(u.x == x && u.y == y);
    }

    @Test
    public void testStaticRotateFloat()
    {
        // Only a simple test.  Rotate 90 degrees counter clockwise.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = rotate(1, 0, (float)(Math.PI / 2), v);
        assertTrue(u.x > -0.001 
                && u.x < 0.001 
                && u.y > 0.999 
                && u.y < 1.001);
    }
    
    @Test
    public void testStaticRotateVector2()
    {
        // Only a simple test.  Rotate 90 degrees counter clockwise.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = new Vector2(1, 0);
        u = rotate(u, (float)(Math.PI / 2), v);
        assertTrue(u == v);
        assertTrue(u.x > -0.001 
                && u.x < 0.001 
                && u.y > 0.999 
                && u.y < 1.001);
    }
    
    @Test
    public void testStaticScaleToFloat() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = scaleTo(AX, AY, 15.0f, v);
        assertTrue(u == v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        u = scaleTo(AX, AY, 0, v);
        assertTrue(u.x == 0 && u.y == 0);
    }

    @Test
    public void testStaticScaleToVector2() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = scaleTo(mA1, 15.0f, v);
        assertTrue(u == v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        u = scaleTo(mA1, 0, v);
        assertTrue(u.x == 0 && u.y == 0);
    }

    @Test
    public void testStaticSloppyEqualsFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.0f));

        v = new Vector2(AX + 0.019f, AY);
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.0f));
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.018f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.019f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.020f));
        
        v = new Vector2(AX - 0.019f, AY);
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.0f));
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.018f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.019f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.020f));

        v = new Vector2(AX, AY + 0.019f);
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.0f));
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.018f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.019f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.020f));
        
        v = new Vector2(AX, AY - 0.019f);
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.0f));
        assertFalse(sloppyEquals(AX, AY, v.x, v.y, 0.018f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.019f));
        assertTrue(sloppyEquals(AX, AY, v.x, v.y, 0.020f));
    }

    @Test
    public void testStaticSloppyEqualsVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        assertTrue(sloppyEquals(mA1, v, 0.0f));

        v = new Vector2(AX + 0.019f, AY);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector2(AX - 0.019f, AY);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));

        v = new Vector2(AX, AY + 0.019f);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector2(AX, AY - 0.019f);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
    }
    
    @Test
    public void testStaticSubtractFloats() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = subtract(AX, AY, BX, BY, v);
        assertTrue(u == v);
        assertTrue(u.x == AX - BX);
        assertTrue(u.y == AY - BY);
    }

    @Test
    public void testStaticSubtractVectors() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = subtract(mA1, mB1, v);
        assertTrue(u == v);
        assertTrue(u.x == AX - BX);
        assertTrue(u.y == AY - BY);
    }
    
    @Test
    public void testStaticTruncateLengthFloat() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = truncateLength(AX, AY, 15.0f, v);
        assertTrue(u == v);
        assertTrue(u.x == AX && u.y == AY);
        
        u = truncateLength(AX, AY, 5.0f, v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        u = truncateLength(AX, AY, 0, v);
        assertTrue(u.x == 0 && u.y == 0);
    }

    @Test
    public void testStaticTruncateLengthVector2() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = truncateLength(mA1, 15.0f, v);
        assertTrue(u == v);
        assertTrue(u.x == AX && u.y == AY);
        
        u = truncateLength(mA1, 5.0f, v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        u = truncateLength(mA1, 0, v);
        assertTrue(u.x == 0 && u.y == 0);
    }
    
    @Test
    public void testStaticVectorAddFloat() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = add(AX, AY, BX, BY, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BY);
    }
    
    @Test
    public void testStaticVectorAddVector2() 
    {
        Vector2 v = new Vector2(AX, AY);
        Vector2 u = add(mA1, mB1, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BY);
    }

}
