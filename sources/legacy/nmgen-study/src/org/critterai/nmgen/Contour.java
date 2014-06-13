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
package org.critterai.nmgen;

import java.util.ArrayList;

/**
 * Represents the detailed and simplified versions of a contour.
 * A contour is expected to always represent a simple polygon.
 * (Convex or concave.)
 * <p>
 * <a href=
 * "http://www.critterai.org/projects/nmgen/images/cont_11_simplified_full.png"
 * target="_parent">
 * <img class="insert" height="465" src=
 * "http://www.critterai.org/projects/nmgen/images/cont_11_simplified_full.jpg"
 * width="620" />
 * </a></p>
 * @see ContourSetBuilder
 */
public final class Contour
{
    /*
     * Recast Reference: rcContour in Recast.h
     */
    
    /**
     * The region associated with the contour.
     */
    public final int regionID;
    
    /**
     * The vertices which represent the raw (or detailed) contour.
     * <p>Vertices are clockwise wrapped in the form (x, y, z, regionID),
     * where regionID is the external region the vertex is considered to be
     * connected to.</p>
     */
    public final int[] rawVerts;
    
    /**
     * The raw vertex count.  (A convenience value.)
     */
    public final int rawVertCount;
    
    /**
     * The vertices which represent the simplified contour.
     * <p>Vertices are clockwise wrapped in the form (x, y, z, regionID),
     * where regionID is the external region the vertex is considered to be
     * connected to.</p>
     */
    public final int[] verts;
    
    /**
     * The detail vertex count.  (A convenience value.)
     */
    public final int vertCount;
    
    /**
     * Constructor
     * <p>All vertex lists are expected to be clockwise wrapped in
     * the form (x, y, z, regionID), where regionID is the external
     * region the vertex is considered to be connected to.</p>
     * @param regionID The region associated with the contour.
     * @param rawList The vertices which represent the raw (or detailed)
     * contour.
     * @param vertList The vertices which represent the detailed contour.
     * @throws IllegalArgumentException  If either vertex list is null.
     * The size of the vertex lists is not checked.
     */
    public Contour(int regionID
                    , ArrayList<Integer> rawList
                    , ArrayList<Integer> vertList)
        throws IllegalArgumentException
    {
        if (rawList == null || vertList == null)
            throw new IllegalArgumentException(
                            "One or both vertex lists are null.");
        
        this.regionID = regionID;
        
        rawVerts = new int[rawList.size()];
        for (int i = 0; i < rawVerts.length; i++)
            rawVerts[i] = rawList.get(i);
        
        rawVertCount = rawVerts.length / 4;
        
        verts = new int[vertList.size()];
        for (int i = 0; i < verts.length; i++)
            verts[i] = vertList.get(i);
        
        vertCount = verts.length / 4;
    }
    
}
