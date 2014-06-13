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
package org.critterai.math;

/**
 * Provides various math related utility operations.
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks, no divide by zero checks, etc.</p>
 * <p>Static operations are thread safe.</p>
 */
public class MathUtil 
{

    /**
     * A standard epsilon value.  (Minimum positive value greater than zero.)
     */
    public static final float EPSILON_STD = 0.00001f;
    
    /**
     * A standard tolerance value.
     */
    public static final float TOLERANCE_STD = 0.0001f;
    
    private MathUtil() { }
    
    /**
     * Determines whether the values are within the specified tolerance
     * of each other. 
     * <p>The values are considered equal if the following condition is met:
     * (b >= a - tolerance && b <= a + tolerance)</p>
     * @param a The a-value to compare the b-value against.
     * @param b The b-value to compare against the a-value.
     * @param tolerence The tolerance to use for the comparison.
     * @return TRUE if the values are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(float a, float b, float tolerence)
    {
        return !(b < a - tolerence || b > a + tolerence);
    }
    
    /**
     * Clamps the value to a positive non-zero value.
     * @param value The value to clamp.
     * @return The value clamped to a minimum of {@link Float#MIN_VALUE}.
     */
    public static float clampToPositiveNonZero(float value)
    {
        return Math.max(Float.MIN_VALUE, value);
    }
    
    /**
     * Clamps the value to the specified range.  The clamp is inclusive
     * such that minimum <= result <= maximum.
     * @param value The value to clamp.
     * @param minimum The minimum allowed value.
     * @param maximum The maximum allowed value.
     * @return A value clamped to the specified range.
     */
    public static float clamp(float value, float minimum, float maximum)
    {
        return (value < minimum ? minimum : (value > maximum ? maximum : value));
    }
    
    /**
     * Clamps the value to the specified range.  The clamp is inclusive
     * such that minimum <= result <= maximum.
     * @param value The value to clamp.
     * @param minimum The minimum allowed value.
     * @param maximum The maximum allowed value.
     * @return A value clamped to the specified range.
     */
    public static int clamp(int value, int minimum, int maximum)
    {
        return (value < minimum ? minimum : (value > maximum ? maximum : value));
    }
    
    /**
     * Clamps the value to the specified range.  The clamp is inclusive
     * such that minimum <= result <= maximum.
     * @param value The value to clamp.
     * @param minimum The minimum allowed value.
     * @param maximum The maximum allowed value.
     * @return A value clamped to the specified range.
     */
    public static short clamp(short value, short minimum, short maximum)
    {
        return (value < minimum ? minimum : (value > maximum ? maximum : value));
    }
    
    /**
     * Returns the maximum value in the list of values.
     * @param values The values to search.
     * @return The maximum value in the list of values.
     */
    public static float max(float ...values)
    {
        float result = values[0];
        for (int i = 1; i < values.length; i++)
            result = Math.max(result, values[i]);
        return result;
    }
    
    /**
     * Returns the minimum value in the list of values.
     * @param values The values to search.
     * @return The minimum value in the list of values.
     */
    public static float min(float ...values)
    {
        float result = values[0];
        for (int i = 1; i < values.length; i++)
            result = Math.min(result, values[i]);
        return result;
    }
    
}
