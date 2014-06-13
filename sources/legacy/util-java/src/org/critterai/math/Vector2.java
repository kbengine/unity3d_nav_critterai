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

import static org.critterai.math.MathUtil.EPSILON_STD;

/**
 * Represents a mutable 2-dimensional vector.
 * <p>Contains various static operations applicable to 2D vectors.</p>
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks, no divide by zero checks, etc.</p>
 * <p>All operations support the use of the same object reference in multiple arguments.
 * For example: Vector2.multiply(myVector, 5, myVector) will function 
 * the same as myVector.multiply(5)</p>
 * <p>Instances of this class are not thread safe. Static operations are thread safe.</p>
 */
public class Vector2
{

    /**
     * The x-value for the vector (x, y).
     */
    public float x;

    /**
     * The y-value for the vector (x, y).
     */
    public float y;

    /**
     * Constructor for the vector (0.0, 0.0). (Default)
     */
    public Vector2() 
    {  
        x = 0.0f;
        y = 0.0f; 
    }
    
    /**
     * Constructor.
     * @param x The x-value for the vector (x, y).
     * @param y The y-value for the vector (x, y).
     */
    public Vector2(float x, float y) 
    {  
        this.x = x;
        this.y = y; 
    }

    /**
     * Adds the provided vector to this vector.
     * <p>The values of this vector are mutated.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return A reference to this vector.
     */
    public Vector2 add(float x, float y) 
    {
        this.x += x;
        this.y += y; 
        return this;
    }
    
    /**
     * Adds the provided vector to this vector.
     * <p>The values of this vector are mutated.</p>
     * @param v The vector to add to this vector.
     * @return A reference to this vector.
     */
    public Vector2 add(Vector2 v) 
    {
        return add(v.x, v.y);
    }
    
    /**
     * Divides this vector by the provided value.
     * <p>The values of this vector are mutated.</p>
     * @param byValue The value to divide the elements of this vector by.
     * @return A reference to this vector.
     */
    public Vector2 divide(float byValue) 
    {
        this.x /= byValue;
        this.y /= byValue;
        return this;
    }
    
    /**
     * Returns the dot product of this vector and the provided vector.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return The dot product of this vector and the provided vector.
     * @see <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">Wikipedia- Dot Product</a>
     */
    public float dot(float x, float y) 
    { 
        return (this.x * x) + (this.y * y); 
    }
    
    /**
     * Returns the dot product of this vector and the provided vector.
     * @param v The vector.
     * @return The dot product of this vector and the provided vector.
     * @see <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">Wikipedia- Dot Product</a>
     */
    public float dot(Vector2 v) 
    { 
        return (this.x * v.x) + (this.y * v.y); 
    }
    
    /**
     * Determines whether or not this vector is equal to the provided vector.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return Returns TRUE if this vector is equal to the provided vector.  Otherwise FALSE.
     */
    public boolean equals(float x, float y) 
    {
        
        if (Float.floatToIntBits(this.x) != Float.floatToIntBits(x)
                || Float.floatToIntBits(this.y) != Float.floatToIntBits(y))
            return false; 
        return true;
    }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public boolean equals(Object obj) 
    {
        if (this == obj) 
            return true;
        if (obj == null) 
            return false;
        if (!(obj instanceof Vector2)) 
            return false;
        Vector2 other = (Vector2) obj;
        if (Float.floatToIntBits(x) != Float.floatToIntBits(other.getX()))
            return false;
        if (Float.floatToIntBits(y) != Float.floatToIntBits(other.getY()))
            return false;
        return true;
    }

    /**
     * Determines whether or not this vector is equal to the provided vector.
     * <p>This operation is slightly faster than the {@link #equals(Object)} form.</p>
     * @param v The vector to compare to. (A value of null will result in a runtime error.)
     * @return Returns TRUE if this vector is equal to the provided vector.  Otherwise FALSE.
     */
    public boolean equals(Vector2 v)
    {
        return equals(v.x, v.y);
    }
    
    /**
     * The x-value for this vector.
     * @return The x-value for the vector (x, y).
     */
    public float getX() { return x; }
    
    /**
     * The y-value for this vector.
     * @return The y-value for the vector (x, y).
     */
    public float getY() { return y; }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public int hashCode() 
    {
        final int prime = 31;
        int result = 1;
        result = prime * result + Float.floatToIntBits(x);
        result = prime * result + Float.floatToIntBits(y);
        return result;
    }

    /**
     * Returns TRUE if the length of the provided vector is zero.
     * @return TRUE if the length of the provided vector is zero. Otherwise FALSE.
     */
    public boolean isZeroLength() 
    {
        return (x == 0 && y == 0);
    }

