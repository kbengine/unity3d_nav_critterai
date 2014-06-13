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

import static org.critterai.math.Vector3.*;

import static org.junit.Assert.*;

import org.critterai.math.Vector3;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Vector3} class.
 */
public class Vector3Tests {

    public static final float AX = 1.5f;
    public static final float AY = 8.0f;
    public static final float AZ = -3.2f;
    public static final float BX = -17.112f;
    public static final float BY = 77.5f;
    public static final float BZ = 22.42f;
    
    private Vector3 mA1;
    private Vector3 mB1;
    
    float[] mVectors;
    
    @Before
    public void setUp() 
        throws Exception 
    {
        mA1 = new Vector3(AX, AY, AZ);
        mB1 = new Vector3(BX, BY, BZ);
        mVectors = new float[6];
        mVectors[0] = AX;
        mVectors[1] = AY;
        mVectors[2] = AZ;    
        mVectors[3] = BX;
        mVectors[4] = BY;
        mVectors[5] = BZ;    
    }

    @Test
    public void testConstructorArray() 
    {
        Vector3 v = new Vector3(mVectors, 1);
        assertTrue(v.x == BX);
        assertTrue(v.y == BY);
        assertTrue(v.z == BZ);
    }
    
    @Test
    public void testConstructorDefault() 
    {
        Vector3 v = new Vector3();
        assertTrue(v.x == 0 && v.y == 0 && v.z == 0);
    }

