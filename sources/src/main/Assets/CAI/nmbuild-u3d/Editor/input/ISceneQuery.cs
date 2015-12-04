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
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// A ScriptableObject used to query the scene for components. (Editor Only)
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is only valid when implemented by a ScriptableObject.
    /// </para>
    /// </remarks>
    public interface ISceneQuery
    {
        /// <summary>
        /// Initializes the object before each use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called by the manager of the object before each use.  It allows the 
        /// object to refresh its internal state.
        /// </para>
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Gets all components of the specified type, based on the object's query restrictions.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>
        /// All components of the specified type, or a zero length array if there are none.
        /// </returns>
        T[] GetComponents<T>() where T : Component;
    }
}
