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

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Represents the build state for an <see cref="IncrementalBuilder"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There are three finished states: <see cref="Complete"/>, <see cref="NoResult"/>, 
    /// and <see cref="Aborted"/>.
    /// </para>
    /// </remarks>
    public enum NMGenState
    {
        /// <summary>
        /// The builder is initialized and ready to start the build.
        /// </summary>
        Initialized = 0,

        /// <summary>
        /// The build was aborted due to an error. (Finished state.)
        /// </summary>
        Aborted,

        /// <summary>
        /// The build was completed and produced a result. (Finished state.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The build produced at least a <see cref="org.critterai.nmgen.PolyMesh"/> object 
        /// with at least one polygon.
        /// </para>
        /// <para>
        /// It is possible to complete, but have no resulting meshes. See <see cref="NoResult"/>.
        /// </para>
        /// </remarks>
        Complete,

        /// <summary>
        /// The build completed without producing a result. (Finished state.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// While having no result is usually considered a failure when building a single 
        /// tile mesh, it is not unexpected for tiled meshes. (Some tiles may not contain 
        /// input geometry, or not enough to result in a usable surface.)
        /// </para>
        /// </remarks>
        NoResult,

        /// <summary>
        /// At the step to build the heightfield.
        /// </summary>
        HeightfieldBuild,

        /// <summary>
        /// At the step to build the compact heightfield.
        /// </summary>
        CompactFieldBuild,

        /// <summary>
        /// At the step to build the compact heightfield regions.
        /// </summary>
        RegionBuild,

        /// <summary>
        /// At the step to build the raw and detail contours.
        /// </summary>
        ContourBuild,

        /// <summary>
        /// At the step to build the polygon mesh.
        /// </summary>
        PolyMeshBuild,

        /// <summary>
        /// At the step to build the detail mesh.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This state will only be reached if the build is set up to produce a detail mesh.
        /// </para>
        /// </remarks>
        DetailMeshBuild,
    }
}
