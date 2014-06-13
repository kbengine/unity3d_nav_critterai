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
 * Adds vertices to a contour such that no null region edge segment exceeds
 * the allowed edge length.
 * <p>Only null region edges are operated on.</p>
 * <p>
 * <a href=
 * "http://www.critterai.org/projects/nmgen/images/main_maxedge_off.png"
 * target="_parent">
 * <img class="insert" height="464" src=
 * "http://www.critterai.org/projects/nmgen/images/main_maxedge_off.jpg"
 * width="620" />
 * </a></p>
 * <p>
 * <a href=
 * "http://www.critterai.org/projects/nmgen/images/main_maxedge_on.png"
 * target="_parent">
 * <img class="insert" height="464" src=
 * "http://www.critterai.org/projects/nmgen/images/main_maxedge_on.jpg"
 * width="620" />
 * </a></p>
 */
public final class NullRegionMaxEdge
    implements IContourAlgorithm
{
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
    
    /**
     * The maximum length allowed for line segments that are part of a null
     * region edge.
     */
    private final int mMaxEdgeLength;
    
    /**
     * Constructor
     * @param maxEdgeLength The maximum length of polygon edges that
     * represent the border of the navmesh.
     * <p>More vertices will be added to navmesh border edges if this value
     * is exceeded for a particular edge. In certain cases this will reduce
     * the number of thin, long triangles in the navmesh.</p>
     * <p>A value of zero will disable this feature.</p>
     * <p>Constraints:  >= 0</p>
     */
    public NullRegionMaxEdge(int maxEdgeLength)
    {
        mMaxEdgeLength = Math.max(maxEdgeLength, 0);
    }
    
    /**
     * {@inheritDoc}
     * <p>The resultVerts argument is expected to be seeded with at least
     * one edge.</p>
     */
    @Override
    public void apply(ArrayList<Integer> sourceVerts
                    , ArrayList<Integer> resultVerts)
    {
        
        // See the interface documentation for details on what the argument
        // lists contain.
        
        if (mMaxEdgeLength <= 0)
            return;
        
        final int sourceVertCount = sourceVerts.size() / 4;
        int resultVertCount = resultVerts.size() / 4;
        int iVertA = 0;
        
        /*
         * Insert verts into null-region edges that are two long.
         * 
         * The basic process is to look at each edge in the result list.
         * If it connects to the null region and exceeds the allowed length
         * then insert a vertex from the source vertices list that is
         * closest to the middle of the current edge, splitting it in half.
         * 
         * Note: The number of result vertices may increase, which is why a
         * while loop is being used.
         */
        while (iVertA < resultVertCount)
        {
            
            // Get vertices for the current edge.
            
            // Wrap if necessary.
            final int iVertB = (iVertA + 1) % resultVertCount;
            
            final int ax = resultVerts.get(iVertA * 4);
            final int az = resultVerts.get(iVertA * 4 + 2);
            final int iVertASource = resultVerts.get(iVertA * 4 + 3);
            
            final int bx = resultVerts.get(iVertB * 4);
            final int bz = resultVerts.get(iVertB * 4 + 2);
            final int iVertBSource = resultVerts.get(iVertB * 4 + 3);
            
            int iNewVert = -1;  // -1 indicates no need to add new vertex.
            // Wrap if necessary.
            final int iTestVert = (iVertASource + 1) % sourceVertCount;
            
            // Find maximum deviation from the edge.
            
            if (sourceVerts.get(iTestVert * 4 + 3) == NULL_REGION)
            {
                // This is a null-region edge.  Check its length against
                // the maximum allowed.
                final int dx = bx - ax;
                final int dz = bz - az;
                if (dx * dx + dz * dz > mMaxEdgeLength * mMaxEdgeLength)
                {
                    // The current edge is too long and needs to be split.
                    // Find original number of vertices between the vertA
                    // and vertB.
                    final int indexDistance = iVertBSource < iVertASource
                        ? (iVertBSource + (sourceVertCount - iVertASource))
                                : (iVertBSource - iVertASource);
                    // Choose the vertex that is half way between vertA
                    // and vertB. Not distance wise, but step wise.
                    // More wrapping.
                    iNewVert =
                        (iVertASource + indexDistance/2) % sourceVertCount;
                }
            }
            
            if (iNewVert != -1)
            {
                // A new vertex needs to be inserted.  Do it.
                resultVerts.add((iVertA + 1) * 4
                        , sourceVerts.get(iNewVert * 4));
                resultVerts.add((iVertA + 1) * 4 + 1
                        , sourceVerts.get(iNewVert * 4 + 1));
                resultVerts.add((iVertA + 1) * 4 + 2
                        , sourceVerts.get(iNewVert * 4 + 2));
                resultVerts.add((iVertA + 1) * 4 + 3
                        , iNewVert);
                // Update the vertex count since a new vertex was added.
                resultVertCount = resultVerts.size() / 4;
                // The vertex index is not being incremented because on the
                // next loop we want to perform the check from iVertA to
                // the newly inserted vertex.
            }
            else
                // This edge is finished.  Move to the next vertex.
                iVertA++;
        }
    }

}
