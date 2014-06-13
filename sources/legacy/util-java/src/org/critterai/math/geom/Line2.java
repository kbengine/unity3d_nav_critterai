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

import org.critterai.math.MathUtil;
import org.critterai.math.Vector2;

/**
 * Provides operations related to 2-dimensional lines and line segments.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks, divide by zero checks only when needed by the algorithm, etc.</p>
 * <p>Static operations are thread safe.</p>
 */
public final class Line2
{
    
    private Line2() { }
    
    /**
     * Indicates whether or not line AB intersects line BC.
     * @param ax The x-value for point A on line AB.
     * @param ay The y-value for point A on line AB.
     * @param bx The x-value for point B on line AB.
     * @param by The y-value for point B on line AB.
     * @param cx The x-value for point C on line CD.
     * @param cy The y-value for point C on line CD.
     * @param dx The x-value for point D on line CD.
     * @param dy The y-value for point D on line CD.
     * @return TRUE if the two lines are either collinear or intersect at one point.
     * Otherwise FALSE.
     */
    public static boolean linesIntersect(int ax, int ay
            , int bx, int by
            , int cx, int cy
            , int dx, int dy)
    {
        int numerator = ((ay - cy) * (dx - cx)) - ((ax - cx) * (dy - cy));
        int denominator = ((bx - ax) * (dy - cy)) - ((by - ay) * (dx - cx));
        if (denominator == 0 && numerator != 0)
            // Lines are parallel.
            return false; 
        // Lines are collinear or intersect at a single point.
        return true;  
    }
    
    /**
     * Indicates whether or not line AB intersects line BC.
     * @param ax The x-value for point A on line AB.
     * @param ay The y-value for point A on line AB.
     * @param bx The x-value for point B on line AB.
     * @param by The y-value for point B on line AB.
     * @param cx The x-value for point C on line CD.
     * @param cy The y-value for point C on line CD.
     * @param dx The x-value for point D on line CD.
     * @param dy The y-value for point D on line CD.
     * @return TRUE if the two lines are either collinear or intersect at one point.
     * Otherwise FALSE.
     */
    public static boolean linesIntersect(float ax, float ay
            , float bx, float by
            , float cx, float cy
            , float dx, float dy)
    {
        float numerator = ((ay - cy) * (dx - cx)) - ((ax - cx) * (dy - cy));
        float denominator = ((bx - ax) * (dy - cy)) - ((by - ay) * (dx - cx));
        if (denominator == 0 && numerator != 0)
            // Lines are parallel.
            return false; 
        // Lines are collinear or intersect at a single point.
        return true;  
    }
    
    /**
     * Returns the distance squared from point P to line segment AB.
     * @param px The x-value of point P.
     * @param py The y-value of point P.
     * @param ax The x-value of point A of line segment AB.
     * @param ay The y-value of point A of line segment AB.
     * @param bx The x-value of point B of line segment AB.
     * @param by The y-value of point B of line segment AB.
     * @return The distance squared from point B to line segment AB.
     */
    public static float getPointSegmentDistanceSq(float px, float py
            , float ax, float ay
            , float bx, float by)
    {
        
        /*
         * Reference: http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
         * 
         * The goal of the algorithm is to find the point on line AB that is
         * closest to P and then calculate the distance between P and that point.
         */
        
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        final float deltaAPx = px - ax;
        final float deltaAPy = py - ay;        
        
        final float segmentABLengthSq = deltaABx * deltaABx + deltaABy * deltaABy;
        
        if (segmentABLengthSq < MathUtil.EPSILON_STD)
            // AB is not a line segment.  So just return
            // distanceSq from P to A
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
            
        final float u = (deltaAPx * deltaABx + deltaAPy * deltaABy) / segmentABLengthSq;
        
        if (u < 0)
            // Closest point on line AB is outside outside segment AB and closer to A.
            // So return distanceSq from P to A.
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
        else if (u > 1)
            // Closest point on line AB is outside segment AB and closer to B.
            // So return distanceSq from P to B.
            return (px - bx)*(px - bx) + (py - by)*(py - by);
        
        // Closest point on lineAB is inside segment AB.  So find the exact point on AB
        // and calculate the distanceSq from it to P.
        
        // The calculation in parenthesis is the location of the point on the line segment.
        final float deltaX = (ax + u * deltaABx) - px;
        final float deltaY = (ay + u * deltaABy) - py;
    
        return deltaX*deltaX + deltaY*deltaY;
    }
    
