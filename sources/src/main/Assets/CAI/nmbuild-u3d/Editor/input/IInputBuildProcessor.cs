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
using org.critterai;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// A ScriptableObject processor used to build input for a 
    /// <see cref="org.critterai.nav.Navmesh"/> build. (Editor Only)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Processors are called during each step of the input build, in ascending priority.
    /// </para>
    /// <para>
    /// This interface is only valid when implemented by a ScriptableObject.
    /// </para>
    /// </remarks>
    public interface IInputBuildProcessor
        : IPriorityItem
    {
        /// <summary>
        /// The name of the processor.
        /// </summary>
        string Name { get; }    // Don't get rid of this.  It is convenient.

        /// <summary>
        /// Processes the context.
        /// </summary>
        /// <param name="state">The current state of the input build.</param>
        /// <param name="context">The input context to process.</param>
        /// <returns>False if the input build should abort.</returns>
        bool ProcessInput(InputBuildContext context, InputBuildState state);

        /// <summary>
        /// True if the multiple processors of the same type can be included in a build.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is false, then the input builder will discard duplicate processors of the
        /// same type.  Which duplicate is discarded is undefined.
        /// </para>
        /// <para>This restiction only effects type comparison.  The input builder never accepts duplicate
        /// objects.</para>
        /// </remarks>
        bool DuplicatesAllowed { get; }
    }
}
