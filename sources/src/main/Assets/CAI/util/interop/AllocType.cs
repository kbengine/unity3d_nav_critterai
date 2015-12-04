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

namespace org.critterai.interop
{
    /// <summary>
    /// Indicates how an object's unmanaged resources have been allocated and are managed.
    /// </summary>
    public enum AllocType : byte
    {
        /// <summary>
        /// Unmanaged resources were allocated locally and must be freed locally.
        /// </summary>
        Local = 0,

        /// <summary>
        /// Unmanaged resources were allocated by an external library and a call must be made 
        /// to the library to free them.
        /// </summary>
        External = 1,

        /// <summary>
        /// Unmanaged resources were allocated and are managed by an external library.  There is 
        /// no local responsiblity to directly free the resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Objects of this type are usually allocated and owned by another unmanaged object.  So 
        /// its resources are freed by its owner when its owner is freed.
        /// </para>
        /// </remarks>
        ExternallyManaged = 2
    }
}
