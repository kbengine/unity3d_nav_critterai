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
using org.critterai.nmgen;
using org.critterai.nmbuild.u3d.editor;
using UnityEngine;

/// <summary>
/// Assigns areas to components based on the component's tag. (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// Will not override the assignment of <see cref="NMGen.NullArea"/>.
/// </para>
/// </remarks>
[System.Serializable]
public sealed class TagAreaDef 
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// True if components whose parents have one of the tags should be assigned the area.
    /// </summary>
    public bool recursive;

    /// <summary>
    /// Tags to associate with areas.
    /// </summary>
    public List<string> tags;

    /// <summary>
    /// The area associated with each tag.
    /// </summary>
    public List<byte> areas;

    [SerializeField]
    private int mPriority = NMBuild.DefaultPriority;

    /// <summary>
    /// The priority of the processor.
    /// </summary>    
    public int Priority { get { return mPriority; } }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    /// <param name="value">The new priority.</param>
    public void SetPriority(int value)
    {
        mPriority = NMBuild.ClampPriority(value);
    }

    /// <summary>
    /// Duplicates allowed. (Always true.)
    /// </summary>
    public bool DuplicatesAllowed { get { return true; } }

    /// <summary>
    /// Processes the context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied during the <see cref="InputBuildState.ApplyAreaModifiers"/> state.
    /// </para>
    /// </remarks>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public bool ProcessInput(InputBuildContext context, InputBuildState state)
    {
        if (state != InputBuildState.ApplyAreaModifiers)
            return true;

        context.info.areaModifierCount++;

        if (tags == null || areas == null || tags.Count != areas.Count)
        {
            context.Log("Mesh/Area size error. (Invalid processor state.)", this);
            return false;
        }

        if (tags.Count == 0)
        {
            context.Log("No action taken. No tags defined.", this);
            return true;
        }

        List<Component> targetItems = context.components;
        List<byte> targetAreas = context.areas;

        int applied = 0;
        for (int iTarget = 0; iTarget < targetItems.Count; iTarget++)
        {
            Component targetItem = targetItems[iTarget];

            if (targetItem == null || targetAreas[iTarget] == NMGen.NullArea)
                // Don't override null area.
                continue;

            int iSource = tags.IndexOf(targetItem.tag);

            if (iSource != -1)
            {
                targetAreas[iTarget] = areas[iSource];
                applied++;
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
                        targetAreas[iTarget] = areas[iSource];
                        applied++;
                        break;
                    }
                    parent = parent.parent;
                }
            }
        }

        context.Log(string.Format("{1}: Applied area(s) to {0} components", applied, name), this);

        return true;
    }
}
