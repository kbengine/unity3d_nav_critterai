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
import static org.critterai.math.geom.Polygon3.*;

import org.critterai.math.Vector3;
import org.critterai.math.geom.Polygon3;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Polygon3} class.
 */
public class Polygon3Tests {

    private static final float AX = -2;
    private static final float AY = -2;
    private static final float AZ = 1;
    private static final float BX = -1;
    private static final float BY = 0;
    private static final float BZ = 2;
    private static final float CX = 0;
    private static final float CY = 2;
    private static final float CZ = 2;
    private static final float DX = 1;
    private static final float DY = 4;
    private static final float DZ = 1;
    private static final float EX = 1;
    private static final float EY = 4;
    private static final float EZ = 0;
    private static final float FX = 0;
    private static final float FY = 2;
    private static final float FZ = -1;
    private static final float GX = -1;
    private static final float GY = 0;
    private static final float GZ = -1;
    private static final float HX = -2;
    private static final float HY = -2;
    private static final float HZ = 0;
    private static final float JX = 2;
    private static final float JY = 6;
    private static final float JZ = 1;
    private static final float KX = -4;
    private static final float KY = 0;
    private static final float KZ = 2;
    
    private static final float CENX = -0.5f;
    private static final float CENY = 1.0f;
    private static final float CENZ = 0.5f;
    
    private float[] mVerts;
    
    @Before
    public void setUp() throws Exception 
    {
        mVerts = new float[10*3];
        mVerts[0] = KX;    // Padding
        mVerts[1] = KY;
        mVerts[2] = KZ;
        mVerts[3] = AX;    // Start of poly
        mVerts[4] = AY;
        mVerts[5] = AZ;
        mVerts[6] = BX;
        mVerts[7] = BY;
        mVerts[8] = BZ;
        mVerts[9] = CX;
        mVerts[10] = CY;
        mVerts[11] = CZ;
        mVerts[12] = DX;
        mVerts[13] = DY;
        mVerts[14] = DZ;
        mVerts[15] = EX; // J insertion point.
        mVerts[16] = EY;
        mVerts[17] = EZ;
        mVerts[18] = FX;
        mVerts[19] = FY;
        mVerts[20] = FZ;
        mVerts[21] = GX;
        mVerts[22] = GY;
        mVerts[23] = GZ; 
        mVerts[24] = HX; 
        mVerts[25] = HY;
        mVerts[26] = HZ; // End of poly
        mVerts[27] = KX; // Padding and centroid storage location.
        mVerts[28] = KY;
        mVerts[29] = KZ;
    }

    @Test
    public void testIsConvexStandardTrue() 
    {
        assertTrue(isConvex(mVerts, 1, 8));
    }
    
    @Test
    public void testIsConvexStandardFalse() 
    {
        mVerts[15] = JX;
        mVerts[16] = JY;
        mVerts[17] = JZ;
        assertFalse(isConvex(mVerts, 1, 8));
    }
    
    @Test
    public void testIsConvexVerticalTrue() 
    {
        for (int p = 1; p < mVerts.length; p += 3)
        {
            mVerts[p] = mVerts[p+1];
            mVerts[p+1] = -2;
        }
        assertTrue(isConvex(mVerts, 1, 8));
    }
    
    @Test
    public void testIsConvexVerticalFalse() 
    {
        mVerts[15] = JX;
        mVerts[16] = JY;
        mVerts[17] = JZ;
        for (int p = 1; p < mVerts.length; p += 3)
        {
            mVerts[p] = mVerts[p+1];
            mVerts[p+1] = -2;
        }
        assertFalse(isConvex(mVerts, 1, 8));
    }

    @Test
    public void testGetCentroidArray() 
    {
        assertTrue(mVerts == getCentroid(mVerts, 1, 8, mVerts, 9));
        assertTrue(mVerts[27] == CENX);
        assertTrue(mVerts[28] == CENY);
        assertTrue(mVerts[29] == CENZ);
    }

    @Test
    public void testGetCentroidVector3() 
    {
        Vector3 v = new Vector3(5, 5, 5);  // Needs to be seeded with non-zero.
        assertTrue(v == getCentroid(mVerts, 1, 8, v));
        assertTrue(v.x == CENX);
        assertTrue(v.y == CENY);
        assertTrue(v.z == CENZ);
    }
    
    @Test
    public void testGetCentroidFloatList() 
    {
        Vector3 v = new Vector3(5, 5, 5);  // Needs to be seeded with non-zero.
        assertTrue(v == getCentroid(v
                , AX, AY, AZ
                , BX, BY, BZ
                , CX, CY, CZ
                , DX, DY, DZ
                , EX, EY, EZ
                , FX, FY, FZ
                , GX, GY, GZ
                , HX, HY, HZ));
        assertTrue(v.x == CENX);
        assertTrue(v.y == CENY);
        assertTrue(v.z == CENZ);
    }

}
