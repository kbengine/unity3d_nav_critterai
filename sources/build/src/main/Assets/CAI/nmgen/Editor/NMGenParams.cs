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
using System;
using System.Runtime.InteropServices;

namespace org.critterai.nmgen
{
    /// <summary>
    /// Common configuration parameters used during the NMGen build process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class represents an aggregation of parameters used at different stages in the 
    /// build process.  Not all parameters are used in all builds.
    /// </para>
    /// <para>
    /// There is no such thing as a 'one size fits all' configuration.  The default constructor 
    /// initializes all values for a normal human sized agent. You will usually discover 
    /// configurations that work well for categories of source geometry. E.g. A configuration 
    /// for interior environments and another for outdoor environments.
    /// </para>
    /// <para>In general, derive and set parameter values as follows:</para>
    /// <ol>
    /// <li>Determine the agent values in world units. (Agent height, step, radius.)</li>
    /// <li>
    /// Derive <see cref="XZCellSize"/> and <see cref="YCellSize"/> from the agent values.
    /// </li>
    /// <li>Derive and set the agent based parameters in cell units.</li>
    /// <li>Derive and set the <see cref="TileSize"/> and <see cref="BorderSize"/> parameters.</li>
    /// <li>Set the rest of the parameters.</li>
    /// </ol>
    /// <para>
    /// All properties and methods will auto-clamp fields to valid values. For example, 
    /// if the <see cref="TileSize"/> property is set to -1, the value will be clamped to 0.
    /// </para>
    /// <para>
    /// Fields are minimally documented.  See the property documentation for details.
    /// </para>
    /// <para>
    /// Implemented as a class with public fields in order to support Unity serialization.  
    /// Care must be taken not to set the fields to invalid values.
    /// </para>
    /// </remarks>
    /// <seealso cref="NMGenTileParams"/>
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public sealed class NMGenParams
    {
        /// <summary>
        /// Tile size.
        /// </summary>
        public int tileSize = 0;

        /// <summary>
        /// Border size.
        /// </summary>
        public int borderSize = 0;

        /// <summary>
        /// XZ-plane cell size.
        /// </summary>
        public float xzCellSize = 0.2f;

        /// <summary>
        /// Y-axis cell size.
        /// </summary>
        public float yCellSize = 0.1f;

        /// <summary>
        /// Maximum walkable slope.
        /// </summary>
        public float walkableSlope = 45.5f;

        /// <summary>
        /// Walkable height.
        /// </summary>
        public int walkableHeight = 19;

        /// <summary>
        /// Walkable step.
        /// </summary>
        public int walkableStep = 3;

        /// <summary>
        /// Walkable radius.
        /// </summary>
        public int walkableRadius = 2;

        /// <summary>
        /// Maximum edge length.
        /// </summary>
        public int maxEdgeLength = 0;

        /// <summary>
        /// Maximum edge deviation.
        /// </summary>
        public float edgeMaxDeviation = 3;

        /// <summary>
        /// Minimum region area.
        /// </summary>
        public int minRegionArea = 400;

        /// <summary>
        /// Merge region area.
        /// </summary>
        public int mergeRegionArea = 75;

        /// <summary>
        /// Maximum vertices per polygon.
        /// </summary>
        public int maxVertsPerPoly = 6;

        /// <summary>
        /// Detail sample distance.
        /// </summary>
        public float detailSampleDistance = 6;

        /// <summary>
        /// Detail maximum deviation.
        /// </summary>
        public float detailMaxDeviation = 1;

        /// <summary>
        /// Options to use when building the contour set.
        /// </summary>
        public ContourBuildFlags contourOptions = 
            ContourBuildFlags.TessellateAreaEdges | ContourBuildFlags.TessellateWallEdges;

        /// <summary>
        /// If true, use monotone region generation.
        /// </summary>
        public bool useMonotone = false;

        /// <summary>
        /// The xz-plane voxel size to use when sampling the source geometry.
        /// [Limit: >= <see cref="NMGen.MinCellSize"/>]
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Also known as the 'grid size' and 'voxel size'.
        /// </para>
        ///<para>
        /// This parameter effects how accurately the final navigation mesh can conform to the 
        /// source geometry on the xz-plane. E.g. How close it can follow the edges of 
        /// obstructions.  It has side effects on most parameters that apply to the xz-plane. 
        /// </para>
        /// <para>
        /// The primary governing factor for choosing this value is the value of the agent radius.
        /// Start with <c>(maxAgentRadius / 2)</c> for outdoor environments.  For indoor 
        /// environments or when more accuracy is needed, start with <c>(maxAgentRadius / 3)</c>.
        /// </para>
        /// <para>
        /// This parameter has a large impact on memmory useage.  The smaller the value, the higher
        /// the memory cost during the build.  So there are special considerations when the source 
        /// geometry covers a large area.  The best solution is use multiple polygon meshes built 
        /// for use in a multi-tile navigation mesh.  Otherwise it may be necessary to lower 
        /// the resolution.  If this is the case, set the cell size based on the source geometry's 
        /// longest xz-axis.  Start with a value of <c>(longestAxis / 1000)</c>, then reduce the 
        /// value as performance and memory allows.
        /// </para>
        /// </remarks>
        public float XZCellSize
        {
            get { return xzCellSize; }
            set { xzCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// The y-axis voxel size to use when sampling the source geometry.
        /// [Limit >= <see cref="NMGen.MinCellSize"/>]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Also known at the the 'voxel size' for the y-axis.
        /// </para>
        /// <para>
        /// Effects how accurately the height of the final navigation mesh can conform to the 
        /// source geometry. It has side effects on most parameters that apply to the y-axis.
        /// </para>
        /// <para>
        /// This parameter is based primarily on how far up/down the agent can step. 
        /// Start with <c>(maxAgentStep / 2.5)</c> and decrease as needed.  Note that
        /// 'game world' geometry is usually designed for a maximum agent step that is larger than
        /// in the real world.  Maximum agent step can sometimes be as high as 
        /// <c>(maxAgentHeight / 2)</c>
        /// </para>
        /// <para>
        /// Smaller values can result in a moderate increase in build times. The effect 
        /// on memory is minimal.
        /// </para>
        /// </remarks>
        public float YCellSize
        {
            get { return yCellSize; }
            set { yCellSize = Math.Max(NMGen.MinCellSize, value); }
        }

        /// <summary>
        /// The width/depth size of the tile on the xz-plane. 
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>A value of zero indicates non-tiled.</para>
        /// <para>
        /// The tile size should usually be between 500 and 1000. A tile size that is too small 
        /// can result in extra, unnecessary polygons and less than optimal pathfinding. A value 
        /// that is too large can be result in memory and performance issues during the build 
        /// process. In general, pick the largest size that also results in a good tile layout 
        /// along the x and z axes. (You want to avoid creation of thin tiles along the upper bounds 
        /// of the navigation mesh.)
        /// </para>
        /// </remarks>
        public int TileSize
        {
            get { return tileSize; }
            set { tileSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The tile size in world units.
        /// </summary>
        public float TileWorldSize
        {
            get { return tileSize * xzCellSize; }
        }

        /// <summary>
        /// The closest the mesh should come to the xz-plane AABB of the source geometry.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The border size exists primarily to support multi-tile meshes and is usually set to
        /// zero for single-tile meshes.
        /// </para>
        /// <para>
        /// Determining the exact value to use for multi-tile meshes can be tricky.  It is best to 
        /// set all the other parameters, then use <see cref="DeriveBorderSize"/> to get an 
        /// initial value.
        /// </para>
        /// </remarks>
        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = Math.Max(0, value); }
        }

        /// <summary>
        /// Derives the recommended border size for tiled mesh builds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tile size and agent parameters should be set before deriving the border size.
        /// </para>
        /// </remarks>
        /// <param name="config">The configuration to derive the border size from.</param>
        /// <returns>The recommended border size.</returns>
        public static int DeriveBorderSize(NMGenParams config)
        {
            if (config.TileSize > 0)
            {
                return config.WalkableRadius + 3;
            }
            return 0;
        }

        /// <summary>
        /// The maximum slope that is considered walkable.
        /// [Limits: 0 &lt;= value &lt;= <see cref="NMGen.MaxAllowedSlope"/>]
        /// [Units: Degrees]
        /// </summary>
        /// <remarks>
        /// <para>
        /// This parameter is used early in the build process to filter out source triangles that 
        /// have a high slope. The choice of the value is entirely dependant 
        /// on the maximum slope the agent's locomotion can handle. A good value to start with is 
        /// slightly over 45 degrees.
        /// </para>
        /// </remarks>
        public float WalkableSlope
        {
            get { return walkableSlope; }
            set
            {
                walkableSlope =
                    Math.Max(0, Math.Min(NMGen.MaxAllowedSlope, value));
            }
        }

        /// <summary>
        /// Minimum floor to 'ceiling' height that will still allow the floor area to be 
        /// considered walkable. 
        /// [Limit: >= <see cref="NMGen.MinWalkableHeight"/>]
        /// [Units: YCellSize]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Permits detection of overhangs in the source geometry that make the geometry 
        /// below un-walkable.
        /// </para>
        /// <para>
        /// Usually set to the maximum agent height.
        /// </para>
        /// <para>
        /// Example: Consider a table located on a floor. With this parameter set correctly, 
        /// the floor area under the table will not be considered walkable.
        /// </para>
        /// <img alt="Value: Walkable Height" src="../media/Value-WalkableHeight.jpg" />
        /// </remarks>
        public int WalkableHeight
        {
            get { return walkableHeight; }
            set { walkableHeight = Math.Max(NMGen.MinWalkableHeight, value); }
        }

        /// <summary>
        /// The walkable height in world units.
        /// </summary>
        public float WorldWalkableHeight
        {
            get { return walkableHeight * yCellSize; }
        }

        /// <summary>
        /// Derives <see cref="WalkableHeight"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="YCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldHeight">The walkable height in world units.</param>
        public void SetWalkableHeight(float worldHeight)
        {
            WalkableHeight = (int)Math.Ceiling(worldHeight / yCellSize);
        }

        /// <summary>
        /// Maximum ledge height that is considered to be traversable.
        /// [Limit: >=0] 
        /// [Units: YCellSize]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Determines when a ledge in the source geometry can be traversed, rather than acting as 
        /// an obstruction. Allows the mesh to flow over low lying obstructions such as curbs and 
        /// up/down stairways. 
        /// </para>
        /// <para>
        /// This parameter is normally based on how far up/down the agent can step, though often 
        /// it needs to be set higher because of the way the source geometry is designed. Start 
        /// with <c>ceil(maxAgentStep / <see cref="YCellSize"/>)</c> and adjust from there.
        /// </para>
        /// <img alt="Value: Waklable Step" src="../media/Value-WaklableStep.jpg" />
        /// </remarks>
        public int WalkableStep
        {
            get { return walkableStep; }
            set { walkableStep = Math.Max(0, value); }
        }

        /// <summary>
        /// The walkable set in world units.
        /// </summary>
        public float WorldWalkableStep
        {
            get { return walkableStep * yCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableStep"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="YCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldStep">The walkable step in world units.</param>
        public void SetWalkableStep(float worldStep)
        {
            WalkableStep = (int)Math.Floor(worldStep / yCellSize);
        }

        /// <summary>
        /// Represents the closest any part of a mesh should get to an obstruction in the source 
        /// geometry.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Effects the size of the border around obstacles. Many path planners (including CAINav) 
        /// treat agents as a point on the navigation mesh. So the navigation mesh needs to be 
        /// eroded by the agent's radius so the planner won't plan a path that comes too close 
        /// to obstructions, or passes through areas that are too thin for the agent to fit. 
        /// </para>
        /// <para>
        /// Start with a value of <c>ceil(maxAgentRadius / <see cref="XZCellSize"/>)</c>. 
        /// A value of zero is not recommended.
        /// </para>
        /// <img alt="Value: Waklable Radius" src="../media/Value-WalkableRadius.jpg" />
        /// </remarks>
        public int WalkableRadius
        {
            get { return walkableRadius; }
            set { walkableRadius = Math.Max(0, value); }
        }

        /// <summary>
        /// The walkable radius in world units.
        /// </summary>
        public float WorldWalkableRadius
        {
            get { return walkableRadius * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableRadius"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldRadius">The walkable radius in world units.</param>
        public void SetWalkableRadius(float worldRadius)
        {
            WalkableRadius = (int)Math.Ceiling(worldRadius / xzCellSize);
        }

        /// <summary>
        /// If true, use monotone region generation.  Otherwise use watershed partitioning.
        /// </summary>
        /// <seealso cref="CompactHeightfield.BuildRegionsMonotone"/>
        /// <seealso cref="CompactHeightfield.BuildRegions"/>
        public bool UseMonotone
        {
            get { return useMonotone; }
            set { useMonotone = value; }
        }

        /// <summary>
        /// The minimum number of cells allowed to form isolated island meshes.
        /// [Limit: >=0]
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>Prevents the formation of meshes that are too small to be of use.</para>
        /// <para>
        /// If you really don't want or expect any island regions, then set this parameter to a 
        /// high value. Otherwise the value will depend entirely on your needs.
        /// </para>
        /// <para>
        ///  In the example below, the value has been set too low, allowing the formation of 
        ///  small meshes on the top of obstructions.
        /// </para>
        /// <img alt="Value: MinRegionArea" src="../media/Value-MinRegionArea.jpg" />
        /// </remarks>
        public int MinRegionArea
        {
            get { return minRegionArea; }
            set { minRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The minimum region area in world units.
        /// </summary>
        public float WorldMinRegionArea
        {
            get { return minRegionArea * xzCellSize * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="MinRegionArea"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldArea">The minimum region area in world units.</param>
        public void SetMinRegionArea(float worldArea)
        {
            MinRegionArea = (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        }

        /// <summary>
        /// Any regions with an cell count smaller than this value will, if possible, be merged 
        /// with larger regions.
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        public int MergeRegionArea
        {
            get { return mergeRegionArea; }
            set { mergeRegionArea = Math.Max(0, value); }
        }

        /// <summary>
        /// The merge region area in world units.
        /// </summary>
        public float WorldMergeRegionArea
        {
            get { return mergeRegionArea * xzCellSize * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="MergeRegionArea"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldArea">The merge region area in world units.</param>
        public void SetMergeRegionArea(float worldArea)
        {
            MergeRegionArea = (int)Math.Ceiling(worldArea / (xzCellSize * xzCellSize));
        }

        /// <summary>
        /// The maximum allowed length for edges on the border of the mesh. 
        /// [Limit: >=0] 
        /// [Units: XZCellSize]
        /// </summary>
        /// <remarks>
        /// <para>
        /// Sometimes the build process can result in long thin triangles along the the mesh border. 
        /// This parameters is used during the contour build process to add extra border vertices
        /// to prevent this from happening.
        /// </para>
        /// <para>
        /// Start with a maximum edge length of zero (disabled). If you see a problem, then start 
        /// with <c>(<see cref="WalkableRadius"/> * 8)</c> and adjust from there.
        /// </para>
        /// </remarks>
        public int MaxEdgeLength
        {
            get { return maxEdgeLength; }
            set { maxEdgeLength = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum edge length in world units.
        /// </summary>
        public float WorldMaxEdgeLength
        {
            get { return maxEdgeLength * xzCellSize; }
        }

        /// <summary>
        /// Derives the <see cref="WalkableStep"/> from a world units value.
        /// </summary>
        /// <remarks>
        /// <para>The value will be snapped to the base unit type. (Cell size.)</para>
        /// <para>The <see cref="XZCellSize"/> must be set before using this method.</para>
        /// </remarks>
        /// <param name="worldLength">The walkable radius in world units.</param>
        public void SetMaxEdgeLength(float worldLength)
        {
            MaxEdgeLength = (int)Math.Ceiling(worldLength / xzCellSize);
        }

        /// <summary>
        /// The maximum distance the edges of the mesh should deviate from the source geometry. 
        /// [Limit: >=0] 
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>
        /// The 
        /// <a href="http://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm" target="_blank">
        /// Douglas–Peucker</a> algorithm is used when simplifying the borders of the mesh.
        /// This parameter effects how much simplification occurs. A higher number will result in 
        /// more lost detail, but also fewer unnecessary polygons around the border. A good place 
        /// to start is between 1.1 and 1.5. Low values, especially zero (no simplification), are 
        /// not recommended since they can result in a large number of small triangles around the 
        /// border.
        /// </para>
        /// <para>Applies only to the xz-plane.</para>
        /// </remarks>
        public float EdgeMaxDeviation
        {
            get { return edgeMaxDeviation; }
            set { edgeMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// Options to use when building the contour set.
        /// </summary>
        /// <seealso cref="ContourSet"/>
        public ContourBuildFlags ContourOptions
        {
            get { return contourOptions; }
            set { contourOptions = value; }
        }

        /// <summary>
        /// Sets the sample distance to use when matching the detail mesh surface to the source 
        /// geometry. (Height detail only.)
        /// [Limits: 0 or >= 0.9] 
        /// [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>
        /// This parameter is used in conjunction with <see cref="DetailMaxDeviation"/>
        /// to control how much extra height detail a <see cref="PolyMeshDetail"/> object will 
        /// contain.
        /// </para>
        /// <para>
        /// The sample distance is used to lay out sample points along the edges and across the
        /// surface of the source <see cref="PolyMesh"/> object's polygons. The height distance 
        /// from the polygon edge/surface is tested. If the maximum deviation is exceeded, then the 
        /// sample point is added as a vertex to the detail mesh.
        /// </para>
        /// <para>
        /// The sample distance and deviation should be set as high as possible while still 
        /// getting the desired height detail. Setting the sample distance to less than 0.9 will 
        /// effectively disable sampling. Setting the maximum deviation to zero is not recommended 
        /// since it will result in a large number of detail triangles.
        /// </para>
        /// <para>
        /// Start with a sample distance of <c>(longestXZAxis / 100)</c>, where 'longestXZAxis' is 
        /// based on the bounds of the source geometry.  Start with a maximum deviation of
        /// <c>(<see cref="YCellSize"/> * 20)</c>.
        /// </para>
        /// </remarks>
        public float DetailSampleDistance
        {
            get { return detailSampleDistance; }
            set { detailSampleDistance = value < 0.9f ? 0 : value; }
        }

        /// <summary>
        /// The maximum distance the mesh surface should deviate from the surface of the source 
        /// geometry. (For height detail only.) [Limit: >=0] [Units: World]
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see cref="DetailSampleDistance"/> for information on this parameter.
        /// </para>
        /// </remarks>
        public float DetailMaxDeviation
        {
            get { return detailMaxDeviation; }
            set { detailMaxDeviation = Math.Max(0, value); }
        }

        /// <summary>
        /// The maximum number of vertices allowed for polygons generated during the contour 
        /// to polygon conversion process.
        /// [Limits: 3 &lt;= value &lt; <see cref="NMGen.MaxAllowedVertsPerPoly"/>]
        /// </summary>
        public int MaxVertsPerPoly
        {
            get { return maxVertsPerPoly; }
            set { maxVertsPerPoly = Math.Max(3, Math.Min(NMGen.MaxAllowedVertsPerPoly, value)); }
        }

        /// <summary>
        /// Validates the parameter values.
        /// </summary>
        /// <returns>True if the parameter values meet the manditory limits.</returns>
        public bool IsValid()
        {
            return !(tileSize < 0
                || xzCellSize < NMGen.MinCellSize
                || yCellSize < NMGen.MinCellSize
                || walkableHeight < NMGen.MinWalkableHeight
                || walkableRadius < 0
                || walkableRadius < 0
                || walkableSlope < 0
                || walkableSlope > NMGen.MaxAllowedSlope
                || walkableStep < 0
                || borderSize < 0
                || maxEdgeLength < 0
                || edgeMaxDeviation < 0
                || (detailSampleDistance != 0 && detailSampleDistance < 0.9f)
                || detailMaxDeviation < 0
                || minRegionArea < 0
                || mergeRegionArea < 0
                || maxVertsPerPoly > NMGen.MaxAllowedVertsPerPoly);
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>A clone.</returns>
        public NMGenParams Clone()
        {
            NMGenParams result = new NMGenParams();
            result.xzCellSize = xzCellSize;
            result.yCellSize = yCellSize;
            result.detailMaxDeviation = detailMaxDeviation;
            result.detailSampleDistance = detailSampleDistance;
            result.edgeMaxDeviation = edgeMaxDeviation;
            result.borderSize = borderSize;
            result.walkableSlope = walkableSlope;
            result.walkableStep = walkableStep;
            result.maxVertsPerPoly = maxVertsPerPoly;
            result.walkableRadius = walkableRadius;
            result.maxEdgeLength = maxEdgeLength;
            result.walkableHeight = walkableHeight;
            result.mergeRegionArea = mergeRegionArea;
            result.minRegionArea = minRegionArea;
            result.tileSize = tileSize;
            result.contourOptions = contourOptions;
            result.useMonotone = useMonotone;

            return result;
        }

        /// <summary>
        /// Clamps all parameters to the mandatory limits.
        /// </summary>
        public void Clean()
        {
            XZCellSize = xzCellSize;
            WalkableHeight = walkableHeight;
            YCellSize = yCellSize;

            DetailMaxDeviation = detailMaxDeviation;
            DetailSampleDistance = detailSampleDistance;
            EdgeMaxDeviation = edgeMaxDeviation;
            BorderSize = borderSize;
            MaxEdgeLength = maxEdgeLength;
            MaxVertsPerPoly = MaxVertsPerPoly;
            MergeRegionArea = mergeRegionArea;
            MinRegionArea = minRegionArea;
            WalkableRadius = walkableRadius;
            WalkableSlope = walkableSlope;
            WalkableStep = walkableStep;

            TileSize = tileSize;
        }
    }
}
