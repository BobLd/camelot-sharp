using System;
using System.Collections.Generic;

namespace Camelot.ImageProcessing.Tests
{
    public static class TestHelper
    {
        public class ListTuple4EqualityComparer : IEqualityComparer<List<(float, float, float, float)>>
        {
            private readonly int precision;

            public ListTuple4EqualityComparer(int precision)
            {
                this.precision = precision;
            }

            public bool Equals(List<(float, float, float, float)> x, List<(float, float, float, float)> y)
            {
                if (x.Count != y.Count) return false;
                for (int i = 0; i < x.Count; i++)
                {
                    var _x = x[i];
                    var _y = y[i];
                    if (!Math.Round(_x.Item1, precision).Equals(Math.Round(_y.Item1, precision)))
                    {
                        return false;
                    }

                    if (!Math.Round(_x.Item2, precision).Equals(Math.Round(_y.Item2, precision)))
                    {
                        return false;
                    }

                    if (!Math.Round(_x.Item3, precision).Equals(Math.Round(_y.Item3, precision)))
                    {
                        return false;
                    }

                    if (!Math.Round(_x.Item4, precision).Equals(Math.Round(_y.Item4, precision)))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(List<(float, float, float, float)> obj)
            {
                return obj.ConvertAll(t => (Math.Round(t.Item1, precision), Math.Round(t.Item2, precision), Math.Round(t.Item3, precision), Math.Round(t.Item4, precision))).GetHashCode();
            }
        }
    }
}
