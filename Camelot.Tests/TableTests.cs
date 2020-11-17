using System.Collections.Generic;
using Xunit;
using static Camelot.Core;

namespace Camelot.Tests
{
    public class TableTests
    {
        public static readonly List<(float, float)> FooRows = new List<(float, float)>()
        {
            (275.8804000000001f, 257.59320000000014f),
            (257.59320000000014f, 235.4314000000001f),
            (235.4314000000001f, 222.56070000000008f),
            (222.56070000000008f, 215.66520000000003f),
            (215.66520000000003f, 209.90370000000007f),
            (209.90370000000007f, 204.17370000000005f),
            (204.17370000000005f, 193.52591999999999f),
            (193.52591999999999f, 178.97604f),
            (178.97604f, 163.46604000000002f),
            (163.46604000000002f, 147.98604f),
            (147.98604f, 132.47604f),
            (132.47604f, 119.69604000000001f)
        };

        public static readonly List<(float, float)> FooCols = new List<(float, float)>()
        {
            (71.99965000000003f, 168.58018f),
            (168.58018f, 208.05893000000003f),
            (208.05893000000003f, 258.91048f),
            (258.91048f, 317.52743000000004f),
            (317.52743000000004f, 373.1978496725f),
            (373.1978496725f, 432.92212758790004f),
            (432.92212758790004f, 540.41949376f)
        };

