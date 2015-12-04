/*
 * Copyright (c) 2011 Stephen A. Pratt
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

namespace org.critterai.nav
{
    /// <summary>
    /// Navigation status flags.
    /// </summary>
    [System.Flags]
    public enum NavStatus : uint
    {
        /// <summary>
        /// The operation has failed.  (Completion status.)
        /// </summary>
        Failure = 1u << 31,

        /// <summary>
        /// The operation has succeeded. (Completion status.)
        /// </summary>
        Sucess = 1u << 30,

        /// <summary>
        /// The operation is in progress. (Incomplete)
        /// </summary>
        InProgress = 1u << 29,

        /// <summary>
        /// Input data was not recognized.
        /// </summary>
        WrongMagic = 1 << 0,

        /// <summary>
        /// Input data was wrong version.
        /// </summary>
        WrongVersion = 1 << 1,

        /// <summary>
        /// Operation ran out of memory.
        /// </summary>
        OutOfMemory = 1 << 2,

        /// <summary>
        /// An input parameter was invalid.
        /// </summary>
        InvalidParam = 1 << 3,

        /// <summary>
        /// Result buffer for the operation was too small to store the entire result.
        /// </summary>
        BufferTooSmall = 1 << 4,

        /// <summary>
        /// The navigation query ran out of nodes during the search.
        /// </summary>
        OutOfNodes = 1 << 5,

        /// <summary>
        /// The navigation query did not reach the end location.  The result
        /// is a best guess.
        /// </summary>
        PartialResult = 1 << 6
    }
}
