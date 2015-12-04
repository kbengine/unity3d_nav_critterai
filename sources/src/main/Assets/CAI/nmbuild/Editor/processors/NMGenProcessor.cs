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

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A standard processor used during the NMGen build process.
    /// </summary>
    public abstract class NMGenProcessor
        : INMGenProcessor
    {
        private readonly string mName;
        private readonly int mPriority;

        /// <summary>
        /// The name of the processor.
        /// </summary>
        public string Name { get { return mName; } }

        /// <summary>
        /// The processor priority.
        /// </summary>
        public int Priority { get { return mPriority; } }

        /// <summary>
        /// True if the processor is safe to use for threaded builds.
        /// </summary>
        public abstract bool IsThreadSafe { get; }

        /// <summary>
        /// The build assets that should be preserved past their normal disposal point.
        /// </summary>
        public virtual NMGenAssetFlag PreserveAssets { get { return 0; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the processor.</param>
        /// <param name="priority">The processor priority.</param>
        public NMGenProcessor(string name, int priority)
        {
            mPriority = NMBuild.ClampPriority(priority);
            mName = (name == null || name.Length == 0) ? "Unnamed" : name;
        }

        /// <summary>
        /// Process the build context.
        /// </summary>
        /// <param name="state">The current build state.</param>
        /// <param name="context">The context to process.</param>
        /// <returns>False if the build should abort.  Otherwise true.</returns>
        public abstract bool ProcessBuild(NMGenContext context, NMGenState state);
    }
}