    /**
     * Returns the square of the vector's length. (length * length)
     * @return The square of the vector's length.
     */
    public float lengthSq() 
    { 
        return  (x * x) + (y * y); 
    }

    /**
     * Multiplies (scales) this vector by the provided value.
     * <p>The values of this vector are mutated.</p>
     * @param byValue The value to multiply the elements of this vector by.
     * @return A reference to this vector.
     */
    public Vector2 multiply(float byValue) 
    {
        this.x *= byValue;
        this.y *= byValue;
        return this;
    }
    
    /**
     * Normalizes this vector such that its length is one.
     * <p>The values of this vector are mutated.</p>
     * <p>WARNING: This is a costly operation.</p>
     * @return A reference to this vector.
     */
    public Vector2 normalize()
    {    
        float length = (float)Math.sqrt((x * x) + (y * y));
        if (length <= EPSILON_STD) 
            length = 1;
        
        x /= length;
        y /= length;
        
        if (Math.abs(x) < EPSILON_STD) 
            x = 0;
        if (Math.abs(y) < EPSILON_STD) 
            y = 0;
        
        return this;
    }
    
    /**
     * Reverses the direction of the vector.
     * @return A reference to this vector.
     */
    public Vector2 reverse()
    {
        x = -x;
        y = -y;
        return this;
    }
    
    /**
     * Rotates the vector counter-clockwise by the specified angle.
     * <p>The values of this vector are mutated.</p>
     * <p>This is a non-trivial operation.</p>
     * @param angle Angle of counter-clockwise rotation. (In Radians.)
     * @return A reference to this vector.
     */
    public Vector2 rotate(float angle) 
    {
        float ca = (float)Math.cos(angle);
        float sa = (float)Math.sin(angle);
        float tx = (x * ca) - (y * sa);
        y = (x * sa) + (y * ca);
        x = tx;
        return this;
    }
    
    /**
     * Scales the vector to the provided length.
     * <p>The values of this vector are mutated.</p>
     * <p>WARNING: This is a costly operation.</p>
     * @param length The length to scale the vector to.
     * @return A reference to this vector.
     */
    public Vector2 scaleTo(float length) 
    {
        if (length == 0 || isZeroLength())
        { 
            x = 0;
            y = 0;
            return this;
        }
        return multiply(length / (float)(Math.sqrt((x * x) + (y * y))));
    }
    
    /**
     * Sets the values of this vector.
     * @param x The x-value for the vector (x, y).
     * @param y The y-value for the vector (x, y).
     * @return A reference to this vector.
     */
    public Vector2 set(float x, float y) 
    { 
        this.x = x; 
        this.y = y; 
        return this;
    }
    
    /**
     * Sets the values of this vector to match the provided vector.
     * @param v The vector to match this vector to.
     * @return A reference to this vector.
     */
    public Vector2 set(Vector2 v) 
    { 
        this.x = v.x; 
        this.y = v.y; 
        return this;
    }
    
    /**
     * Sets the x-value of this vector.
     * @param value The new the x-value for the vector (x, y).
     */
    public void setX(float value) { x = value; }
    
    /**
     * Sets the y-value for the vector (x, y).
     * @param value The new the y-value for the vector (x, y).
     */
    public void setY(float value) { y = value; }
    
    /**
     * Determines whether or not the elements of the provided vector are equal within
     * the specified tolerance of this vector.
     * <p>The vectors are considered equal if the following condition is met:
     * (vx >= x - tolerance && vx <= x + tolerance) 
     * && (vy >= y - tolerance && vy <= y + tolerance)</p>
     * @param vx The x-value for the vector (vx, vy).
     * @param vy The y-value for the vector (vx, vy).
     * @param tolerance The tolerance to use for the comparison.
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public boolean sloppyEquals(float vx, float vy, float tolerance)
    {
        tolerance = Math.max(0, tolerance);
        if (vx < x - tolerance || vx > x + tolerance) 
            return false;
        if (vy < y - tolerance || vy > y + tolerance) 
            return false;
        return true;
    }

    /**
     * Determines whether or not the elements of the provided vector are equal within
     * the specified tolerance of this vector.
     * <p>The vectors are considered equal if the following condition is met:
     * (v.x >= x - tolerance && v.x <= x + tolerance) 
     * && (v.y >= y - tolerance && v.y <= y + tolerance)</p>
     * @param v The vector to compare against.
     * @param tolerance The tolerance for the comparison.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public boolean sloppyEquals(Vector2 v, float tolerance)
    {
        return sloppyEquals(v.x, v.y, tolerance);
    }
    
    /**
     * Subtracts the provided vector from this vector. (this - providedVector)
     * <p>The values of this vector are mutated.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return A reference to this vector.
     */
    public Vector2 subtract(float x, float y)
    {
        this.x -= x;
        this.y -= y;
        return this;
    }
    
