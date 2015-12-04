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
using UnityEditor;
using System.IO;

namespace org.critterai.u3d.editor
{
    /// <summary>
    /// Provides general purpose utility functions for the Unity Editor.
    /// </summary>
    public static class EditorUtil
    {
        /// <summary>
        /// The base priority for the CAI asset creation menu.
        /// </summary>
        public const int AssetGroup = 100;

        /// <summary>
        /// The base priority for the CAI game object creation menu.
        /// </summary>
        public const int GameObjectGroup = 1000;

        /// <summary>
        /// The base priority for the CAI view menu.
        /// </summary>
        public const int ViewGroup = 2000;

        /// <summary>
        /// The base priority for the CAI manager menu group.
        /// </summary>
        public const int ManagerGroup = 3000;

        /// <summary>
        /// The base priority for the global menu group.
        /// </summary>
        public const int GlobalGroup = 4000;

        /// <summary>
        /// The root CAI menu name.
        /// </summary>
        public const string MainMenu = "CritterAI/";

        /// <summary>
        /// The navigation asset menu name.
        /// </summary>
        public const string NavAssetMenu = MainMenu + "Create Nav Asset/";

        /// <summary>
        /// The NMGen asset menu name.
        /// </summary>
        public const string NMGenAssetMenu = MainMenu + "Create NMGen Asset/";

        /// <summary>
        /// The NMGen GameObject menu name.
        /// </summary>
        public const string NMGenGameObjectMenu = MainMenu + "Create NMGen/";

        /// <summary>
        /// The navigation GameObject menu name.
        /// </summary>
        public const string NavGameObjectMenu = MainMenu + "Create Nav/";

        /// <summary>
        /// The CAI view menu name.
        /// </summary>
        public const string ViewMenu = MainMenu + "View/";

        private static GUIStyle mHelpStyle;
        private static GUIStyle mWarningStyle;
        private static GUIStyle mErrorStyle;

        /// <summary>
        /// A general purpose error text box.
        /// </summary>
        public static GUIStyle ErrorStyle
        {
            get
            {
                if (mErrorStyle == null)
                {
                    mErrorStyle = new GUIStyle(GUI.skin.box);
                    mErrorStyle.wordWrap = true;
                    mErrorStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.red;
                    c.a = 0.75f;
                    mErrorStyle.normal.textColor = c;
                }
                return mErrorStyle;
            }
        }

        /// <summary>
        /// A general purpose warning text box.
        /// </summary>
        public static GUIStyle WarningStyle
        {
            get
            {
                if (mWarningStyle == null)
                {
                    mWarningStyle = new GUIStyle(GUI.skin.box);
                    mWarningStyle.wordWrap = true;
                    mWarningStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.yellow;
                    c.a = 0.75f;
                    mWarningStyle.normal.textColor = c;
                }
                return mWarningStyle;
            }
        }

        /// <summary>
        /// A general purpose help text box.
        /// </summary>
        public static GUIStyle HelpStyle
        {
            get
            {
                if (mHelpStyle == null)
                {
                    mHelpStyle = new GUIStyle(GUI.skin.box);
                    mHelpStyle.wordWrap = true;
                    mHelpStyle.alignment = TextAnchor.UpperLeft;

                    Color c = Color.white;
                    c.a = 0.75f;
                    mHelpStyle.normal.textColor = c;

                }
                return mHelpStyle;
            }
        }

        /// <summary>
        /// Provides an editor GUI for adding/removing objects from a list based on object type.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="label">The GUI label. (Type description.)</param>
        /// <param name="items">The list of items to manage.</param>
        /// <param name="allowScene">Allow scene objects in the list.</param>
        /// <returns>True if the GUI changed within the method.</returns>
        public static bool OnGUIManageObjectList<T>(string label, List<T> items, bool allowScene) 
            where T : UnityEngine.Object
        {
            if (items == null)
                return false;

            bool origChanged = GUI.changed;
            GUI.changed = false;

            // Never allow nulls.  So get rid of them first.
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null)
                {
                    items.RemoveAt(i);
                    GUI.changed = true;
                }
            }

            GUILayout.Label(label);

            if (items.Count > 0)
            {
                int delChoice = -1;

                for (int i = 0; i < items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    T item = (T)EditorGUILayout.ObjectField(items[i], typeof(T), allowScene);

                    if (item == items[i] || !items.Contains(item))
                        items[i] = item;

                    if (GUILayout.Button("X", GUILayout.Width(30)))
                        delChoice = i;

                    EditorGUILayout.EndHorizontal();
                }

                if (delChoice >= 0)
                {
                    GUI.changed = true;
                    items.RemoveAt(delChoice);
                }
            }

            EditorGUILayout.Separator();

            T nitem = (T)EditorGUILayout.ObjectField("Add", null, typeof(T), allowScene);

            if (nitem != null)
            {
                if (!items.Contains(nitem))
                {
                    items.Add(nitem);
                    GUI.changed = true;
                }
            }

            bool result = GUI.changed;
            GUI.changed = GUI.changed || origChanged;

