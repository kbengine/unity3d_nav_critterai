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
using org.critterai.geom;
using org.critterai.nmbuild;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.nmgen;
using UnityEngine;

/// <summary>
/// Loads and compiles a terrain component. (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// The compiler is associated with a TerrainData asset.  If the scene contains a Terrain component
/// that references the TerrainData it will be loaded and compiled.  Only one Terrain will ever be
/// loaded. (Mutliple Terrain components referencing the same TerrainData is not supported.)
/// </para>
/// </remarks>
[System.Serializable]
public sealed class TerrainCompiler
    : ScriptableObject, IInputBuildProcessor
{
    /// <summary>
    /// The asset to use for the compile.
    /// </summary>
    public TerrainData terrainData;

    /// <summary>
    /// True if tree's should also be triangulated.
    /// </summary>
    public bool includeTrees;

    [SerializeField]
    private float mResolution = 0.1f;

    /// <summary>
    /// The resolution factor to use when triangulating the terrain surface. 
    /// [0.001f &lt;= value &lt;= 1]
    /// </summary>
    /// <remarks>
    /// <para>
    /// The TerrainData heightfield has a build-in resolution that is usually much higher than
    /// required for building the navigation mesh.  For example, a 1024x1024 heightmap for a
    /// 2000x2000 unit terrain can generate over 8 million triangles at 100% resolution.
    /// This property allows the resolution to be scaled down.  A good place to start for a normal
    /// terrain is 0.1.
    /// </para>
    /// </remarks>
    public float Resolution
    {
        get { return mResolution; }
        set { mResolution = Mathf.Max(0.001f, Mathf.Clamp01(value)); }
    }

    /// <summary>
    /// Duplicates allowed. (Always true.)
    /// </summary>
    public bool DuplicatesAllowed { get { return true; } }

    /// <summary>
    /// The name of the processor
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// The priority of the processor.
    /// </summary>    
    public int Priority { get { return NMBuild.MinPriority; } }

    internal Terrain GetTerrain()
    {
        return GetTerrain(null);
    }

    private Terrain GetTerrain(InputBuildContext context)
    {
        if (terrainData == null)
            return null;

        Terrain[] items;

        if (context == null)
            items = (Terrain[])FindObjectsOfType(typeof(Terrain));
        else
            items = context.GetFromScene<Terrain>();

        if (items.Length == 0)
            return null;

        Terrain selected = null;
        bool multiple = false;

        foreach (Terrain item in items)
        {
            if (item.terrainData == terrainData)
            {
                if (selected == null)
                    selected = item;
                else
                    multiple = true;
            }
        }

        if (multiple)
        {
            string msg = string.Format(
                "Multiple terrains in the scene use the same data. {0} was selected"
                , selected.name);

            if (context == null)
                Debug.LogWarning(msg, this);
            else
                context.Log(msg, this);
        }

        return selected;
    }

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
        if (context != null && terrainData)
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
        
        Terrain item = GetTerrain(context);

        if (!item)
        {
            context.Log(string.Format("No terrain found using {0} terrain data.", terrainData.name)
                , this);
        }
        else
        {
            context.Load(item);
            context.Log(string.Format("Loaded the {0} terrain object.", terrainData.name), this);
        }
    }

    private void Compile(InputBuildContext context)
    {
        context.info.compilerCount++;

        InputGeometryCompiler compiler = context.geomCompiler;
        List<Component> items = context.components;
        List<byte> areas = context.areas;

        for (int i = 0; i < items.Count; i++)
        {
            Component item = items[i];

            if (item is Terrain)
            {
                Terrain terrain = (Terrain)item;

                if (terrain.terrainData != terrainData)
                    continue;

                TriangleMesh mesh = TerrainUtil.TriangulateSurface(terrain, mResolution);
                byte[] lareas = NMGen.CreateAreaBuffer(mesh.triCount, areas[i]);

                if (compiler.AddTriangles(mesh, lareas))
                {
                    string msg = string.Format("Compiled the {0} terrain surface. Triangles: {1}"
                        , terrain.name, mesh.triCount);

                    context.Log(msg, this);
                }
                else
                {
                    string msg = 
                        string.Format("Compiler rejected mesh for the {0} terrain.", terrain.name);

                    context.LogError(msg, this);

                    return;
                }

                if (includeTrees)
                {
                    int before = compiler.TriCount;

                    TerrainUtil.TriangluateTrees(terrain, areas[i], compiler);

                    string msg = string.Format("Compiled the {0} terrain trees. Triangles: {1}"
                        , terrain.name, compiler.TriCount - before);

                    context.Log(msg, this);
                }

                break;
            }
        }
    }
}
