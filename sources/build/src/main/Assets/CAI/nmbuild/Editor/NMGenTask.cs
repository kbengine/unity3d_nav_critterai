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
using org.critterai.nav;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A task used to manage the build of an <see cref="IncrementalBuilder"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The primary purpose of this class is to run the builder on a background thread.
    /// It handles aborts and exception handling, which are not possible if the
    /// builder is run directly on a thread.
    /// </para>
    /// </remarks>
    public sealed class NMGenTask
        : BuildTask<NMGenAssets>
    {
        private readonly IncrementalBuilder mBuilder;

        private NMGenTask(IncrementalBuilder builder, int priority)
            : base(priority)
        {
            mBuilder = builder;
        }

        /// <summary>
        /// If true, the task is safe to run on its own thread.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will be true if the root builder is thread-safe.
        /// </para>
        /// </remarks>
        public override bool IsThreadSafe { get { return mBuilder.IsThreadSafe; } }

        /// <summary>
        /// The x-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileX { get { return mBuilder.TileX; } }

        /// <summary>
        /// The z-index of the tile within the tile grid. (x, z)
        /// </summary>
        public int TileZ { get { return mBuilder.TileZ; } }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="builder">The builder to manage.</param>
        /// <param name="priority">The task priority.</param>
        /// <returns>A task, or null on error.</returns>
        public static NMGenTask Create(IncrementalBuilder builder, int priority)
        {
            if (builder == null || builder.IsFinished)
                return null;

            return new NMGenTask(builder, priority);
        }

        /// <summary>
        /// Performs a work increment.
        /// </summary>
        /// <returns>True if the task is not yet finished.  Otherwise false.</returns>
        protected override bool LocalUpdate()
        {
            mBuilder.Build();
            return !mBuilder.IsFinished;  // Go to false when IsFinished.
        }

        /// <summary>
        /// Gets the result of the completed task.
        /// </summary>
        /// <param name="result">The result of the completed task.</param>
        /// <returns>
        /// True if the result is available, false if the task should abort with no result. 
        /// (I.e. An internal abort.)
        /// </returns>
        protected override bool GetResult(out NMGenAssets result)
        {
            AddMessages(mBuilder.GetMessages());

            switch (mBuilder.State)
            {
                case NMGenState.Complete:

                    result = mBuilder.Result;
                    return true;

                case NMGenState.NoResult:

                    result = new NMGenAssets(mBuilder.TileX, mBuilder.TileZ, null, null);
                    return true;

                default:

                    result = new NMGenAssets();
                    return false;
            }
        }
    }
}
