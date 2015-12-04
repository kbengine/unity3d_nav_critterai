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
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// The context for an input build. (Editor Only)
    /// </summary>
    public class InputBuildContext
        : UnityBuildContext
    {
        private readonly ISceneQuery mSceneQuery;

        private readonly InputBuildOption mOptions;

        /// <summary>
        /// The components included in the input build.
        /// </summary>
        public readonly List<Component> components = new List<Component>();

        /// <summary>
        /// The area associated with each component in <see cref="components"/>.
        /// </summary>
        public readonly List<byte> areas = new List<byte>();

        /// <summary>
        /// The processors to run during the NMGen build.
        /// </summary>
        public readonly List<INMGenProcessor> processors = new List<INMGenProcessor>();

        /// <summary>
        /// The connection compiler used to add connections.
        /// </summary>
        public readonly ConnectionSetCompiler connCompiler = new ConnectionSetCompiler(50);

        /// <summary>
        /// The geometry compiler used to add geometry.
        /// </summary>
        public readonly InputGeometryCompiler geomCompiler = new InputGeometryCompiler(131072, 262144);

        /// <summary>
        /// Input build information.
        /// </summary>
        public InputBuildInfo info = new InputBuildInfo();

        /// <summary>
        /// Input build options.
        /// </summary>
        public InputBuildOption Options { get { return mOptions; } } 

        /// <summary>
        /// Construction.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The scene query will be initialized during construction.
        /// </para>
        /// <para>
        /// If no scene query is provided, the query scope will be all game objects in the scene.
        /// </para>
        /// </remarks>
        /// <param name="sceneQuery">The scene query. (Optional)</param>
        /// <param name="options">Build options.</param>
        public InputBuildContext(ISceneQuery sceneQuery, InputBuildOption options)
        {
            mOptions = options;
            mSceneQuery = sceneQuery;
            if (sceneQuery != null)
                mSceneQuery.Initialize();
        }

        /// <summary>
        /// Gets components of the specified type using the scene query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All queries are against the currently open scene.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The components, or a zero length array if none are found.</returns>
        public T[] GetFromScene<T>() where T : Component
        {
            if (mSceneQuery == null)
                return (T[])Object.FindObjectsOfType(typeof(T));
            else
                return mSceneQuery.GetComponents<T>();
        }

        /// <summary>
        /// Loads all components of the specified type into the <see cref="components"/> list,
        /// using the scene query.
        /// </summary>
        /// <remarks>
        /// <para>
        /// All queries are against the currently open scene.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The number of components loaded.</returns>
        public int LoadFromScene<T>() where T : Component
        {
            int result;

            T[] items = GetFromScene<T>();
            result = items.Length; 

            components.AddRange(items);

            return result;
        }

        /// <summary>
        /// Loads the components into the <see cref="components"/> list.
        /// </summary>
        /// <param name="items">The components to load.</param>
        public void Load(Component[] items)
        {
            components.AddRange(items);
        }

        /// <summary>
        /// Loads a component into the <see cref="components"/> list.
        /// </summary>
        /// <param name="item">The component to load.</param>
        public void Load(Component item)
        {
            components.Add(item);
        }
    }
}
