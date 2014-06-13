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
 * Represents a mutable 3-dimensional vector.
 * <p>Contains various static operations applicable to 3D vectors.</p>
 * <p>This class is optimized for speed.  To support this priority, no argument validation is
 * performed.  E.g. No null checks, no divide by zero checks, etc.</p>
 * <p>All operations support the use of the same object reference in multiple arguments.
 * For example: Vector3.normalize(vectorArray, 1, vectorArray, 1) will normalize the vector at
 * position 1 in the vectorArray.</p> 
 * <p>Instances of this class are not thread safe. Static operations are thread safe.</p>
 */
public class Vector3 
{

    /**
     * The x-value for the vector (x, y, z).
     */
    public float x;

    /**
     * The y-value for the vector (x, y, z).
     */
    public float y;

    
    /**
     * The z-value for the vector (x, y, z).
     */
    public float z;
    
    /**
     * Constructor for the vector (0, 0, 0). (Default)
     */
    public Vector3() 
    {  
        x = 0.0f; 
        y = 0.0f; 
        z = 0.0f; 
    }
    
    /**
     * Constructor.
     * @param x The x-value for the vector (x, y, z).
     * @param y The y-value for the vector (x, y, z).
     * @param z The z-value for the vector (x, y, z).
     */
    public Vector3(float x, float y, float z) 
    {  
        this.x = x; 
        this.y = y; 
        this.z = z; 
    }
    
    /**
     * Constructs a vector from an array entry.
     * @param vectorArray An array of vectors in the form (x, y, z).
     * @param index The index of the vector entry to use. The expected stride is three.  
     * So the extraction point will be index*3.
     * @throws IllegalArgumentException If the array size or index is invalid.
     */
    public Vector3(float[] vectorArray, int index)
        throws IllegalArgumentException
    {
        if (vectorArray == null || index*3+2 >= vectorArray.length)
            throw new IllegalArgumentException("Invalid array or index.");
        x = vectorArray[index*3];
        y = vectorArray[index*3+1];
        z = vectorArray[index*3+2];
    }
    
    /**
     * Adds the provided vector to this vector.
     * <p>The values of this vector are mutated.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @param z The z-value of the vector (x, y).
     * @return A reference to this vector.
     */
    public Vector3 add(float x, float y, float z) 
    {
        this.x += x;
        this.y += y; 
        this.z += z;
        return this;
    }
    
    /**
     * Adds the provided vector to this vector.
     * <p>The values of this vector are mutated.</p>
     * @param v The vector to add to this vector.
     * @return A reference to this vector.
     */
    public Vector3 add(Vector3 v) 
    {
        return add(v.x, v.y, v.z);
    }

    /**
     * Divides this vector by the provided value.
     * <p>The values of this vector are mutated.</p>
     * @param byValue The value to divide the elements of this vector by.
     * @return A reference to this vector.
     */
    public Vector3 divide(float byValue) 
    {
        this.x /= byValue;
        this.y /= byValue;
        this.z /= byValue;
        return this;
    }

    /**
     * Returns the dot product of this vector and the provided vector.
     * @param x The x-value for the vector (x, y, z).
     * @param y The y-value for the vector (x, y, z).
     * @param z The z-value for the vector (x, y, z).
     * @return The dot product of this vector and the provided vector.
     */
    public float dot(float x, float y, float z) 
    { 
        return (this.x * x) + (this.y * y) + (this.z * z); 
    }
    
    /**
     * Returns the dot product of this vector and the provided vector.
     * @param v The vector.
     * @return The dot product of this vector and the provided vector.
     */
    public float dot(Vector3 v) 
    { 
        return dot(v.x, v.y, v.z); 
    }
    
