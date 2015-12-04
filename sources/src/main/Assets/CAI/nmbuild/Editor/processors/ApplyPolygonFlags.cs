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

using System;
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A processor that applies polygon flags to all polygons in a <see cref="PolyMesh"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The flags are additive. (I.e. <c>poly.flags |= flags</c>)
    /// </para>
    /// </remarks>
    public sealed class ApplyPolygonFlags
        : NMGenProcessor
    {
        private readonly ushort mFlags;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <param name="priority">The prioity.</param>
        /// <param name="flags">The flags to apply.</param>
        public ApplyPolygonFlags(string name, int priority, ushort flags)
            : base(name, priority)
        {
            mFlags = flags;
        }

        /// <summary>
        /// The flags to apply.
        /// </summary>
        public ushort Flags { get { return mFlags; } }

        /// <summary>
        /// Always threadsafe. (True)
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The flags will be applied during the <see cref="NMGenState.PolyMeshBuild"/> state.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>True</returns>
        public override bool ProcessBuild(NMGenContext context, NMGenState state)
        {
            if (state != NMGenState.PolyMeshBuild)
                return true;

            PolyMeshData data = context.PolyMesh.GetData(false);

            for (int i = 0; i < data.flags.Length; i++)
            {
                data.flags[i] |= mFlags;
            }

            context.PolyMesh.Load(data);
            context.Log(string.Format("{0}: Applied flag(s) to all polys. Flag(s): 0x{1:X}"
                , Name, mFlags)
                , this);

            return true;
        }
    }
}
