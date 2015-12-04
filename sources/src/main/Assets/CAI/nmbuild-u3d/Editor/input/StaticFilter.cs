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
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d.editor;
using UnityEngine;

/// <summary>
/// Filters out all components that are not marked as static. (Editor Only)
/// </summary>
public sealed class StaticFilter
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public int Priority { get { return NMBuild.MinPriority - 1; } }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public bool DuplicatesAllowed { get { return false; } }

    /// <summary>
    /// Processes the context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied during the <see cref="InputBuildState.FilterComponents"/> state.
    /// </para>
    /// </remarks>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public bool ProcessInput(InputBuildContext context, InputBuildState state)
    {
        if (state != InputBuildState.FilterComponents || context == null)
            return true;

        context.info.filterCount++;

        List<Component> items = context.components;

        int count = 0;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (!items[i].gameObject.isStatic)
            {
                items.RemoveAt(i);
                count++;
            }
        }

        context.Log(string.Format("{0}: Applied static filter. Removed {1} components."
            , name, count)
            , this);

        return true;
    }
}
