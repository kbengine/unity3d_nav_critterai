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

/// <summary>
/// <see cref="TagSceneQuery"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(TagSceneQuery))]
public sealed class TagSceneQueryEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        TagSceneQuery targ = (TagSceneQuery)target;

        // Has someone done something naughty?

        if (targ.tags == null)
        {
            Debug.LogError(targ.name + "Data null reference. Resetting component.");
            targ.tags = new List<string>();
        }

        EditorGUILayout.Separator();

        EditorGUIUtility.LookLikeControls(120);

        targ.IncludeChildren = EditorGUILayout.Toggle("Include Children", targ.IncludeChildren);

        EditorGUIUtility.LookLikeControls();

        EditorGUILayout.Separator();

        EditorUtil.OnGUIManageStringList(targ.tags, true);

        EditorGUILayout.Separator();

        GUILayout.Box(
            "Scene Query\n\nComponent search will include game objects with the specified tags."
            + " The search can optionally include child game objects."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Scene Query : Tag", false, NMBEditorUtil.SceneGroup)]
    static void CreateAsset()
    {
        TagSceneQuery item = EditorUtil.CreateAsset<TagSceneQuery>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
