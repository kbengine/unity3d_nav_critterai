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
/// Loads and creates geometry from mesh filter components. (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// Loads mesh filters from the scene and compiles input geometry from attached meshes.
/// </para>
/// <para>
/// If colliders are set as preferred and a supported collider is colocated with the mesh filter,
/// the geometry will be created from the collider instead.  Supported colliders include:
/// SphereCollider, BoxCollider, and MeshCollider.
/// </para>
/// </remarks>
[System.Serializable]
public sealed class MeshCompiler
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// Colocation option. (Whether or not to perfer colliders.)
    /// </summary>
    public MeshColocationOption colocationOption;

    /// <summary>
    /// The priority of the processor.
    /// </summary>
    public int Priority { get { return NMBuild.MinPriority - 1; } }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// No duplicates allowed.  (Always false.)
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

        int count = context.LoadFromScene<MeshFilter>();

        context.Log(string.Format("{0}: Loaded {1} MeshFilters", name, count), this);
    }

    private void Compile(InputBuildContext context)
    {
        context.info.compilerCount++;

        ColliderHelper colliderHelper = (colocationOption == MeshColocationOption.Collider)
            ? new ColliderHelper()
            : null;

        InputGeometryCompiler compiler = context.geomCompiler;

        List<Component> master = new List<Component>(context.components);
        List<byte> areas = new List<byte>(context.areas);

        Queue<MeshFilter> filters = new Queue<MeshFilter>();

        int count = 0;
        int ignored = 0;
        while (master.Count > 0)
        {
            byte area = 0;

            for (int i = master.Count - 1; i >= 0; i--)
            {
                Component item = master[i];

                if (item is MeshFilter)
                {
                    MeshFilter filter = (MeshFilter)item;
                    if (filter.sharedMesh == null)
                    {
                        ignored++;
                        areas.RemoveAt(i);
                        master.RemoveAt(i);
                    }
                    else
                    {
                        if (filters.Count == 0)
                            area = areas[i];

                        if (areas[i] == area)
                        {
                            count++;
                            filters.Enqueue(filter);
                            areas.RemoveAt(i);
                            master.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    areas.RemoveAt(i);
                    master.RemoveAt(i);
                }
            }

            if (filters.Count > 0)
                CombineMeshes(filters, area, compiler, colliderHelper);
        }

        if (colliderHelper != null)
            colliderHelper.Dispose();

        if (ignored > 0)
        {
            string msg = string.Format("{0}: Ignored {1} MeshFilters with a null mesh."
                , name, ignored);

            context.Log(msg, this);
        }

        context.Log(string.Format("{0}: Compiled {1} MeshFilters.", name, count), this);
    }

    private void CombineMeshes(Queue<MeshFilter> filters
        , byte area
        , InputGeometryCompiler compiler
        , ColliderHelper helper)
    {
        Queue<CombineInstance> combineInstances = new Queue<CombineInstance>();

        while (filters.Count != 0)
        {
            MeshFilter filter = filters.Dequeue();

            if (helper != null)
            {
                Collider collider = filter.GetComponent<Collider>();

                if (collider)
                {
                    CombineInstance ci;

                    if (helper.Get(collider, out ci))
                    {
                        combineInstances.Enqueue(ci);
                        continue;
                    }
                }
            }

            // Note: Null shared meshes were filtered out by the calling method.

            for (int subIndex = 0; subIndex < filter.sharedMesh.subMeshCount; ++subIndex)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = filter.sharedMesh;
                combineInstance.transform = filter.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = subIndex;

                combineInstances.Enqueue(combineInstance);
            }
        }

        MeshUtil.CombineMeshes(combineInstances, area, compiler);
    }
}
