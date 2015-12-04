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
/// Loads and compiles all <see cref="OFMConnection"/> components in the scene, based on the
/// standard scene query behavior. (Editor Only)
/// </summary>
public sealed class OFMConnectionCompiler
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public int Priority { get { return NMBuild.MinPriority; } }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// Duplicates not allowed. (Always false.)
    /// </summary>
    public bool DuplicatesAllowed { get { return false; } }

    /// <summary>
    /// Processes the context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Processes during the <see cref="InputBuildState.LoadComponents"/> 
    /// and <see cref="InputBuildState.CompileInput"/> states.
    /// </para>
    /// </remarks>
    /// <param name="state">The current state of the input build.</param>
    /// <param name="context">The input context to process.</param>
    /// <returns>False if the input build should abort.</returns>
    public bool ProcessInput(InputBuildContext context, InputBuildState state)
    {
        if (context != null)
        {
            switch (state)
            {
                case InputBuildState.CompileInput:

                    Compile(context);
                    break;

                case InputBuildState.LoadComponents:

                    Load(context);
                    break;
            }
        }

        return true;
    }

    private void Load(InputBuildContext context)
    {
        context.info.loaderCount++;

        int count = context.LoadFromScene<OFMConnection>();

        context.Log(string.Format("Loaded {0} off-mesh connections.", count), this);
    }

    private void Compile(InputBuildContext context)
    {
        context.info.compilerCount++;

        ConnectionSetCompiler compiler = context.connCompiler;
        List<Component> items = context.components;
        List<byte> areas = context.areas;

        int count = 0;

        for (int i = 0; i < items.Count; i++)
        {
            Component item = items[i];

            if (item is OFMConnection)
            {
                OFMConnection conn = (OFMConnection)item;
                byte area = (conn.OverrideArea ? conn.Area : areas[i]);

                compiler.Add(conn.StartPoint, conn.EndPoint
                    , conn.Radius
                    , conn.IsBidirectional
                    , area
                    , conn.Flags
                    , (uint)conn.UserId);

                count++;
            }
        }

        context.Log(string.Format("Compiled off-mesh connections: {0}", count), this);
    }
}
