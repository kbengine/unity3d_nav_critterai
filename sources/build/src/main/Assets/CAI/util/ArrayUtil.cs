/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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

namespace org.critterai
{
    /// <summary>
    /// Provides array related utility methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Static methods are thread safe.
    /// </para>
    /// </remarks>
    public static class ArrayUtil
    {
        /// <summary>
        /// Compresses an array by removing all null values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only valid for use with arrays of reference types.
        /// </para>
        /// <para>
        /// No guarentees are made concerning the order of the items in the returned array.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="items">The array.</param>
        /// <returns>
        /// A reference to original array if it contained no nulls, or a new array with all nulls 
        /// removed.
        /// </returns>
        public static T[] Compress<T>(T[] items)
        {
            if (items == null)
                return null;

            if (items.Length == 0)
                return items;

            int count = 0;

            foreach (T item in items)
            {
                count += (item == null) ? 0 : 1;
            }

            if (count == items.Length)
                return items;

            T[] result = new T[count];

            if (count == 0)
                return result;

            count = 0;
            foreach (T item in items)
            {
                if (item != null)
                    result[count++] = item;
            }

            return result;
        }
    }
}