    /**
     * Determines whether or not this vector is equal to the provided vector.
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return Returns TRUE if this vector is equal to the provided vector.  Otherwise FALSE.
     */
    public boolean equals(float x, float y, float z) 
    {
        
        if (Float.floatToIntBits(this.x) != Float.floatToIntBits(x)
                || Float.floatToIntBits(this.y) != Float.floatToIntBits(y)
                || Float.floatToIntBits(this.z) != Float.floatToIntBits(z))
            return false; 
        return true;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public boolean equals(Object obj) 
    {
        if (this == obj) return true;
        if (obj == null) return false;
        if (!(obj instanceof Vector3)) return false;
        Vector3 other = (Vector3) obj;
        if (Float.floatToIntBits(x) != Float.floatToIntBits(other.getX()))
            return false;
        if (Float.floatToIntBits(y) != Float.floatToIntBits(other.getY()))
            return false;
        if (Float.floatToIntBits(z) != Float.floatToIntBits(other.getZ()))
            return false;
        return true;
    }

    /**
     * Determines whether or not this vector is equal to the provided vector.
     * <p>This operation is slightly faster than the {@link #equals(Object)} form.</p>
     * @param v The vector to compare to. (A value of null will result in a runtime error.)
     * @return Returns TRUE if this vector is equal to the provided vector.  Otherwise FALSE.
     */
    public boolean equals(Vector3 v)
    {
        return equals(v.x, v.y, v.z);
    }

    /**
     * Inserts the content of this vector into the specified array.
     * <p>Warning: No argument validations are performed.</p>
     * @param out The array to insert the vector into.
     * @param outIndex The insertion point.  The expected stride is 3.  So the insertion
     * point will be outIndex*3.
     * @return A reference to the out argument.
     */
    public float[] get(float[] out, int outIndex) 
    {
        out[outIndex*3] = x;
        out[outIndex*3+1] = y;
        out[outIndex*3+2] = z;
        return out;
    }

    /**
     * The x-value for this vector.
     * @return The x-value for this vector.
     */
    public float getX() { return x; }

    /**
     * The y-value for this vector.
     * @return The y-value for this vector.
     */
    public float getY() { return y; }

    /**
     * The z-value for this vector.
     * @return The z-value for this vector.
     */
    public float getZ() { return z; }

    /**
     * {@inheritDoc}
     */
    @Override
    public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + Float.floatToIntBits(x);
        result = prime * result + Float.floatToIntBits(y);
        result = prime * result + Float.floatToIntBits(z);
        return result;
    }
    
    /**
     * Returns TRUE if the length of this vector is zero.
     * @return TRUE if the length of this vector is zero. Otherwise FALSE.
     */
    public boolean isZeroLength() 
    {
        return (x == 0 && y == 0 && z == 0);
    }
    
    /**
     * Returns the square of this vector's length. (length * length)
     * @return The square of this vector's length.
     */
    public float lengthSq() 
    { 
        return (x * x) + (y * y) + (z * z);
    }
    
    /**
     * Multiplies (scales) this vector by the provided value.
     * <p>The values of this vector are mutated.</p>
     * @param byValue The value to multiply the elements of this vector by.
     * @return A reference to this vector.
     */
    public Vector3 multiply(float byValue) 
    {
        this.x *= byValue;
        this.y *= byValue;
        this.z *= byValue;
        return this;
    }
    
    /**
     * Normalizes this vector such that its length is one.
     * <p>The values of this vector are mutated.</p>
     * <p>WARNING: This is a costly operation</p>
     * @return A reference to this vector.
     */
    public Vector3 normalize()
    {    
        float length = (float)Math.sqrt((x * x) + (y * y) + (z * z));
        if (length <= EPSILON_STD) 
            length = 1;
        
        x /= length;
        y /= length;
        z /= length;
        
        if (Math.abs(x) < EPSILON_STD) 
            x = 0;
        if (Math.abs(y) < EPSILON_STD) 
            y = 0;
        if (Math.abs(z) < EPSILON_STD) 
            z = 0;
        
        return this;
    }

    /**
     * Scales this vector to the provided length.
     * <p>The values of this vector are mutated.</p>
     * <p>WARNING: This is a costly operation.</p>
     * @param length The length to scale the vector to.
     * @return A reference to this vector.
     */
    public Vector3 scaleTo(float length) 
    {
        if (length == 0 || isZeroLength())
        { 
            x = 0;
            y = 0;
            z = 0;
            return this;
        }
        return multiply(length / (float)(Math.sqrt((x * x) + (y * y) + (z * z))));
    }

