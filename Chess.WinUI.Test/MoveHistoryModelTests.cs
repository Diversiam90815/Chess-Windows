using Chess.UI.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.Test
{
    public class MoveHistoryModelTests
    {
        [Fact]
        public void RemoveLastMove_WhenHistoryExists_RemovesLastMove()
        {
            // Arrange
            var model = new MockMoveHistoryModel();
            model.MoveHistory.Add("e4");
            model.MoveHistory.Add("e5");
            bool eventRaised = false;
            model.MoveHistoryUpdated += () => eventRaised = true;

            // Act
            model.RemoveLastMove();

            // Assert
            Assert.Single(model.MoveHistory);
            Assert.Equal("e4", model.MoveHistory[0]);
            Assert.True(eventRaised);
        }

        [Fact]
        public void RemoveLastMove_WhenHistoryEmpty_DoesNothing()
        {
            // Arrange
            var model = new MockMoveHistoryModel();
            bool eventRaised = false;
            model.MoveHistoryUpdated += () => eventRaised = true;

            // Act
            model.RemoveLastMove();

            // Assert
            Assert.Empty(model.MoveHistory);
            Assert.False(eventRaised);
        }

        // Mock implementation for testing
        private class MockMoveHistoryModel : IMoveHistoryModel
        {
            public List<string> MoveHistory { get; } = new List<string>();

            public event Action? MoveHistoryUpdated;

            public void RemoveLastMove()
            {
                if (MoveHistory.Count > 0)
                {
                    MoveHistory.RemoveAt(MoveHistory.Count - 1);
                    MoveHistoryUpdated?.Invoke();
                }
            }
        }
    }
}
