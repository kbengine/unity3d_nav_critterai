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
using org.critterai.geom;
using org.critterai.nmbuild.u3d.editor;
using org.critterai.u3d;

/// <summary>
/// <see cref="TerrainCompiler"/> editor.
/// </summary>
/// <exclude />
[CustomEditor(typeof(TerrainCompiler))]
public sealed class TerrainCompilerEditor
    : Editor
{
    private const float CheckDelay = 5;

    private static bool mDebugEnabled;
    private static int mDebugZoneSize = 10;
    private static float mDebugOffset = 0;

    private Terrain mDebugTerrain = null;
    private Vector3 mDebugPosition;

    private double mLastCheck = 0;
    private TerrainData mLastSource = null;

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    /// <summary>
    /// Controls behavior of the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        TerrainCompiler targ = (TerrainCompiler)target;

        EditorGUILayout.Separator();

        GUILayout.Label("Priority: " + targ.Priority);

        EditorGUILayout.Separator();

        targ.terrainData = (TerrainData)EditorGUILayout.ObjectField("Terrain Data"
            , targ.terrainData
            , typeof(TerrainData)
            , false);

        targ.includeTrees = EditorGUILayout.Toggle("Include Trees", targ.includeTrees);

        float res = EditorGUILayout.Slider("Resolution (%)", targ.Resolution * 100, 0.01f, 100);

        targ.Resolution = (float)System.Math.Round(Mathf.Max(0.001f, res * 0.01f), 3);

        EditorGUILayout.Separator();

        OnGUIDebug(targ);

        GUI.enabled = true;

        EditorGUILayout.Separator();

        GUILayout.Box("Input Build Processor\n\nLoads and compiles a " 
            + typeof(Terrain).Name + " component if it references the specified " 
            + typeof(TerrainData).Name + " object."
            , EditorUtil.HelpStyle
            , GUILayout.ExpandWidth(true));

        EditorGUILayout.Separator();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void OnGUIDebug(TerrainCompiler targ)
    {
        // Getting the terrain requires a full terrain search.  So only check
        // for it occationally.
        if (mDebugEnabled)
        {
            double time = EditorApplication.timeSinceStartup;
            if (targ.terrainData != mLastSource || mLastCheck + CheckDelay > time)
            {
                Terrain terrain = targ.GetTerrain();
                mLastSource = targ.terrainData;
                mLastCheck = time;
                if (mDebugTerrain != terrain)
                {
                    mDebugTerrain = terrain;
                    SceneView.RepaintAll();
                }
            }
        }
        else
        {
            mDebugTerrain = null;
            mLastCheck = 0;
            mLastSource = null;
        }

        EditorGUILayout.Separator();

        GUI.enabled = targ.terrainData;

        bool origChanged = GUI.changed;

        mDebugEnabled = EditorGUILayout.Toggle("Enable Preview", mDebugEnabled);

        if (!mDebugEnabled || !targ.terrainData)
            return;

        GUILayout.Label("Size");
        mDebugZoneSize = (int)GUILayout.HorizontalSlider(mDebugZoneSize, 10, 50);

        GUILayout.Label("Offset");
        mDebugOffset = GUILayout.HorizontalSlider(mDebugOffset, 0, 10);

        EditorGUILayout.Separator();

        string helpText;
        if (mDebugTerrain == null)
            helpText = "There is no enabled terrain in the scene using the source terrain data.";
        else
        {
            int xc;
            int zc;
            Vector3 scale = TerrainUtil.DeriveScale(mDebugTerrain, targ.Resolution, out xc, out zc);
            int triCount = (xc - 1) * (zc - 1) * 2;

            helpText = string.Format("Mouse-over the terrain in the scene to see a "
                + " triangulation preview. Trees are not included in the preview.\n\n"
                + "Total surface triangles: {0:N0}\n"
                + "Sample distance: {1:F1} x {2:F1}"
                , triCount, scale.x, scale.z);
        }

        GUILayout.Box(helpText, EditorUtil.HelpStyle, GUILayout.ExpandWidth(true));

        if (GUI.changed)
            SceneView.RepaintAll();

        GUI.changed = origChanged;

    }

    void OnSceneGUI(SceneView view)
    {
        TerrainCompiler targ = (TerrainCompiler)target;

        if (!mDebugEnabled || mDebugTerrain == null)
            // Nothing to do.
            return;

        Vector3 mousePos = Event.current.mousePosition;
        Camera cam = Camera.current;

        Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, -mousePos.y + cam.pixelHeight));

        Vector3 point = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            Terrain terrain = hit.collider.gameObject.GetComponent<Terrain>();
            if (terrain == mDebugTerrain)
                point = hit.point;
        }

        if (mDebugPosition != point)
        {
            mDebugPosition = point;
            SceneView.RepaintAll();
        }

        if (mDebugPosition == Vector3.zero)
            return;

        Color c = Color.yellow;
        c.a = 0.25f;

        int trash;
        Vector3 scale = TerrainUtil.DeriveScale(mDebugTerrain, targ.Resolution, out trash, out trash);

        float xmin = mDebugPosition.x - scale.x * mDebugZoneSize;
        float zmin = mDebugPosition.z - scale.z * mDebugZoneSize;
        float xmax = mDebugPosition.x + scale.x * mDebugZoneSize;
        float zmax = mDebugPosition.z + scale.z * mDebugZoneSize;

        TriangleMesh mesh = TerrainUtil.TriangulateSurface(mDebugTerrain
            , xmin, zmin, xmax, zmax
            , targ.Resolution
            , mDebugOffset);

        if (mesh != null)
            DebugDraw.TriangleMesh(mesh.verts, mesh.tris, mesh.triCount, true, c);
    }

    [MenuItem(EditorUtil.NMGenAssetMenu + "Compiler : Terrain", false, NMBEditorUtil.CompilerGroup)]
    static void CreateAsset()
    {
        TerrainCompiler item = EditorUtil.CreateAsset<TerrainCompiler>(NMBEditorUtil.AssetLabel);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = item;
    }
}
