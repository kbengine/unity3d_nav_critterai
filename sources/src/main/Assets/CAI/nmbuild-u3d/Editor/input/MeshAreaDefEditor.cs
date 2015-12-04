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
using org.critterai.nmgen;
using System.Collections.Generic;
using org.critterai.nav;
using org.critterai.u3d.editor;
using org.critterai.nmbuild.u3d.editor;

/// <summary>
/// <see cref="MeshAreaDef"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(MeshAreaDef))]
public sealed class MeshAreaDefEditor
    : Editor
{
    private CAINavEditorSettingsEditor.AreaGUIControl mAreaControl;

    void OnEnable()
    {
        mAreaControl = CAINavEditorSettingsEditor.CreateAreaControl("");
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        MeshAreaDef targ = (MeshAreaDef)target;

        // Has someone done something naughty?

        if (targ.areas == null || targ.meshes == null)
        {
            Debug.LogError(targ.name + "Data null reference. Resetting component.");
            targ.areas = new List<byte>();
            targ.meshes = new List<Mesh>();
        }

        List<byte> areas = targ.areas;
        List<Mesh> meshes = targ.meshes;

        if (areas.Count > meshes.Count)
        {
            areas.RemoveRange(meshes.Count, areas.Count - meshes.Count);
            Debug.LogError(targ.name + "Data size mismatch. Area list truncated.");
        }
        else if (meshes.Count > areas.Count)
        {
            meshes.RemoveRange(areas.Count, meshes.Count - areas.Count);
            Debug.LogError(targ.name + "Data size mismatch. Mesh list truncated.");
        }

        EditorGUILayout.Separator();

        targ.SetPriority(EditorGUILayout.IntField("Priority", targ.Priority));

        EditorGUILayout.Separator();

        targ.matchType = (MatchType)
            EditorGUILayout.EnumPopup("Match Type", targ.matchType);

        EditorGUILayout.Separator();

        GUILayout.Label("Mesh / Area");

        EditorGUILayout.Separator();

        if (areas.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            int delChoice = -1;

            for (int i = 0; i < areas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                Mesh mesh = (Mesh)
                    EditorGUILayout.ObjectField(meshes[i], typeof(Mesh), false);

                if (mesh == null)
                    delChoice = i;
                else if (mesh == meshes[i] || !meshes.Contains(mesh))
                    meshes[i] = mesh;

                areas[i] = mAreaControl.OnGUI(areas[i]);

                if (GUILayout.Button("X", GUILayout.Width(30)))
                    delChoice = i;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                meshes.RemoveAt(delChoice);
                areas.RemoveAt(delChoice);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        Mesh nmesh = (Mesh)
            EditorGUILayout.ObjectField("Add", null, typeof(Mesh), false);

        if (nmesh != null && !meshes.Contains(nmesh))
        {
            meshes.Add(nmesh);
            areas.Add(NMGen.MaxArea);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        string msg = "Input Build Processor\n\nApplies areas to " + typeof(MeshFilter).Name 
            + " compoents based on the " + typeof(Mesh).Name + " object they reference.\n\n";

        switch (targ.matchType)
        {
            case MatchType.Strict:

                msg += "Will match only on strict mesh equality.";
                break;

            case MatchType.NameBeginsWith:

                msg += "Will match any mesh that starts with the mesh names. Example: If the"
                    + " source mesh name is 'Column', then 'ColumnWooden' and 'Column Iron' will"
                    + " match.";
                break;

            case MatchType.AnyInstance:

                msg += "Will match any instance, based on name. The check is lazy.  Example:"
                    + " If the source mesh name is 'SwampMesh', then both 'SwampMesh Instance' and "
                    + " 'SwampMesh NotReallyAnInstance' will match.";
                break;
        }

        GUILayout.Box(msg, EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Area Definition : Mesh", false, NMBEditorUtil.AreaGroup)]
    static void CreateAsset()
    {
        MeshAreaDef item = EditorUtil.CreateAsset<MeshAreaDef>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