    /**
     * Subtracts the provided vector from this vector. (this - v)
     * <p>The values of this vector are mutated.</p>
     * @param v The vector to subtract from this vector.
     * @return A reference to this vector.
     */
    public Vector2 subtract(Vector2 v)
    {
        return subtract(v.x, v.y);
    }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public String toString() { return "(" + x + ", " + y + ")"; }
    
    /**
     * Truncates the length of this vector to the provided value.
     * <p>The values of this vector are mutated.</p>
     * <p>If the vector's length is longer than the provided value the length
     * of the vector is scaled back to the provided maximum length.</p>
     * <p>If the vector's length is shorter than the provided value, the vector
     * is not changed.</p>
     * <p>WARNING: This is a potentially costly operation.</p>
     * @param maxLength The maximum allowed length of the resulting vector.
     * @return A reference to this vector.
     */
    public Vector2 truncateLength(float maxLength) 
    {
        if (isZeroLength())
            return this;
        if (maxLength == 0)
        { 
            set(0, 0);
            return this;
        }
        float mlsq = maxLength * maxLength;
        float csq = (x * x) + (y * y);
        if (csq > mlsq) 
            multiply((float)(maxLength / Math.sqrt(csq)));
        return this;
    }
    
    /**
     * Adds the vectors (ux, uy) and (vx, vy).
     * @param ux The x-value of the vector (ux, uy).
     * @param uy The y-value of the vector (ux, uy).
     * @param vx The x-value of the vector (vx, vy).
     * @param vy The y-value of the vector (vx, vy).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 add(float ux, float uy, float vx, float vy, Vector2 out) 
    {
        out.set(ux + vx, uy + vy);
        return out;
    }

    /**
     * Adds the value to both elements of the vector.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param value The value to add to both of the vector elements.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 add(float x, float y, float value, Vector2 out) 
    {
        out.set(x + value, y + value);
        return out;
    }
    
    /**
     * Adds the value to both elements of the vector.
     * @param v The vector to add the value to.
     * @param value The value to add to both of the vector elements.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 add(Vector2 v, float value, Vector2 out) 
    {
        return add(v.x, v.y, value, out);
    }

    /**
     * Adds the two provided vectors.
     * @param u Vector to add.
     * @param v Vector to add.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 add(Vector2 u, Vector2 v, Vector2 out) 
    {
        return add(u.x, u.y, v.x, v.y, out);
    }

    /**
     * Divides both elements of the vector by the provided value.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param byValue The value to divide the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 divide(float x, float y, float byValue, Vector2 out) 
    {
        out.set(x / byValue, y / byValue);
        return out;
    }
    
    /**
     * Divides both elements of the vector by the provided value.
     * @param v The vector.
     * @param byValue The value to divide the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 divide(Vector2 v, float byValue, Vector2 out) 
    {
        return divide(v.x, v.y, byValue, out);
    }
    
    /**
     * Returns the dot products of the provided vectors.
     * <p>If you need to the dot product of two vector objects, use {@link #dot(Vector2)}.</p>
     * @param ux The x-value of the vector (ux, uy).
     * @param uy The y-value of the vector (ux, uy).
     * @param vx The x-value of the vector (vx, vy).
     * @param vy The y-value of the vector (vx, vy).
     * @return The dot product of the provided vectors.
     * @see <a href="http://en.wikipedia.org/wiki/Dot_product" target="_blank">Wikipedia- Dot Product</a>
     */
    public static float dot(float ux, float uy, float vx, float vy) 
    {
        return (ux * vx) + (uy * vy);
    }
    
    /**
     * Derives the normalized direction vector for the vector pointing from point A (ax, ay) to
     * point B (bx, by).
     * <p>WARNING: The out array size and validity of the outIndex are not checked.</p>
     * <p>WARNING: This is a costly operation.</p>
     * @param ax The x-value for the starting point A (ax, ay).
     * @param ay The y-value for the starting point A (ax, ay).
     * @param bx The x-value for the end point B (bx, by).
     * @param by The y-value for the end point B (bx, by).
     * @param out The array to load the result into in the form (x, y).
     * @param outIndex The vector index to load the result into. (Stride = 2.  So insertion location
     * will be outIndex*2.)
     * @return A reference to the out argument.
     */
    public static float[] getDirectionAB(float ax, float ay
            , float bx, float by
            , float[] out
            , int outIndex)
    {
        // Subtract.
        float x = bx - ax;
        float y = by - ay;
        
        // Normalize.
        float length = (float)Math.sqrt((x * x) + (y * y));
        if (length <= EPSILON_STD) 
            length = 1;
        
        x /= length;
        y /= length;
        
        if (Math.abs(x) < EPSILON_STD) 
            x = 0;
        if (Math.abs(y) < EPSILON_STD) 
            y = 0;    
        
        out[outIndex*2] = x;
        out[outIndex*2+1] = y;
        
        return out;
    }
    
