package org.critterai.nmgen;

/**
 * Provides shared computational geometry operations.
 * <p>This is a temporary class.  Its functionality will eventually
 * be merged into classes in the utility library.</p>
 */
public final class Geometry
{
    
    /*
     * Design Notes:
     * 
     * Until computational geometry functions are moved to the utilities
     * library, they will slowly be migrated here as needed for easier 
     * unit testing.
     * 
     */
    
    // TODO: GENERALIZATION: Move these algorithms to the utility library.
    
    private Geometry() { }
    
    /**
     * Returns the distance squared from the point to the line segment.
     * <p>Behavior is undefined if the the closest distance is outside the
     * line segment.</p>
     * @param px The x-value of point (px, py).
     * @param py The y-value of point (px, py)
     * @param ax The x-value of the line segment's vertex A.
     * @param ay The y-value of the line segment's vertex A.
     * @param bx The x-value of the line segment's vertex B.
     * @param by The y-value of the line segment's vertex B.
     * @return The distance squared from the point (px, py) to line segment AB.
     */
    public static float getPointSegmentDistanceSq(int px, int py
            , int ax, int ay
            , int bx, int by)
    {
        
        /*
         * Reference: http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
         * 
         * The goal of the algorithm is to find the point on line segment AB
         * that is closest to P and then calculate the distance between P
         * and that point.
         */
        
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        final float deltaAPx = px - ax;
        final float deltaAPy = py - ay;
        
        final float segmentABLengthSq =
            deltaABx * deltaABx + deltaABy * deltaABy;
        
        if (segmentABLengthSq == 0)
            // AB is not a line segment.  So just return
            // distanceSq from P to A
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
            
        final float u =
            (deltaAPx * deltaABx + deltaAPy * deltaABy) / segmentABLengthSq;
        
        if (u < 0)
            // Closest point on line AB is outside outside segment AB and
            // closer to A. So return distanceSq from P to A.
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
        else if (u > 1)
            // Closest point on line AB is outside segment AB and closer to B.
            // So return distanceSq from P to B.
            return (px - bx)*(px - bx) + (py - by)*(py - by);
        
        // Closest point on lineAB is inside segment AB.  So find the exact
        // point on AB and calculate the distanceSq from it to P.
        
        // The calculation in parenthesis is the location of the point on
        // the line segment.
        final float deltaX = (ax + u * deltaABx) - px;
        final float deltaY = (ay + u * deltaABy) - py;
    
        return deltaX*deltaX + deltaY*deltaY;
    }
    
    public static float getPointSegmentDistanceSq(float px
                    , float py
                    , float pz
                    , float ax
                    , float ay
                    , float az
                    , float bx
                    , float by
                    , float bz)
    {
        
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        final float deltaABz = bz - az;
        float deltaAPx = px - ax;
        float deltaAPy = py - ay;
        float deltaAPz = pz - az;
        
        final float segmentABDistSq = deltaABx * deltaABx
                                        + deltaABy * deltaABy
                                        + deltaABz * deltaABz;
        if (segmentABDistSq == 0)
            // AB is not a line segment.  So just return
            // distanceSq from P to A.
            return deltaAPx * deltaAPx
                        + deltaAPy * deltaAPy
                        + deltaAPz * deltaAPz;
        
        float u = (deltaABx * deltaAPx
                            + deltaABy * deltaAPy
                            + deltaABz * deltaAPz) / segmentABDistSq;
        
        if (u < 0)
            // Closest point on line AB is outside outside segment AB and
            // closer to A. So return distanceSq from P to A.
            return deltaAPx * deltaAPx
                        + deltaAPy * deltaAPy
                        + deltaAPz * deltaAPz;
        else if (u > 1)
            // Closest point on line AB is outside segment AB and closer to B.
            // So return distanceSq from P to B.
            return (px - bx)*(px - bx) 
                        + (py - by)*(py - by) 
                        + (pz - bz)*(pz - bz);
        
        
        // Closest point on lineAB is inside segment AB.  So find the exact
        // point on AB and calculate the distanceSq from it to P.
        
        // The calculation in parenthesis is the location of the point on
        // the line segment.
        final float deltaX = (ax + u * deltaABx) - px;
        final float deltaY = (ay + u * deltaABy) - py;
        final float deltaZ = (az + u * deltaABz) - pz;
    
        return deltaX*deltaX + deltaY*deltaY + deltaZ*deltaZ;

    }
    
    /**
     * Returns TRUE if line segment AB intersects with line segment CD in any
     * manner. Either collinear or at a single point.
     * @param ax The x-value for point (ax, ay) in line segment AB.
     * @param ay The y-value for point (ax, ay) in line segment AB.
     * @param bx The x-value for point (bx, by) in line segment AB.
     * @param by The y-value for point (bx, by) in line segment AB.
     * @param cx The x-value for point (cx, cy) in line segment CD.
     * @param cy The y-value for point (cx, cy) in line segment CD.
     * @param dx The x-value for point (dx, dy) in line segment CD.
     * @param dy The y-value for point (dx, dy) in line segment CD.
     * @return TRUE if line segment AB intersects with line segment CD in any
     * manner.
     */
    public static boolean segmentsIntersect(int ax
            , int ay
            , int bx
            , int by
            , int cx
            , int cy
            , int dx
            , int dy)
    {
        
        /*
         * This is modified 2D line-line intersection/segment-segment
         * intersection test.
         */
        
        int deltaABx = bx - ax;
        int deltaABy = by - ay;
        int deltaCAx = ax - cx;
        int deltaCAy = ay - cy;
        int deltaCDx = dx - cx;
        int deltaCDy = dy - cy;

        int numerator = (deltaCAy * deltaCDx) - (deltaCAx * deltaCDy);
        int denominator = (deltaABx * deltaCDy) - (deltaABy * deltaCDx);

        // Perform early exit tests.
        if (denominator == 0 && numerator != 0)
        {
            // If numerator is zero, then the lines are colinear.
            // Since it isn't, then the lines must be parallel.
            return false;
        }

        // Lines intersect.  But do the segments intersect?
        
        // Forcing float division on both of these via casting of the
        // denominator.
        float factorAB = numerator / (float)denominator;
        float factorCD = ((deltaCAy * deltaABx) - (deltaCAx * deltaABy))
                                / (float)denominator;

        // Determine the type of intersection
        if ((factorAB >= 0.0f)
                && (factorAB <= 1.0f)
                && (factorCD >= 0.0f)
                && (factorCD <= 1.0f))
        {
            return true;  // The two segments intersect.
        }
        
        // The lines intersect, but segments to not.

        return false;
    }
}
