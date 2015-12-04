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
/// Filters out all components with the specified tags. (Editor Only)
/// </summary>
[System.Serializable]
public sealed class TagInputFilter
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// True if the filter should apply to components with a tagged parent.
    /// </summary>
    public bool recursive;

    /// <summary>
    /// The tags that should result in filtering.
    /// </summary>
    public List<string> tags = new List<string>();

    [SerializeField]
    private int mPriority = NMBuild.DefaultPriority;

    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public int Priority 
    { 
        get { return mPriority; }
        set { mPriority = NMBuild.ClampPriority(value); }
    }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// Multiple processors of this type are allowed. (Always true.)
    /// </summary>
    public bool DuplicatesAllowed { get { return true; } }

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
        if (state != InputBuildState.FilterComponents)
            return true;

        context.info.filterCount++;

        if (tags == null)
        {
            context.Log(name + " Mesh exclusion list is null. (Invalid processor state.)", this);
            return false;
        }

        if (tags.Count == 0)
        {
            context.Log(name + ": Filter is inactive. No tags configured to filter.", this);
            return true;
        }

        List<Component> targetItems = context.components;

        int removed = 0;
        for (int iTarget = targetItems.Count - 1; iTarget >= 0; iTarget--)
        {
            Component targetItem = targetItems[iTarget];

            if (!targetItem)
                continue;

            int iSource = tags.IndexOf(targetItem.tag);

            if (iSource != -1)
            {
                // One of the tags is on this item.
                targetItems.RemoveAt(iTarget);
                removed++;
                continue;
            }

            if (recursive)
            {
                // Need to see if the tag is on any parent.
                Transform parent = targetItem.transform.parent;

                while (parent != null)
                {
                    iSource = tags.IndexOf(parent.tag);

                    if (iSource != -1)
                    {
                        // One of the tags is on this item.
                        targetItems.RemoveAt(iTarget);
                        removed++;
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }

        if (removed > 0)
            context.Log(string.Format("{0}: Filtered out {1} components.", name, removed), this);
        else
            context.Log(name + ": No components filtered.", this);

        return true;
    }
}
