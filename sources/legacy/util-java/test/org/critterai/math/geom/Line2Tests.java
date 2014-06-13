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
import static org.critterai.math.geom.Line2.*;

import org.critterai.math.Vector2;
import org.critterai.math.geom.Line2;
import org.critterai.math.geom.LineRelType;
import org.junit.Before;
import org.junit.Test;

/**
 * Unit tests for the {@link Line2} class.
 */
public class Line2Tests {

    private static final float AX = -5;
    private static final float AY = 3;
    private static final float BX = 1;
    private static final float BY = 1;
    private static final float CX = -3;
    private static final float CY = 0;
    private static final float DX = -1;
    private static final float DY = 4;
    private static final float EX = -2;
    private static final float EY = 2;
    private static final float FX = 4;
    private static final float FY = 0;
    private static final float GX = 0;
    private static final float GY = 3;
    private static final float HX = 2;
    private static final float HY = -1;
    private static final float JX = -4;
    private static final float JY = 1;
    private static final float KX = -4;
    private static final float KY = -2;
    
    private static final int AXI = -5;
    private static final int AYI = 3;
    private static final int BXI = 1;
    private static final int BYI = 1;
    private static final int CXI = -3;
    private static final int CYI = 0;
    private static final int DXI = -1;
    private static final int DYI = 4;
    private static final int EXI = -2;
    private static final int EYI = 2;
    private static final int FXI = 4;
    private static final int FYI = 0;
    private static final int GXI = 0;
    private static final int GYI = 3;
    
    @Before
    public void setUp() throws Exception 
    {
    }

    @Test
    public void testLinesIntersectFloat() 
    {
        // Standard. Cross within segments.
        assertTrue(linesIntersect(AX, AY, BX, BY, CX, CY, DX, DY));
        assertTrue(linesIntersect(BX, BY, AX, AY, CX, CY, DX, DY));
        assertTrue(linesIntersect(BX, BY, AX, AY, DX, DY, CX, CY));
        assertTrue(linesIntersect(AX, AY, BX, BY, DX, DY, CX, CY));
        
        // Standard. Cross outside segment.
        assertTrue(linesIntersect(AX, AY, BX, BY, DX, DY, GX, GY));
        assertTrue(linesIntersect(BX, BY, AX, AY, DX, DY, GX, GY));
        assertTrue(linesIntersect(BX, BY, AX, AY, GX, GY, DX, DY));
        assertTrue(linesIntersect(AX, AY, BX, BY, GX, GY, DX, DY));
        
        // Collinear
        assertTrue(linesIntersect(AX, AY, BX, BY, EX, EY, FX, FY));
        assertTrue(linesIntersect(BX, BY, AX, AY, EX, EY, FX, FY));
        assertTrue(linesIntersect(BX, BY, AX, AY, FX, FY, EX, EY));
        assertTrue(linesIntersect(AX, AY, BX, BY, FX, FY, EX, EY));
        
        // Parallel Diagonal
        assertFalse(linesIntersect(AX, AY, BX, BY, EX-2, EY, FX-2, FY));
        assertFalse(linesIntersect(BX, BY, AX, AY, EX-2, EY, FX-2, FY));
        assertFalse(linesIntersect(BX, BY, AX, AY, FX-2, FY, EX-2, EY));
        assertFalse(linesIntersect(AX, AY, BX, BY, FX-2, FY, EX-2, EY));
        
        // Parallel Vertical
        assertFalse(linesIntersect(AX, 5, BX, 5, EX, 3, FX, 3));

        // Parallel Horizontal
        assertFalse(linesIntersect(5, AY, 5, BY, 2, CY, 2, DY));
    }

    @Test
    public void testLinesIntersectInt() 
    {
        // Standard. Cross within segments.
        assertTrue(linesIntersect(AXI, AYI, BXI, BYI, CXI, CYI, DXI, DYI));
        
        // Standard. Cross outside segment.
        assertTrue(linesIntersect(AXI, AYI, BXI, BYI, DXI, DYI, GXI, GYI));
        
        // Collinear
        assertTrue(linesIntersect(AXI, AYI, BXI, BYI, EXI, EYI, FXI, FYI));
        
        // Parallel Diagonal
        assertFalse(linesIntersect(AXI, AYI, BXI, BYI, EXI-2, EYI, FXI-2, FYI));
        
        // Parallel Vertical
        assertFalse(linesIntersect(AXI, 5, BXI, 5, EXI, 3, FXI, 3));

        // Parallel Horizontal
        assertFalse(linesIntersect(5, AYI, 5, BYI, 2, CYI, 2, DYI));    
    }
    
