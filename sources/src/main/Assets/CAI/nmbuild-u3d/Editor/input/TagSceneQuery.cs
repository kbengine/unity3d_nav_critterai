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
using System.Collections.Generic;
using org.critterai.nmbuild.u3d.editor;

/// <summary>
/// Queries the scene for components based on game object tag. (Editor Only)
/// </summary>
/// <remarks>
/// <para>
/// Components are returned based the tags assigned to game objects.  The query can be local
/// to the tagged objects, or can include children.
/// </para>
/// </remarks>
[System.Serializable]
public sealed class TagSceneQuery 
    : ScriptableObject, ISceneQuery 
{
    /// <summary>
    /// One or more tags to base the query on.
    /// </summary>
    public List<string> tags = new List<string>();

    [SerializeField]
    private bool mIncludeChildren = true;

    private GameObject[] mObjects;

    /// <summary>
    /// True if components whose parents have one of the tags should be included in the results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If true, the query behavior will be the same as calling GetComponentsInChildren() on the
    /// tagged game object.  If false, the query behavior will be the same as GetComponent().
    /// </para>
    /// </remarks>
    public bool IncludeChildren
    {
        get { return mIncludeChildren; }
        set { mIncludeChildren = value; }
    }

    /// <summary>
    /// Initializes the object before each use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called by the manager of the object before each use.  It allows the 
    /// object to refresh its internal state.
    /// </para>
    /// </remarks>
    public void  Initialize()
    {
        if (tags == null || tags.Count == 0)
        {
            mObjects = new GameObject[0];
            return;
        }

        if (tags.Count == 1)
            // Shortcut.
            mObjects = GameObject.FindGameObjectsWithTag(tags[0]);
        else
        {
            // Need to aggregate.
            List<GameObject> list = new List<GameObject>();
            foreach (string tag in tags)
            {
                if (tag != null && tag.Length > 0)
                {
                    GameObject[] g = GameObject.FindGameObjectsWithTag(tag);
                    list.AddRange(g);
                }
            }
            mObjects = list.ToArray();
        }
    }

    /// <summary>
    /// Gets all scene components of the specified type, based the component or parent tags.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All queries are against the currently open scene.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>All components of the specified type.</returns>
    public T[] GetComponents<T>() where T : Component
    {
        List<T> result = new List<T>();
        foreach (GameObject go in mObjects)
        {
            if (go == null || !go.active)
                continue;

            if (mIncludeChildren)
            {
                T[] cs = go.GetComponentsInChildren<T>(false);
                result.AddRange(cs);
            }
            else
            {
                T cs = go.GetComponent<T>();

                if (cs != null)
                    result.Add(cs);
            }
        }
        return result.ToArray();
    }
}
