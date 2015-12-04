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
/// Represents an off-mesh connection used during the NMGen build process.
/// </summary>
[ExecuteInEditMode]
public sealed class OFMConnection
    : NMGenComponent 
{
    [SerializeField]
    private Vector3 mEnd;

    [SerializeField]
    private float mRadius = 1.0f;
    [SerializeField]
    private bool mIsBidirectional;

    [SerializeField]
    private bool mOverrideArea;

    [SerializeField]
    private byte mArea = MaxArea;

    [SerializeField]
    private int mUserId;

    [SerializeField]
    private int mFlags = 1;

    /// <summary>
    /// The connection start point.
    /// </summary>
    public Vector3 StartPoint { get { return transform.position; } }

    /// <summary>
    /// The connection end point.
    /// </summary>
    public Vector3 EndPoint
    {
        get { return mEnd; }
        set { mEnd = value; }
    }

    /// <summary>
    /// The connection point radii.
    /// </summary>
    public float Radius
    {
        get { return mRadius; }
        set { mRadius = Mathf.Max(0, value); }
    }

    /// <summary>
    /// True if the conneciton can be traversed in both directions, or false if only traversable
    /// from the start to the end point.
    /// </summary>
    public bool IsBidirectional
    {
        get { return mIsBidirectional; }
        set { mIsBidirectional = value; }
    }

    /// <summary>
    /// True if the locally defined area should be applied to the connection, or false if
    /// the build's default area should be used.
    /// </summary>
    public bool OverrideArea
    {
        get { return mOverrideArea; }
        set { mOverrideArea = value; }
    }

    /// <summary>
    /// The area to apply to the connection.
    /// </summary>
    public byte Area
    {
        get { return mArea; }
        set { mArea = ClampArea(value); }
    }

    /// <summary>
    /// The connection flags.
    /// </summary>
    public ushort Flags
    {
        get { return (ushort)mFlags; }
        set{ mFlags = value; }
    }

    /// <summary>
    /// The connection user id. [Limit: >= 0]
    /// </summary>
    public int UserId
    {
        get { return mUserId; }
        set { mUserId = Mathf.Max(0, value); }
    }

    void OnRenderObject()
    {
        if (!debugEnabled && !debugEnabledLocal)
            return;

        Vector3 pos = transform.position;

        const float markerScale = 0.5f;

        // The color for area zero is hard to see. So using black.
        Color color = (Area == 0 ? new Color(0, 0, 0, 0.8f) : ColorUtil.IntToColor(Area, 0.8f));

        DebugDraw.SimpleMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        GL.Color(color);

        DebugDraw.AppendXMarker(pos, markerScale);
        DebugDraw.AppendCircle(pos, mRadius);

        DebugDraw.AppendArc(pos, mEnd, 0.25f
            , (mIsBidirectional ? 0.6f : 0)
            , 0.6f);

        DebugDraw.AppendCircle(mEnd, mRadius);

        GL.End();
    }
}