    /**
     * Sets the values for this vector.
     * @param x The x-value for this vector.
     * @param y The y-value for this vector.
     * @param z The z-value for this vector.
     * @return A reference to this vector.
     */
    public Vector3 set(float x, float y, float z) 
    { 
        this.x = x; 
        this.y = y; 
        this.z = z; 
        return this;
    }
    
    /**
     * Sets the values for this vector to match the provided vector.
     * @param v The vector to match this vector to.
     * @return A reference to this vector.
     */
    public Vector3 set(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
        return this;
    }
    
    /**
     * Sets the x-value for this vector.
     * @param value The new x-value for this vector.
     */
    public void setX(float value) { x = value; }

    /**
     * Sets the y-value for this vector.
     * @param value The new y-value for this vector.
     */
    public void setY(float value) { y = value; }
    
    /**
     * Sets the z-value for this vector.
     * @param value The new z-value for this vector.
     */
    public void setZ(float value) { z = value; }
    
    /**
     * Determines whether or not the elements of the provided vector are equal within
     * the specified tolerance of the elements of this vector.
     * <p>The vectors are considered equal if the following condition is met:
     * (vx >= x - tolerance && vx <= x + tolerance) 
     * && (vy >= y - tolerance && vy <= y + tolerance)
     * && (vz >= z - tolerance && vz <= z + tolerance)</p>
     * @param vx The x-value for the vector (vx, vy, vz).
     * @param vy The y-value for the vector (vx, vy, vz).
     * @param vz The z-value for the vector (vx, vy, vz).
     * @param tolerance The tolerance for the comparison.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public boolean sloppyEquals(float vx, float vy, float vz, float tolerance)
    {
        tolerance = Math.max(0, tolerance);
        if (vx < x - tolerance || vx > x + tolerance) 
            return false;
        if (vy < y - tolerance || vy > y + tolerance) 
            return false;
        if (vz < z - tolerance || vz > z + tolerance) 
            return false;
        return true;
    }
    
    /**
     * Determines whether or not the elements of the provided vector are equal within
     * the specified tolerance of the elements of this vector.
     * <p>The vectors are considered equal if the following condition is met:
     * (vx >= x - tolerance && vx <= x + tolerance) 
     * && (vy >= y - tolerance && vy <= y + tolerance)
     * && (vz >= z - tolerance && vz <= z + tolerance)</p>
     * @param v The vector to compare against.
     * @param tolerance The tolerance for the comparison.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public boolean sloppyEquals(Vector3 v, float tolerance)
    {
        return sloppyEquals(v.x, v.y, v.z, tolerance);
    }

    /**
     * Subtracts the provided vector from this vector. (this - providedVector)
     * <p>The values of this vector are mutated.</p>
     * @param x The x-value of the vector (x, y).
     * @param y The y-value of the vector (x, y).
     * @return A reference to this vector.
     */
    public Vector3 subtract(float x, float y, float z)
    {
        this.x -= x;
        this.y -= y;
        this.z -= z;
        return this;
    }

    /**
     * Subtracts the provided vector from this vector. (this - v)
     * <p>The values of this vector are mutated.</p>
     * @param v The vector to subtract from this vector.
     * @return A reference to this vector.
     */
    public Vector3 subtract(Vector3 v)
    {
        return subtract(v.x, v.y, v.z);
    }
    
    /**
     * {@inheritDoc}
     */
    @Override
    public String toString() { return "(" + x + ", " + y + ", " + z + ")"; }
    
    /**
     * Truncates the length of the vector to the provided value.
     * <p>The values of this vector are mutated.</p>
     * <p>If the vector's length is longer than the provided value the length
     * of the vector is scaled back to the provided maximum length.</p>
     * <p>If the vector's length is shorter than the provided value, the vector
     * is not changed.</p>
     * <p>WARNING: This is a potentially costly operation.</p>
     * @param maxLength The maximum allowed length of the resulting vector.
     * @return A reference to this vector.
     */
    public Vector3 truncateLength(float maxLength) 
    {
        if (isZeroLength())
            return this;
        if (maxLength == 0)
        { 
            set(0, 0, 0);
            return this;
        }
        float mlsq = maxLength * maxLength;
        float csq = (x * x) + (y * y) + (z * z);
        if (csq > mlsq) 
            multiply((float)(maxLength / Math.sqrt(csq)));
        return this;
    }

