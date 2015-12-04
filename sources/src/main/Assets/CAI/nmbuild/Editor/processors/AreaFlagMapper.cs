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
using org.critterai.nmgen;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// Applies flags to the polygons in a <see cref="PolyMesh"/> based on area assignment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This process will add the flags to all polygons assigned to the associated area.
    /// Existing flags are not altered.
    /// </para>
    /// </remarks>
	public sealed class AreaFlagMapper
        : NMGenProcessor
	{
        private readonly byte[] mAreas;
        private readonly ushort[] mFlags;

        private AreaFlagMapper(string name, int priority, byte[] areas, ushort[] flags)
            : base(name, priority)
        {
            mAreas = areas;
            mFlags = flags;
        }

        /// <summary>
        /// Always threadsafe. (True)
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The flags will be applied during the <see cref="NMGenState.PolyMeshBuild"/>
        /// state.
        /// </para>
        /// </remarks>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False on error, otherwise true.</returns>
        public override bool ProcessBuild(NMGenContext context, NMGenState state)
        {
            if (state != NMGenState.PolyMeshBuild)
                return true;

            PolyMesh mesh = context.PolyMesh;
            PolyMeshData data = mesh.GetData(false);

            if (data.polyCount == 0)
                return true;

            bool applied = false;

            for (int i = 0; i < mAreas.Length; i++)
            {
                byte area = mAreas[i];
                ushort flags = mFlags[i];

                int marked = 0;

                for (int iPoly = 0; iPoly < data.polyCount; iPoly++)
                {
                    if (data.areas[iPoly] == area)
                    {
                        data.flags[iPoly] |= flags;
                        marked++;
                    }
                }

                if (marked > 0)
                {
                    string msg = string.Format(
                        "{0} : Added '0x{1:X}' flag(s) to {2} poylgons assigned to area {3}."
                        , Name, flags, marked, area);

                    context.Log(msg, this);

                    applied = true;
                }
            }

            if (applied)
                mesh.Load(data);
            else
                context.Log(Name + ": No flags applied.", this);

            return true;
        }

        /// <summary>
        /// Creates a new mapper.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return null if parameters are invalid. Must have at least one valid area/flag map.
        /// </para>
        /// </remarks>
        /// <param name="name">The processor name.</param>
        /// <param name="priority">The processor priority.</param>
        /// <param name="areas">The areas check for.</param>
        /// <param name="flags">The flags to apply.</param>
        /// <returns>A new marker, or null on error.</returns>
        public static AreaFlagMapper Create(string name, int priority, byte[] areas, ushort[] flags)
        {
            if (areas == null || flags == null || areas.Length != flags.Length
                || !NMGen.IsValidAreaBuffer(areas, areas.Length))
            {
                return null;
            }

            return new AreaFlagMapper(name, priority
                , (byte[])areas.Clone(), (ushort[])flags.Clone());
        }
    }
}