    /**
     * Returns the distance squared from point P to line AB.
     * @param px The x-value of point P.
     * @param py The y-value of point P.
     * @param ax The x-value of point A of line AB.
     * @param ay The y-value of point A of line AB.
     * @param bx The x-value of point B of line AB.
     * @param by The y-value of point B of line AB.
     * @return The distance squared from point B to line AB.
     */
    public static float getPointLineDistanceSq(float px, float py, float ax, float ay, float bx, float by)
    {
        
        /*
         * Reference: http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
         * 
         * The goal of the algorithm is to find the point on line AB that is
         * closest to P and then calculate the distance between P and that point.
         */
        
        final float deltaABx = bx - ax;
        final float deltaABy = by - ay;
        final float deltaAPx = px - ax;
        final float deltaAPy = py - ay;        
        
        final float segmentABLengthSq = deltaABx * deltaABx + deltaABy * deltaABy;
        
        if (segmentABLengthSq < MathUtil.EPSILON_STD)
            // AB is not a line segment.  So just return
            // distanceSq from P to A
            return deltaAPx * deltaAPx + deltaAPy * deltaAPy;
            
        final float u = (deltaAPx * deltaABx + deltaAPy * deltaABy) / segmentABLengthSq;
        
        // The calculation in parenthesis is the location of the point on the line segment.
        final float deltaX = (ax + u * deltaABx) - px;
        final float deltaY = (ay + u * deltaABy) - py;
    
        return deltaX*deltaX + deltaY*deltaY;
    }
    
    /**
     * Returns the normalized vector that is perpendicular to line AB, or a zero
     * length vector if points A and B do not form a line.
     * <p>The direction of the vector will be to the right when viewed from point A to point B
     * along the line.</p>
     * <p>WARNING: This is an expensive operation.</p>
     * @param ax The x-value of point A on line AB.
     * @param ay The y-value of point A on line AB.
     * @param bx The x-value of point B on line AB.
     * @param by The y-value of point B on line AB.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 getNormalAB(float ax, float ay
            , float bx, float by
            , Vector2 out)
    {
        if (ax == bx && ay == by)
        {
            // Points do not form a line.
            out.set(0, 0);
        }
        else
        {
            Vector2.getDirectionAB(ax, ay, bx, by, out);
            out.set(out.y, -out.x);            
        }
        return out;
    }
    
    /**
     * Determines the relationship between lines AB and CD.
     * <p>While this check is technically inclusive of the segment end points, floating point
     * errors can result in end point intersection being missed.  If this matters, 
     * a {@link Vector2#sloppyEquals(float, float, float)} or similar test of the intersection point can
     * be performed to check for this issue.</p>
     * @param ax The x-value for point A on line AB.
     * @param ay The y-value for point A on line AB.
     * @param bx The x-value for point B on line AB.
     * @param by The y-value for point B on line AB.
     * @param cx The x-value for point C on line CD.
     * @param cy The y-value for point C on line CD.
     * @param dx The x-value for point D on line CD.
     * @param dy The y-value for point D on line CD.
     * @param outIntersectionPoint  If provided and the lines intersect at
     * a single point, the vector will be updated with the point of intersection.
     * (This argument may be null.)
     * @return The relationship between lines AB and CD.
     */
    public static LineRelType getRelationship(float ax, float ay
            , float bx, float by
            , float cx, float cy
            , float dx, float dy
            , Vector2 outIntersectionPoint)
    {
        
        float deltaAxBx = bx - ax;    
        float deltaAyBy = by - ay;
        
        float deltaCxDx = dx - cx;
        float deltaCyDy = dy - cy;
        
        float deltaCyAy = ay - cy;
        float deltaCxAx = ax - cx;    
        
        float numerator = (deltaCyAy * deltaCxDx) - (deltaCxAx * deltaCyDy);
        float denominator = (deltaAxBx * deltaCyDy) - (deltaAyBy * deltaCxDx);

        // Exit early if the lines do not intersect at a single point.
        if (denominator == 0)
        {
            if (numerator == 0)
                return LineRelType.COLLINEAR;
            return LineRelType.PARALLEL;
        }

        // Lines definitely intersect at a single point.
        
        float factorAB = numerator / denominator;
        float factorCD = ((deltaCyAy * deltaAxBx) - (deltaCxAx * deltaAyBy)) / denominator;

        if (outIntersectionPoint != null)
            outIntersectionPoint.set(ax + (factorAB * deltaAxBx)
                    , ay + (factorAB * deltaAyBy));            

        // Determine the type of intersection
        if ((factorAB >= 0.0f) && (factorAB <= 1.0f) && (factorCD >= 0.0f) && (factorCD <= 1.0f))
        {
            return LineRelType.SEGMENTS_INTERSECT;
        }
        else if ((factorCD >= 0.0f) && (factorCD <= 1.0f))
        {
            return LineRelType.ALINE_CROSSES_BSEG;
        }
        else if ((factorAB >= 0.0f) && (factorAB <= 1.0f))
        {
            return LineRelType.BLINE_CROSSES_ASEG;
        }

        return LineRelType.LINES_INTERSECT;
        
    }
    
}