    /**
     * Adds the vectors (ux, uy, uz) and (vx, vy, vz).
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 add(float ux, float uy, float uz
            , float vx, float vy, float vz
            , Vector3 out) 
    {
        out.set(ux + vx, uy + vy, uz + vz);
        return out;
    }
    
    /**
     * Adds the value to all elements of the vector.
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param value The value to add to each of the vector elements.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 add(float x, float y, float z, float value, Vector3 out) 
    {
        out.set(x + value, y + value, z + value);
        return out;
    }
    
    /**
     * Adds the value to all elements of the vector.
     * @param v The vector to add the value to.
     * @param value The value to add to each of the vector elements.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 add(Vector3 v, float value, Vector3 out) 
    {
        return add(v.x, v.y, v.z, value, out);
    }
    
    /**
     * Adds the two provided vectors.
     * @param u Vector u
     * @param v Vector v
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 add(Vector3 u, Vector3 v, Vector3 out) 
    {
        return add(u.x, u.y, u.z, v.x, v.y, v.z, out);
    }
    
    /**
     * Performs a vector "righthanded" cross product. (u x v)
     * The resulting vector will be perpendicular to the plane 
     * containing the two provided vectors.
     * <p>Special Case: The result will be zero if the two 
     * vectors are parallel</p>
     * <p>WARNING:  No argument validations are peformed.</p>
     * @param vx The x-value of the vector (ux, uy, uz).
     * @param vy The y-value of the vector (ux, uy, uz).
     * @param vz The z-value of the vector (ux, uy, uz).
     * @param ux The x-value of the vector (vx, vy, vz).
     * @param uy The y-value of the vector (vx, vy, vz).
     * @param uz The z-value of the vector (vx, vy, vz).
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outIndex The vector index to store the result.  The expected stride is three, so the
     * insertion point will be outIndex*3.
     * @return A reference to the out array.
     */
    public static float[] cross(
              float ux, float uy, float uz
            , float vx, float vy, float vz
            , float[] out
            , int outIndex)
    {
        out[outIndex*3] = uy * vz - uz * vy;
        out[outIndex*3+1] = -ux * vz + uz * vx;
        out[outIndex*3+2] = ux * vy - uy * vx;
        return out;
    }
    
    /**
     * Performs a vector "righthanded" cross product. (u x v)
     * The resulting vector will be perpendicular to the plane 
     * containing the two provided vectors.
     * <p>Special Case: The result will be zero if the two 
     * vectors are parallel</p>
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 cross(
              float ux, float uy, float uz
            , float vx, float vy, float vz
            , Vector3 out)
    {
        out.x = uy * vz - uz * vy;
        out.y = -ux * vz + uz * vx;
        out.z = ux * vy - uy * vx;
        return out;
    }
    
    /**
     * Performs a vector "righthanded" cross product. (vectorA x vectorB)
     * The resulting vector will be perpendicular to the plane 
     * containing the two provided vectors.
     * <p>Special Case: The result will be zero if the two 
     * vectors are parallel</p>
     * <p>WARNING:  No argument validations are performed.</p>
     * <p>All arrays are expected to have a stride of three.  So vectors in an array
     * are located at index*3.</p>
     * @param vectorsA An array of vectors in the form (x, y, z).
     * @param vectorAIndex The index of vectorA within the vectorsA array.
     * @param vectorsB An array of vectors in the form (x, y, z).
     * @param vectorBIndex The index of vectorB within the vectorsB array.
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outIndex The vector index in the out array to insert the result into.
     * @return A reference to the out array.
     */
    public static float[] cross(float[] vectorsA
            , int vectorAIndex
            , float[] vectorsB
            , int vectorBIndex
            , float[] out
            , int outIndex)
    {
        return cross(vectorsA[vectorAIndex*3]
                        , vectorsA[vectorAIndex*3+1]
                        , vectorsA[vectorAIndex*3+2]
                        , vectorsB[vectorBIndex*3]
                        , vectorsB[vectorBIndex*3+1]
                        , vectorsB[vectorBIndex*3+2]
                        , out
                        , outIndex);
    }
    
