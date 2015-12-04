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
using org.critterai.nav;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.nmgen;
using UnityEngine;

/// <summary>
/// Defines a mapping between areas and flags and applies the flags during the NMGen build process.
/// (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// Any polygon or off-mesh connection assigned to one of the defined areas will have the 
/// associated flags added.  E.g. The 'water' area gets the 'swim' flag.
/// </para>
/// <para>
/// The flags are applied to <see cref="PolyMesh"/> polygons during the NMGen build, and to 
/// off-mesh connections during input post-processing.
/// </para>
/// </remarks>
[System.Serializable]
public sealed class AreaFlagDef
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// Flags to associate with areas.
    /// </summary>
    public List<int> flags;

    /// <summary>
    /// The area associated with the flags.
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
    /// Applied during the <see cref="InputBuildState.CompileInput"/> and
    /// <see cref="InputBuildState.PostProcess"/> states.
    /// </para>
    /// </remarks>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public bool ProcessInput(InputBuildContext context, InputBuildState state)
    {
        if (state == InputBuildState.CompileInput)
            return ProcessCompile(context);

        if (state == InputBuildState.PostProcess)
            return ProcessPost(context);

        return true;
    }

    private bool ProcessPost(InputBuildContext context)
    {
        if (!ProcessValidation(context))
            return false;

        context.info.postCount++;

        if (areas.Count == 0)
        {
            context.Log("No area/flag maps.  No action taken.", this);
            return true;
        }

        ConnectionSetCompiler conns = context.connCompiler;

        bool applied = false;

        for (int i = 0; i < areas.Count; i++)
        {
            byte area = areas[i];
            ushort flag = (ushort)flags[i];

            int marked = 0;

            for (int iConn = 0; iConn < conns.Count; iConn++)
            {
                OffMeshConnection conn = conns[iConn];

                if (conn.area == area)
                {
                    conn.flags |= flag;
                    conns[iConn] = conn;
                    marked++;
                }
            }

            if (marked > 0)
            {
                string msg = string.Format(
                    "Added '0x{0:X}' flags to {1} connections with the area {2}."
                   , flag, marked, area);

                context.Log(msg, this);

                applied = true;
            }
        }

        if (!applied)
            context.Log("No flags applied.", this);

        return true;
    }

    private bool ProcessValidation(InputBuildContext context)
    {
        if (flags == null || areas == null || flags.Count != areas.Count)
        {
            context.Log("Area/Flag size error. (Invalid processor state.)", this);
            return false;
        }
        return true;
    }

    private bool ProcessCompile(InputBuildContext context)
    {
        if (!ProcessValidation(context))
            return false;

        context.info.compilerCount++;

        if (areas.Count == 0)
        {
            context.Log("No area/flag maps.  No action taken.", this);
            return true;
        }

        ushort[] sflags = new ushort[flags.Count];

        for (int i = 0; i < sflags.Length; i++)
        {
            // The editor should prevent overflows.
            sflags[i] = (ushort)flags[i];
        }

        AreaFlagMapper mapper = AreaFlagMapper.Create(name, Priority, areas.ToArray(), sflags);

        if (mapper == null)
        {
            context.LogError("Failed to create NMGen processor. Unexpected invalid data.", this);
            return false;
        }

        context.processors.Add(mapper);

        context.Log(string.Format("Added {0} NMGen processor.", typeof(AreaFlagMapper).Name), this);

        return true;
    }
}
