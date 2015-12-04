/*
 * Copyright (c) 2010-2012 Stephen A. Pratt
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
#if NUNITY
using Vector2 = org.critterai.Vector2;
#else
using Vector2 = UnityEngine.Vector2;
#endif

namespace org.critterai.geom
{
    /// <summary>
    /// Provides 2D line and line segment utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class Line2
    {
        /// <summary>
        /// Indicates whether or not line AB intersects line BC. (Integer version.)
        /// </summary>
        /// <param name="ax">The x-value for point A on line AB.</param>
        /// <param name="ay">The y-value for point A on line AB.</param>
        /// <param name="bx">The x-value for point B on line AB.</param>
        /// <param name="by">The y-value for point B on line AB.</param>
        /// <param name="cx">The x-value for point C on line CD.</param>
        /// <param name="cy">The y-value for point C on line CD.</param>
        /// <param name="dx">The x-value for point D on line CD.</param>
        /// <param name="dy">The y-value for point D on line CD.</param>
        /// <returns>True if the two lines are either collinear or intersect at one point.</returns>
        public static bool LinesIntersect(int ax, int ay, int bx, int by
                , int cx, int cy, int dx, int dy)
        {
            int numerator = 
                ((ay - cy) * (dx - cx)) - ((ax - cx) * (dy - cy));
            int denominator = 
                ((bx - ax) * (dy - cy)) - ((by - ay) * (dx - cx));
            if (denominator == 0 && numerator != 0)
                // Lines are parallel.
                return false; 
            // Lines are collinear or intersect at a single point.
            return true;  
        }
        
        /// <summary>
        /// Indicates whether or not line AB intersects line BC.
        /// </summary>
        /// <param name="a">Point A on line AB.</param>
        /// <param name="b">Point B on ling AB.</param>
        /// <param name="c">Point C on line CD.</param>
        /// <param name="d">Point D on line CD.</param>
        /// <returns>True if the two lines are either collinear or intersect at one point.</returns>
        public static bool LinesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float numerator = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));

            if (denominator == 0 && numerator != 0)
                // Lines are parallel.
                return false;

            // Lines are collinear or intersect at a single point.
            return true;  
        }
        
        /// <summary>
        /// Returns the distance squared from point P to the nearest point on line segment AB.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="a">Endpoint A of line segment AB.</param>
        /// <param name="b">Endpoing B of line segment AB.</param>
        /// <returns>The distance squared from the point to line segment AB.</returns>
        public static float GetPointSegmentDistanceSq(Vector2 p, Vector2 a, Vector2 b)
        {
            /*
             * Reference: 
             * http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
             * 
             * The goal of the algorithm is to find the point on line AB that 
             * is closest to P and then calculate the distance between P and 
             * that point.
             */

            Vector2 deltaAB = b - a;
            Vector2 deltaAP = p - a;    
            
            float segmentABLengthSq = deltaAB.x * deltaAB.x + deltaAB.y * deltaAB.y;
            
            if (segmentABLengthSq < MathUtil.Epsilon)
                // AB is not a line segment.  So just return
                // distanceSq from P to A
                return deltaAP.x * deltaAP.x + deltaAP.y * deltaAP.y;
                
            float u = (deltaAP.x * deltaAB.x + deltaAP.y * deltaAB.y) / segmentABLengthSq;
            
            if (u < 0)
                // Closest point on line AB is outside segment AB and 
                // closer to A. So return distanceSq from P to A.
                return deltaAP.x * deltaAP.x + deltaAP.y * deltaAP.y;
            else if (u > 1)
                // Closest point on line AB is outside segment AB and closer 
                // to B. So return distanceSq from P to B.
                return (p.x - b.x) * (p.x - b.x) + (p.y - b.y) * (p.y - b.y);
            
            // Closest point on lineAB is inside segment AB.  So find the exact
            // point on AB and calculate the distanceSq from it to P.
            
            // The calculation in parenthesis is the location of the point on 
            // the line segment.
            float deltaX = (a.x + u * deltaAB.x) - p.x;
            float deltaY = (a.y + u * deltaAB.y) - p.y;
        
            return deltaX * deltaX + deltaY * deltaY;
        }
        
        /// <summary>
        /// Returns the distance squared from point P to the nearest point on line AB.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="a">Point A on line AB.</param>
        /// <param name="b">Point B on line AB.</param>
        /// <returns>The distance squared from the point to line AB.</returns>
        public static float GetPointLineDistanceSq(Vector2 p, Vector2 a, Vector2 b)
        {
            /*
             * Reference: 
             * http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
             * 
             * The goal of the algorithm is to find the point on line AB that is
             * closest to P and then calculate the distance between P and that 
             * point.
             */

            Vector2 deltaAB = b - a;
            Vector2 deltaAP = p - a;         
            
            float segmentABLengthSq =  deltaAB.x * deltaAB.x + deltaAB.y * deltaAB.y;
            
            if (segmentABLengthSq < MathUtil.Epsilon)
                // AB is not a line segment.  So just return
                // distanceSq from P to A
                return deltaAP.x * deltaAP.x + deltaAP.y * deltaAP.y;
                
            float u = (deltaAP.x * deltaAB.x + deltaAP.y * deltaAB.y) / segmentABLengthSq;
            
            // The calculation in parenthesis is the location of the point on 
            // the line segment.
            float deltaX = (a.x + u * deltaAB.x) - p.x;
            float deltaY = (a.y + u * deltaAB.y) - p.y;
        
            return deltaX * deltaX + deltaY * deltaY;
        }
        
        /// <summary>
        /// Returns the normalized vector that is perpendicular to line AB. (Costly method!)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The direction of the vector will be to the right when viewed from point A to point B 
        /// along the line.
        /// </para>
        /// <para>
        /// Special Case: A zero length vector will be returned if the points are collocated.
        /// </para>
        /// </remarks>
        /// <param name="a">Point A on line AB.</param>
        /// <param name="b">Point B on line AB.</param>
        /// <returns>
        /// The normalized vector that is perpendicular to line AB, or a zero length vector if the 
        /// points are collocated.
        /// </returns>
        public static Vector2 GetNormalAB(Vector2 a, Vector2 b)
        {
            if (Vector2Util.SloppyEquals(a, b, MathUtil.Tolerance))
                // Points do not form a line.
                return new Vector2();

            Vector2 result = Vector2Util.GetDirectionAB(a, b);

            float origX = result.x;
            result.x = result.y;
            result.y = -origX;

            return result;
        }
        
        /// <summary>
        /// Determines the relationship between lines AB and CD.
        /// </summary>
        /// <remarks>
        /// <para>
        /// While this check is technically inclusive of segment end points, floating point errors
        /// can result in end point intersection being missed.  If this matters, a 
        /// <see  cref="Vector2Util.SloppyEquals(Vector2, Vector2, float)">
        /// SloppyEquals</see> or similar test of the intersection point can be performed.
        /// </para>
        /// </remarks>
        /// <param name="a">Point A on line AB.</param>
        /// <param name="b">Point B on ling AB.</param>
        /// <param name="c">Point C on line CD.</param>
        /// <param name="d">Point D on line CD.</param>
        /// <param name="intersectPoint">The point of intersection, if applicable.</param>
        /// <returns>The relationship between lines AB and CD.</returns>
        public static LineRelType GetRelationship(Vector2 a, Vector2 b, Vector2 c, Vector2 d
            , out Vector2 intersectPoint)
        {
            Vector2 deltaAB = b - a;
            Vector2 deltaCD = d - c;
            Vector2 deltaCA = a - c; 
            
            float numerator =  (deltaCA.y * deltaCD.x) - (deltaCA.x * deltaCD.y);
            float denominator = (deltaAB.x * deltaCD.y) - (deltaAB.y * deltaCD.x);

            // Exit early if the lines do not intersect at a single point.
            if (denominator == 0)
            {
                intersectPoint = Vector2Util.Zero;
                if (numerator == 0)
                    return LineRelType.Collinear;
                return LineRelType.Parallel;
            }

            // Lines definitely intersect at a single point.
            
            float factorAB = numerator / denominator;
            float factorCD = ((deltaCA.y * deltaAB.x) - (deltaCA.x * deltaAB.y)) / denominator;

            intersectPoint = 
                new Vector2(a.x + (factorAB * deltaAB.x), a.y + (factorAB * deltaAB.y));            

            // Determine the type of intersection
            if ((factorAB >= 0.0f) 
                && (factorAB <= 1.0f) 
                && (factorCD >= 0.0f) 
                && (factorCD <= 1.0f))
            {
                return LineRelType.SegmentsIntersect;
            }
            else if ((factorCD >= 0.0f) && (factorCD <= 1.0f))
            {
                return LineRelType.ALineCrossesBSeg;
            }
            else if ((factorAB >= 0.0f) && (factorAB <= 1.0f))
            {
                return LineRelType.BLineCrossesASeg;
            }

            return LineRelType.LinesIntersect;
        }
    }
}
