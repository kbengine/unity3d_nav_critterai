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
 * Specifies the relationship between two lines.
 */
public enum LineRelType
{
    
    /**
     * Lines are parallel and overlap each other.  (Share all points.)
     */
    COLLINEAR,
    
    /**
     * Lines intersect, but their segments do not.
     */
    LINES_INTERSECT,
    
    /**
     * Line segments intersect each other.
     */
    SEGMENTS_INTERSECT,
    
    /**
     * Line segment B is crossed by line A.
     */
    ALINE_CROSSES_BSEG, 
    
    /**
     * Line segment A is crossed by line B.
     */
    BLINE_CROSSES_ASEG,
    
    /**
     * Lines are parallel and do NOT overlap each other.  (Share no points.)
     */
    PARALLEL,
    
    /**
     * Lines do not intersect, but are not parallel.
     * (Share no points. Only applicable to 3-dimensional lines.)
     */
    SKEW
}
