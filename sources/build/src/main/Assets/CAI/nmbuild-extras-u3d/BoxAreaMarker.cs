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
using org.critterai.u3d;

/// <summary>
/// Provides data useful for marking an area using a box shaped convex polygon.
/// </summary>
/// <remarks>
/// <para>
/// The data is in a format compatible with the CompactHeightfield.MarkConvexPolyArea() method.
/// </para>
/// <para>
/// The box is projected onto the xz-plane.  Y-axis rotation will behave as expected. 
/// Compining non-zero x- and z-rotations will skew the box into a parallelogram shape.
/// </para>
/// </remarks>
[ExecuteInEditMode]
public sealed class BoxAreaMarker
    : NMGenAreaMarker
{
    /// <summary>
    /// Convex marker data.
    /// </summary>
    public struct MarkerData
    {
        /// <summary>
        /// The convex polygon vertices. [Length: 4]
        /// </summary>
        public Vector3[] verts;

        /// <summary>
        /// The world y-maximum.
        /// </summary>
        public float ymin;

        /// <summary>
        /// The world y-minimum.
        /// </summary>
        public float ymax;

        /// <summary>
        /// The area to apply.
        /// </summary>
        public byte area;

        /// <summary>
        /// The priority of the marker.
        /// </summary>
        public int priority;
    }

    /// <summary>
    /// Gets the marker data.
    /// </summary>
    /// <returns>The marker data.</returns>
    public MarkerData GetMarkerData()
    {
        Transform trans = transform;
        Vector3 pos = trans.position;
        Vector3 scale = trans.localScale;

        Vector3 fwd = trans.forward * scale.z;
        Vector3 right = trans.right * scale.x;

        Vector3[] verts = new Vector3[4];
        verts[0] = pos + fwd + right;
        verts[1] = pos + fwd - right;
        verts[2] = pos - fwd - right;
        verts[3] = pos - fwd + right;

        MarkerData result = new MarkerData();
        
        result.verts = verts;
        result.ymin = pos.y -trans.localScale.y;
        result.ymax = pos.y + trans.localScale.y;
        result.area = Area;
        result.priority = Priority;

        return result;
    }

    void OnRenderObject()
    {
        if (!debugEnabled && !debugEnabledLocal)
            return;

        // The color for area zero is hard to see. So using black.
        Color color = (Area == 0 ? new Color(0, 0, 0, 0.8f) : ColorUtil.IntToColor(Area, 0.8f));

        Transform trans = transform;
        Vector3 pos = trans.position;
        Vector3 scale = trans.localScale;

        Vector3 fwd = trans.forward * scale.z;
        Vector3 right = trans.right * scale.x;
        Vector3 up = Vector3.up * scale.y;
        
        Vector3 a = pos + fwd + right;
        Vector3 b = pos + fwd - right;
        Vector3 c = pos - fwd - right;
        Vector3 d = pos - fwd + right;

        // Flatten.
        a.y = pos.y - up.y;
        b.y = pos.y - up.y;
        c.y = pos.y - up.y;
        d.y = pos.y - up.y;

        DebugDraw.SimpleMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        GL.Color(color);

        GL.Vertex(a);
        GL.Vertex(b);
        GL.Vertex(b);
        GL.Vertex(c);
        GL.Vertex(c);
        GL.Vertex(d);
        GL.Vertex(d);
        GL.Vertex(a);

        GL.Vertex(a + up * 2);
        GL.Vertex(b + up * 2);
        GL.Vertex(b + up * 2);
        GL.Vertex(c + up * 2);
        GL.Vertex(c + up * 2);
        GL.Vertex(d + up * 2);
        GL.Vertex(d + up * 2);
        GL.Vertex(a + up * 2);

        GL.Vertex(a);
        GL.Vertex(a + up * 2);
        GL.Vertex(b);
        GL.Vertex(b + up * 2);
        GL.Vertex(c);
        GL.Vertex(c + up * 2);
        GL.Vertex(d);
        GL.Vertex(d + up * 2);

        GL.End();
    }
}