    /**
     * Derives the normalized direction vector for the vector pointing from point A (ax, ay) to
     * point B (bx, by).
     * <p>WARNING: This is a costly operation.</p>
     * @param ax The x-value for the starting point A (ax, ay).
     * @param ay The y-value for the starting point A (ax, ay).
     * @param bx The x-value for the end point B (bx, by).
     * @param by The y-value for the end point B (bx, by).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 getDirectionAB(float ax, float ay
            , float bx, float by
            , Vector2 out)
    {
        // Subtract.
        out.x = bx - ax;
        out.y = by - ay;
        
        // Normalize.
        float length = (float)Math.sqrt((out.x * out.x) + (out.y * out.y));
        if (length <= EPSILON_STD) 
            length = 1;
        
        out.x /= length;
        out.y /= length;
        
        if (Math.abs(out.x) < EPSILON_STD) 
            out.x = 0;
        if (Math.abs(out.y) < EPSILON_STD) 
            out.y = 0;    
        
        return out;
    }

    /**
     * Returns the square of the distance between the two provided points. (distance * distance)
     * @param ax The x-value of the point (ax, ay).
     * @param ay The y-value of the point (ax, ay).
     * @param bx The x-value of the point (bx, by).
     * @param by The y-value of the point (bx, by).
     * @return The square of the distance between the two provided points.
     */
    public static float getDistanceSq(float ax, float ay, float bx, float by) 
    {
        float dx = ax - bx;
        float dy = ay - by;
        return (dx * dx + dy * dy);
    }
    
    /**
     * Returns the square of the distance between the two provided points. (distance * distance)
     * @param a Point A
     * @param b Point B
     * @return The square of the distance between the two provided points.
     */
    public static float getDistanceSq(Vector2 a, Vector2 b) 
    {
        return getDistanceSq(a.x, a.y, b.x, b.y);
    }

    /**
     * Returns the square of the length of the vector. (length * length)
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return The square of the length of the vector.
     */
    public static float getLengthSq(float x, float y) 
    {
        return (x * x + y * y);
    }

