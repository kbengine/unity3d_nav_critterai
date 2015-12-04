/*
 * Copyright (c) 2012 Stephen A. Pratt
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
using System.Collections.Generic;

namespace org.critterai
{
    /// <summary>
    /// Compares two priority items.
    /// </summary>
    /// <typeparam name="T">The type of the priority item.</typeparam>
	public sealed class PriorityComparer<T> 
        : IComparer<T> where T : IPriorityItem
	{
        private bool mAscending;

        /// <summary>
        /// True if the comparison is for an ascending sort.
        /// </summary>
        public bool Ascending { get { return mAscending; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ascending">True for an ascending sort, otherwise false.</param>
        public PriorityComparer(bool ascending)
        {
            mAscending = ascending;
        }

        /// <summary>
        /// Compares two priority items and returns a value indicating whether one is less than,
        /// equal to, or greater than the other.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returns zero if the priorities are equal.
        /// </para>
        /// <para>
        /// If ascending, returns -1 if x is less than y, and 1 if x is greater than y.
        /// </para>
        /// <para>
        /// If descending, return -1 if x is greater than y, and 1 if x is less than y.
        /// </para>
        /// </remarks>
        /// <param name="x">A priority item.</param>
        /// <param name="y">A priority item.</param>
        /// <returns>The comparision result. (See remarks.)</returns>
        public int Compare(T x, T y)
        {
            int xp = x.Priority;
            int yp = y.Priority;

            if (mAscending)
                return (xp == yp ? 0 : xp < yp ? -1 : 1);
            else
                return (xp == yp ? 0 : xp > yp ? -1 : 1);
        }
    }
}
