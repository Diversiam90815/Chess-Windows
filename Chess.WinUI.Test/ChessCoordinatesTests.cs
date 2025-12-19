using Chess.UI.Coordinates;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class ChessCoordinateTests
    {
        [Fact]
        public void GetNumBoardSquares_Returns64()
        {
            // Arrange
            var coordinate = new ChessCoordinate();

            // Act
            int squares = coordinate.GetNumBoardSquares();

            // Assert
            Assert.Equal(64, squares);
        }

        [Theory]
        [InlineData(0, 0, 0)]  // Bottom-left corner
        [InlineData(7, 7, 0)]  // Top-left corner
        [InlineData(63, 7, 7)] // Top-right corner
        public void FromIndex_ReturnsCorrectPosition(int index, int expectedX, int expectedY)
        {
            // Arrange
            var coordinate = new ChessCoordinate();

            // Act
            var position = coordinate.FromIndex(index);

            // Assert
            Assert.Equal(expectedX, position.x);
            Assert.Equal(expectedY, position.y);
        }

        [Theory]
        [InlineData(0, 0, 0)]   // Bottom-left corner
        [InlineData(7, 7, 63)]  // Top-right corner
        [InlineData(4, 5, 44)]  // Middle position
        public void ToIndex_ReturnsCorrectIndex(int x, int y, int expectedIndex)
        {
            // Arrange
            var coordinate = new ChessCoordinate();
            var position = new PositionInstance(x, y);

            // Act
            int index = coordinate.ToIndex(position);

            // Assert
            Assert.Equal(expectedIndex, index);
        }

        [Theory]
        [InlineData(0, 0, 0, 7)]  // Bottom-left to top-left
        [InlineData(7, 7, 7, 0)]  // Top-right to bottom-right
        [InlineData(3, 4, 3, 3)]
        public void ToDisplayCoordinates_ConvertsCorrectly(int engineX, int engineY, int displayX, int displayY)
        {
            // Arrange
            var coordinate = new ChessCoordinate();
            var enginePos = new PositionInstance(engineX, engineY);

            // Act
            var displayPos = coordinate.ToDisplayCoordinates(enginePos);

            // Assert
            Assert.Equal(displayX, displayPos.x);
            Assert.Equal(displayY, displayPos.y);
        }

        [Theory]
        [InlineData(0, 7, 0, 0)]  // Top-left to bottom-left
        [InlineData(7, 0, 7, 7)]  // Bottom-right to top-right
        [InlineData(3, 3, 3, 4)]
        public void FromDisplayCoordinates_ConvertsCorrectly(int displayX, int displayY, int engineX, int engineY)
        {
            // Arrange
            var coordinate = new ChessCoordinate();
            var displayPos = new PositionInstance(displayX, displayY);

            // Act
            var enginePos = coordinate.FromDisplayCoordinates(displayPos);

            // Assert
            Assert.Equal(engineX, enginePos.x);
            Assert.Equal(engineY, enginePos.y);
        }
    }
}