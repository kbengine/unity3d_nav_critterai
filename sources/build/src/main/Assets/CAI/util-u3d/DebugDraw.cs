/*
 * Copyright (c) 2011-2012 Stephen A. Pratt
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

namespace org.critterai.u3d
{
    /// <summary>
    /// Provides various GL-based drawing methods suitable for debug views.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Append methods require the caller to handle all GL setup and teardown. Unless otherwise
    /// noted they expect GL to be in GL.Lines mode.
    /// </para>
    /// <code>
    /// // Example: Using append methods:
    /// 
    /// Vector3 startPos = new Vector3(1, 0, 0);
    /// Vector3 endPos = new Vector3(10, 15, 20);
    /// float scale = 1;
    /// float height = 5;
    /// 
    /// DebugDraw.SimpleMaterial.SetPass(0);
    ///
    /// GL.Begin(GL.LINES);
    /// 
    /// GL.Color(Color.green);
    /// DebugDraw.AppendXMarker(startPos, scale);
    /// DebugDraw.AppendArc(startPos, endPos, height, scale * 0, scale * 0.1f);
    /// 
    /// GL.Color(Color.red)
    /// DebugDraw.AppendXMarker(endPos, scale);
    ///
    /// GL.End();
    /// </code>
    /// </remarks>
    public static class DebugDraw
    {
        private const float DarkenFactor = 2;

        private const int CircleSegments = 40;
        private const float Epsilon = 0.001f;

        private static float[] mDir;

        private static float[] Dir
        {
            get
            {
                if (mDir == null)
                {
                    mDir = new float[CircleSegments * 2];
                    for (int i = 0; i < CircleSegments; i++)
                    {
                        float a = 
                            (float)i / (float)CircleSegments * Mathf.PI * 2;
                        mDir[i * 2] = Mathf.Cos(a);
                        mDir[i * 2 + 1] = Mathf.Sin(a);
                    }
                }
                return mDir;
            }
        }

        private static Material mSimpleMaterial = null;

        /// <summary>
        /// A shared material suitable for simple drawing operations. 
        /// </summary>
        public static Material SimpleMaterial
        {
            get
            {
                if (!mSimpleMaterial)
                {
                    mSimpleMaterial = new Material(
                        "Shader \"Lines/Colored Blended\" {"
                        + "SubShader { Pass { "
                        + "	BindChannels { Bind \"Color\",color } "
                        + "	Blend SrcAlpha OneMinusSrcAlpha "
                        + "	ZWrite Off Cull Off Fog { Mode Off } "
                        + "} } }");
                    mSimpleMaterial.hideFlags = HideFlags.HideAndDontSave;
                    mSimpleMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
                }
                return mSimpleMaterial;
            }
        }

        /// <summary>
        /// Draw a diamond shape at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <paramref name="scale"/> of 1.0 will result in a diamond with a width and height of 
        /// 2.0. (Each diamond point will be 1.0 units from the position.)
        /// </para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="scale">The scale of the diamond.</param>
        /// <param name="color">The surface color of the diamond.</param>
        public static void DiamondMarker(Vector3 position, float scale, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            Vector3 u = position + Vector3.up * scale;
            Vector3 d = position + Vector3.down * scale;
            Vector3 r = position + Vector3.right * scale;
            Vector3 l = position + Vector3.left * scale;
            Vector3 f = position + Vector3.forward * scale;
            Vector3 b = position + Vector3.back * scale;

            GL.Begin(GL.TRIANGLES);

            GL.Color(color);

            // Top

            GL.Vertex(u);
            GL.Vertex(r);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(f);
            GL.Vertex(l);

            GL.Vertex(u);
            GL.Vertex(l);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(b);
            GL.Vertex(r);

            // Bottom

            GL.Vertex(d);
            GL.Vertex(f);
            GL.Vertex(r);

            GL.Vertex(d);
            GL.Vertex(r);
            GL.Vertex(b);

            GL.Vertex(d);
            GL.Vertex(b);
            GL.Vertex(l);

            GL.Vertex(d);
            GL.Vertex(l);
            GL.Vertex(f);

            GL.End();

            GL.Begin(GL.LINES);

            color.a *= DarkenFactor;
            GL.Color(color);

            // Top

            GL.Vertex(u);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(r);

            GL.Vertex(u);
            GL.Vertex(l);

            // Bottom

            GL.Vertex(u);
            GL.Vertex(f);

            GL.Vertex(u);
            GL.Vertex(b);

            GL.Vertex(u);
            GL.Vertex(r);

            GL.Vertex(u);
            GL.Vertex(l);

            // Center

            GL.Vertex(r);
            GL.Vertex(f);

            GL.Vertex(r);
            GL.Vertex(b);

            GL.Vertex(l);
            GL.Vertex(f);

            GL.Vertex(l);
            GL.Vertex(b);

            GL.End();
        }

        /// <summary>
        /// Draw an X-marker at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <paramref name="scale"/> of 1.0 will result in a marker with a width and height of 
        /// 2.0. (Each point of the x-marker will be 1.0 from the position.)
        /// </para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="scale">The scale of the marker.</param>
        /// <param name="color">The color of the marker.</param>
        public static void XMarker(Vector3 position
            , float scale
            , Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendXMarker(position, scale);

            GL.End();
        }

        /// <summary>
        /// Draw an X-marker at the specified position.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <paramref name="scale"/> of 1.0 will result in a marker with a width and height of 
        /// 2.0. (Each point of the x-marker will be 1.0 from the position.)
        /// </para>
        /// </remarks>
        /// <param name="position">The position.</param>
        /// <param name="scale">The scale of the marker.</param>
        public static void AppendXMarker(Vector3 position, float scale)
        {
            GL.Vertex(position + Vector3.right * scale);
            GL.Vertex(position + Vector3.left * scale);
            GL.Vertex(position + Vector3.forward * scale);
            GL.Vertex(position + Vector3.back * scale);
            GL.Vertex(position + Vector3.up * scale * 2);
            GL.Vertex(position + Vector3.down * scale * 2);
        }

        /// <summary>
        /// Append a filled convex polygon.
        /// </summary>
        /// <param name="verts">
        /// The polygon vertices. [Length: >= <paramref name="vertCount"/>]
        /// </param>
        /// <param name="vertCount">The number of vertices.</param>
        /// <param name="color">The color of the polygon surface.</param>
        public static void ConvexPoly(Vector3[] verts, int vertCount, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.TRIANGLES);

            GL.Color(color);

            for (int i = 2; i < vertCount; i++)
            {
                int pB = i - 1;
                int pC = i + 0;

                GL.Vertex(verts[0]);
                GL.Vertex(verts[pB]);
                GL.Vertex(verts[pC]);
            }

            GL.End();

            GL.Begin(GL.LINES);

            color.a *= DarkenFactor;
            GL.Color(color);

            for (int i = 1; i < vertCount; i++)
            {
                int pA = i - 1;
                int pB = i + 0;

                GL.Vertex(verts[pA]);
                GL.Vertex(verts[pB]);
            }

            GL.End();
        }


        /// <summary>
        /// Draw a wireframe axis-aligned bounding box.
        /// </summary>
        /// <param name="boundsMin">The minimum bounds.</param>
        /// <param name="boundsMax">The maximum bounds.</param>
        /// <param name="color">The wireframe color</param>
        public static void Bounds(Vector3 boundsMin, Vector3 boundsMax, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendBounds(boundsMin, boundsMax);

            GL.End();
        }

        /// <summary>
        /// Append a wireframe axis-aligned bounding box.
        /// </summary>
        /// <param name="boundsMin">The minimum bounds.</param>
        /// <param name="boundsMax">The maximum bounds.</param>
        public static void AppendBounds(Vector3 boundsMin, Vector3 boundsMax)
        {
            Vector3 ua = boundsMax;
            Vector3 ub = boundsMax;
            Vector3 uc = boundsMax;
            Vector3 ud = boundsMax;
            ub.x = boundsMin.x;
            uc.x = boundsMin.x;
            uc.z = boundsMin.z;
            ud.z = boundsMin.z;

            Vector3 la = boundsMin;
            Vector3 lb = boundsMin;
            Vector3 lc = boundsMin;
            Vector3 ld = boundsMin;

            la.x = boundsMax.x;
            la.z = boundsMax.z;
            lb.z = boundsMax.z;
            ld.x = boundsMax.x;

            // Top
            GL.Vertex(ua);
            GL.Vertex(ub);
            GL.Vertex(ub);
            GL.Vertex(uc);
            GL.Vertex(uc);
            GL.Vertex(ud);
            GL.Vertex(ud);
            GL.Vertex(ua);

            // Bottom
            GL.Vertex(la);
            GL.Vertex(lb);
            GL.Vertex(lb);
            GL.Vertex(lc);
            GL.Vertex(lc);
            GL.Vertex(ld);
            GL.Vertex(ld);
            GL.Vertex(la);

            // Risers
            GL.Vertex(ua);
            GL.Vertex(la);
            GL.Vertex(ub);
            GL.Vertex(lb);
            GL.Vertex(uc);
            GL.Vertex(lc);
            GL.Vertex(ud);
            GL.Vertex(ld);
        }

        /// <summary>
        /// Draw a wireframe axis-aligned bounding box representing the extents.
        /// </summary>
        /// <param name="position">The center of the bounding box.</param>
        /// <param name="extents">The extents of the bounding box. (Half-lengths.)</param>
        /// <param name="color">The wireframe color.</param>
        public static void Extents(Vector3 position, Vector3 extents, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendExtents(position, extents);

            GL.End();
        }

        /// <summary>
        /// Append a wireframe axis-aligned bounding box representing the extents.
        /// </summary>
        /// <param name="position">The center of the bounding box.</param>
        /// <param name="extents">The extents of the bounding box. (Half-lengths.)</param>
        public static void AppendExtents(Vector3 position, Vector3 extents)
        {
            Vector3 ua = position + extents;
            Vector3 ub = new Vector3(
                  position.x - extents.x
                , position.y + extents.y
                , position.z + extents.z);
            Vector3 uc = new Vector3(
                  position.x - extents.x
                , position.y + extents.y
                , position.z - extents.z);
            Vector3 ud = new Vector3(
                  position.x + extents.x
                , position.y + extents.y
                , position.z - extents.z);

            Vector3 la = new Vector3(
                  position.x + extents.x
                , position.y - extents.y
                , position.z + extents.z);
            Vector3 lb = new Vector3(
                  position.x - extents.x
                , position.y - extents.y
                , position.z + extents.z);
            Vector3 lc = position - extents;
            Vector3 ld = new Vector3(
                  position.x + extents.x
                , position.y - extents.y
                , position.z - extents.z);

            // Top
            GL.Vertex(ua);
            GL.Vertex(ub);
            GL.Vertex(ub);
            GL.Vertex(uc);
            GL.Vertex(uc);
            GL.Vertex(ud);
            GL.Vertex(ud);
            GL.Vertex(ua);

            // Bottom
            GL.Vertex(la);
            GL.Vertex(lb);
            GL.Vertex(lb);
            GL.Vertex(lc);
            GL.Vertex(lc);
            GL.Vertex(ld);
            GL.Vertex(ld);
            GL.Vertex(la);

            // Risers
            GL.Vertex(ua);
            GL.Vertex(la);
            GL.Vertex(ub);
            GL.Vertex(lb);
            GL.Vertex(uc);
            GL.Vertex(lc);
            GL.Vertex(ud);
            GL.Vertex(ld);
        }

        /// <summary>
        /// Draw a simple wireframe cylinder with four risers.
        /// </summary>
        /// <param name="position">The center point of the cylinder base.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="height">The cylinder height, relative to the base.</param>
        /// <param name="includeHalf">
        /// True if a circle should be drawn halfway from the base to the top of the cyclinder.
        /// </param>
        /// <param name="color">The wireframe color.</param>
        public static void Cylinder(Vector3 position, float radius, float height
            , bool includeHalf
            , Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendCylinder(position, radius, height, includeHalf);

            GL.End();
        }

        /// <summary>
        /// Append a simple wireframe cylinder with four risers.
        /// </summary>
        /// <param name="position">The center point of the cylinder base.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="height">The cylinder height, relative to the base.</param>
        /// <param name="includeHalf">
        /// True if a circle should be drawn halfway from the base to the top of the cyclinder.
        /// </param>
        public static void AppendCylinder(Vector3 position, float radius, float height
            , bool includeHalf)
        {
            AppendCircle(position, radius);
            AppendCircle(position + Vector3.up * height, radius);

            if (includeHalf)
                AppendCircle(position + Vector3.up * height * 0.5f, radius);

            float[] dir = Dir;

            for (int i = 0, j = CircleSegments - 1; i < CircleSegments; j = i += 5)
            {
                float x = position.x + dir[j * 2 + 0] * radius;
                float z = position.z + dir[j * 2 + 1] * radius;
                GL.Vertex3(x, position.y, z);
                GL.Vertex3(x, position.y + height, z);
            }
        }

        /// <summary>
        /// Draw an unfilled circle on the xz-plane.
        /// </summary>
        /// <param name="position">The center position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="color">The color.</param>
        public static void Circle(Vector3 position, float radius, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendCircle(position, radius);

            GL.End();
        }

        /// <summary>
        /// Append an unfilled circle on the xz-plane.
        /// </summary>
        /// <param name="position">The center position.</param>
        /// <param name="radius">The radius.</param>
        public static void AppendCircle(Vector3 position, float radius)
        {
            float[] dir = Dir;

            for (int i = 0, j = CircleSegments - 1; i < CircleSegments; j = i++)
            {
                GL.Vertex3(position.x + dir[j * 2 + 0] * radius
                    , position.y
                    , position.z + dir[j * 2 + 1] * radius);
                GL.Vertex3(position.x + dir[i * 2 + 0] * radius
                    , position.y
                    , position.z + dir[i * 2 + 1] * radius);
            }
        }

        /// <summary>
        /// Draw an arrow.
        /// </summary>
        /// <param name="pointA">Endpoint A.</param>
        /// <param name="pointB">Endpoint B.</param>
        /// <param name="headScaleA">
        /// The scale of endpoint A arrow head. (Or zero for no head.)
        /// </param>
        /// <param name="headScaleB">
        /// The scale of the endpoint B arrow head.(Or zero for no head.)
        /// </param>
        /// <param name="color">The color.</param>
        public static void Arrow(Vector3 pointA, Vector3 pointB
            , float headScaleA, float headScaleB
            , Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendArrow(pointA, pointB, headScaleA, headScaleB);

            GL.End();
        }

        /// <summary>
        /// Append an arrow.
        /// </summary>
        /// <param name="pointA">Endpoint A.</param>
        /// <param name="pointB">Endpoint B.</param>
        /// <param name="headScaleA">
        /// The scale of endpoint A arrow head. (Or zero for no head.)
        /// </param>
        /// <param name="headScaleB">
        /// The scale of the endpoint B arrow head.(Or zero for no head.)
        /// </param>
        public static void AppendArrow(Vector3 pointA, Vector3 pointB
            , float headScaleA, float headScaleB)
        {
            GL.Vertex(pointA);
            GL.Vertex(pointB);

            if (headScaleA > Epsilon)
                AppendArrowHead(pointA, pointB, headScaleA);
            if (headScaleB > Epsilon)
                AppendArrowHead(pointB, pointA, headScaleB);
        }

        private static void AppendArrowHead(Vector3 start
            , Vector3 end
            , float headScale)
        {
            if (Vector3.SqrMagnitude(end - start) < Epsilon * Epsilon)
                return;

            Vector3 az = (end - start).normalized;
            //Vector3 ax = Vector3.Cross(Vector3.up, az);
            //// Vector3 ay = Vector3.Cross(az, ax).normalized;

            Vector3 vbase = start + az * headScale;
            Vector3 offset = Vector3.Cross(Vector3.up, az) * headScale * 0.333f;

            GL.Vertex(start);
            GL.Vertex(vbase + offset);

            GL.Vertex(start);
            GL.Vertex(vbase - offset);
        }

        /// <summary>
        /// Draw a triangle mesh.
        /// </summary>
        /// <param name="verts">The vertices.</param>
        /// <param name="tris">
        /// The triangle indices. 
        /// [(vertAIndex, vertBIndex, vertCIndex) * <paramref name="triCount"/>]
        /// </param>
        /// <param name="triCount">The number of triangles.</param>
        /// <param name="includeEdges">True if the triangle edges should be drawn.</param>
        /// <param name="color">The surface color.</param>
        public static void TriangleMesh(Vector3[] verts, int[] tris, int triCount
            , bool includeEdges
            , Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            int length = triCount * 3;

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);

            for (int p = 0; p < length; p += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex(verts[tris[p + i]]);
                }
            }
            GL.End();

            if (!includeEdges)
                return;

            color.a *= DarkenFactor;
            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int p = 0; p < length; p += 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    GL.Vertex(verts[tris[p + i]]);
                }
                GL.Vertex(verts[tris[p]]);
            }

            GL.End();
        }

        /// <summary>
        /// Draw a triangle mesh, colored per triangle.
        /// </summary>
        /// <param name="verts">The vertices.</param>
        /// <param name="tris">
        /// The triangle indices. 
        /// [(vertAIndex, vertBIndex, vertCIndex) * <paramref name="triCount"/>]
        /// </param>
        /// <param name="areas">The color area assigned to the triangle.</param>
        /// <param name="triCount">The number of triangles.</param>
        /// <param name="includeEdges">True if the triangle edges should be drawn.</param>
        /// <param name="surfaceAlpha">The surface alpha.</param>
        public static void TriangleMesh(Vector3[] verts
            , int[] tris, byte[] areas, int triCount
            , bool includeEdges
            , float surfaceAlpha)
        {
            DebugDraw.SimpleMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);

            for (int i = 0; i < triCount; i++)
            {
                GL.Color(ColorUtil.IntToColor(areas[i], surfaceAlpha));
                for (int j = 0; j < 3; j++)
                {
                    GL.Vertex(verts[tris[i * 3 + j]]);
                }
            }

            GL.End();
            GL.Begin(GL.LINES);

            Color c = Color.gray;
            c.a = surfaceAlpha * DarkenFactor;

            for (int i = 0; i < triCount; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    GL.Vertex(verts[tris[i * 3 + j]]);
                }

                GL.Vertex(verts[tris[i * 3]]);
            }

            GL.End();
        }

        /// <summary>
        /// Draw an axis aligned rectangle on the xz-plane.
        /// </summary>
        /// <param name="origin">The mimimum origin.</param>
        /// <param name="width">The width along the x-axis.</param>
        /// <param name="depth">The depth along the z-axis.</param>
        /// <param name="color">The color.</param>
        /// <param name="fill">True if the rectangle should be filled.</param>
        public static void Rect(Vector3 origin, float width, float depth, Color color, bool fill)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            Vector3 a = origin;
            Vector3 b = origin;
            Vector3 c = origin;
            Vector3 d = origin;

            b.x += width;
            c.x += width;
            c.z += depth;
            d.z += depth;

            if (fill)
            {
                GL.Begin(GL.TRIANGLES);
                GL.Color(color);

                GL.Vertex(a);
                GL.Vertex(b);
                GL.Vertex(c);

                GL.Vertex(a);
                GL.Vertex(c);
                GL.Vertex(d);

                GL.End();

                color.a *= DarkenFactor;
            }

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

            GL.End();
        }

        /// <summary>
        /// Draws a simple arc.
        /// </summary>
        /// <param name="pointA">Endpoint A.</param>
        /// <param name="pointB">Endpoint B.</param>
        /// <param name="height">The relative height of the arc.</param>
        /// <param name="headScaleA">
        /// The scale endpoint A arrow head. (Or zero for no head.)
        /// </param>
        /// <param name="headScaleB">
        /// The scale of endpoint B arrow head.(Or zero for no head.)
        /// </param>
        /// <param name="color">The color.</param>
        public static void Arc(Vector3 pointA, Vector3 pointB, float height
            , float headScaleA, float headScaleB
            , Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            AppendArc(pointA, pointB, height, headScaleA, headScaleB);

            GL.End();
        }

        /// <summary>
        /// Appends a simple arc.
        /// </summary>
        /// <param name="pointA">Endpoint A.</param>
        /// <param name="pointB">Endpoint B.</param>
        /// <param name="height">The relative height of the arc.</param>
        /// <param name="headScaleA">
        /// The scale of the arrow head for point A. (Or zero for no head.)
        /// </param>
        /// <param name="headScaleB">
        /// The scale of the arrow head for point B.(Or zero for no head.)
        /// </param>
        public static void AppendArc(Vector3 pointA, Vector3 pointB, float height
            , float headScaleA, float headScaleB)
        {
	        const int ArcPointCount = 8;
	        const float Pad = 0.05f;
	        const float ArcPointsScale = (1.0f - Pad  *2) / ArcPointCount;

            Vector3 delta = pointB - pointA;
	        float len = delta.magnitude;

	        Vector3 prev = EvalArc(pointA, delta, len*height, Pad);

	        for (int i = 1; i <= ArcPointCount; ++i)
	        {
		        float u = Pad + i * ArcPointsScale;

		        Vector3 pt = EvalArc(pointA, delta, len*height, u);

                GL.Vertex(prev);
                GL.Vertex(pt);

                prev = pt;
	        }

	        // End arrows
	        if (headScaleA > 0.001f)
	        {
		        Vector3 p = EvalArc(pointA, delta, len * height, Pad);
                Vector3 q = EvalArc(pointA, delta, len * height, Pad + 0.05f);
                AppendArrowHead(p, q, headScaleA);
	        }

	        if (headScaleB > 0.001f)
	        {
                Vector3 p = EvalArc(pointA, delta, len * height, 1 - Pad);
                Vector3 q = EvalArc(pointA, delta, len * height, 1 - (Pad + 0.05f));
                AppendArrowHead(p, q, headScaleB);
	        }
        }

        private static Vector3 EvalArc(Vector3 point, Vector3 delta, float h, float u)
        {
            return new Vector3(point.x + delta.x * u
                , point.y + delta.y * u + h * (1 - (u * 2 - 1) * (u * 2 - 1))
                , point.z + delta.z * u);

        }

        /// <summary>
        /// Draw a simple wireframe grid on the xz-plane.
        /// </summary>
        /// <param name="origin">The origin of the grid.</param>
        /// <param name="gridSize">The grid cell size. [Limit: > 0]</param>
        /// <param name="width">The number of cells along the x-axis.</param>
        /// <param name="depth">The number of cells along the z-axis.</param>
        /// <param name="color">The color.</param>
        public static void Grid(Vector3 origin, float gridSize, int width, int depth, Color color)
        {
            DebugDraw.SimpleMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(color);

            for (int x = 0; x < width + 1; x++)
            {
                Vector3 a = origin;

                a.x += x * gridSize;
                GL.Vertex(a);

                a.z += depth * gridSize;
                GL.Vertex(a);
            }

            for (int z = 0; z < depth + 1; z++)
            {
                Vector3 a = origin;

                a.z += z * gridSize;
                GL.Vertex(a);

                a.x += width * gridSize;
                GL.Vertex(a);
            }

            GL.End();
        }
    }
}
