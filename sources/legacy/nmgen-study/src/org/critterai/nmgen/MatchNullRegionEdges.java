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
 * Applies an algorithm to contours which results in null-region edges
 * following the original detail source geometry edge more closely.
 * @see <a href="http://www.critterai.org/nmgen_contourgen#nulledgesimple"
 * target="_parent">Visualizations</a>
 */
public final class MatchNullRegionEdges
    implements IContourAlgorithm
{
    
    /*
     * Recast Reference: simplifyContour() in RecastContour.cpp
     */
    
    private static final int NULL_REGION = OpenHeightSpan.NULL_REGION;
    
    /**
     * The maximum distance the edge of the contour may deviate from the source
     * geometry.
     */
    private final float mThreshold;
    
    /**
     * Constructor.
     * @param threshold The maximum distance the edge of the contour may
     * deviate from the source geometry.
     * <p>Setting this lower will result in the navmesh edges following the
     * geometry contour more accurately at the expense of an increased
     * vertex count.</p>
     * <p>Setting the value to zero is not recommended since it can result
     * in a large increase in the number of vertices at a high processing
     * cost.</p>
     * <p>Constraints:  >= 0</p>
     */
    public MatchNullRegionEdges(float threshold)
    {
        this.mThreshold = Math.max(threshold, 0);
    }
    
    /**
     * {@inheritDoc}
     * <p>Adds vertices from the source list to the result list such that
     * if any null  region vertices are compared against the result list,
     * none of the vertices will be further from the null region edges than
     * the allowed threshold.</p>
     * <p>Only null-region edges are operated on.  All other edges are
     * ignored.</p>
     * <p>The result vertices is expected to be seeded with at least two
     * source vertices.</p>
     */
    @Override
    public void apply(ArrayList<Integer> sourceVerts
            , ArrayList<Integer> inoutResultVerts)
    {
        if (sourceVerts == null || inoutResultVerts == null)
            return;
        
        final int sourceVertCount = sourceVerts.size() / 4;
        int simplifiedVertCount = inoutResultVerts.size() / 4;  // Will change.
        int iResultVertA = 0;
        
        /*
         * Loop through all edges in this contour.
         * 
         * NOTE: The simplifiedVertCount in the loop condition
         * increases over iterations.  That is what keeps the loop going beyond
         * the initial vertex count.
         */
        while (iResultVertA < simplifiedVertCount)
        {
            int iResultVertB = (iResultVertA + 1) % simplifiedVertCount;
            
            // The line segment's beginning vertex.
            final int ax = inoutResultVerts.get(iResultVertA*4);
            final int az = inoutResultVerts.get(iResultVertA*4+2);
            final int iVertASource = inoutResultVerts.get(iResultVertA*4+3);
            
            // The line segment's ending vertex.
            final int bx = inoutResultVerts.get(iResultVertB*4);
            final int bz = inoutResultVerts.get(iResultVertB*4+2);
            final int iVertBSource = inoutResultVerts.get(iResultVertB*4+3);
        
            // The source index of the next vertex to test.  (The vertex just
            // after the current vertex in the source vertex list.)
            int iTestVert = (iVertASource + 1) % sourceVertCount;
            float maxDeviation = 0;
            
            // Default to no index.  No new vert to add.
            int iVertToInsert = -1;
            
            if (sourceVerts.get(iTestVert*4+3) == NULL_REGION)
            {
                /*
                 * This test vertex is part of a null region edge.
                 * Loop through the source vertices until the end vertex
                 * is found, searching for the vertex that is farthest from
                 * the line segment formed by the begin/end vertices.
                 * 
                 * Visualizations:
                 * http://www.critterai.org/nmgen_contourgen#nulledgesimple
                 */
                while (iTestVert != iVertBSource)
                {
                    final float deviation = Geometry.getPointSegmentDistanceSq(
                            sourceVerts.get(iTestVert * 4)
                            , sourceVerts.get(iTestVert * 4 + 2)
                            , ax
                            , az
                            , bx
                            , bz);
                    if (deviation > maxDeviation)
                    {
                        // A new maximum deviation was detected.
                        maxDeviation = deviation;
                        iVertToInsert = iTestVert;
                    }
                    // Move to the next vertex.
                    iTestVert = (iTestVert+1) % sourceVertCount;
                }
            }
            
            if (iVertToInsert != -1 && maxDeviation > (mThreshold * mThreshold))
            {
                // A vertex was found that is further than allowed from the
                // current edge. Add this vertex to the contour.
                inoutResultVerts.add((iResultVertA+1)*4
                        , sourceVerts.get(iVertToInsert*4));
                inoutResultVerts.add((iResultVertA+1)*4+1
                        , sourceVerts.get(iVertToInsert*4+1));
                inoutResultVerts.add((iResultVertA+1)*4+2
                        , sourceVerts.get(iVertToInsert*4+2));
                inoutResultVerts.add((iResultVertA+1)*4+3
                        , iVertToInsert);
                // Update the vertex count since a new vertex was added.
                simplifiedVertCount = inoutResultVerts.size() / 4;
                // Not incrementing the vertex since we need to test the edge
                // formed by vertA  and this this new vertex on the next
                // iteration of the loop.
            }
            else
                // This edge segment does not need to be altered.  Move to
                // the next vertex.
                iResultVertA++;
        }
    }
}
