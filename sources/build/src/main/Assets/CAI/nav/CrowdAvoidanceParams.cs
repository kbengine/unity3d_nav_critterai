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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace org.critterai.nav
{
    /// <summary>
    /// Configuration parameters that define steering behaviors for agents managed by a crowd 
    /// manager.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Minimal available documentation.
    /// </para>
    /// <para>
    /// Implemented as a class with public fields in order to support Unity serialization.  Care 
    /// must be taken not to set the fields to invalid values.
    /// </para>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class CrowdAvoidanceParams
    {

        /*
         * Source: DetourCrowdAvoidance dtObstacleAvoidanceParams (struct)
         * 
         * Design note: 
         * 
         * Class vs Structure
         * 
         * In this case I've decided to go with a class over a structure.
         * I expect this data will be mostly set at design time and remain
         * static at runtime.  So no need to optimize interop calls, and
         * support for Unity serialization is more important.
         * 
         * The default values are the same as those in dtCrowd.init().
         * 
         */

        /// <summary>
        /// Velocity bias.
        /// </summary>
	    public float velocityBias = 0.4f;

        /// <summary>
        /// Desired velocity weight.
        /// </summary>
        public float weightDesiredVelocity = 2.0f;

        /// <summary>
        /// Current velocity weight.
        /// </summary>
        public float weightCurrentVelocity = 0.75f;

        /// <summary>
        /// Size weight.
        /// </summary>
        public float weightSide = 0.75f;

        /// <summary>
        /// Weight TOI.
        /// </summary>
        public float weightToi = 2.5f;

        /// <summary>
        /// Horizontal time.
        /// </summary>
        public float horizontalTime = 2.5f;

        /// <summary>
        /// Sample grid size.
        /// </summary>
        public byte gridSize = 33;

        /// <summary>
        /// Adaptive divisions.
        /// </summary>
        public byte adaptiveDivisions = 7;

        /// <summary>
        /// Adaptive rings.
        /// </summary>
        public byte adaptiveRings = 2;

        /// <summary>
        /// Adaptive depth.
        /// </summary>
        public byte adaptiveDepth = 5;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CrowdAvoidanceParams() { }

        /// <summary>
        /// Clones the current object. (Usually more appropriate than sharing references.)
        /// </summary>
        /// <returns>A clone of the object.</returns>
        public CrowdAvoidanceParams Clone()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();
            result.velocityBias = velocityBias;
            result.weightDesiredVelocity = weightDesiredVelocity;
            result.weightCurrentVelocity = weightCurrentVelocity;
            result.weightSide = weightSide;
            result.weightToi = weightToi;
            result.horizontalTime = horizontalTime;
            result.gridSize = gridSize;
            result.adaptiveDivisions = adaptiveDivisions;
            result.adaptiveRings = adaptiveRings;
            result.adaptiveDepth = adaptiveDepth;
            return result;
        }

        /// <summary>
        /// Creates a standard low quality configuration.
        /// </summary>
        /// <returns>A high quality configuration.</returns>
        public static CrowdAvoidanceParams CreateStandardLow()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();

            result.velocityBias = 0.5f;
            result.adaptiveDivisions = 5;
            result.adaptiveRings = 2;
            result.adaptiveDepth = 1;

            return result;
        }

        /// <summary>
        /// Creates a standard medium quality configuration.
        /// </summary>
        /// <returns>A high quality configuration.</returns>
        public static CrowdAvoidanceParams CreateStandardMedium()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();

            result.velocityBias = 0.5f;
            result.adaptiveDivisions = 5;
            result.adaptiveRings = 2;
            result.adaptiveDepth = 2;

            return result;
        }

        /// <summary>
        /// Creates a standard good quality configuration.
        /// </summary>
        /// <returns>A high quality configuration.</returns>
        public static CrowdAvoidanceParams CreateStandardGood()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();
 
            result.velocityBias = 0.5f;
            result.adaptiveDivisions = 7;
            result.adaptiveRings = 2;
            result.adaptiveDepth = 3;

            return result;
        }

        /// <summary>
        /// Creates a standard high quality configuration.
        /// </summary>
        /// <returns>A high quality configuration.</returns>
        public static CrowdAvoidanceParams CreateStandardHigh()
        {
            CrowdAvoidanceParams result = new CrowdAvoidanceParams();

            result.velocityBias = 0.5f;
            result.adaptiveDivisions = 7;
            result.adaptiveRings = 3;
            result.adaptiveDepth = 3;

            return result;
        }
    }
}
