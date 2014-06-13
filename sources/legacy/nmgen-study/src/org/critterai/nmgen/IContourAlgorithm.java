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
 * Provides for the application of an algorithm to a contour.
 */
public interface IContourAlgorithm
{
    /**
     * Apply an algorithm to a contour.
     * <p>The implementation is permitted to require that the the result
     * vertices be seeded with existing data.  In this case the argument
     * becomes an in/out argument rather than just an out argument.</p>
     * @param sourceVerts The source vertices that represent the contour
     * in the form (x, y, z, regionID).
     * @param resultVerts The contour vertices produced by the operation
     * in the form (x, y, z, sourceIndex).
     * <p>Source index is the index (not pointer) of the related source
     * vertex in sourcVerts.   E.g. If the vertex in resultsList references
     * the vertex at position 12 of sourceVerts, then sourceIndex will
     * be 3 (12 / 4).</p>
     */
    void apply(ArrayList<Integer> sourceVerts, ArrayList<Integer> resultVerts);
}
