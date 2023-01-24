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

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Smoothed window of some numeric value.
    /// </summary>
    public abstract class SmoothedWindow<E>
    {
        protected E[] samples;
        protected int currentIdx;
        public int Count { get; protected set; }

        public SmoothedWindow(int size)
        {
            samples = new E[size];
        }

        public virtual E AddSample(E value)
        {
            E previous = samples[currentIdx];
            samples[currentIdx] = value;

            currentIdx = (currentIdx + 1) % samples.Length;

            Count = Mathf.Max(Count, currentIdx);
            return previous;
        }
    }

    public class SmoothedVector : SmoothedWindow<Vector3>
    {
        protected Vector3 sum = Vector3.zero;

        public SmoothedVector(int size) : base(size) { }

        public Vector3 Average()
        {
            return sum / Count;
        }

        public override Vector3 AddSample(Vector3 value)
        {
            Vector3 previous = base.AddSample(value);
            sum -= previous;
            sum += value;

            UnityEngine.Debug.Log($"sum:{sum} value:{value} currentIdx:{currentIdx}");

            return previous;
        }
    }
}
