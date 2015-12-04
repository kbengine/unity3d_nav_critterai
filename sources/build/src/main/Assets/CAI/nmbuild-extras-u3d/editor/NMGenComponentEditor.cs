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
using UnityEditor;

/// <summary>
/// Base editor for <see cref="NMGenComponent"/> objects.
/// </summary>
/// <exclude />
public class NMGenComponentEditor
    : Editor
{
    /// <summary>
    /// Run when the editor is disabled. (Goes out of scope.)
    /// </summary>
    protected virtual void OnDisable()
    {
        if (target)
            ((NMGenComponent)target).debugEnabledLocal = false;
    }

    /// <summary>
    /// Toggles the debug visualizations for all <see cref="NMGenComponent"/> objects.
    /// </summary>
    [MenuItem(EditorUtil.ViewMenu + "Toggle NMGen Extras", false, EditorUtil.ViewGroup)]
    public static void ToggleDebugView()
    {
        NMGenComponent.debugEnabled = !NMGenComponent.debugEnabled;
        SceneView.RepaintAll();
    }
}
