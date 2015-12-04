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
using org.critterai.u3d.editor;
using UnityEngine;
using UnityEditor;

/// <summary>
/// <see cref="CylinderAreaMarker"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(CylinderAreaMarker))]
public sealed class CylinderAreaMarkerEditor
    : AreaMarkerEditor
{
    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        OnGUIStandard((CylinderAreaMarker)target);

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    [DrawGizmo(GizmoType.NotSelected | GizmoType.SelectedOrChild | GizmoType.Pickable)]
    static void DrawGizmo(CylinderAreaMarker marker, GizmoType type)
    {
        DrawStandardGizmo(marker, type);
    }

    [MenuItem(EditorUtil.NMGenGameObjectMenu + "Area Marker : Cyl"
        , false
        , EditorUtil.GameObjectGroup)]
    static void CreateGameObject()
    {
        GameObject go = new GameObject("AreaMarker");
        go.transform.position = EditorUtil.GetCreatePosition();

        go.AddComponent<CylinderAreaMarker>();

        Selection.activeGameObject = go;
    }
}