    /**
     * Performs a vector "righthanded" cross product. (u x v)
     * The resulting vector will be perpendicular to the plane 
     * containing the two provided vectors.
     * <p>Special Case: The result will be zero if the two 
     * vectors are parallel</p>
     * @param u Vector U
     * @param v Vector V
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 cross(Vector3 u, Vector3 v, Vector3 out)
    {
        return cross(u.x, u.y, u.z
                , v.x, v.y, v.z
                , out);
    }
    
    /**
     * Divides all elements of the vector by the provided value.
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param byValue The value to divide the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 divide(float x, float y, float z, float byValue, Vector3 out) 
    {
        out.set(x / byValue, y / byValue, z / byValue);
        return out;
    }
    
    /**
     * Divides all elements of the vector by the provided value.
     * @param v The vector.
     * @param byValue The value to divide the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 divide(Vector3 v, float byValue, Vector3 out) 
    {
        return divide(v.x, v.y, v.z, byValue, out);
    }

    /**
     * Returns the dot product of the provided vectors.
     * <p>If you need to dot two vector objects, use {@link #dot(Vector3)}.
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @return The dot product of the provided vectors.
     */
    public static float dot(float ux, float uy, float uz
            , float vx, float vy, float vz) 
    {
        return (ux * vx) + (uy * vy) + (uz * vz);
    }
    
    /**
     * Inserts the provided vectors into the out array.
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outStartIndex The vector index in the out array to start the insertion at.
     * The expected stride is three, so insertion will start at outStartIndex*3.
     * @param vectors The vectors to insert into the out array.  There can be no nulls
     * in this list.
     * @return A reference to the out array.
     */
    public static float[] flatten(float[] out, int outStartIndex, Vector3... vectors)
    {
        
        outStartIndex *= 3;  // Convert to pointer.
        for (Vector3 v : vectors)
        {
            out[outStartIndex] = v.x;
            out[outStartIndex+1] = v.y;
            out[outStartIndex+2] = v.z;
            outStartIndex += 3;
        }
        return out;
    }
    
    /**
     * Returns the square of the distance between the two provided points. (distance * distance)
     * @param ax The x-value of the point (ax, ay, az).
     * @param ay The y-value of the point (ax, ay, az).
     * @param az The z-value of the point (ax, ay, az). 
     * @param bx The x-value of the point (bx, by, bz).
     * @param by The y-value of the point (bx, by, bz).
     * @param bz The z-value of the point (bx, by, bz).
     * @return The square of the distance between the two provided points.
     */
    public static float getDistanceSq(float ax, float ay, float az
            , float bx, float by, float bz) 
    {
        float dx = ax - bx;
        float dy = ay - by;
        float dz = az - bz;
        return (dx * dx + dy * dy + dz * dz);
    }
    
    /**
     * Returns the square of the distance between the two provided points. (distance * distance)
     * @param a Point A
     * @param b Point B
     * @return The square of the distance between the two provided points.
     */
    public static float getDistanceSq(Vector3 a, Vector3 b) 
    {
        return getDistanceSq(a.x, a.y, a.z, b.x, b.y, b.z);
    }
    
    /**
     * Returns the square of the length of the vector. (length * length)
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @return The square of the length of the vector.
     */
    public static float getLengthSq(float x, float y, float z) 
    {
        return (x * x + y * y + z * z);
    }
    
