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
/// Provides data useful for marking an area using a cylinder shape.
/// </summary>
/// <remarks>
/// <para>
/// The data is in a format compatible with the CompactHeightfield.MarkCylinderArea() method.
/// </para>
/// <para>
/// The y- and x-scales are used to size the cylinder.  The z-scale is ignored.
/// </para>
/// <para>
/// Rotation is ignored.
/// </para>
/// </remarks>
[ExecuteInEditMode]
public sealed class CylinderAreaMarker
    : NMGenAreaMarker
{
    /// <summary>
    /// Cylinder marker data.
    /// </summary>
    public struct MarkerData
    {
        /// <summary>
        /// The area to apply.
        /// </summary>
        public byte area;

        /// <summary>
        /// The marker priority.
        /// </summary>
        public int priority;

        /// <summary>
        /// The world center point of the base of the cylinder.
        /// </summary>
        public Vector3 centerBase;

        /// <summary>
        /// The world radius of the cylinder.
        /// </summary>
        public float radius;

        /// <summary>
        /// The world height of the cylinder, relative to its base.
        /// </summary>
        public float height;
    }

    /// <summary>
    /// The marker data.
    /// </summary>
    /// <returns>The marker data.</returns>
    public MarkerData GetMarkerData()
    {
        MarkerData result = new MarkerData();

        Transform trans = transform;
        Vector3 scale = trans.localScale;

        result.area = Area;
        result.priority = Priority;
        result.centerBase = trans.position - Vector3.up * scale.y * 0.5f;
        result.radius = scale.x;
        result.height = scale.y;

        return result;
    }

    void OnRenderObject()
    {
        if (!debugEnabled && !debugEnabledLocal)
            return;

        // The color for area zero is hard to see. So using black.
        Color color = (Area == 0 ? new Color(0, 0, 0, 0.8f) : ColorUtil.IntToColor(Area, 0.8f));

        Transform trans = transform;
        Vector3 scale = trans.localScale;

        DebugDraw.Cylinder(trans.position - Vector3.up * scale.y * 0.5f
            , scale.x, scale.y, false, color);
    }
}
