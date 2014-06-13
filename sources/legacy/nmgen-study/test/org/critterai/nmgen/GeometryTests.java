package org.critterai.nmgen;

import static org.junit.Assert.*;
import static org.critterai.nmgen.Geometry.*;
import org.junit.Test;

/**
 * Tests functionality in the Geomtry class.
 */
public final class GeometryTests
{

    private static final float TOLERANCE = 0.0001f;
    
    @Test
    public void testPointSegmentDistance3DFloatBasic()
    {
        final float ax = -4;
        final float ay = 1;
        final float az = 2;
        
        final float bx = 2;
        final float by = 1;
        final float bz = 2;
        
        float px = 0;
        float py = 1;
        float pz = 2;
        
        float actual = getPointSegmentDistanceSq(px, py, pz
                                        , ax, ay, az
                                        , bx, by, bz);
        assertTrue(sloppyEquals(0.0f, actual, TOLERANCE));
        
        px = 0;
        py = 2;
        pz = 2;
        
        actual = getPointSegmentDistanceSq(px, py, pz
                                        , ax, ay, az
                                        , bx, by, bz);
        assertTrue(sloppyEquals(1.0f, actual, TOLERANCE));
        
        px = 0;
        py = 0;
        pz = 2;
        
        actual = getPointSegmentDistanceSq(px, py, pz
                                        , ax, ay, az
                                        , bx, by, bz);
        assertTrue(sloppyEquals(1.0f, actual, TOLERANCE));
        
        px = 4;
        py = 1;
        pz = 2;
        
        actual = getPointSegmentDistanceSq(px, py, pz
                                        , ax, ay, az
                                        , bx, by, bz);
        assertTrue(sloppyEquals(4.0f, actual, TOLERANCE));
        
        px = -6;
        py = 1;
        pz = 2;
        
        actual = getPointSegmentDistanceSq(px, py, pz
                                        , ax, ay, az
                                        , bx, by, bz);
        assertTrue(sloppyEquals(4.0f, actual, TOLERANCE));
        
    }
    
    /**
     * Determines whether the values are within the specified tolerance
     * of each other. 
     * <p>The values are considered equal if the following condition is met:
     * (b >= a - tolerance && b <= a + tolerance)</p>
     * @param a The a-value to compare the b-value against.
     * @param b The b-value to compare against the a-value.
     * @param tolerence The tolerance to use for the comparison.
     * @return TRUE if the values are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(float a, float b, float tolerence)
    {
        return !(b < a - tolerence || b > a + tolerence);
    }
    
}