    /**
     * Gets the square of the length of a vector entry in an array.
     * <p>WARNINg:  No validations are peformed on the out array or outIndex.</p>
     * @param vectors An array of vectors in the form (x, y, z)
     * @param vectorIndex The index of the vector to get the length of.  Stride expected is three, so
     * the vector is expected to be located at vectorIndex*3.
     * @return The square of the length of the vector entry in the array.
     */
    public static float getLengthSq(float[] vectors, int vectorIndex) 
    {
        return (vectors[vectorIndex*3] * vectors[vectorIndex*3]) 
             + (vectors[vectorIndex*3+1] * vectors[vectorIndex*3+1])
             + (vectors[vectorIndex*3+2] * vectors[vectorIndex*3+2]);
    }

    /**
     * Multiplies all elements of the vector by the provided value.
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param byValue The value to multiply the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 multiply(float x, float y, float z, float byValue, Vector3 out) 
    {
        out.set(x * byValue, y * byValue, z * byValue);
        return out;
    }

    /**
     * Multiplies all elements of the vector by the provided value.
     * @param v The vector.
     * @param byValue The value to multiply the vector by.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 multiply(Vector3 v, float byValue, Vector3 out) 
    {
        return multiply(v.x, v.y, v.z, byValue, out);
    }

    /**
     * Normalizes the vector and stores the result
     * at the specified location within the out array.
     * <p>WARNINg:  No argument validations are peformed.</p>
     * <p>WARNING: This is a costly operation</p>
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outIndex The vector index to store the result at.  The stride is three, so
     * the insertion point in out will be outIndex*3.
     * @return A reference to the out array.
     */
    public static float[] normalize(float x, float y, float z
            , float[] out
            , int outIndex)
    {
        float length = (float)Math.sqrt((x * x) + (y * y) + (z * z));
        if (length <= EPSILON_STD) 
            length = 1;
        
        int pOut = outIndex*3;
        
        out[pOut] = x / length;
        out[pOut+1] = y / length;
        out[pOut+2] = z / length;
        
        if (Math.abs(out[pOut]) < EPSILON_STD) 
            out[pOut] = 0;
        if (Math.abs(out[pOut+1]) < EPSILON_STD)
            out[pOut+1] = 0;
        if (Math.abs(out[pOut+2]) < EPSILON_STD) 
            out[pOut+2] = 0;
        
        return out;
        
    }

    /**
     * Normalizes the provided vector such that its length is equal to one.
     * <p>WARNING: This is a costly operation</p>
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 normalize(float x, float y, float z, Vector3 out) 
    {
        float length = (float)Math.sqrt(getLengthSq(x, y, z));
        if (length <= EPSILON_STD) 
            length = 1;
        
        x /= length;
        y /= length;
        z /= length;
        
        if (Math.abs(x) < EPSILON_STD) 
            x = 0;
        if (Math.abs(y) < EPSILON_STD) 
            y = 0;
        if (Math.abs(z) < EPSILON_STD) 
            z = 0;
        
        out.set(x, y, z);
        
        return out;
    }

    /**
     * Normalizes the vector from the vectors array and stores the result
     * at the specified location within the out array.
     * <p>WARNING:  No argument validations are peformed.</p>
     * <p>All arrays are expected to have a stride of three.  So vectors in the array
     * are located at index*3.
     * <p>WARNING: This is a costly operation</p>
     * @param vectors An array of vectors in the form (x, y, z).
     * @param vectorIndex The vector index of the vector to be normalized.
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outIndex The vector index to store the result at.
     * @return A reference to the out array.
     */
    public static float[] normalize(float[] vectors
            , int vectorIndex
            , float[] out
            , int outIndex)
    {
        
        float length = (float)Math.sqrt(getLengthSq(vectors, vectorIndex));
        if (length <= EPSILON_STD) 
            length = 1;
        
        int pOut = outIndex*3;
        
        out[pOut] = vectors[vectorIndex*3] / length;
        out[pOut+1] = vectors[vectorIndex*3+1] / length;
        out[pOut+2] = vectors[vectorIndex*3+2] / length;
        
        if (Math.abs(out[pOut]) < EPSILON_STD) 
            out[pOut] = 0;
        if (Math.abs(out[pOut+1]) < EPSILON_STD) 
            out[pOut+1] = 0;
        if (Math.abs(out[pOut+2]) < EPSILON_STD) 
            out[pOut+2] = 0;
        
        return out;
    }