            return result;
        }

        /// <summary>
        /// Provides an editor GUI for adding/removing strings from a list.
        /// </summary>
        /// <param name="items">The list of strings to manage.</param>
        /// <param name="isTags">True if the strings represent tags.</param>
        /// <returns>True if the GUI changed within the method.</returns>
        public static bool OnGUIManageStringList(List<string> items, bool isTags)
        {
            if (items == null)
                return false;

            bool origChanged = GUI.changed;
            GUI.changed = false;

            if (items.Count > 0)
            {
                GUILayout.Label((isTags ? "Tags" : "Items"));

                int delChoice = -1;

                for (int i = 0; i < items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    string item;

                    if (isTags)
                        item = EditorGUILayout.TagField(items[i]);
                    else
                        item = EditorGUILayout.TextField(items[i]);

                    if (item == items[i] || !items.Contains(item))
                        items[i] = item;

                    if (GUILayout.Button("X", GUILayout.Width(30)))
                        delChoice = i;

                    EditorGUILayout.EndHorizontal();
                }

                if (delChoice >= 0)
                {
                    GUI.changed = true;
                    items.RemoveAt(delChoice);
                }
            }

            EditorGUILayout.Separator();

            string ntag = EditorGUILayout.TagField("Add", "");

            if (ntag.Length > 0)
            {
                if (!items.Contains(ntag))
                {
                    GUI.changed = true;
                    items.Add(ntag);
                }
            }

            bool result = GUI.changed;
            GUI.changed = GUI.changed || origChanged;

            return result;
        }

        /// <summary>
        /// Creates a new asset in the same directory as the specified asset.
        /// </summary>
        /// <typeparam name="T">The type of asse to create.</typeparam>
        /// <param name="atAsset">The asset where the new asset should be colocated.</param>
        /// <param name="label">The asset label.</param>
        /// <returns>The new asset.</returns>
        public static T CreateAsset<T>(ScriptableObject atAsset, string label) 
            where T : ScriptableObject
        {
            string name = typeof(T).ToString();
            string path = GenerateStandardPath(atAsset, name);

            T result = ScriptableObject.CreateInstance<T>();
            result.name = name;

            AssetDatabase.CreateAsset(result, path);

            if (label.Length > 0)
                AssetDatabase.SetLabels(result, new string[1] { label });

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);

            return result;
        }

        /// <summary>
        /// Creates a new asset at the standard path.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method tries to detect the directory of the currently selected asset.  If it 
        /// can't, it will place the new asset in the root asset directory.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of asse to create.</typeparam>
        /// <param name="label">The asset label.</param>
        /// <returns>The new asset.</returns>
        public static T CreateAsset<T>(string label) 
            where T : ScriptableObject
        {
            string name = typeof(T).Name;
            string path = GenerateStandardPath(Selection.activeObject, name);

            T result = ScriptableObject.CreateInstance<T>();
            result.name = name;

            AssetDatabase.CreateAsset(result, path);

            if (label.Length > 0)
                AssetDatabase.SetLabels(result, new string[1] { label });

            AssetDatabase.SaveAssets(); 
            AssetDatabase.ImportAsset(path);

            return result;
        }

        /// <summary>
        /// Gets the specified global asset.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Global assets are singleton assets that exist in a well known path within the project.
        /// </para>
        /// <para>
        /// If the global asset does not exist a new one will be created.  So this method will
        /// return null only on error.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of global asset.</typeparam>
        /// <returns>The global asset.</returns>
        public static T GetGlobalAsset<T>()
            where T : ScriptableObject
        {
            string name = typeof(T).Name;

            const string rootPath = "Assets/CAI/GlobalAssets/";

            if (!Directory.Exists(rootPath))
            {
                Debug.LogError("Global assets path does not exist: " + rootPath);
                return null;
            }

            string path = rootPath + name + ".asset";

            T result = (T)AssetDatabase.LoadMainAssetAtPath(path);

            if (!result)
            {
                result = ScriptableObject.CreateInstance<T>();
                result.name = name;

                AssetDatabase.CreateAsset(result, path);
                AssetDatabase.ImportAsset(path);
            }

            return result;
        }

        /// <summary>
        /// Attempts to get a scene position based on the current SceneView camera.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is useful for creating game objects in the scene via script.
        /// </para>
        /// <para>
        /// The returned position will be one of the following. (In order of priority.)
        /// </para>
        /// <ol>
        /// <li>The hit position of a ray cast from the SceneView camera.</li>
        /// <li>A position aproximately 15 units forward from the SceneView camera.</li>
        /// <li>The zero vector.</li>
        /// </ol>
        /// </remarks>
        /// <returns>The recommended position in the scene.</returns>
        public static Vector3 GetCreatePosition()
        {
            Camera cam = SceneView.lastActiveSceneView.camera;

            if (!cam)
                return Vector3.zero;

            RaycastHit hit;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out hit, 500))
                return hit.point;
                  
            return cam.transform.position + cam.transform.forward * 15;
        }

        private static string GenerateStandardPath(Object atAsset, string name)
        {
            string dir = AssetDatabase.GetAssetPath(atAsset);

            if (dir.Length == 0)
                // Selection must not be an asset.
                dir = "Assets";
            else if (!Directory.Exists(dir))
                // Selection must be a file asset.
                dir = Path.GetDirectoryName(dir);

            return AssetDatabase.GenerateUniqueAssetPath(dir + "/" + name + ".asset");
        }
    }
}
