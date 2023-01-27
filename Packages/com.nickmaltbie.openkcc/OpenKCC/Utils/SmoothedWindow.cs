// Copyright (C) 2023 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Smoothed window of some numeric value.
    /// </summary>
    public abstract class SmoothedWindow<E>
    {
        /// <summary>
        /// Gets the sample values.
        /// </summary>
        protected E[] Samples { get; private set; }

        /// <summary>
        /// Gets the current selected index.
        /// </summary>
        protected int CurrentIdx { get; private set; }

        /// <summary>
        /// Count of 
        /// </summary>
        /// <value></value>
        public int Count { get; protected set; }

        /// <summary>
        /// Create a smoothed window.
        /// </summary>
        /// <param name="size">Size of smoothing window.</param>
        public SmoothedWindow(int size)
        {
            Samples = new E[size];
        }

        /// <summary>
        /// Adds a sample to the smoothing window at the next
        /// available space. Will overwrite data if any data
        /// is there.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>Previous value removed.</returns>
        public virtual E AddSample(E value)
        {
            E previous = Samples[CurrentIdx];
            Samples[CurrentIdx] = value;
            Count = Mathf.Max(Count, ++CurrentIdx);
            CurrentIdx %= Samples.Length;
            return previous;
        }
    }

    /// <summary>
    /// Smoothed window for a Vector3 value.
    /// </summary>
    public class SmoothedVector : SmoothedWindow<Vector3>
    {
        /// <summary>
        /// Running sum of the values stored in samples.
        /// </summary>
        protected Vector3 sum = Vector3.zero;

        /// <summary>
        /// Create a smoothed vector with a given number of samples..
        /// </summary>
        /// <param name="size">Size of smoothing window.</param>
        public SmoothedVector(int size) : base(size) { }

        /// <summary>
        /// Returns the average of all samples in the window.
        /// </summary>
        public Vector3 Average()
        {
            if (Count == 0)
            {
                return Vector3.zero;
            }

            return sum / Count;
        }

        /// <inheritdoc/>
        public override Vector3 AddSample(Vector3 value)
        {
            Vector3 previous = base.AddSample(value);
            sum += value;
            sum -= previous;
            return previous;
        }
    }
}
