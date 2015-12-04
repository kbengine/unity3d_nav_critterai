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
/// Handles advanced build features for <see cref="NavmeshBuild"/> assets. (Editor Only)
/// </summary>
/// <exclude />
public sealed class NavmeshBuildManager
    : EditorWindow
{
    /*
     * Design notes:
     * 
     * An editor reset will cause disable/enable to run.
     * A scene change is taken care of by the heirarchy change event.
     */

    private const string WindowTitle = "Navmesh Build";
    private const int NoSelection = -1;  // Must be < 0;

    private static BuildProcessor mProcessor;

    private const float ToolbarHeight = 25;
    private const float GlobalStatusHeight = 20;

    private const float MinWidth = ControlUtil.ButtonAreaWidth + 2 * ControlUtil.MarginSize;

    // The config control is the current restriction;
    private const float MinHeight = 455;

    private float mTaskMax;

    void OnEnable()
    {
        minSize = new Vector2(MinWidth, MinHeight);

        // Remember, the processor is static, so it may have persisted.
        SetProcessor();

        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        if (mProcessor.TaskManager.TaskCount == 0)
            // Don't want the the background thread to keep running if there is nothing to do.
            ClearProcessor();

        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private static void ClearProcessor()
    {
        if (mProcessor == null)
            return;

        EditorApplication.update -= mProcessor.Update;
        mProcessor.Dispose();

        mProcessor = null;
    }

    private static void SetProcessor()
    {
        if (mProcessor == null)
        {
            mProcessor = new BuildProcessor();
            EditorApplication.update += mProcessor.Update;
        }
    }

    void OnGUI()
    {
        if (mProcessor.BuildCount == 0)
        {
            GUILayout.Label("Select an advanced navigation mesh build from the project window.");
            return;
        }

        float displayWidth = position.width - 2 * ControlUtil.MarginSize;
        float displayHeight = position.height - 2 * ControlUtil.MarginSize;

        Rect toolBarArea = new Rect(ControlUtil.MarginSize
            , ControlUtil.MarginSize
            , displayWidth
            , ToolbarHeight);

        Rect controlArea = new Rect(ControlUtil.MarginSize
            , toolBarArea.yMax + ControlUtil.MarginSize
            , displayWidth
            , displayHeight - ToolbarHeight - GlobalStatusHeight - 2 * ControlUtil.MarginSize);

        Rect statusArea = new Rect(ControlUtil.MarginSize
            , controlArea.yMax + ControlUtil.MarginSize
            , displayWidth
            , GlobalStatusHeight);

        bool includeMain = position.width >= MinWidth + 3 * ControlUtil.MarginSize;

        BuildSelector.Instance.OnGUI(toolBarArea);
        mProcessor.OnGUI(controlArea, includeMain);

        float taskCount = mProcessor.TaskManager.TaskCount;

        if (taskCount == 0)
        {
            mTaskMax = 0;
            GUI.Box(statusArea, "No Build Tasks.", EditorUtil.HelpStyle);
        }
        else
        {
            mTaskMax = Mathf.Max(mTaskMax, taskCount);
            if (mTaskMax == 1)
                GUI.Box(statusArea, "Build Tasks: " + taskCount, EditorUtil.HelpStyle);
            else
                EditorGUI.ProgressBar(statusArea, 1 - taskCount / mTaskMax, "");
        }
    }

    void OnSceneGUI(SceneView scene)
    {
        mProcessor.OnSceneGUI();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnSelectionChange()
    {
        Object selection = Selection.activeObject;

        if (!selection || !(selection is NavmeshBuild))
            return;

        NavmeshBuild build = (NavmeshBuild)selection;

        if (build.BuildType != NavmeshBuildType.Advanced)
            return;

        BuildSelector.Instance.Select(build);
    }

    internal static void ForceProcessorReset()
    {
        ClearProcessor();
        SetProcessor();
    }

    /// <summary>
    /// Opens and focus' the build manager window.
    /// </summary>
    [MenuItem(EditorUtil.MainMenu + "Navmesh Build Manager", false, EditorUtil.ManagerGroup)]
    public static void OpenWindow()
    {
        NavmeshBuildManager window = EditorWindow.GetWindow<NavmeshBuildManager>(false
                , WindowTitle);

        window.Show();
        window.Focus();        
    }
}
