/*
 * Copyright (c) 2010 Stephen A. Pratt
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
using org.critterai.nav.u3d;
using org.critterai.u3d.editor;
using UnityEditor;
using UnityEngine;

namespace org.critterai.nav.u3d.editor
{
    /// <summary>
    /// Provides various navigation editor related constants and methods.
    /// </summary>
    public static class NavEditorUtil
    {
        /// <summary>
        /// The standard label for navigation assets.
        /// </summary>
        public static string AssetLabel = "Navigation";

        /// <summary>
        /// The menu priority for navigation asset creation.
        /// </summary>
        public const int NavAssetGroup = EditorUtil.AssetGroup + 500;

        /// <summary>
        /// True if the provided information does not match the current scene or is not
        /// available.
        /// </summary>
        /// <param name="info">The build information to check. (Null is valid.)</param>
        /// <returns>True if the provided information does not match the current scene.</returns>
        public static bool SceneMismatch(NavmeshBuildInfo info)
        {
            return !(info == null
                || info.inputScene == EditorApplication.currentScene
                || info.inputScene == null
                || info.inputScene.Length == 0);
        }

        /// <summary>
        /// The standard scene display name for the information.
        /// </summary>
        /// <param name="info">The build information to check. (Null is valid.)</param>
        /// <returns>The standard scene display name for the information.</returns>
        public static string SceneDisplayName(NavmeshBuildInfo info)
        {
            if (info == null || info.inputScene == null || info.inputScene.Length == 0)
                return "Unknown";

            string result = System.IO.Path.GetFileName(info.inputScene);

            return result.Substring(0, result.LastIndexOf("."));
        }

        /// <summary>
        /// Displays an object field that will only accept ScriptableObjects that implement
        /// the <see cref="INavmeshData"/> interface.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Handles error logging when an invalid object is selected.
        /// </para>
        /// </remarks>
        /// <param name="label">The label for the object field.</param>
        /// <param name="item">The ScriptableObject the field shows.</param>
        /// <returns>The ScriptableObject selected by the user.</returns>
        public static INavmeshData OnGUINavmeshDataField(string label, INavmeshData item)
        {
            ScriptableObject so = (ScriptableObject)item;

            ScriptableObject nso = (ScriptableObject)EditorGUILayout.ObjectField(label
                , so, typeof(ScriptableObject), false);

            if (nso is INavmeshData || !nso)  // Null OK.
                return (INavmeshData)nso;

            Debug.LogError(
                string.Format("{0} does not implement {1}.", nso.name, typeof(INavmeshData).Name));

            return item;
        }
    }
}
