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
using org.critterai.nav;
using org.critterai.u3d.editor;

/// <summary>
/// <see cref="CAINavEditorSettings"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(CAINavEditorSettings))]
public sealed class CAINavEditorSettingsEditor
    : Editor
{
    /// <summary>
    /// A control GUI suitable for selecting an area based on its well known name. 
    /// </summary>
    public class AreaGUIControl
    {
        private List<string> mNamesAll = new List<string>(Navmesh.MaxArea + 1);
        private string[] mNamesShort;
        private string mLabel;

        internal AreaGUIControl(string label, string[] areaNames)
        {
            mLabel = label;

            for (int i = 0; i < areaNames.Length; i++)
            {
                if (areaNames[i] != CAINavEditorSettings.Undefined)
                    mNamesAll.Add(areaNames[i]);
            }

            mNamesAll.Add(CAINavEditorSettings.Undefined);

            mNamesShort = mNamesAll.ToArray();

            mNamesAll.Clear();

            mNamesAll.AddRange(areaNames);
        }

        /// <summary>
        /// Displays the area selector.
        /// </summary>
        /// <param name="currentArea">The current area.</param>
        /// <returns>The selected area.</returns>
        public byte OnGUI(byte currentArea)
        {
            return OnGUI(mLabel, currentArea);
        }

        /// <summary>
        /// Displays the area selector with a custom label.
        /// </summary>
        /// <param name="labelOverride">The custom label.</param>
        /// <param name="currentArea">The current area.</param>
        /// <returns>The selected area.</returns>
        public byte OnGUI(string labelOverride, byte currentArea)
        {
            string name = mNamesAll[currentArea];
            
            int undef = mNamesShort.Length - 1;

            int i = undef;
            for (;i >= 0; i--)
            {
                if (name == mNamesShort[i])
                    break;
            }

            if (i == undef)
                mNamesShort[undef] = currentArea + " " + mNamesShort[undef];

            int ni = EditorGUILayout.Popup(labelOverride, i, mNamesShort);

            mNamesShort[undef] = CAINavEditorSettings.Undefined;

            if (i == ni || ni == undef)
                return currentArea;

            return (byte)mNamesAll.IndexOf(mNamesShort[ni]);
        }
    }

    private const string AddAreaName = "AddArea";

    private static bool mShowAreas = true;
    private static bool mShowFlags = true;
    private static bool mShowAvoidance = true;

    private static byte mNewArea = Navmesh.NullArea;
    private static bool mFocusNew = false;

    /// <summary>
    /// Creates a control useful for assigning area values.
    /// </summary>
    /// <param name="label">The default label for the control.</param>
    /// <returns></returns>
    public static AreaGUIControl CreateAreaControl(string label)
    {
        CAINavEditorSettings settings = EditorUtil.GetGlobalAsset<CAINavEditorSettings>();
        return new AreaGUIControl(label, (string[])settings.areaNames.Clone());
    }

    /// <summary>
    /// Gets a clone of the global well know flag names.
    /// </summary>
    /// <returns>A clone of the flag names.</returns>
    public static string[] GetFlagNames()
    {
        CAINavEditorSettings settings = EditorUtil.GetGlobalAsset<CAINavEditorSettings>();
        return (string[])settings.flagNames.Clone();
    }

    /// <summary>
    /// Gets a clone of the global  well known avoidance type names.
    /// </summary>
    /// <returns>A clone of the well known avoidance type names.</returns>
    public static string[] GetAvoidanceNames()
    {
        CAINavEditorSettings settings = EditorUtil.GetGlobalAsset<CAINavEditorSettings>();
        return (string[])settings.avoidanceNames.Clone();
    }

    void OnEnable()
    {
        mNewArea = NextUndefinedArea();
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeControls(80);

        EditorGUILayout.Separator();

        mShowAreas = EditorGUILayout.Foldout(mShowAreas, "Area Names");

        if (mShowAreas)
            OnGUIAreas();

        EditorGUILayout.Separator();

        mShowFlags = EditorGUILayout.Foldout(mShowFlags, "Flag Names");

        if (mShowFlags)
            OnGUIFlags();

        EditorGUILayout.Separator();

        mShowAvoidance = EditorGUILayout.Foldout(mShowAvoidance, "Crowd Avoidance Names");

        if (mShowAvoidance)
            OnGUIAvoidance();

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private byte NextUndefinedArea()
    {
        CAINavEditorSettings targ = (CAINavEditorSettings)target;

        string[] areaNames = targ.areaNames;

        for (int i = 1; i < areaNames.Length; i++)
        {
            if (areaNames[i] == CAINavEditorSettings.Undefined)
                return (byte)i;
        }

        return 0;
    }

    private void OnGUIAreas()
    {
        CAINavEditorSettings targ = (CAINavEditorSettings)target;

        string[] areaNames = targ.areaNames;

        EditorGUILayout.Separator();

        for (int i = 0; i < areaNames.Length; i++)
        {
            if (areaNames[i] == CAINavEditorSettings.Undefined)
                continue;

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = (i != Navmesh.NullArea);

            string areaName = EditorGUILayout.TextField(i.ToString(), areaNames[i]);

            // Note: Extra checks reduce the need to run the last check, which is more expensive.
            if (areaName.Length > 0
                && areaName != areaNames[i]
                && areaName != CAINavEditorSettings.NotWalkable  // Quick check.
                && areaName != CAINavEditorSettings.Undefined  // This check is important.
                && targ.GetArea(areaName) == CAINavEditorSettings.UnknownArea)  
            {
                areaNames[i] = areaName;
            }

            GUI.enabled = !(i == Navmesh.NullArea || i == Navmesh.MaxArea);

            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                areaNames[i] = CAINavEditorSettings.Undefined;

                mNewArea = NextUndefinedArea();
                mFocusNew = true;  // Prevents off GUI behavior.

                GUI.changed = true;
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();

        GUI.SetNextControlName(AddAreaName);
        mNewArea = NavUtil.ClampArea(EditorGUILayout.IntField(mNewArea, GUILayout.Width(80)));
        if (mFocusNew)
        {
            GUI.FocusControl(AddAreaName);
            mFocusNew = false;
        }

        GUI.enabled = (areaNames[mNewArea] == CAINavEditorSettings.Undefined);

        if (GUILayout.Button("Add", GUILayout.Width(80)))
        {
            areaNames[mNewArea] = "Area " + mNewArea;
            mNewArea = NextUndefinedArea();
            mFocusNew = true;
            GUI.changed = true;
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Maximum allowed area: " + Navmesh.MaxArea
            , EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));
    }

    private void OnGUIFlags()
    {
        CAINavEditorSettings targ = (CAINavEditorSettings)target;

        string[] names = targ.flagNames;

        EditorGUILayout.Separator();

        for (int i = 0; i < names.Length; i++)
        {
            string val = EditorGUILayout.TextField(string.Format("0x{0:X}", 1 << i), names[i]);
            names[i] = (val.Length == 0 ? names[i] : val);
        }
    }

    private void OnGUIAvoidance()
    {
        CAINavEditorSettings targ = (CAINavEditorSettings)target;

        string[] names = targ.avoidanceNames;

        EditorGUILayout.Separator();

        for (int i = 0; i < names.Length; i++)
        {
            string val = EditorGUILayout.TextField(i.ToString(), names[i]);
            names[i] = (val.Length == 0 ? names[i] : val);
        }
    }

    [MenuItem(EditorUtil.MainMenu + "Nav Editor Settings", false, EditorUtil.GlobalGroup)]
    static void EditSettings()
    {
        CAINavEditorSettings item = EditorUtil.GetGlobalAsset<CAINavEditorSettings>();
        Selection.activeObject = item;
    }
}
