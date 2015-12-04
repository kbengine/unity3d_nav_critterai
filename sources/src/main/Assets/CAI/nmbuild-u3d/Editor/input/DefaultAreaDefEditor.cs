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
using org.critterai.u3d.editor;
using org.critterai.nmbuild.u3d.editor;

/// <summary>
/// <see cref="DefaultAreaDef"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(DefaultAreaDef))]
public sealed class DefaultAreaDefEditor
    : Editor
{
    private CAINavEditorSettingsEditor.AreaGUIControl mAreaControl;

    void OnEnable()
    {
        mAreaControl = CAINavEditorSettingsEditor.CreateAreaControl("Default Area");
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DefaultAreaDef targ = (DefaultAreaDef)target;

        EditorGUILayout.Separator();

        GUILayout.Label("Priority: " + targ.Priority);

        EditorGUILayout.Separator();

        targ.DefaultArea = mAreaControl.OnGUI(targ.DefaultArea);

        EditorGUILayout.Separator();

        GUILayout.Box(
            "Input Build Processor\n\nApplies the area to all components in the build."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Area Definition : Default", false, NMBEditorUtil.AreaGroup)]
    static void CreateAsset()
    {
        DefaultAreaDef item = EditorUtil.CreateAsset<DefaultAreaDef>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
