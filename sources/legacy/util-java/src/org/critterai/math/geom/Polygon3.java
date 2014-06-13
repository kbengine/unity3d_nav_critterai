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
import org.critterai.math.Vector3;


/**
 * Provides operations related to polygons defined in 3-dimensional space with 
 * coplanar vertices.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks, divide by zero checks only when needed by the algorithm, etc.</p>
 * <p>Static operations are thread safe.</p>
 */
public class Polygon3 
{

    private Polygon3() { }
    
    /**
     * Determines whether a polygon is convex.
     * <p>Behavior is undefined if vertices are not coplanar.</p>
     * <p>If the area of the triangle formed by the first three vertices of the polygon is too small
     * to detect on both the (x, z) and (x, y) planes, then this operation may improperly return
     * FALSE.</p>
     * @param verts An array of vertices which contains a representation of polygons with an 
     * arbitrary number of sides in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
     * Wrap direction does not matter.
     * @param startVertIndex The index of the first vertex in the polygon.
     * @param vertCount The number of vertices in the polygon.
     * @return  TRUE if the polygon is convex.  Otherwise FALSE.
     */
    public static boolean isConvex(float[] verts, int startVertIndex, int vertCount)
    {  
        
        if (vertCount < 3)
            return false;
        if (vertCount == 3)
            // It is a triangle, so it must be convex.
            return true;  
        
        // At this point we know that the polygon has at least 4 sides.
        
        /*
         *  Process will be to step through the sides, 3 vertices at a time.
         *  As long the signed area for the triangles formed by each set of
         *  vertices is the same (negative or positive), then the polygon is convex.
         *  
         *  Using a shortcut by projecting onto the (x, z) or (x, y) plane for all calculations.
         *  For a proper polygon, if the 2D projection is convex, the 3D polygon is convex.
         *  
         *  There is one special case: A polygon that is vertical.  I.e. 2D on the (x, z) plane.
         *  This is detected during the first test.
         */

        int offset = 2;  // Start by projecting to the (x, z) plane.
        
        int pStartVert = startVertIndex*3;
        
        float initDirection = Triangle2.getSignedAreaX2(verts[pStartVert]
                                  , verts[pStartVert+2]
                                  , verts[pStartVert+3]
                                  , verts[pStartVert+5]
                                  , verts[pStartVert+6]
                                  , verts[pStartVert+8]);
        
        if (initDirection > -2 * MathUtil.EPSILON_STD 
                && initDirection < 2 * MathUtil.EPSILON_STD)
        {
            // The polygon is on or very close to the vertical plane.  Switch to projecting on the (x, y) plane.
            offset = 1;
            initDirection = Triangle2.getSignedAreaX2(verts[pStartVert]
                                , verts[pStartVert+1]
                                , verts[pStartVert+3]
                                , verts[pStartVert+4]
                                , verts[pStartVert+6]
                                , verts[pStartVert+7]);
            // Dev note: This is meant to be a strict zero test.
            if (initDirection == 0)
                // Some sort of problem.  Should very rarely ever get here.
                return false;  
        }
        
        int vertLength = (startVertIndex+vertCount)*3;
        for (int vertAPointer = pStartVert+3; vertAPointer < vertLength; vertAPointer += 3)
        {
            int vertBPointer = vertAPointer+3;
            if (vertBPointer >= vertLength) 
                // Wrap it back to the start.
                vertBPointer = pStartVert;  
            int vertCPointer = vertBPointer+3;
            if (vertCPointer >= vertLength)
                // Wrap it back to the start.
                vertCPointer = pStartVert;
            float direction = Triangle2.getSignedAreaX2(
                      verts[vertAPointer]
                    , verts[vertAPointer+offset]
                    , verts[vertBPointer]
                    , verts[vertBPointer+offset]
                    , verts[vertCPointer]
                    , verts[vertCPointer+offset]);
            if (!(initDirection < 0 && direction < 0) && !(initDirection > 0 && direction > 0))
                // The sign of the current direction is not the same as the sign of the
                // initial direction.  Can't be convex.
                return false;
        }
        
        return true;
        
    }
    
