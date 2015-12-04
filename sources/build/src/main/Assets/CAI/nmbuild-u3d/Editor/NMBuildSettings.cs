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
using org.critterai.nmbuild.u3d.editor;
using org.critterai.u3d.editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles setting configuration values related to the Navmesh build. (Editor Only)
/// </summary>
/// <exclude />
public sealed class NMBuildSettings
    : EditorWindow
{
    private const string MaxConcurrKey = "org.critterai.nmbuild.MaxConcurrency";
    private const string WindowTitle = "Build Config";

    void OnEnable()
    {
        minSize = new Vector2(250, 160);
    }

    void OnGUI()
    {
        Rect area = new Rect(ControlUtil.MarginSize
            , ControlUtil.MarginSize
            , position.width - 2 * ControlUtil.MarginSize
            , position.height - 2 * ControlUtil.MarginSize);

        GUILayout.BeginArea(area);

        GUILayout.Label("Maximum Concurrency");

        int orig = BuildProcessor.MaxConcurrency;

        int val = (int)EditorGUILayout.Slider(orig, 1, System.Environment.ProcessorCount);

        if (orig != val)
            BuildProcessor.MaxConcurrency = val;

        // TODO: Review threading recommendation after below noted bug is fixed.
        GUILayout.Box("Recommended: " + BuildProcessor.DefaultConcurrency
            + "\nWill take effect next processor start."
            + "\nWarning: Threading bugs in Unity Editor 3.5 can cause problems if set"
            + " to a higher value."
            , EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));

        GUILayout.Space(ControlUtil.MarginSize * 2);

        if (GUILayout.Button("Force Processor Reset"))
            NavmeshBuildManager.ForceProcessorReset();

        GUILayout.EndArea();
    }

    /// <summary>
    /// Opens and focuses the build settings window.
    /// </summary>
    [MenuItem("CritterAI/Navmesh Build Settings", false, EditorUtil.ManagerGroup + 10)]
    public static void OpenWindow()
    {
        NMBuildSettings window = EditorWindow.GetWindow<NMBuildSettings>(true
                , WindowTitle);

        window.Show();
        window.Focus();
    }
}
