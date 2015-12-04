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
using System.Collections.Generic;

namespace org.critterai.nmbuild
{
    /// <summary>
    /// A standard build task that provides data upon completion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The task is single use.  The task is constructed, run, then its data and messages retrieved.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of data provided upon completion.</typeparam>
    public abstract class BuildTask<T>
        : IBuildTask
    {
        // Design note: Everything is locked on the messages list.

        private T mData;

        private BuildTaskState mState;
        private bool mIsFinished = false;
        private readonly int mPriority;

        private readonly List<string> mMessages = new List<string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="priority">The priority of the task.</param>
        public BuildTask(int priority)
        {
            mPriority = priority;
        }

        /// <summary>
        /// The priority of the item.
        /// </summary>
        /// <remarks>
        /// <para>This value is immutable.</para>
        /// </remarks>
        public int Priority { get { return mPriority; } }

        /// <summary>
        /// If true, the task can be safely run on a separate thread from the object(s) monitoring
        /// its state.
        /// </summary>
        /// <remarks>
        /// <para>This value of immutable.</para>
        /// </remarks>
        public abstract bool IsThreadSafe { get; }

        /// <summary>
        /// Performs a work increment.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will be 'reasonably' responsive.  It should block its thread
        /// for the minimum possible amount of time.
        /// </para>
        /// <para>
        /// Called in a loop by the <see cref="Run"/> method until the task is finished.
        /// </para>
        /// <para>
        /// Not guarenteed to be called.  (I.e. Will not be called if the task is aborted
        /// before the task is run.)
        /// </para>
        /// </remarks>
        /// <returns>True if the task is not yet finished.  Otherwise false.</returns>
        protected abstract bool LocalUpdate();

        /// <summary>
        /// Gets the result of the completed task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Called by <see cref="Run"/> after the task completes and before 
        /// <see cref="FinalizeTask"/> is run.  Will not be called on tasks in the aborted state.
        /// </para>
        /// </remarks>
        /// <param name="result">The result of the completed task.</param>
        /// <returns>
        /// True if the result is available, false if the task should abort with no result. 
        /// (I.e. An internal abort.)
        /// </returns>
        protected abstract bool GetResult(out T result);

        /// <summary>
        /// Finalize the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Used for task cleanup.  Will be called before the <see cref="Run"/> method exits,
        /// even on an exception.  Must never throw an exception.
        /// </para>
        /// </remarks>
        protected virtual void FinalizeTask() { }

        /// <summary>
        /// Adds a message to the message queue.
        /// </summary>
        /// <param name="messsage">The message to add.</param>
        protected void AddMessage(string messsage)
        {
            lock (mMessages)
                mMessages.Add(messsage);
        }
        /// <summary>
        /// Appends an array of messages to the message queue.
        /// </summary>
        /// <param name="messages">The messages to append.</param>
        protected void AddMessages(string[] messages)
        {
            lock (mMessages)
                mMessages.AddRange(messages);
        }

        /// <summary>
        /// The current state of the task.
        /// </summary>
        public BuildTaskState TaskState {  get {  lock (mMessages) return mState; } }

        /// <summary>
        /// The task is in a finished state.
        /// </summary>
        public bool IsFinished { get { return mIsFinished; } }

        /// <summary>
        /// Messages available after the task is finished.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will return a zero length array if the task is not finished or there are
        /// no messages.  Will always provide a message on abort.
        /// </para>
        /// </remarks>
        public string[] Messages { get {  lock (mMessages) return mMessages.ToArray(); } }

        /// <summary>
        /// The data produced by the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Will only contain useable data when the task is finished with a state of
        /// <see cref="BuildTaskState.Complete"/>.
        /// </para>
        /// </remarks>
        public T Result { get { return mData; } }

        /// <summary>
        /// Runs the task through to a finished state.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Never call this method more than once.
        /// </para>
        /// <para>
        /// If <see cref="IsThreadSafe"/> is true, this method can be run on a separate
        /// thread from the object(s) that are monitoring the task state.
        /// </para>
        /// </remarks>
        public void Run()
        {
            lock (mMessages)
            {
                if (mState != BuildTaskState.Inactive)
                    return;

                mState = BuildTaskState.InProgress;
            }

            try
            {
                while (LocalUpdate())
                {
                    lock (mMessages)
                    {
                        if (mState == BuildTaskState.Aborting)
                            break;
                    }
                }
                FinalizeRequest();
            }
            catch (System.Exception ex)
            {
                FinalizeRequest(ex);
            }
            finally
            {
                FinalizeTask();
            }
        }

        /// <summary>
        /// Requests an abort of the task.
        /// </summary>
        /// <remarks>
        /// <para>
        /// There may be a delay in the actual abort for tasks running on a separate thread.
        /// </para>
        /// </remarks>
        /// <param name="reason">The reason for the abort.</param>
        public void Abort(string reason)
        {
            lock (mMessages)
            {
                if (mIsFinished)
                    return;

                AddMessage(reason);

                if (mState == BuildTaskState.Inactive)
                {
                    mState = BuildTaskState.Aborting;
                    FinalizeRequest();
                }
                else
                    mState = BuildTaskState.Aborting;
            }
        }

        private void FinalizeRequest()
        {
            lock (mMessages)
            {
                if (mState == BuildTaskState.Aborting)
                {
                    // Don't care if the task actually finished.
                    // An abort takes presidence.
                    mState = BuildTaskState.Aborted;
                    mData = default(T);
                }
                else
                {
                    try
                    {
                        mState = GetResult(out mData) ? BuildTaskState.Complete : BuildTaskState.Aborted;
                    }
                    catch (System.Exception ex)
                    {
                        FinalizeRequest(ex);
                    }
                }

                mIsFinished = true;  // Always last.
            }
        }

        private void FinalizeRequest(System.Exception ex)
        {
            lock (mMessages)
            {
                mMessages.Add(string.Format("Build task aborted on exception: {0} ({1})"
                    , ex.Message, this.GetType().Name));
                mData = default(T);
                mState = BuildTaskState.Aborted;
                mIsFinished = true;
            }
        }
    }
}