    /**
     * Normalizes the provided vector such that its length is equal to one.
     * <p>WARNING: This is a costly operation</p>
     * @param v The vector to normalize.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 normalize(Vector3 v, Vector3 out) 
    {
        return normalize(v.x, v.y, v.z, out);
    }
    
    /**
     * Scales the vector to the provided length.
     * <p>WARNING: This is a costly operation.</p>
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param length The length to scale the vector to.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 scaleTo(float x, float y, float z, float length, Vector3 out) 
    {
        if (length == 0 || (x == 0 && y == 0 && z == 0))
        { 
            out.set(0, 0, 0);
            return out;
        }
        return multiply(x, y, z
                , (length / (float)(Math.sqrt(getLengthSq(x, y, z)))), out);
    }

    /**
     * Scales the vector to the provided length.
     * <p>WARNING: This is a costly operation.</p>
     * @param v The vector to scale.
     * @param length The length to scale the vector to.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 scaleTo(Vector3 v, float length, Vector3 out) 
    {
        return scaleTo(v.x, v.y, v.z, length, out);
    }

    /**
     * Determines whether or not the elements of the provided vectors are equal within
     * the specified tolerance of each other.
     * <p>The vectors are considered equal if the following condition is met:
     * (vx >= ux - tolerance && vx <= ux + tolerance) 
     * && (vy >= uy - tolerance && vy <= uy + tolerance)
     * && (vz >= uz - tolerance && vz <= uz + tolerance)</p>
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @param tolerance The tolerance for the test.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(float ux, float uy, float uz
            , float vx, float vy, float vz
            , float tolerance)
    {
        tolerance = Math.max(0, tolerance);
        if (vx < ux - tolerance || vx > ux + tolerance) 
            return false;
        if (vy < uy - tolerance || vy > uy + tolerance) 
            return false;
        if (vz < uz - tolerance || vz > uz + tolerance) 
            return false;
        return true;
    }

    /**
     * Determines whether or not the elements of the provided vectors are equal within
     * the specified tolerance of each other.
     * (v.x >= u.x - tolerance && v.x <= u.x + tolerance) 
     * && (v.y >= u.y - tolerance && v.y <= u.y + tolerance)
     * && (v.z >= u.z - tolerance && v.z <= u.z + tolerance)</p>
     * @param u Vector u
     * @param v Vector v
     * @param tolerance The tolerance for the test.  
     * @return TRUE if the the associated elements of each vector are within the specified tolerance
     * of each other.  Otherwise FALSE.
     */
    public static boolean sloppyEquals(Vector3 u, Vector3 v, float tolerance)
    {
        return sloppyEquals(u.x, u.y, u.z, v.x, v.y, v.z, tolerance);
    }

    /**
     * Subtracts vector (vx, vy, vz) from vector (ux, uy, uz) and stores
     * the result in the specified location within the out array. (u - v)
     * <p>WARNING:  No argument validations are peformed.</p>
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @param out The vector array to store the result in, in the form (x, y, z).
     * @param outIndex The vector index to store the result at.  The expected stride is three,
     * so the insertion point in out will be outIndex*3.
     * @return A reference to the out array.
     */
    public static float[] subtract(
              float ux, float uy, float uz
            , float vx, float vy, float vz
            , float[] out
            , int outIndex) 
    {
        out[outIndex*3] = ux - vx;
        out[outIndex*3+1] = uy - vy;
        out[outIndex*3+2] = uz - vz;
        return out;
    }

