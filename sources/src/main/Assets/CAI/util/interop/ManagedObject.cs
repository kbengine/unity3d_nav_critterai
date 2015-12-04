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
    /// Base class for objects that contain references to unmanaged resources.
    /// </summary>
    public abstract class ManagedObject
        : IManagedObject
    {
        private readonly AllocType mResourceType;

        /// <summary>
        /// The type of unmanaged resources held by the object.
        /// </summary>
        public AllocType ResourceType { get { return mResourceType; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceType">The type of unmanaged resource.</param>
        public ManagedObject(AllocType resourceType)
        {
            this.mResourceType = resourceType;
        }

        /// <summary>
        /// Request all unmanaged resources controlled by the object be immediately freed and 
        /// the object marked as disposed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Whether or not unmanaged resources are actually freed depends on whether the 
        /// resources are owned by the object.  In some cases the only action is to mark the 
        /// object as disposed.
        /// </para>
        /// </remarks>
        public abstract void RequestDisposal();

        /// <summary>
        /// True if the object has been disposed and should no longer be used.
        /// </summary>
        public abstract bool IsDisposed { get; }
    }
}
