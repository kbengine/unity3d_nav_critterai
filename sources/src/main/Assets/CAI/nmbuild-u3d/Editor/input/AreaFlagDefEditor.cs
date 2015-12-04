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
using System.Collections.Generic;
using org.critterai.u3d.editor;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.nmgen;

/// <summary>
/// <see cref="AreaFlagDef"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(AreaFlagDef))]
public sealed class AreaFlagDefEditor
    : Editor
{
    private string[] mFlagNames;
    private CAINavEditorSettingsEditor.AreaGUIControl mAreaControl;

    private byte mAddSelection = NMGen.MaxArea;

    void OnEnable()
    {
        mAreaControl = CAINavEditorSettingsEditor.CreateAreaControl("");
        mFlagNames = CAINavEditorSettingsEditor.GetFlagNames();
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        AreaFlagDef targ = (AreaFlagDef)target;

        // Has someone done something naughty?

        if (targ.areas == null || targ.areas == null || targ.areas.Count != targ.flags.Count)
        {
            Debug.LogError("Data null reference or size mismatch. Resetting component.", targ);
            targ.areas = new List<byte>();
            targ.flags = new List<int>();
        }

        List<byte> areas = targ.areas;
        List<int> flags = targ.flags;

        EditorGUILayout.Separator();

        targ.SetPriority(EditorGUILayout.IntField("Priority", targ.Priority));

        EditorGUILayout.Separator();

        GUILayout.Label("Area / Flags");

        EditorGUILayout.Separator();

        if (areas.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            int delChoice = -1;

            for (int i = 0; i < areas.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Note: Duplicates are a waste, but technically ok.

                areas[i] = mAreaControl.OnGUI(areas[i]);
                flags[i] = EditorGUILayout.MaskField(flags[i], mFlagNames);

                if (GUILayout.Button("X", GUILayout.Width(30)))
                    delChoice = i;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                flags.RemoveAt(delChoice);
                areas.RemoveAt(delChoice);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        mAddSelection = mAreaControl.OnGUI(mAddSelection);

        if (GUILayout.Button("Add"))
        {
            areas.Add(mAddSelection);
            flags.Add(org.critterai.nmbuild.NMBuild.DefaultFlag);
            GUI.changed = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        GUILayout.Box("Input Build Processor\n\nAdds an NMGen processor that adds flags to"
                + " polygons based on area assignment. E.g. Add the 'swim' flag to all 'water'"
                + " polygons."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Compiler : Area Flag Def"
        , false, NMBEditorUtil.CompilerGroup)]
    static void CreateAsset()
    {
        AreaFlagDef item = EditorUtil.CreateAsset<AreaFlagDef>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