        private readonly string[][] FooCells = new string[][]
        {
            new string[]
            {
                "<Cell x1=72 y1=257.59 x2=168.58 y2=275.88>",
                "<Cell x1=168.58 y1=257.59 x2=208.06 y2=275.88>",
                "<Cell x1=208.06 y1=257.59 x2=258.91 y2=275.88>",
                "<Cell x1=258.91 y1=257.59 x2=317.53 y2=275.88>",
                "<Cell x1=317.53 y1=257.59 x2=373.2 y2=275.88>",
                "<Cell x1=373.2 y1=257.59 x2=432.92 y2=275.88>",
                "<Cell x1=432.92 y1=257.59 x2=540.42 y2=275.88>"
            },
            new string[]
            {
                "<Cell x1=72 y1=235.43 x2=168.58 y2=257.59>",
                "<Cell x1=168.58 y1=235.43 x2=208.06 y2=257.59>",
                "<Cell x1=208.06 y1=235.43 x2=258.91 y2=257.59>",
                "<Cell x1=258.91 y1=235.43 x2=317.53 y2=257.59>",
                "<Cell x1=317.53 y1=235.43 x2=373.2 y2=257.59>",
                "<Cell x1=373.2 y1=235.43 x2=432.92 y2=257.59>",
                "<Cell x1=432.92 y1=235.43 x2=540.42 y2=257.59>"
            },
            new string[]
            {
                "<Cell x1=72 y1=222.56 x2=168.58 y2=235.43>",
                "<Cell x1=168.58 y1=222.56 x2=208.06 y2=235.43>",
                "<Cell x1=208.06 y1=222.56 x2=258.91 y2=235.43>",
                "<Cell x1=258.91 y1=222.56 x2=317.53 y2=235.43>",
                "<Cell x1=317.53 y1=222.56 x2=373.2 y2=235.43>",
                "<Cell x1=373.2 y1=222.56 x2=432.92 y2=235.43>",
                "<Cell x1=432.92 y1=222.56 x2=540.42 y2=235.43>"
            },
            new string[]
            {
                "<Cell x1=72 y1=215.67 x2=168.58 y2=222.56>",
                "<Cell x1=168.58 y1=215.67 x2=208.06 y2=222.56>",
                "<Cell x1=208.06 y1=215.67 x2=258.91 y2=222.56>",
                "<Cell x1=258.91 y1=215.67 x2=317.53 y2=222.56>",
                "<Cell x1=317.53 y1=215.67 x2=373.2 y2=222.56>",
                "<Cell x1=373.2 y1=215.67 x2=432.92 y2=222.56>",
                "<Cell x1=432.92 y1=215.67 x2=540.42 y2=222.56>"
            },
            new string[]
            {
                "<Cell x1=72 y1=209.9 x2=168.58 y2=215.67>",
                "<Cell x1=168.58 y1=209.9 x2=208.06 y2=215.67>",
                "<Cell x1=208.06 y1=209.9 x2=258.91 y2=215.67>",
                "<Cell x1=258.91 y1=209.9 x2=317.53 y2=215.67>",
                "<Cell x1=317.53 y1=209.9 x2=373.2 y2=215.67>",
                "<Cell x1=373.2 y1=209.9 x2=432.92 y2=215.67>",
                "<Cell x1=432.92 y1=209.9 x2=540.42 y2=215.67>"
            },
            new string[]
            {
                "<Cell x1=72 y1=204.17 x2=168.58 y2=209.9>",
                "<Cell x1=168.58 y1=204.17 x2=208.06 y2=209.9>",
                "<Cell x1=208.06 y1=204.17 x2=258.91 y2=209.9>",
                "<Cell x1=258.91 y1=204.17 x2=317.53 y2=209.9>",
                "<Cell x1=317.53 y1=204.17 x2=373.2 y2=209.9>",
                "<Cell x1=373.2 y1=204.17 x2=432.92 y2=209.9>",
                "<Cell x1=432.92 y1=204.17 x2=540.42 y2=209.9>"
            },
            new string[]
            {
                "<Cell x1=72 y1=193.53 x2=168.58 y2=204.17>",
                "<Cell x1=168.58 y1=193.53 x2=208.06 y2=204.17>",
                "<Cell x1=208.06 y1=193.53 x2=258.91 y2=204.17>",
                "<Cell x1=258.91 y1=193.53 x2=317.53 y2=204.17>",
                "<Cell x1=317.53 y1=193.53 x2=373.2 y2=204.17>",
                "<Cell x1=373.2 y1=193.53 x2=432.92 y2=204.17>",
                "<Cell x1=432.92 y1=193.53 x2=540.42 y2=204.17>"
            },
            new string[]
            {
                "<Cell x1=72 y1=178.98 x2=168.58 y2=193.53>",
                "<Cell x1=168.58 y1=178.98 x2=208.06 y2=193.53>",
                "<Cell x1=208.06 y1=178.98 x2=258.91 y2=193.53>",
                "<Cell x1=258.91 y1=178.98 x2=317.53 y2=193.53>",
                "<Cell x1=317.53 y1=178.98 x2=373.2 y2=193.53>",
                "<Cell x1=373.2 y1=178.98 x2=432.92 y2=193.53>",
                "<Cell x1=432.92 y1=178.98 x2=540.42 y2=193.53>"
            },
            new string[]
            {
                "<Cell x1=72 y1=163.47 x2=168.58 y2=178.98>",
                "<Cell x1=168.58 y1=163.47 x2=208.06 y2=178.98>",
                "<Cell x1=208.06 y1=163.47 x2=258.91 y2=178.98>",
                "<Cell x1=258.91 y1=163.47 x2=317.53 y2=178.98>",
                "<Cell x1=317.53 y1=163.47 x2=373.2 y2=178.98>",
                "<Cell x1=373.2 y1=163.47 x2=432.92 y2=178.98>",
                "<Cell x1=432.92 y1=163.47 x2=540.42 y2=178.98>"
            },
            new string[]
            {
                "<Cell x1=72 y1=147.99 x2=168.58 y2=163.47>",
                "<Cell x1=168.58 y1=147.99 x2=208.06 y2=163.47>",
                "<Cell x1=208.06 y1=147.99 x2=258.91 y2=163.47>",
                "<Cell x1=258.91 y1=147.99 x2=317.53 y2=163.47>",
                "<Cell x1=317.53 y1=147.99 x2=373.2 y2=163.47>",
                "<Cell x1=373.2 y1=147.99 x2=432.92 y2=163.47>",
                "<Cell x1=432.92 y1=147.99 x2=540.42 y2=163.47>"
            },
            new string[]
            {
                "<Cell x1=72 y1=132.48 x2=168.58 y2=147.99>",
                "<Cell x1=168.58 y1=132.48 x2=208.06 y2=147.99>",
                "<Cell x1=208.06 y1=132.48 x2=258.91 y2=147.99>",
                "<Cell x1=258.91 y1=132.48 x2=317.53 y2=147.99>",
                "<Cell x1=317.53 y1=132.48 x2=373.2 y2=147.99>",
                "<Cell x1=373.2 y1=132.48 x2=432.92 y2=147.99>",
                "<Cell x1=432.92 y1=132.48 x2=540.42 y2=147.99>"
            },
            new string[]
            {
                "<Cell x1=72 y1=119.7 x2=168.58 y2=132.48>",
                "<Cell x1=168.58 y1=119.7 x2=208.06 y2=132.48>",
                "<Cell x1=208.06 y1=119.7 x2=258.91 y2=132.48>",
                "<Cell x1=258.91 y1=119.7 x2=317.53 y2=132.48>",
                "<Cell x1=317.53 y1=119.7 x2=373.2 y2=132.48>",
                "<Cell x1=373.2 y1=119.7 x2=432.92 y2=132.48>",
                "<Cell x1=432.92 y1=119.7 x2=540.42 y2=132.48>"
            }
        };

        // left, bottom, right, top
        private readonly bool[][][] FooBorders = new bool[][][]
        {
            new bool[][]
            {
                new bool[] { true, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, true, true }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, true, false }
            },
            new bool[][]
            {
                new bool[] { true, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, true, false }
            }
        };

