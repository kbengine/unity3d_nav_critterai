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
/**
 * Provides various geometry related classes and operations.
 * <p>A high priority is given to performance.  In order to achieve the performance goal, the
 * following standards have been implemented:</p>
 * <ul>
 * <li>There is no validation of arguments outside of constructors.
 * For example, the 
 * {@link org.critterai.math.geom.Line2#getRelationship(float, float, float, float, float, float, float, float, org.critterai.math.Vector2)}
 * operation does not validate that the {@link org.critterai.math.Vector2} argument is non-null.  
 * If the argument is null a runtime error will occur.</li>
 * <li>All static operations that return an object require the object be passed in as an "out" argument.
 * The out object is updated with the result and its reference returned.  This reduces the construction
 * costs by allowing clients of the class to re-use the out objects for multiple calls.</p>
 * </li>
 * </ul>
 */
package org.critterai.math.geom;