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
using UnityEditor;
using UnityEngine;

namespace org.critterai.nmbuild.u3d.editor
{
    /// <summary>
    /// A generic unity build context. (Editor Only)
    /// </summary>
    public class UnityBuildContext
        : BuildContext
    {
        internal const string TraceKey = "org.critterai.nmbuild.TraceMessages";

        private static bool mTraceEnabled;

        /// <summary>
        /// True if the owner of the context should periodically post trace messages.
        /// </summary>
        public static bool TraceEnabled
        {
            get { return mTraceEnabled; }
            set
            {
                if (mTraceEnabled != value)
                {
                    mTraceEnabled = value;
                    EditorPrefs.SetBool(TraceKey, mTraceEnabled);
                }
            }
        }

        /// <summary>
        /// Flushes all log messages to the console as an error.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The log will be reset.
        /// </para>
        /// <para>
        /// This method should only be used by the owner of the context.  Other context users,
        /// such as build processors, should use the <see cref="BuildContext.LogError"/> method.
        /// </para>
        /// </remarks>
        /// <param name="summary">The error summary.</param>
        /// <param name="context">The Unity object context. (Optional)</param>
        internal void PostError(string summary, Object context)
        {
            Debug.LogError(string.Format("{0}\n{1}"
                    , summary
                    , GetMessagesFlat())
                , context);
            ResetLog();
        }

        /// <summary>
        /// Appends the specified messages to the log then flushes all log messages to the console 
        /// as an error.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The log will be reset.
        /// </para>
        /// <para>
        /// This method should only be used by the owner of the context.  Other context users,
        /// such as build processors, should use the <see cref="BuildContext.LogError"/> method.
        /// </para>
        /// </remarks>
        /// <param name="summary">The error summary.</param>
        /// <param name="messages">
        /// Messages to append to the log before posting to the console. (Optional)
        /// </param>
        /// <param name="context">The Unity object context. (Optional)</param>
        internal void PostError(string summary, string[] messages, Object context)
        {
            Log(messages);
            PostError(summary, context);
        }

        /// <summary>
        /// Flushes all log messages to the console.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The log will be reset.
        /// </para>
        /// <para>
        /// This method should only be used by the owner of the context.  Other context users,
        /// such as build processors, should use the standard log methods.
        /// </para>
        /// </remarks>
        /// <param name="summary">The trace summary.</param>
        /// <param name="context">The Unity object context. (Optional)</param>
        internal void PostTrace(string summary, Object context)
        {
            if (mTraceEnabled)
            {
                Debug.Log(string.Format("{0}\n{1}"
                        , summary
                        , GetMessagesFlat())
                    , context);
            }
            ResetLog();
        }

        /// <summary>
        /// Appends the specified messages to the log then flushes all log messages to the console.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The log will be reset.
        /// </para>
        /// <para>
        /// This method should only be used by the owner of the context.  Other context users,
        /// such as build processors, should use the standard log methods.
        /// </para>
        /// </remarks>
        /// <param name="summary">The trace summary.</param>
        /// <param name="messages">
        /// Messages to append to the log before posting to the console. (Optional)
        /// </param>
        /// <param name="context">The Unity object context. (Optional)</param>
        internal void PostTrace(string summary, string[] messages, Object context)
        {
            if (mTraceEnabled)
            {
                Log(messages);
                PostTrace(summary, context);
            }
        }
    }
}
