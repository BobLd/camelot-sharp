using Xunit;
using static Camelot.Core;

namespace Camelot.Tests
{
    public class TextEdgeTests
    {
        [Fact]
        public void UpdateCoords()
        {
            var te0 = new TextEdge(0, 1, 2);
            Assert.Equal(0, te0.x, 4);
            Assert.Equal(1, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.False(te0.is_valid);
            Assert.Equal(0, te0.intersections);

            te0.update_coords(1, 10);
            Assert.Equal(1.0, te0.x, 4);
            Assert.Equal(10, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.False(te0.is_valid);
            Assert.Equal(1, te0.intersections);

            te0.update_coords(1.54f, 8.5f);
            Assert.Equal(1.27, te0.x, 4);
            Assert.Equal(8.5, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.False(te0.is_valid);
            Assert.Equal(2, te0.intersections);

            te0.update_coords(2.48f, 5.56f);
            Assert.Equal(1.6733333333333331, te0.x, 4);
            Assert.Equal(5.56, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.False(te0.is_valid);
            Assert.Equal(3, te0.intersections);

            te0.update_coords(7.8f, 15.94f);
            Assert.Equal(3.205, te0.x, 4);
            Assert.Equal(15.94, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.False(te0.is_valid);
            Assert.Equal(4, te0.intersections);

            te0.update_coords(4.8f, 5.41f);
            Assert.Equal(3.524, te0.x, 4);
            Assert.Equal(5.41, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.True(te0.is_valid);
            Assert.Equal(5, te0.intersections);

            // more than 50, intersection count is unchanged
            te0.update_coords(4.8f, 60.41f);
            Assert.Equal(3.524, te0.x, 4);
            Assert.Equal(5.41, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.True(te0.is_valid);
            Assert.Equal(5, te0.intersections);

            te0.update_coords(80.8f, 6.41f);
            Assert.Equal(16.403333333333332, te0.x, 4);
            Assert.Equal(6.41, te0.y0, 4);
            Assert.Equal(2, te0.y1, 4);
            Assert.Equal("left", te0.align);
            Assert.True(te0.is_valid);
            Assert.Equal(6, te0.intersections);
        }
    }
}
