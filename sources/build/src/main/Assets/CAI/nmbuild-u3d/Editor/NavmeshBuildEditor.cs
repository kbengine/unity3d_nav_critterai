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
using org.critterai.nav.u3d;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.u3d.editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using org.critterai.nav.u3d.editor;
using org.critterai.geom;

/// <summary>
/// <see cref="NavmeshBuild"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(NavmeshBuild))]
public sealed class NavmeshBuildEditor
    : Editor
{
    private const string ShowInputKey = "org.critterai.nmbuild.ShowInputConfig";
    private const string ShowNMGenKey = "org.critterai.nmbuild.ShowNMGenConfig";
    private const string ShowNMGenPriKey = "org.critterai.nmbuild.ShowNMGenPri";
    private const string ShowNMGenAdvKey = "org.critterai.nmbuild.ShowNMGenAdv";

    // Foldout controls.
    private static bool mShowInputConfig = false;
    private static bool mShowNMGenConfig = false;
    private static bool mShowPrimaryConfig = true;
    private static bool mShowAdvancedConfig = false;

    void OnEnable()
    {
        UnityBuildContext.TraceEnabled = EditorPrefs.GetBool(UnityBuildContext.TraceKey);

        mShowInputConfig = EditorPrefs.GetBool(ShowInputKey);

        mShowNMGenConfig = EditorPrefs.GetBool(ShowNMGenKey);
        mShowPrimaryConfig = EditorPrefs.GetBool(ShowNMGenPriKey);
        mShowAdvancedConfig = EditorPrefs.GetBool(ShowNMGenAdvKey);
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        NavmeshBuild targ = (NavmeshBuild)target;

        EditorGUIUtility.LookLikeControls(100);

        OnGUIState(targ);

        GUI.enabled = !targ.HasBuildData;

        EditorGUILayout.Separator();

        targ.BuildTarget = NavEditorUtil.OnGUINavmeshDataField("Bake Target", targ.BuildTarget);

        EditorGUILayout.Separator();

        NavmeshBuildType buildType = (NavmeshBuildType)
            EditorGUILayout.EnumPopup("Build Type", targ.BuildType);

        if (buildType != targ.BuildType)
        {
            targ.BuildType = buildType;
            if (buildType == NavmeshBuildType.Advanced)
                BuildSelector.Instance.Select(targ);
        }

        EditorGUILayout.Separator();

        TileBuildData tdata = targ.BuildData;

        GUI.enabled = (tdata == null && targ.TargetHasNavmesh);

        if (GUILayout.Button("Log Mesh State"))
            Debug.Log(targ.BuildTarget.GetMeshReport());

        GUI.enabled = true;

        switch (targ.BuildType)
        {
            case NavmeshBuildType.Standard:
                OnGUIStandard(targ);
                break;
            case NavmeshBuildType.Advanced:
                OnGUIAdvanced(targ);  
                break;
        }

        EditorGUILayout.Separator();

        bool orig = mShowInputConfig;
        mShowInputConfig = EditorGUILayout.Foldout(mShowInputConfig, "Input Configuration");
        if (orig != mShowInputConfig)
            EditorPrefs.SetBool(ShowInputKey, mShowInputConfig);

        EditorGUIUtility.LookLikeControls();

        if (mShowInputConfig)
        {
            GUI.enabled = !targ.HasBuildData;

            EditorGUILayout.Separator();

            targ.AutoCleanGeometry = EditorGUILayout.Toggle("Auto-Clean", targ.AutoCleanGeometry);

            EditorGUILayout.Separator();

            ScriptableObject so = (ScriptableObject)targ.SceneQuery;
            ScriptableObject nso = (ScriptableObject)EditorGUILayout.ObjectField(
                "Scene Query"
                , so
                , typeof(ScriptableObject)
                , false);

            if (nso != so)
            {
                if (!nso || nso is ISceneQuery)
                    targ.SceneQuery = (ISceneQuery)nso;
                else
                {
                    Debug.LogError(string.Format("{0} does not implement {1}."
                        , nso.name, typeof(ISceneQuery).Name));
                }
            }

            EditorGUILayout.Separator();

            if (OnGUIManageProcessorList(targ.inputProcessors))
                targ.IsDirty = true;

            GUI.enabled = true;
        }

        EditorGUILayout.Separator();

        if (targ.IsDirty)
        {
            EditorUtility.SetDirty(target);
            targ.IsDirty = false;
        }
    }

    private static bool OnGUIManageProcessorList(List<ScriptableObject> items)
    {
        bool origChanged = GUI.changed;
        GUI.changed = false;

        // Never allow nulls.  So get rid of them first.
        // Can happen it target assets have been deleted.
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (!items[i])
            {
                items.RemoveAt(i);
                GUI.changed = true;
            }
        }

        GUILayout.Label("Processors");

        if (items.Count > 0)
        {
            int delChoice = -1;

            for (int i = 0; i < items.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                ScriptableObject currItem = (ScriptableObject)items[i];
                ScriptableObject item = (ScriptableObject)
                    EditorGUILayout.ObjectField(currItem, typeof(ScriptableObject), false);

                if (!item || GUILayout.Button("X", GUILayout.Width(30)))
                    delChoice = i;
                else if (item != currItem && OkToAdd(items, item))
                    items[i] = item;

                EditorGUILayout.EndHorizontal();
            }

            if (delChoice >= 0)
            {
                GUI.changed = true;
                items.RemoveAt(delChoice);
            }
        }

        EditorGUILayout.Separator();

        ScriptableObject nitem = (ScriptableObject)
            EditorGUILayout.ObjectField("Add", null, typeof(ScriptableObject), false);

        if (nitem && OkToAdd(items, nitem))
            items.Add(nitem);

        bool result = GUI.changed;
        GUI.changed = GUI.changed || origChanged;

        return result;
    }

    private static bool OkToAdd(List<ScriptableObject> items, ScriptableObject item)
    {
        if (!item)
        {
            Debug.LogError("Can't set item to none.");
            return false;
        }

        if (!(item is IInputBuildProcessor))
        {
            Debug.LogError(string.Format("{0} does not implement {1}."
                , item.name, typeof(IInputBuildProcessor).Name));
            return false;
        }

        if (items.Contains(item))
        {
            Debug.LogError(item.name + " already in the processor list.");
            return false;
        }

        IInputBuildProcessor pa = (IInputBuildProcessor)item;

        if (pa.DuplicatesAllowed)
            return true;

        foreach (ScriptableObject itemB in items)
        {
            IInputBuildProcessor pb = (IInputBuildProcessor)itemB;

            if (pb.DuplicatesAllowed)
                continue;

            System.Type ta = pb.GetType();
            System.Type tb = pa.GetType();

            if (ta.IsAssignableFrom(tb) || tb.IsAssignableFrom(ta))
            {
                Debug.LogError(string.Format(
                    "Disallowed dulicate detected. {0} and {1} are same type."
                    , pa.Name, pb.Name));
                return false;
            }
        }

        return true;
    }

    private static void OnGUIAdvanced(NavmeshBuild build)
    {
        EditorGUILayout.Separator();

        if (GUILayout.Button("Manage Build"))
            NavmeshBuildManager.OpenWindow();

        //// Only allow a reset when not in the buildable state.
        if (build.BuildState != NavmeshBuildState.Inactive)
        {
            GUI.enabled = true;
            EditorGUILayout.Separator();
            if (GUILayout.Button("Reset Build"))
                build.ResetBuild();
        }
    }

    private static void OnGUIStandard(NavmeshBuild build)
    {
        EditorGUILayout.Separator();

        GUI.enabled = (build.BuildState != NavmeshBuildState.Invalid);

        if (GUILayout.Button("Build & Bake"))
        {
            NavmeshBuildHelper helper = new NavmeshBuildHelper(build);
            helper.Build();
        }

        EditorGUILayout.Separator();

        bool origChanged = GUI.changed;

        UnityBuildContext.TraceEnabled = 
            EditorGUILayout.Toggle("Trace", UnityBuildContext.TraceEnabled);

        GUI.changed = origChanged;

        EditorGUILayout.Separator();

        GUI.enabled = true;

        bool orig = mShowNMGenConfig;
        mShowNMGenConfig = EditorGUILayout.Foldout(mShowNMGenConfig, "NMGen Configuration");
        if (orig != mShowNMGenConfig)
            EditorPrefs.SetBool(ShowNMGenKey, mShowNMGenConfig);

        if (mShowNMGenConfig)
        {
            EditorGUILayout.Separator();

            NMGenConfig config = build.Config;

            EditorGUILayout.Separator();

            NMGenConfigControl.OnGUIButtons(build, config, true);

            GUI.enabled = (build.BuildState != NavmeshBuildState.Invalid);

            if (GUILayout.Button("Derive"))
            {
                NavmeshBuildHelper helper = new NavmeshBuildHelper(build);

                InputAssets assets = helper.BuildInput();

                if (assets.geometry != null)
                {
                    TriangleMesh m = assets.geometry;

                    Vector3 bmin;
                    Vector3 bmax;

                    m.GetBounds(out bmin, out bmax);

                    config.Derive(bmin, bmax);
                    config.ApplyDecimalLimits();

                    GUI.changed = true;
                }
            }

            GUI.enabled = true;

            EditorGUILayout.Separator();

            orig = mShowPrimaryConfig;
            mShowPrimaryConfig = EditorGUILayout.Foldout(mShowPrimaryConfig, "Primary");
            if (orig != mShowPrimaryConfig)
                EditorPrefs.SetBool(ShowNMGenPriKey, mShowPrimaryConfig);

            if (mShowPrimaryConfig)
                NMGenConfigControl.OnGUIPrimary(build, config, true);

            EditorGUILayout.Separator();

            orig = mShowAdvancedConfig;
            mShowAdvancedConfig =
                EditorGUILayout.Foldout(mShowAdvancedConfig, "Advanced");
            if (orig != mShowAdvancedConfig)
                EditorPrefs.SetBool(ShowNMGenAdvKey, mShowAdvancedConfig);

            if (mShowAdvancedConfig)
                NMGenConfigControl.OnGUIAdvanced(config, true);

            if (GUI.changed)
                build.IsDirty = true;
        }
    }

    private static void OnGUIState(NavmeshBuild build)
    {
        INavmeshData btarget = build.BuildTarget;
        NavmeshBuildInfo binfo = (btarget == null ? null : btarget.BuildInfo);

        EditorGUILayout.Separator();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (build.BuildState == NavmeshBuildState.Invalid)
        {

            if (!(ScriptableObject)btarget)
                 sb.AppendLine("No build target.");

            if (build.inputProcessors.Count == 0)
                sb.AppendLine("No input processors.");

            GUILayout.Label(sb.ToString().Trim()
                , EditorUtil.ErrorStyle, GUILayout.ExpandWidth(true));

            return;
        }

        sb.AppendLine("Input Scene: " + NavEditorUtil.SceneDisplayName(binfo));

        if (build.SceneQuery == null)
            sb.AppendLine("Search scope: Entire scene");

        if (build.TargetHasNavmesh)
            sb.AppendLine("Target has existing navmesh.");
        else
            sb.AppendLine("Target does not have a navmesh.");

        if (build.BuildType == NavmeshBuildType.Advanced)
        {
            TileBuildData tdata = build.BuildData;
            sb.AppendLine("Build state: " + build.BuildState);
            sb.AppendLine("Active builds: " 
                + (tdata == null ? 0 : build.BuildData.GetActive()));
        }

        GUILayout.Label(sb.ToString().Trim()
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));


        if (NavEditorUtil.SceneMismatch(binfo))
        {
            GUILayout.Box("Current scene does not match last input scene."
                , EditorUtil.WarningStyle
                , GUILayout.ExpandWidth(true));
        }

        EditorGUILayout.Separator();

        GUI.enabled = (btarget != null);

        NavmeshSceneDraw.Instance.OnGUI(build.BuildTarget, "Show Mesh", true, true);

        GUI.enabled = true;

        return;
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Navmesh Build : Empty"
        , false
        , NMBEditorUtil.BuildGroup)]
    static void CreateEmptyAsset()
    {
        NavmeshBuild item = EditorUtil.CreateAsset<NavmeshBuild>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Navmesh Build : Standard"
        , false, NMBEditorUtil.BuildGroup)]
    static void CreateLoadedAsset()
    {
        NavmeshBuild item = EditorUtil.CreateAsset<NavmeshBuild>(NMBEditorUtil.AssetLabel);

        ScriptableObject child;

        child = EditorUtil.CreateAsset<MeshCompiler>(item, NMBEditorUtil.AssetLabel);
        item.inputProcessors.Add(child);

        CAIBakedNavmesh nm = 
            EditorUtil.CreateAsset<CAIBakedNavmesh>(item, NMBEditorUtil.AssetLabel);
        item.BuildTarget = nm;

        EditorUtility.SetDirty(item);

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