    /**
     * Multiplies both elements of the vector by the provided value.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param byValue The value to multiply the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 multiply(float x, float y, float byValue, Vector2 out) 
    {
        out.set(x * byValue, y * byValue);
        return out;
    }

    /**
     * Multiplies both elements of the vector by the provided value.
     * @param v The vector.
     * @param byValue The value to multiply the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 multiply(Vector2 v, float byValue, Vector2 out) 
    {
        return multiply(v.x, v.y, byValue, out);
    }

    /**
     * Normalizes the provided vector such that its length is equal to one.
     * <p>WARNING: This is a costly operation.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 normalize(float x, float y, Vector2 out) 
    {
        float length = (float)Math.sqrt(getLengthSq(x, y));
        if (length <= EPSILON_STD) 
            length = 1;
        
        x /= length;
        y /= length;
        
        if (Math.abs(x) < EPSILON_STD) 
            x = 0;
        if (Math.abs(y) < EPSILON_STD) 
            y = 0;
        
        out.set(x, y);
        
        return out;
    }

    /**
     * Normalizes the provided vector such that its length is equal to one.
     * <p>WARNING: This is a costly operation.</p>
     * @param v The vector to normalize.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 normalize(Vector2 v, Vector2 out) 
    {
        return normalize(v.x, v.y, out);
    }

    /**
     * Rotates the vector counter-clockwise by the specified angle.
     * <p>This is a non-trivial operation.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param angle Angle of counter-clockwise rotation. (In Radians.)
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 rotate(float x, float y, float angle, Vector2 out) 
    {
        float ca = (float)Math.cos(angle);
        float sa = (float)Math.sin(angle);
        out.set((x * ca) - (y * sa), (x * sa) + (y * ca));
        return out;
    }
    
    /**
     * Rotates the vector counter-clockwise by the specified angle.
     * <p>This is a non-trivial operation.</p>
     * @param v The vector to rotate.
     * @param angle Angle of counter-clockwise rotation. (In Radians.)
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 rotate(Vector2 v, float angle, Vector2 out) 
    {
        return rotate(v.x, v.y, angle, out);
    }

    /**
     * Scales the vector to the provided length.
     * <p>WARNING: This is a costly operation.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param length The length to scale the vector to.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 scaleTo(float x, float y, float length, Vector2 out) 
    {
        if (length == 0 || (x == 0 && y == 0))
        { 
            out.set(0, 0);
            return out;
        }
        return multiply(x, y
                , (length / (float)(Math.sqrt(getLengthSq(x, y)))), out);
    }

    /**
     * Scales the vector to the provided length.
     * <p>WARNING: This is a costly operation.</p>
     * @param v The vector to scale.
     * @param length The length to scale the vector to.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 scaleTo(Vector2 v, float length, Vector2 out) 
    {
        return scaleTo(v.x, v.y, length, out);
    }

    /**
     * Determines whether or not the elements of the provided vectors are equal within
     * the specified tolerance.
     * <p>The vectors are considered equal if the following condition is met:
     * (vx >= ux - tolerance && vx <= ux + tolerance) 
     * && (vy >= uy - tolerance && vy <= uy + tolerance)</p>
     * @param ux The x-value of the vector (ux, uy).
     * @param uy The y-value of the vector (ux, uy).
     * @param vx The x-value of the vector (vx, vy).
     * @param vy The y-value of the vector (vx, vy).
     * @param tolerance The tolerance for the test.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(float ux, float uy, float vx, float vy, float tolerance)
    {
        tolerance = Math.max(0, tolerance);
        if (vx < ux - tolerance || vx > ux + tolerance) 
            return false;
        if (vy < uy - tolerance || vy > uy + tolerance) 
            return false;
        return true;
    }

    /**
     * Determines whether or not the elements of the provided vectors are equal within
     * the specified tolerance.
     * <p>The vectors are considered equal if the following condition is met:
     * (v.x >= u.x - tolerance && v.x <= u.x + tolerance) 
     * && (v.y >= u.y - tolerance && v.y <= u.y + tolerance)</p>
     * @param u Vector v
     * @param v Vector u
     * @param tolerance The tolerance for the test.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(Vector2 u, Vector2 v, float tolerance)
    {
        return sloppyEquals(u.x, u.y, v.x, v.y, tolerance);
    }

    /**
     * Subtracts vector (vx, vy) from vector (ux, uy)
     * @param ux The x-value of the vector (ux, uy).
     * @param uy The y-value of the vector (ux, uy).
     * @param vx The x-value of the vector (vx, vy).
     * @param vy The y-value of the vector (vx, vy).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 subtract(float ux, float uy, float vx, float vy, Vector2 out) 
    {
        out.set(ux - vx, uy - vy);
        return out;
    }

    /**
     * Subtracts two vectors.  (u - v)
     * @param u Vector to be subtracted from.
     * @param v Vector to subtract.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 subtract(Vector2 u, Vector2 v, Vector2 out) 
    {
        return subtract(u.x, u.y, v.x, v.y, out);
    }

    /**
     * Truncates the length of the vector to the provided value.
     * <p>If the vector's length is longer than the provided value the length
     * of the vector is scaled back to the provided maximum length.</p>
     * <p>If the vector's length is shorter than the provided value, the vector
     * is not changed.</p>
     * <p>WARNING: This is a potentially costly operation.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param maxLength The maximum allowed length of the resulting vector.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 truncateLength(float x, float y, float maxLength, Vector2 out) 
    {
        if (maxLength == 0 || (x == 0 && y == 0))
        { 
            out.set(0, 0);
            return out;
        }
        float mlsq = maxLength * maxLength;
        float csq = getLengthSq(x, y);
        if (csq <= mlsq) 
        {
            out.set(x, y);
            return out;
        }
        return multiply(x, y, (float)(maxLength / Math.sqrt(csq)), out);
    }

    /**
     * Truncates the length of the vector to the provided value.
     * <p>If the vector's length is longer than the provided value the length
     * of the vector is scaled back to the provided maximum length.</p>
     * <p>If the vector's length is shorter than the provided value, the vector
     * is not changed.</p>
     * <p>WARNING: This is a potentially costly operation.</p>
     * @param v The vector to truncate.
     * @param maxLength The maximum allowed length of the resulting vector.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector2 truncateLength(Vector2 v, float maxLength, Vector2 out) 
    {
        return truncateLength(v.x, v.y, maxLength, out);
    }
    
}
