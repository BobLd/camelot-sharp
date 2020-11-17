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
            Assert.Equal(0, te0.X, 4);
            Assert.Equal(1, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.False(te0.IsValid);
            Assert.Equal(0, te0.Intersections);

            te0.UpdateCoords(1, 10);
            Assert.Equal(1.0, te0.X, 4);
            Assert.Equal(10, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.False(te0.IsValid);
            Assert.Equal(1, te0.Intersections);

            te0.UpdateCoords(1.54f, 8.5f);
            Assert.Equal(1.27, te0.X, 4);
            Assert.Equal(8.5, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.False(te0.IsValid);
            Assert.Equal(2, te0.Intersections);

            te0.UpdateCoords(2.48f, 5.56f);
            Assert.Equal(1.6733333333333331, te0.X, 4);
            Assert.Equal(5.56, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.False(te0.IsValid);
            Assert.Equal(3, te0.Intersections);

            te0.UpdateCoords(7.8f, 15.94f);
            Assert.Equal(3.205, te0.X, 4);
            Assert.Equal(15.94, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.False(te0.IsValid);
            Assert.Equal(4, te0.Intersections);

            te0.UpdateCoords(4.8f, 5.41f);
            Assert.Equal(3.524, te0.X, 4);
            Assert.Equal(5.41, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.True(te0.IsValid);
            Assert.Equal(5, te0.Intersections);

            // more than 50, intersection count is unchanged
            te0.UpdateCoords(4.8f, 60.41f);
            Assert.Equal(3.524, te0.X, 4);
            Assert.Equal(5.41, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.True(te0.IsValid);
            Assert.Equal(5, te0.Intersections);

            te0.UpdateCoords(80.8f, 6.41f);
            Assert.Equal(16.403333333333332, te0.X, 4);
            Assert.Equal(6.41, te0.Y0, 4);
            Assert.Equal(2, te0.Y1, 4);
            Assert.Equal("left", te0.Align);
            Assert.True(te0.IsValid);
            Assert.Equal(6, te0.Intersections);
        }
    }
}
