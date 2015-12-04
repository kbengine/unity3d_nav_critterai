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
/// <see cref="TagInputFilter"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(TagInputFilter))]
public sealed class TagInputFilterEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        TagInputFilter targ = (TagInputFilter)target;

        // Has someone done something naughty?

        if (targ.tags == null)
        {
            Debug.LogError(targ.name + "Data null reference. Resetting component.");
            targ.tags = new List<string>();
        }

        EditorGUILayout.Separator();

        targ.Priority = EditorGUILayout.IntField("Priority", targ.Priority);

        EditorGUILayout.Separator();

        targ.recursive = EditorGUILayout.Toggle("Recursive", targ.recursive);

        EditorGUILayout.Separator();

        EditorUtil.OnGUIManageStringList(targ.tags, true);

        EditorGUILayout.Separator();

        string msg = "Input Build Processor\n\nFilters out components with the specified tags.";

        if (targ.recursive)
        {
            msg += 
                "\n\nRecursive: A component will be filtered out if any of its parents"
                + " have one of the tags.";
        }

        GUILayout.Box(msg, EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Component Filter : Tag", false, NMBEditorUtil.FilterGroup)]
    static void CreateAsset()
    {
        TagInputFilter item = EditorUtil.CreateAsset<TagInputFilter>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