    @Test
    public void testConstructorFloatFloatFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        assertTrue(v.x == AX && v.y == AY && v.z == AZ);
    }

    @Test
    public void testDotFloat() 
    {
        float result = mA1.dot(BX, BY, BZ);
        float expected = (AX * BX) + (AY * BY) + (AZ * BZ);
        assertTrue(result == expected);
    }

    @Test
    public void testDotVector3() 
    {
        float result = mA1.dot(mB1);
        float expected = (AX * BX) + (AY * BY) + (AZ * BZ);
        assertTrue(result == expected);
    }
    
    @Test
    public void testEqualsFloat() 
    {
        assertTrue(mA1.equals(AX, AY, AZ));
        assertFalse(mA1.equals(AY, AX, AZ));
        assertFalse(mA1.equals(AZ, AX, AY));
        assertFalse(mA1.equals(AX, AZ, AY));
    }
    
    @Test
    public void testEqualsObject() 
    {
        assertTrue(mA1.equals((Object)new Vector3(AX, AY, AZ)));
        assertFalse(mA1.equals((Object)mB1));
        assertFalse(mA1.equals((Object)new Vector3(AY, AX, AZ)));
        assertFalse(mA1.equals((Object)new Vector3(AZ, AX, AY)));
        assertFalse(mA1.equals((Object)new Vector3(AX, AZ, AY)));
    }

    @Test
    public void testEqualsVector3() 
    {
        assertTrue(mA1.equals(new Vector3(AX, AY, AZ)));
        assertFalse(mA1.equals(mB1));
        assertFalse(mA1.equals(new Vector3(AY, AX, AZ)));
        assertFalse(mA1.equals(new Vector3(AZ, AX, AY)));
        assertFalse(mA1.equals(new Vector3(AX, AZ, AY)));
    }

    @Test
    public void testFields()
    {
        assertTrue(mA1.x == AX);            
        assertTrue(mB1.x == BX);
        assertTrue(mA1.y == AY);            
        assertTrue(mB1.y == BY);
        assertTrue(mA1.z == AZ);            
        assertTrue(mB1.z == BZ);
    }

    @Test
    public void testFlatten() 
    {
        float[] v = flatten(mVectors, 0, mB1, mA1);
        assertTrue(v == mVectors);
        assertTrue(v[0] == BX);
        assertTrue(v[1] == BY);
        assertTrue(v[2] == BZ);
        assertTrue(v[3] == AX);
        assertTrue(v[4] == AY);
        assertTrue(v[5] == AZ);
        v = flatten(mVectors, 1, mB1);
        assertTrue(v[0] == BX);
        assertTrue(v[1] == BY);
        assertTrue(v[2] == BZ);
        assertTrue(v[3] == BX);
        assertTrue(v[4] == BY);
        assertTrue(v[5] == BZ);
    }

    @Test
    public void testGet() 
    {
        float[] va = mA1.get(mVectors, 1);
        assertTrue(va == mVectors);
        assertTrue(mVectors[0] == AX);
        assertTrue(mVectors[1] == AY);
        assertTrue(mVectors[2] == AZ);    
        assertTrue(mVectors[3] == AX);
        assertTrue(mVectors[4] == AY);
        assertTrue(mVectors[5] == AZ);    
        mB1.get(mVectors, 0);
        assertTrue(mVectors[0] == BX);
        assertTrue(mVectors[1] == BY);
        assertTrue(mVectors[2] == BZ);    
        assertTrue(mVectors[3] == AX);
        assertTrue(mVectors[4] == AY);
        assertTrue(mVectors[5] == AZ);    
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
    public void testGetZ() 
    {
        assertTrue(mA1.getZ() == AZ);
    }

    @Test
    public void testHashCode() 
    {
        assertTrue(mA1.hashCode() == new Vector3(AX, AY, AZ).hashCode());
        assertTrue(mA1.hashCode() != mB1.hashCode());
        assertTrue(mA1.hashCode() != new Vector3(AY, AX, AZ).hashCode());
        assertTrue(mA1.hashCode() != new Vector3(AZ, AX, AY).hashCode());
        assertTrue(mA1.hashCode() != new Vector3(AX, AZ, AY).hashCode());
    }

    @Test
    public void testIsZeroLength() 
    {
        assertTrue(new Vector3().isZeroLength());
        assertFalse(mA1.isZeroLength());
    }
    
    @Test
    public void testLengthSq() 
    {
        float len = (AX * AX) + (AY * AY) + (AZ * AZ);
        assertTrue(mA1.lengthSq() == len);
    }
    
    @Test
    public void testMutatorAddFloat() 
    {
        assertTrue(mA1.add(BX, BY, BZ) == mA1);
        assertTrue(mA1.x == AX + BX);
        assertTrue(mA1.y == AY + BY);
        assertTrue(mA1.z == AZ + BZ);
    }

    @Test
    public void testMutatorAddVector3() 
    {
        assertTrue(mA1.add(mB1) == mA1);
        assertTrue(mA1.x == AX + BX);
        assertTrue(mA1.y == AY + BY);
        assertTrue(mA1.z == AZ + BZ);
    }
    
    @Test
    public void testMutatorDivide() 
    {
        assertTrue(mA1.divide(5) == mA1);
        assertTrue(mA1.x == AX / 5);
        assertTrue(mA1.y == AY / 5);
        assertTrue(mA1.z == AZ / 5);
    }
    
    @Test
    public void testMutatorMultiply() 
    {
        assertTrue(mA1.multiply(5) == mA1);
        assertTrue(mA1.x == AX * 5);
        assertTrue(mA1.y == AY * 5);
        assertTrue(mA1.z == AZ * 5);
    }
    
    @Test
    public void testMutatorNormalize() 
    {
        float len = (float) Math.sqrt((AX * AX) + (AY * AY) + (AZ * AZ));
        float expectedX = AX / len;
        float expectedY = AY / len;
        float expectedZ = AZ / len;
        
        assertTrue(mA1.normalize() == mA1);
        assertTrue(mA1.x == expectedX);
        assertTrue(mA1.y == expectedY);
        assertTrue(mA1.z == expectedZ);
    }
    
    @Test
    public void testMutatorScaleTo() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        assertTrue(mA1.scaleTo(15.0f) == mA1);
        float len = (float)Math.sqrt(mA1.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        mA1 = new Vector3(AX, AY, AZ).scaleTo(0);
        assertTrue(mA1.x == 0 && mA1.y == 0);
    }
    
    @Test
    public void testMutatorSubtractFloat() 
    {
        assertTrue(mA1.subtract(BX, BY, BZ) == mA1);
        assertTrue(mA1.x == AX - BX);
        assertTrue(mA1.y == AY - BY);
        assertTrue(mA1.z == AZ - BZ);
    }
    
    @Test
    public void testMutatorSubtractVector() 
    {
        assertTrue(mA1.subtract(mB1) == mA1);
        assertTrue(mA1.x == AX - BX);
        assertTrue(mA1.y == AY - BY);
        assertTrue(mA1.z == AZ - BZ);
    }

    @Test
    public void testMutatorTruncateLength() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        assertTrue(mA1.truncateLength(15.0f) == mA1);
        assertTrue(mA1.x == AX && mA1.y == AY && mA1.z == mA1.z);
        
        mA1 = new Vector3(AX, AY, AZ).truncateLength(5);
        float len = (float)Math.sqrt(mA1.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        mA1 = new Vector3(AX, AY, AZ).truncateLength(0);
        assertTrue(mA1.x == 0 && mA1.y == 0 && mA1.z == 0);
    }
    
    @Test
    public void testMutatorXValue() 
    {
        mA1.setX(BX);
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == AY);
        assertTrue(mA1.z == AZ);
    }
    
    @Test
    public void testMutatorYValue() 
    {
        mA1.setY(BY);
        assertTrue(mA1.x == AX);
        assertTrue(mA1.y == BY);
        assertTrue(mA1.z == AZ);
    }
    
    @Test
    public void testMutatorZValue() 
    {
        mA1.setZ(BZ);
        assertTrue(mA1.x == AX);
        assertTrue(mA1.y == AY);
        assertTrue(mA1.z == BZ);
    }
    
    @Test
    public void testSetValueFloat() 
    {
        assertTrue(mA1 == mA1.set(BX, BY, BZ));
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == BY);
        assertTrue(mA1.z == BZ);
        
    }
    
    @Test
    public void testSetValueVector3() 
    {
        assertTrue (mA1 == mA1.set(mB1));
        assertTrue(mA1.x == BX);
        assertTrue(mA1.y == BY);
        assertTrue(mA1.z == BZ);
    }
    
    @Test
    public void testSloppyEqualsFloat() 
    {
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.0f));

        mA1 = new Vector3(AX + 0.019f, AY, AZ);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));
        
        mA1 = new Vector3(AX - 0.019f, AY, AZ);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));

        mA1 = new Vector3(AX, AY + 0.019f, AZ);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));
        
        mA1 = new Vector3(AX, AY - 0.019f, AZ);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));
        
        mA1 = new Vector3(AX, AY, AZ + 0.019f);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));
        
        mA1 = new Vector3(AX, AY, AZ - 0.019f);
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.0f));
        assertFalse(mA1.sloppyEquals(AX, AY, AZ, 0.018f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.019f));
        assertTrue(mA1.sloppyEquals(AX, AY, AZ, 0.020f));
    }

    @Test
    public void testSloppyEqualsVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        assertTrue(v.sloppyEquals(mA1, 0.0f));

        v = new Vector3(AX + 0.019f, AY, AZ);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector3(AX - 0.019f, AY, AZ);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));

        v = new Vector3(AX, AY + 0.019f, AZ);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector3(AX, AY - 0.019f, AZ);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector3(AX, AY, AZ + 0.019f);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
        
        v = new Vector3(AX, AY, AZ - 0.019f);
        assertFalse(v.sloppyEquals(mA1, 0.0f));
        assertFalse(v.sloppyEquals(mA1, 0.018f));
        assertTrue(v.sloppyEquals(mA1, 0.019f));
        assertTrue(v.sloppyEquals(mA1, 0.020f));
    }

    @Test
    public void testStaticAddFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = add(AX, AY, AZ, BX, BY, BZ, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BY);
        assertTrue(u.z == AZ + BZ);
    }
    
    @Test
    public void testStaticAddValueFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = add(AX, AY, AZ, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BX);
        assertTrue(u.z == AZ + BX);
    }

    @Test
    public void testStaticAddValueVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = add(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BX);
        assertTrue(u.z == AZ + BX);
    }
    
    @Test
    public void testStaticAddVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = add(mA1, mB1, v);
        assertTrue(u == v);
        assertTrue(u.x == AX + BX);
        assertTrue(u.y == AY + BY);
        assertTrue(u.z == AZ + BZ);
    }

    
    @Test
    public void testStaticCrossArrayArray() 
    {
        float expectedX = AY*BZ - AZ*BY;
        float expectedY = -AX*BZ + AZ*BX;
        float expectedZ = AX*BY - AY*BX;
        float[] v = cross(mVectors, 0, mVectors, 1, mVectors, 1);
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == expectedX);
        assertTrue(v[4] == expectedY);
        assertTrue(v[5] == expectedZ);
    }
    
    @Test
    public void testStaticCrossFloat() 
    {
        float expectedX = AY*BZ - AZ*BY;
        float expectedY = -AX*BZ + AZ*BX;
        float expectedZ = AX*BY - AY*BX;
        Vector3 v = Vector3.cross(AX, AY, AZ, BX, BY, BZ, mA1);
        assertTrue(v == mA1);
        assertTrue(v.x == expectedX);
        assertTrue(v.y == expectedY);
        assertTrue(v.z == expectedZ);
    }

    @Test
    public void testStaticCrossFloatArray() 
    {
        float expectedX = AY*BZ - AZ*BY;
        float expectedY = -AX*BZ + AZ*BX;
        float expectedZ = AX*BY - AY*BX;
        float[] v = cross(AX, AY, AZ, BX, BY, BZ, mVectors, 1);
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == expectedX);
        assertTrue(v[4] == expectedY);
        assertTrue(v[5] == expectedZ);
    }

    @Test
    public void testStaticCrossVector3() 
    {
        float expectedX = AY*BZ - AZ*BY;
        float expectedY = -AX*BZ + AZ*BX;
        float expectedZ = AX*BY - AY*BX;
        Vector3 v = cross(mA1, mB1, mA1);
        assertTrue(v == mA1);
        assertTrue(v.x == expectedX);
        assertTrue(v.y == expectedY);
        assertTrue(v.z == expectedZ);
    }
    
    @Test
    public void testStaticDivideFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = divide(AX, AY, AZ, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX / BX);
        assertTrue(u.y == AY / BX);
        assertTrue(u.z == AZ / BX);
    }
    
    @Test
    public void testStaticDivideVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = divide(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX / BX);
        assertTrue(u.y == AY / BX);
    }

    @Test
    public void testStaticDotFloat() 
    {
        float expected = (AX * BX) + (AY * BY) + (AZ * BZ); 
        assertTrue(dot(AX, AY, AZ, BX, BY, BZ) == expected);
    }
    
    @Test
    public void testStaticGetDistanceSqFloat() 
    {
        float result = getDistanceSq(AX, AY, AZ, BX, BY, BZ);
        float deltaX = BX - AX;
        float deltaY = BY - AY;
        float deltaZ = BZ - AZ;
        float expected = (deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ);
        assertTrue(result == expected);
    }

    @Test
    public void testStaticGetDistanceSqVector() 
    {
        float result = getDistanceSq(mA1, mB1);
        float deltaX = BX - AX;
        float deltaY = BY - AY;
        float deltaZ = BZ - AZ;
        float expected = (deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ);
        assertTrue(result == expected);
    }

    @Test
    public void testStaticGetLengthSq() 
    {
        float len = (AX * AX) + (AY * AY) + (AZ * AZ);
        assertTrue(getLengthSq(AX, AY, AZ) == len);
    }

    @Test
    public void testStaticMultiplyFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = multiply(AX, AY, AZ, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX * BX);
        assertTrue(u.y == AY * BX);
        assertTrue(u.z == AZ * BX);
    }
    
    @Test
    public void testStaticMultiplyVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = multiply(mA1, BX, v);
        assertTrue(u == v);
        assertTrue(u.x == AX * BX);
        assertTrue(u.y == AY * BX);
        assertTrue(u.z == AZ * BX);
    }

    @Test
    public void testStaticNormalizeArrayArray() 
    {
        float[] v = normalize(mVectors, 1, mVectors, 1);
        float len = (float) Math.sqrt((BX * BX) + (BY * BY) + (BZ * BZ));
        float expectedX = BX / len;
        float expectedY = BY / len;
        float expectedZ = BZ / len;
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == expectedX);
        assertTrue(v[4] == expectedY);
        assertTrue(v[5] == expectedZ);
    }
    
    @Test
    public void testStaticNormalizeFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = normalize(AX, AY, AZ, v);
        float len = (float) Math.sqrt((AX * AX) + (AY * AY) + (AZ * AZ));
        float x = AX / len;
        float y = AY / len;
        float z = AZ / len;
        assertTrue(u == v);
        assertTrue(u.x == x && u.y == y && u.z == z);
    }

    @Test
    public void testStaticNormalizeFloatArrray() 
    {
        float[] v = normalize(AX, AY, AZ, mVectors, 1);
        float len = (float) Math.sqrt((AX * AX) + (AY * AY) + (AZ * AZ));
        float expectedX = AX / len;
        float expectedY = AY / len;
        float expectedZ = AZ / len;
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == expectedX);
        assertTrue(v[4] == expectedY);
        assertTrue(v[5] == expectedZ);
    }
    
    @Test
    public void testStaticNormalizeVector() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = normalize(mA1, v);
        float len = (float) Math.sqrt((AX * AX) + (AY * AY) + (AZ * AZ));
        float x = AX / len;
        float y = AY / len;
        float z = AZ / len;
        assertTrue(u == v);
        assertTrue(u.x == x && u.y == y && u.z == z);
    }
    
    @Test
    public void testStaticScaleToFloat() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = scaleTo(AX, AY, AZ, 15.0f, v);
        assertTrue(u == v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        u = scaleTo(AX, AY, AZ, 0, v);
        assertTrue(u.x == 0 && u.y == 0 && u.z == 0);
    }    
    
    
    
    
    
    
    
    
    
    






    @Test
    public void testStaticScaleToVector3() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = scaleTo(mA1, 15.0f, v);
        assertTrue(u == v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 14.999f && len < 15.001f);
        
        u = scaleTo(mA1, 0, v);
        assertTrue(u.x == 0 && u.y == 0 && u.z == 0);
    }

    @Test
    public void testStaticSloppyEqualsFloat() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));

        v = new Vector3(AX + 0.019f, AY, AZ);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));
        
        v = new Vector3(AX - 0.019f, AY, AZ);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));

        v = new Vector3(AX, AY + 0.019f, AZ);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));
        
        v = new Vector3(AX, AY - 0.019f, AZ);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));
        
        v = new Vector3(AX, AY, AZ + 0.019f);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));
        
        v = new Vector3(AX, AY, AZ - 0.019f);
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.0f));
        assertFalse(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.018f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.019f));
        assertTrue(sloppyEquals(AX, AY, AZ, v.x, v.y, v.z, 0.020f));
    }

    @Test
    public void testStaticSloppyEqualsVector3() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        assertTrue(sloppyEquals(mA1, v, 0.0f));

        v = new Vector3(AX + 0.019f, AY, AZ);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector3(AX - 0.019f, AY, AZ);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));

        v = new Vector3(AX, AY + 0.019f, AZ);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector3(AX, AY - 0.019f, AZ);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector3(AX, AY, AZ + 0.019f);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
        
        v = new Vector3(AX, AY, AZ - 0.019f);
        assertFalse(sloppyEquals(mA1, v, 0.0f));
        assertFalse(sloppyEquals(mA1, v, 0.018f));
        assertTrue(sloppyEquals(mA1, v, 0.019f));
        assertTrue(sloppyEquals(mA1, v, 0.020f));
    }

    @Test
    public void testStaticSubtractArrayArray() 
    {
        float[] v = subtract(mVectors, 1, mVectors, 1, mVectors, 1);
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == 0);
        assertTrue(v[4] == 0);
        assertTrue(v[5] == 0);
    }

    @Test
    public void testStaticSubtractFloatArray() 
    {
        float[] v = subtract(AX, AY, AZ, BX, BY, BZ, mVectors, 1);
        assertTrue(v == mVectors);
        assertTrue(v[0] == AX);
        assertTrue(v[1] == AY);
        assertTrue(v[2] == AZ);
        assertTrue(v[3] == AX - BX);
        assertTrue(v[4] == AY - BY);
        assertTrue(v[5] == AZ - BZ);
    }

    @Test
    public void testStaticSubtractFloats() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = subtract(AX, AY, AZ, BX, BY, BZ, v);
        assertTrue(u == v);
        assertTrue(u.x == AX - BX);
        assertTrue(u.y == AY - BY);
        assertTrue(u.z == AZ - BZ);
    }

    @Test
    public void testStaticSubtractVectors() 
    {
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = subtract(mA1, mB1, v);
        assertTrue(u == v);
        assertTrue(u.x == AX - BX);
        assertTrue(u.y == AY - BY);
        assertTrue(u.z == AZ - BZ);
    }

    @Test
    public void testStaticTranslateTowardFloat()
    {
        final float factor = 0.62f;
        final float x = AX + (BX - AX) * factor;
        final float y = AY + (BY - AY) * factor;
        final float z = AZ + (BZ - AZ) * factor;
        Vector3 v = new Vector3();
        Vector3 u = translateToward(AX, AY, AZ, BX, BY, BZ, factor, v);
        assertTrue(u == v);
        assertTrue(u.x == x);
        assertTrue(u.y == y);
        assertTrue(u.z == z);
    }
    
    @Test
    public void testStaticTruncateLengthFloat() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = truncateLength(AX, AY, AZ, 15.0f, v);
        assertTrue(u == v);
        assertTrue(u.x == AX && u.y == AY && u.z == AZ);
        
        u = truncateLength(AX, AY, AZ, 5.0f, v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        u = truncateLength(AX, AY, AZ, 0, v);
        assertTrue(u.x == 0 && u.y == 0 && u.z == 0);
    }

    @Test
    public void testStaticTruncateLengthVector3() 
    {
        // Can improve this test by checking for proper setting
        // of both x and y.
        Vector3 v = new Vector3(AX, AY, AZ);
        Vector3 u = truncateLength(mA1, 15.0f, v);
        assertTrue(u == v);
        assertTrue(u.x == AX && u.y == AY && u.z == AZ);
        
        u = truncateLength(mA1, 5.0f, v);
        float len = (float)Math.sqrt(u.lengthSq());
        assertTrue(len > 4.999f && len < 5.001f);
        
        u = truncateLength(mA1, 0, v);
        assertTrue(u.x == 0 && u.y == 0 && u.z == 0);
    }

}