        // hspan, vspan
        private readonly bool[][][] FooSpans = new bool[][][]
        {
            new bool[][]
            {
                new bool[] { false, false },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { false, false }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true }
            },
            new bool[][]
            {
                new bool[] { false, false },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { true, true },
                new bool[] { false, false }
            }
        };

        private readonly int[][] FooBounds = new int[][]
        {
            new int[]{ 2, 1, 1, 1, 1, 1, 2, },
            new int[]{ 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 1, 0, 0, 0, 0, 0, 1, },
            new int[] { 2, 1, 1, 1, 1, 1, 2, }
        };

        private readonly bool[][][] FooEdges = new bool[][][]
        {
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true },
                new bool[] { false, false, false, true }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, false, true, false },
                new bool[] { true, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            },
            new bool[][]
            {
                new bool[] { true, false, false, false },
                new bool[] { false, true, true, false },
                new bool[] { true, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false },
                new bool[] { false, true, false, false }
            }
        };

        [Fact]
        public void FooTable()
        {
            Table table = new Table(FooCols, FooRows);

            // cells
            Assert.Equal(FooCells.Length, table.cells.Count);

            for (int r = 0; r < table.cells.Count; r++)
            {
                var expectedRow = FooCells[r];
                var row = table.cells[r];
                Assert.Equal(expectedRow.Length, row.Count);

                for (int c = 0; c < row.Count; c++)
                {
                    Assert.Equal(expectedRow[c], row[c].ToString());
                }
            }

            // set borders
            for (int r = 0; r < table.cells.Count; r++)
            {
                var row = table.cells[r];
                for (int c = 0; c < row.Count; c++)
                {
                    // left, bottom, right, top
                    Assert.False(row[c].left);
                    Assert.False(row[c].bottom);
                    Assert.False(row[c].right);
                    Assert.False(row[c].top);
                }
            }

            table.set_border();
            for (int r = 0; r < table.cells.Count; r++)
            {
                var expectedRow = FooBorders[r];
                var row = table.cells[r];
                Assert.Equal(expectedRow.Length, row.Count);

                for (int c = 0; c < row.Count; c++)
                {
                    // left, bottom, right, top
                    Assert.Equal(expectedRow[c][0], row[c].left);
                    Assert.Equal(expectedRow[c][1], row[c].bottom);
                    Assert.Equal(expectedRow[c][2], row[c].right);
                    Assert.Equal(expectedRow[c][3], row[c].top);
                }
            }

            // set spans
            for (int r = 0; r < table.cells.Count; r++)
            {
                var row = table.cells[r];
                for (int c = 0; c < row.Count; c++)
                {
                    Assert.False(row[c].hspan);
                    Assert.False(row[c].vspan);
                }
            }

            table.set_span();
            for (int r = 0; r < table.cells.Count; r++)
            {
                var expectedRow = FooSpans[r];
                var row = table.cells[r];
                Assert.Equal(expectedRow.Length, row.Count);

                for (int c = 0; c < row.Count; c++)
                {
                    // hspan, vspan
                    Assert.Equal(expectedRow[c][0], row[c].hspan);
                    Assert.Equal(expectedRow[c][1], row[c].vspan);
                }
            }

            // check bounds
            for (int r = 0; r < table.cells.Count; r++)
            {
                var expectedRow = FooBounds[r];
                var row = table.cells[r];
                Assert.Equal(expectedRow.Length, row.Count);

                for (int c = 0; c < row.Count; c++)
                {
                    Assert.Equal(expectedRow[c], row[c].bound);
                }
            }
        }

        [Fact]
        public void FooSetAllEdges()
        {
            Table table = new Table(FooCols, FooRows);
            table.set_all_edges();

            for (int r = 0; r < table.cells.Count; r++)
            {
                var row = table.cells[r];
                for (int c = 0; c < row.Count; c++)
                {
                    // left, bottom, right, top
                    Assert.True(row[c].left);
                    Assert.True(row[c].bottom);
                    Assert.True(row[c].right);
                    Assert.True(row[c].top);
                }
            }
        }

        [Fact]
        public void FooSetEdges()
        {
            Table table = new Table(FooCols, FooRows);

            List<float[]> vertical = new List<float[]>()
            {
                new float[] {71, 71, 71, 275 },
                new float[] {168, 168, 257, 168 },
                new float[] {208, 168, 235, 235 },
                new float[] {208, 258, 235, 235 }
            };

            List<float[]> horizontal = new List<float[]>()
            {
                new float[] {71, 71, 71, 275 },
                new float[] {168, 168, 257, 168 },
                new float[] {208, 168, 235, 235 },
                new float[] {208, 258, 235, 373 }
            };

            table.set_edges(vertical, horizontal, 2);

            for (int r = 0; r < table.cells.Count; r++)
            {
                var expectedRow = FooEdges[r];
                var row = table.cells[r];
                for (int c = 0; c < row.Count; c++)
                {
                    // left, bottom, right, top
                    Assert.Equal(expectedRow[c][0], row[c].left);
                    Assert.Equal(expectedRow[c][1], row[c].bottom);
                    Assert.Equal(expectedRow[c][2], row[c].right);
                    Assert.Equal(expectedRow[c][3], row[c].top);
                }
            }
        }
    }
}