    /**
     * Subtracts vector (vx, vy, vz) from vector (ux, uy, uz). (u - v)
     * @param ux The x-value of the vector (ux, uy, uz).
     * @param uy The y-value of the vector (ux, uy, uz).
     * @param uz The z-value of the vector (ux, uy, uz).
     * @param vx The x-value of the vector (vx, vy, vz).
     * @param vy The y-value of the vector (vx, vy, vz).
     * @param vz The z-value of the vector (vx, vy, vz).
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 subtract(float ux, float uy, float uz
            , float vx, float vy, float vz
            , Vector3 out) 
    {
        out.set(ux - vx, uy - vy, uz - vz);
        return out;
    }
    
    /**
     * Subtracts vectorB from vectorA. (vectorA - vectorB)
     * <p>WARNING:  No argument validations are peformed.</p>
     * <p>All arrays are expected to have a stride of three.  So vectors in the array
     * are located at index*3.
     * @param vectorsA An array of vectors in the form (x, y, z).
     * @param vectorAIndex The index of vectorA within the vectorsA array.
     * @param vectorsB An array of vectors in the form (x, y, z).
     * @param vectorBIndex The index of vectorB within the vectorsB array.
     * @param out The vector array to store the result in.
     * @param outIndex The vector index in the out array to insert the result at.
     * @return A reference to the out array.
     */
    public static float[] subtract(float[] vectorsA
            , int vectorAIndex
            , float[] vectorsB
            , int vectorBIndex
            , float[] out
            , int outIndex) 
    {
        out[outIndex*3] = vectorsA[vectorAIndex*3] - vectorsB[vectorBIndex*3];
        out[outIndex*3+1] = vectorsA[vectorAIndex*3+1] - vectorsB[vectorBIndex*3+1];
        out[outIndex*3+2] = vectorsA[vectorAIndex*3+2] - vectorsB[vectorBIndex*3+2];
        return out;
    }

    /**
     * Subtracts the two vectors.  (u - v)
     * @param u Vector to be subtracted from.
     * @param v Vector to subtract.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 subtract(Vector3 u, Vector3 v, Vector3 out) 
    {
        return subtract(u.x, u.y, u.z, v.x, v.y, v.z, out);
    }

    /**
     * Traslates point A toward point B by the specified factor of the 
     * distance between them.
     * <p>Examples:</p>
     * <p>If the factor is 0.0, then the result will equal A.<br/>
     * If the factor is 0.5, then the result will be the midpoint between A and B.<br/>
     * If the factor is 1.0, then the result will equal B.<br/></p>
     * @param ax The x-value of the point (ax, ay, az).
     * @param ay The y-value of the point (ax, ay, az).
     * @param az The z-value of the point (ax, ay, az). 
     * @param bx The x-value of the point (bx, by, bz).
     * @param by The y-value of the point (bx, by, bz).
     * @param bz The z-value of the point (bx, by, bz).
     * @param factor The factor which governs the distance the point is translated
     * from A toward B.
     * @param out The vector to store the result in.
     * @return A reference to the out argument.
     */
    public static Vector3 translateToward(float ax, float ay, float az
            , float bx, float by, float bz
            , float factor
            , Vector3 out)
    {
        Vector3.subtract(bx, by, bz, ax, ay, az, out);
        out.multiply(factor);
        return out.add(ax, ay, az);
    }

    /**
     * Truncates the length of the vector to the provided value.
     * <p>If the vector's length is longer than the provided value the length
     * of the vector is scaled back to the provided maximum length.</p>
     * <p>If the vector's length is shorter than the provided value, the vector
     * is not changed.</p>
     * <p>WARNING: This is a potentially costly operation.</p>
     * @param x The x-value of the vector (x, y, z).
     * @param y The y-value of the vector (x, y, z).
     * @param z The z-value of the vector (x, y, z).
     * @param maxLength The maximum allowed length of the resulting vector.
     * @param out The vector to load the result into.
     * @return A reference to the out argument.
     */
    public static Vector3 truncateLength(float x, float y, float z
            , float maxLength
            , Vector3 out) 
    {
        if (maxLength == 0 || (x == 0 && y == 0 && z == 0))
        { 
            out.set(0, 0, 0);
            return out;
        }
        float mlsq = maxLength * maxLength;
        float csq = getLengthSq(x, y, z);
        if (csq <= mlsq)
        {
            out.set(x, y, z);
            return out;
        }
        return multiply(x, y, z, (float)(maxLength / Math.sqrt(csq)), out);
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
    public static Vector3 truncateLength(Vector3 v, float maxLength, Vector3 out) 
    {
        return truncateLength(v.x, v.y, v.z, maxLength, out);
    }
    
}
