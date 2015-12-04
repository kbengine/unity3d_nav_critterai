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
    /// A standard build task.
    /// </summary>
    public interface IBuildTask
        : IPriorityItem
    {
        /// <summary>
        /// If true, the task can be safely run on a separate thread from the object(s) monitoring
        /// its state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If true, the <see cref="Run"/> method will never throw exceptions.
        /// </para>
        /// <para>
        /// The value of this property is immutable after construction.
        /// </para>
        /// </remarks>
        bool IsThreadSafe { get; }

        /// <summary>
        /// The task is in a finished state.
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// Messages available after the task is finished.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return a zero length array if the task is not finished or there are
        /// no messages.  Will always provide a message on abort.
        /// </para>
        /// </remarks>
        string[] Messages { get; }

        /// <summary>
        /// Requests an abort of the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There may be a delay in the actual abort for tasks running on a separate thread.
        /// </para>
        /// </remarks>
        /// <param name="reason">The reason for the abort.</param>
        void Abort(string reason);

        /// <summary>
        /// The current state of the task.
        /// </summary>
        BuildTaskState TaskState { get; }

        /// <summary>
        /// Runs the task through to a finished state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <see cref="IsThreadSafe"/> is true, this method can be run on a separate
        /// thread from the object(s) that are monitoring the task state.
        /// </para>
        /// </remarks>
        void Run();
    }
}
