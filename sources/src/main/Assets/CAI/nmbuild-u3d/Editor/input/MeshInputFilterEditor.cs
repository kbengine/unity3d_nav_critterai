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
using org.critterai.nmbuild;

/// <summary>
/// <see cref="MeshInputFilter"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(MeshInputFilter))]
public sealed class MeshInputFilterEditor
    : Editor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        MeshInputFilter targ = (MeshInputFilter)target;

        EditorGUILayout.Separator();

        // Clamp before sending to property.
        targ.Priority = EditorGUILayout.IntField("Priority", targ.Priority);

        targ.matchType = (MatchType)
            EditorGUILayout.EnumPopup("Match Type", targ.matchType);

        EditorGUILayout.Separator();

        EditorUtil.OnGUIManageObjectList("Meshes", targ.meshes, false);

        EditorGUILayout.Separator();

        string msg = "Input Build Processor\n\nFilters out " + typeof(MeshFilter).Name
            + " components that reference the specified " + typeof(Mesh).Name + " objects.\n\n";

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

    [MenuItem(EditorUtil.NMGenAssetMenu + "Component Filter : Mesh", false, NMBEditorUtil.FilterGroup)]
    static void CreateAsset()
    {
        MeshInputFilter item = 
            EditorUtil.CreateAsset<MeshInputFilter>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