    @Test
    public void testGetPointSegmentDistanceSqFloat() 
    {
        // Closest to end point B.
        float expected = Vector2.getDistanceSq(HX, HY, BX, BY);
        assertTrue(getPointSegmentDistanceSq(HX, HY, EX, EY, BX, BY) == expected);
        
        // Closest to end point E.
        expected = Vector2.getDistanceSq(JX, JY, EX, EY);
        assertTrue(getPointSegmentDistanceSq(JX, JY, EX, EY, BX, BY) == expected);
        
        // Closest to mid-point of AB. (E)
        expected = Vector2.getDistanceSq(-1, 5, EX, EY);
        assertTrue(getPointSegmentDistanceSq(-1, 5, AX, AY, BX, BY) == expected);
        expected = Vector2.getDistanceSq(-3, -1, EX, EY);
        assertTrue(getPointSegmentDistanceSq(-3, -1, AX, AY, BX, BY) == expected);
    }
    
    @Test
    public void testGetPointLineDistanceSqFloat() 
    {
        
        float expected = Vector2.getDistanceSq(-3, -1, EX, EY);
        float actual = getPointLineDistanceSq(-3, -1, AX, AY, BX, BY);
        assertTrue(actual == expected);
        expected = Vector2.getDistanceSq(-1, 5, EX, EY);
        actual = getPointLineDistanceSq(-1, 5, AX, AY, BX, BY);
        assertTrue(actual == expected);
        expected = 0;
        actual = getPointLineDistanceSq(FX, FY, AX, AY, BX, BY);
        assertTrue(actual == expected);
        
        
    }
    
    @Test
    public void testGetNormalAB() 
    {
        
        // Diagonal
        Vector2 v = new Vector2();
        Vector2 expected = new Vector2(-1, -3).normalize();
        assertTrue(v == getNormalAB(AX, AY, BX, BY, v));
        assertTrue(v.sloppyEquals(expected, 0.0001f));
        
        // Reversed Diagonal
        expected = new Vector2(1, 3).normalize();
        getNormalAB(BX, BY, AX, AY, v);
        assertTrue(v.sloppyEquals(expected, 0.0001f));
        
        // Vertical
        expected = new Vector2(-1, 0);
        getNormalAB(5, AY, 5, BY, v);
        assertTrue(v.sloppyEquals(expected, 0.0001f));
        
        // Horizontal
        expected = new Vector2(0, -1);
        getNormalAB(AX, 5, BX, 5, v);
        assertTrue(v.sloppyEquals(expected, 0.0001f));
        
        // Not a line.
        getNormalAB(AX, AY, AX, AY, v);
        assertTrue(v.equals(0, 0));
        
    }

    @Test
    public void testGetRelationship() 
    {
        Vector2 v = new Vector2();
        assertTrue(LineRelType.SEGMENTS_INTERSECT 
                == getRelationship(AX, AY, BX, BY, CX, CY, DX, DY, v));
        assertTrue(v.sloppyEquals(EX, EY, 0.0001f));
    
        assertTrue(LineRelType.ALINE_CROSSES_BSEG 
                == getRelationship(AX, AY, EX, EY, GX, GY, HX, HY, v));
        assertTrue(v.sloppyEquals(BX, BY, 0.0001f));
        
        // Line reversal checks.
        v = new Vector2();
        assertTrue(LineRelType.ALINE_CROSSES_BSEG 
                == getRelationship(EX, EY, AX, AY, GX, GY, HX, HY, v));
        assertTrue(v.sloppyEquals(BX, BY, 0.0001f));
        v = new Vector2();
        assertTrue(LineRelType.ALINE_CROSSES_BSEG 
                == getRelationship(EX, EY, AX, AY, HX, HY, GX, GY, v));
        assertTrue(v.sloppyEquals(BX, BY, 0.0001f));
        v = new Vector2();
        assertTrue(LineRelType.ALINE_CROSSES_BSEG 
                == getRelationship(AX, AY, EX, EY, HX, HY, GX, GY, v));
        assertTrue(v.sloppyEquals(BX, BY, 0.0001f));

        assertTrue(LineRelType.BLINE_CROSSES_ASEG 
                == getRelationship(AX, AY, BX, BY, KX, KY, CX, CY, v));
        assertTrue(v.sloppyEquals(EX, EY, 0.0001f));
        
        v = new Vector2();
        assertTrue(LineRelType.LINES_INTERSECT 
                == getRelationship(KX, KY, CX, CY, FX, FY, BX, BY, v));
        assertTrue(v.sloppyEquals(EX, EY, 0.0001f));
        
        // NULL version.
        assertTrue(LineRelType.SEGMENTS_INTERSECT 
                == getRelationship(AX, AY, BX, BY, CX, CY, DX, DY, null));
        
        v = new Vector2(JX, JY);    
        
        assertTrue(LineRelType.PARALLEL 
                == getRelationship(AX, AY, BX, BY, EX-2, EY, FX-2, FY, null));
        assertTrue(v.equals(JX, JY));
        
        // Collinear - No segment overlap.
        assertTrue(LineRelType.COLLINEAR 
                == getRelationship(AX, AY, EX, EY, BX, BY, FX, FY, null));
        assertTrue(v.equals(JX, JY));
        
        // Collinear - Segment overlap.
        assertTrue(LineRelType.COLLINEAR 
                == getRelationship(AX, AY, BX, BY, EX, EY, FX, FY, null));
        assertTrue(v.equals(JX, JY));
        
        
        
    }

}
