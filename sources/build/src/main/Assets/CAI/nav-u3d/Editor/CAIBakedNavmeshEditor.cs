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
using UnityEditor;
using org.critterai.nav;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using org.critterai.u3d.editor;
using org.critterai.nav.u3d.editor;

/// <summary>
/// <see cref="CAIBakedNavmesh"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(CAIBakedNavmesh))]
public sealed class CAIBakedNavmeshEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        CAIBakedNavmesh targ = (CAIBakedNavmesh)target;

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Status", (targ.HasNavmesh ? "Has mesh" : "Empty"));
        EditorGUILayout.LabelField("Version", targ.Version.ToString());
        EditorGUILayout.LabelField("Input Scene", NavEditorUtil.SceneDisplayName(targ.BuildInfo));

        EditorGUILayout.Separator();

        NavmeshSceneDraw.Instance.OnGUI(targ, "Show Mesh", true, true);

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = targ.HasNavmesh;
        if (GUILayout.Button("Save"))
        {
            string filePath = EditorUtility.SaveFilePanel(
                "Save Navigation Mesh"
                , ""
                , targ.name
                , "navmesh");
            SaveMesh(targ, filePath);
        }
        GUI.enabled = true;

        if (GUILayout.Button("Load"))
        {
            string filePath = EditorUtility.OpenFilePanel(
                "Select Serialized Navmesh"
                , ""
                , "navmesh");
            if (LoadMesh(targ, filePath))
                GUI.changed = true;
        }

        EditorGUILayout.EndHorizontal();

        if (targ.HasNavmesh)
        {
            EditorGUILayout.Separator();

            if (GUILayout.Button("Log Mesh State"))
                Debug.Log(targ.GetMeshReport());
        }

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private static bool LoadMesh(CAIBakedNavmesh targ, string filePath)
    {
        string msg = null;

        if (filePath.Length == 0)
            return false;

        FileStream fs = null;
        BinaryFormatter formatter = new BinaryFormatter();

        try
        {
            fs = new FileStream(filePath, FileMode.Open);
            System.Object obj = formatter.Deserialize(fs);

            NavStatus status = targ.Load((byte[])obj, null);
            if ((status & NavStatus.Sucess) == 0)
                msg = status.ToString();
        }
        catch (System.Exception ex)
        {
            msg = ex.Message;
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }

        if (msg != null)
        {
            Debug.LogError(targ.name + ": BakedNavmesh: Load bytes failed: "
                + msg);
            return false;
        }

        return true;
    }

    private static void SaveMesh(CAIBakedNavmesh targ, string filePath)
    {
        if (filePath.Length == 0 || !targ.HasNavmesh)
            return;

        FileStream fs = null;
        BinaryFormatter formatter = new BinaryFormatter();

        try
        {
            fs = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(fs, targ.GetNavmesh().GetSerializedMesh());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(targ.name + ": BakedNavmesh: Save bytes failed: "
                + ex.Message);
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }
    }

    [MenuItem(EditorUtil.NavAssetMenu + "Baked Navmesh", false, NavEditorUtil.NavAssetGroup)]
    static void CreateAsset()
    {
        CAIBakedNavmesh item = EditorUtil.CreateAsset<CAIBakedNavmesh>(NavEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }

    [MenuItem(EditorUtil.ViewMenu + "Hide Navmesh", true)]
    static bool HideNavmeshValidate()
    {
        return NavmeshSceneDraw.Instance.IsShown();
    }

    [MenuItem(EditorUtil.ViewMenu + "Hide Navmesh", false, EditorUtil.ViewGroup)]
    static void HideNavmesh()
    {
        NavmeshSceneDraw.Instance.Hide();
    }
}