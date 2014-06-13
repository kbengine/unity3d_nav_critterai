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


/**
 * Provides operations related to 2-dimensional rectangles.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No checks are performed to ensure that maximums are greater than minimums.</p>
 * <p>Static operations are thread safe.</p>
 */
public class Rectangle2 
{

    private Rectangle2() { }
    
    /**
     * Indicates whether or not a point is contained within an axis-aligned rectangle.
     * The test is inclusive of the edges of the rectangle.  I.e. If the point lies
     * on the edge of the rectangle, then it is contained by the rectangle.
     * @param minX The minimum x-axis bounds of the rectangle.
     * @param minY The minimum y-axis bounds of the rectangle.
     * @param maxX The maximum x-axis bounds of the rectangle.
     * @param maxY The maximum y-axis bounds of the rectangle.
     * @param x The x-value of the point (x, y) to test.
     * @param y The y-value of the point (x, y) to test.
     * @return TRUE if the point lies within the rectangle.  Otherwise FALSE.
     */
    public static boolean contains(float minX, float minY, float maxX, float maxY, float x, float y)
    {
        return !(x < minX || y < minY || x > maxX || y > maxY);
    }
    
    /**
     * Indicates whether or not an axis-alighed rectangle (B) is contained within another
     * axis-aligned rectangle (A).
     * The test is inclusive of the edges of the rectangles.
     * @param minXA The minimum x-axis bounds of rectangle A.
     * @param minYA The minimum y-axis bounds of rectangle A.
     * @param maxXA The maximum x-axis bounds of rectangle A.
     * @param maxYA The maximum y-axis bounds of rectangle A.
     * @param minXB The minimum x-axis bounds of rectangle B.
     * @param minYB The minimum y-axis bounds of rectangle B.
     * @param maxXB The maximum x-axis bounds of rectangle B.
     * @param maxYB The maximum y-axis bounds of rectangle B.
     * @return TRUE if rectangle B is fully contained by rectangle A.  Otherwise FALSE.
     */
    public static boolean contains(float minXA, float minYA, float maxXA, float maxYA
            , float minXB, float minYB, float maxXB, float maxYB)
    {
        return (minXB >= minXA && minYB >= minYA && maxXB <= maxXA && maxYB <= maxYA);
    }
    
    /**
     * Indicates whether or not two axis-aligned rectangles intersect. 
     * The test is inclusive of the edges of the rectangles.
     * @param minAX The minimum x-axis bounds of rectangle A.
     * @param minAY The minimum y-axis bounds of rectangle A.
     * @param maxAX The maximum x-axis bounds of rectangle A.
     * @param maxAY The maximum y-axis bounds of rectangle A.
     * @param minBX The minimum x-axis bounds of rectangle B.
     * @param minBY The minimum y-axis bounds of rectangle B.
     * @param maxBX The maximum x-axis bounds of rectangle B.
     * @param maxBY The maximum y-axis bounds of rectangle B.
     * @return TRUE if the two rectangles intersect in any manner.  Otherwise FALSE.
     */
    public static boolean intersectsAABB(float minAX, float minAY, float maxAX, float maxAY
            , float minBX, float minBY, float maxBX, float maxBY)
    {
        return !(maxBX < minAX || maxAX < minBX || maxBY < minAY || maxAY < minBY );
    }
    
}
