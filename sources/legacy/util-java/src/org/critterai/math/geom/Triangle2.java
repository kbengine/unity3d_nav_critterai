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

import static org.critterai.math.Vector2.dot;

/**
 * Provides operations related to 2-dimensional triangles.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No checks are performed to ensure the arguments represent a valid triangle.</p>
 * <p>Static operations are thread safe.</p>
 */
public class Triangle2 
{
    
    /**
     * Returns TRUE if the point (px, py) is contained by the triangle.
     * <p>The test is inclusive.  So points on the vertices or edges
     * of the triangle are considered to be contained by the triangle.</p>
     * @param px The x-value for the point to test. (px, py)
     * @param py The y-value for the poitn to test. (px, py)
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @return TRUE if the point (x, y) is contained by the triangle ABC.
     */
    public static boolean contains(float px, float py
            , float ax, float ay
            , float bx, float by
            , float cx, float cy)
    {
        float dirABx = bx - ax;
        float dirABy = by - ay;
        float dirACx = cx - ax;
        float dirACy = cy - ay;
        float dirAPx = px - ax;
        float dirAPy = py - ay;        
        
        float dotABAB = dot(dirABx, dirABy, dirABx, dirABy);
        float dotACAB = dot(dirACx, dirACy, dirABx, dirABy);
        float dotACAC = dot(dirACx, dirACy, dirACx, dirACy);
        float dotACAP = dot(dirACx,dirACy, dirAPx, dirAPy);
        float dotABAP = dot(dirABx, dirABy, dirAPx, dirAPy);
        
        float invDenom = 1 / (dotACAC * dotABAB - dotACAB * dotACAB);
        float u = (dotABAB * dotACAP - dotACAB * dotABAP) * invDenom;
        float v = (dotACAC * dotABAP - dotACAB * dotACAP) * invDenom;
        
        // Altered this slightly from the reference so that points on the vertices and edges
        // are considered to be inside the triangle.
        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }
    
    /**
     * The absolute value of the returned value is two times the area of the
     * triangle ABC.
     * <p>A positive value indicates:</p>
     * <ul>
     * <li>Counterclockwise wrapping of the vertices.</li>
     * <li>Vertex B lies to the right of line AC, looking from A toward C.</li>
     * </ul>
     * <p>A negative value indicates:</p>
     * <ul>
     * <li>Clockwise wrapping of the vertices.</li>
     * <li>Vertex B lies to the left of line AC, looking from A toward C.</li>
     * </ul>
     * <p>A value of zero indicates that all points are collinear or represent the same point.</p>
     * <p>This is a low cost operation.</p>
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @return The absolute value of the returned value is two times the area of the
     * triangle ABC.
     */
    public static float getSignedAreaX2(float ax, float ay, float bx, float by, float cx, float cy)
    {
        // References:
        // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
        // http://mathworld.wolfram.com/TriangleArea.html (Search for "signed".)
        return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
    }
    
    /**
     * The absolute value of the returned value is two times the area of the
     * triangle ABC.
     * <p>A positive value indicates:</p>
     * <ul>
     * <li>Counterclockwise wrapping of the vertices.</li>
     * <li>Vertex B lies to the right of line AC, looking from A toward C.</li>
     * </ul>
     * <p>A negative value indicates:</p>
     * <ul>
     * <li>Clockwise wrapping of the vertices.</li>
     * <li>Vertex B lies to the left of line AC, looking from A toward C.<li>
     * <ul>
     * <p>A value of zero indicates that all points are collinear or represent the same point.</p>
     * <p>This is a low cost operation.</p>
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @return The absolute value of the returned value is two times the area of the
     * triangle ABC.
     */
    public static int getSignedAreaX2(int ax, int ay, int bx, int by, int cx, int cy)
    {
        // References:
        // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
        // http://mathworld.wolfram.com/TriangleArea.html (Search for "signed".)
        return (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
    }
    
}
