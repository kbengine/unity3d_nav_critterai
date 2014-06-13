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

import org.critterai.math.Vector3;

/**
 * Provides operations related to triangles in 3-dimensional space.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No checks are performed to ensure the arguments represent a valid triangle,
 * no null checks of arguments, etc.</p>
 * <p>Static operations are thread safe.</p>
 */
public final class Triangle3 
{

    /**
     * Returns the area of the triangle ABC.
     * <p>WARNING: This is an costly operation.  If the value is only needed for comparison with other
     * triangles, then use 
     * {@link #getAreaComp(float, float, float, float, float, float, float, float, float) getAreaComp()}
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param az The z-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param bz The z-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @param cz The z-value for vertex C in triangle ABC
     * @return The area of the triangle ABC.
     */
    public static float getArea(
              float ax, float ay, float az
            , float bx, float by, float bz
            , float cx, float cy, float cz)
    {
        return (float)(Math.sqrt(getAreaComp(ax, ay, az, bx, by, bz, cx, cy, cz)) / 2);
    }
    
    /**
     * Returns a value suitable for comparing the relative size of two triangles.
     * E.g. Is triangleA larger than triangleB.  
     * <p>The value returned by this operation can be converted to area as follows: 
     * Area = Math.sqrt(value)/2</p>
     * <p>Useful for quickly comparing the size of triangles.</p>
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param az The z-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param bz The z-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @param cz The z-value for vertex C in triangle ABC
     * @return A value suitable for comparing the relative size of two triangles.
     */
    public static float getAreaComp(
              float ax, float ay, float az
            , float bx, float by, float bz
            , float cx, float cy, float cz)
    {
        
        // References:
        // http://softsurfer.com/Archive/algorithm_0101/algorithm_0101.htm#Modern%20Triangles
        
        // Get directional vectors.
        // A -> B
        float ux = bx - ax;
        float uy = by - ay;
        float uz = bz - az;
        // A -> C
        float vx = cx - ax;
        float vy = cy - ay;
        float vz = cz - az;

        // Cross product.
        float nx = uy * vz - uz * vy;
        float ny = -ux * vz + uz * vx;
        float nz = ux * vy - uy * vx;
        
        return Vector3.getLengthSq(nx, ny, nz);
        
    }
    
    /**
     * Returns the normal for the  triangle.  (The vector perpendicular to
     * the triangle's plane.)  The direction of the normal is determined by 
     * the right-handed rule.
     * <p>WARNING: This is a costly operation.</p>
     * @param ax The x-value for vertex A in triangle ABC
     * @param ay The y-value for vertex A in triangle ABC
     * @param az The z-value for vertex A in triangle ABC
     * @param bx The x-value for vertex B in triangle ABC
     * @param by The y-value for vertex B in triangle ABC
     * @param bz The z-value for vertex B in triangle ABC
     * @param cx The x-value for vertex C in triangle ABC
     * @param cy The y-value for vertex C in triangle ABC
     * @param cz The z-value for vertex C in triangle ABC
     * @param out The vector to store the result in.
     * @return A reference to the out argument.
     */
    public static Vector3 getNormal(float ax, float ay, float az
            , float bx, float by, float bz
            , float cx, float cy, float cz
            , Vector3 out)
    {
         
        // Reference: http://en.wikipedia.org/wiki/Surface_normal#Calculating_a_surface_normal
         // N = (B - A) x (C - A) with the final result normalized.
         
         Vector3.cross(bx - ax
                 , by - ay
                 , bz - az
                 , cx - ax
                 , cy - ay
                 , cz - az
                 , out);
         out.normalize();
        
        return out;
        
    }
    
    /**
     * Returns the normal for the  triangle.  (The vector perpendicular to
     * the triangle's plane.)  The direction of the normal is determined by 
     * the right-handed rule.
     * <p>WARNING: This is a costly operation.</p>
     * @param vertices An array of vertices which contains a representation of triangles in the
     * form (ax, ay, az, bx, by, bz, cx, cy, cz).  The wrap direction is expected to be
     * clockwise.
     * @param startVertIndex The index of the first vertex in the triangle.
     * @param out The vector to store the result in.
     * @return A reference to the out argument.
     */
    public static Vector3 getNormal(float[] vertices, int startVertIndex, Vector3 out)
    {
         
        int pStartVert = startVertIndex*3;
        return getNormal(vertices[pStartVert], vertices[pStartVert+1], vertices[pStartVert+2]
                             , vertices[pStartVert+3], vertices[pStartVert+4], vertices[pStartVert+5]
                             , vertices[pStartVert+6], vertices[pStartVert+7], vertices[pStartVert+8]
                             , out);
    }
    
}