    /**
     * Returns the centroid of a convex polygon.
     * <p>Behavior is undefined if the polygon is not convex.</p>
     * <p>Behavior is undefined if the vector being overwritten in the out array
     * is a vertex in the polygon.  (Can only happen if the verts and out arrays
     * are the same object.)</p>
     * @param verts  An array of vertices which contains a representation of a polygon with an 
     * arbitrary number of sides in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
     * Wrap direction does not matter.
     * @param startVertIndex The index of the first vertex in the polygon.
     * @param vertCount The number of vertices in the polygon.
     * @param out The array to store the result in.
     * @param outVectorIndex The vector index in the out array to store the result in.  (The stride
     * is expected to be three.  So the insertion point will be outVectorIndex*3.)
     * @return A reference to the out argument.
     * @see <a href="http://en.wikipedia.org/wiki/Centroid">Centroid</a>
     */
    public static float[] getCentroid(float[] verts
            , int startVertIndex
            , int vertCount
            , float[] out
            , int outVectorIndex)
    {
        // Reference: http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
        
        int vertLength = (startVertIndex+vertCount)*3;
        int pOut = outVectorIndex*3;
        out[pOut] = 0;
        out[pOut+1] = 0;
        out[pOut+2] = 0;
        for (int pVert = startVertIndex*3; pVert < vertLength; pVert += 3)
        {
            out[pOut] += verts[pVert];
            out[pOut+1] += verts[pVert+1];
            out[pOut+2] += verts[pVert+2];
        }

        out[pOut] /= vertCount;
        out[pOut+1] /= vertCount;
        out[pOut+2] /= vertCount;
        
        return out;
    }
    
    /**
     * Returns the centroid of a polygon.
     * @param verts  An array of vertices which contains a representation of a polygon with an 
     * arbitrary number of sides in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn).  
     * Wrap direction does not matter.
     * <p>Behavior is undefined if the polygon is not convex.</p>
     * @param startVertIndex The index of the first vertex in the polygon.
     * @param vertCount The number of vertices in the polygon.
     * @param out The vector to store the result in.
     * @return A reference to the out argument.
     * @see <a href="http://en.wikipedia.org/wiki/Centroid">Centroid</a>
     */
    public static Vector3 getCentroid(float[] verts
            , int startVertIndex
            , int vertCount
            , Vector3 out)
    {
        // Reference: http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
        
        out.set(0, 0, 0);
        int vertLength = (startVertIndex+vertCount)*3;
        for (int pVert = startVertIndex*3; pVert < vertLength; pVert += 3)
        {
            out.x += verts[pVert];
            out.y += verts[pVert+1];
            out.z += verts[pVert+2];
        }

        out.x /= vertCount;
        out.y /= vertCount;
        out.z /= vertCount;
        
        return out;
        
    }
    
    /**
     * Returns the centroid of a polygon.
     * @param out The vector to store the result in.
     * @param verts An list of vertices which represent a polygon with an 
     * arbitrary number of sides in the form (x1, y1, z1, x2, y2, z2, ..., xn, yn, zn). 
     * @return A reference to the out argument.
     * @see <a href="http://en.wikipedia.org/wiki/Centroid">Centroid</a>
     */
    public static Vector3 getCentroid(Vector3 out, float...verts)
    {
        // Reference: http://en.wikipedia.org/wiki/Centroid#Of_a_finite_set_of_points
        
        out.set(0, 0, 0);
        
        int vertCount = 0;
        for (int pVert =0; pVert < verts.length; pVert += 3)
        {
            out.x += verts[pVert];
            out.y += verts[pVert+1];
            out.z += verts[pVert+2];
            vertCount++;
        }

        out.x /= vertCount;
        out.y /= vertCount;
        out.z /= vertCount;
        
        return out;
    }
    
}
