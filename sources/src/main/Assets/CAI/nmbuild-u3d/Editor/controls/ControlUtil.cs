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

namespace org.critterai.nmbuild.u3d.editor
{
	internal static class ControlUtil
	{
        public const float MarginSize = 5;
        public const float ButtonAreaWidth = 150;

        public static Color StandardHighlight
        {
            get { return new Color(0.43f, 0.77f, 0.13f); } // Green
        }

        public static Color SelectionColor
        {
            get { return new Color(0.84f, 0.75f, 0.08f); } // Yellow
        }

        private static GUIStyle mHighlightedButton;

        public static GUIStyle HighlightedButton
        {
            get
            {
                if (mHighlightedButton == null)
                {
                    mHighlightedButton = new GUIStyle(GUI.skin.button);
                    mHighlightedButton.normal.textColor = StandardHighlight;
                }
                return mHighlightedButton;
            }
        }

        public static bool OnGUIStandardButtons(ControlContext context
            , DebugViewContext debugContext
            , bool resetAllowed)
        {
            NavmeshBuild build = context.Build;

            if (!build)
                return false;

            TileBuildData tdata = build.BuildData;

            GUILayout.FlexibleSpace();

            // Note: It is assumed that you should't get any debug display options unless
            // you can reset the build.  So they are inside this condition.
            if (resetAllowed)
            {
                if (build.BuildState == NavmeshBuildState.Buildable || build.HasInputData)
                    // One or more debug display options are allowed.
                    GUILayout.Label("Show");

                // Always call these.
                debugContext.OnGUIMeshDisplayOptions();
                debugContext.OnGUIDebugExtras();

                GUILayout.Space(MarginSize);

                GUIStyle style = (tdata != null && tdata.NeedsBakingCount() == 0)
                    ? ControlUtil.HighlightedButton : GUI.skin.button;

                return OnGUIResetButton(context, debugContext, style);
            }

            return false;
        }

        public static bool OnGUIResetButton(ControlContext context, DebugViewContext debug, GUIStyle style)
        {
            if (GUILayout.Button("Exit Build", style))
            {
                context.AbortAllReqests("User requested build reset.");
                context.Build.ResetBuild();  // Abort requests first!
                debug.NeedsRepaint = true;
                return true;
            }
            GUILayout.Space(MarginSize);
            return false;
        }

        public static void BeginButtonArea(Rect area)
        {
            GUILayout.BeginArea(area, GUI.skin.box);
        }

        public static void EndButtonArea()
        {
            GUILayout.EndArea();
        }
	}
}
