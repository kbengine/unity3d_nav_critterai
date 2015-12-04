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

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// Information related to an input build. (Editor Only)
    /// </summary>
    /// <remarks>
    /// <para>
    /// This information is used for GUI presentation and debug purposes.
    /// </para>
    /// </remarks>
    public struct InputBuildInfo
    {
        /// <summary>
        /// The number of components loaded by the loader processors.
        /// </summary>
        public int compCountPre;

        /// <summary>
        /// The number of components that survived the filter processors.
        /// </summary>
        public int compCountPost;

        /// <summary>
        /// The number of loader processors applied.
        /// </summary>
        public int loaderCount;

        /// <summary>
        /// The number of filter processors applied.
        /// </summary>
        public int filterCount;

        /// <summary>
        /// The number of area modifier processors applied.
        /// </summary>
        public int areaModifierCount;

        /// <summary>
        /// The number of compiler processors applied.
        /// </summary>
        public int compilerCount;

        /// <summary>
        /// The number of post-processors applied.
        /// </summary>
        public int postCount;
    }
}
