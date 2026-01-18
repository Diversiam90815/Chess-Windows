using Chess.UI.Models;
using Chess.UI.ViewModels;
using Chess.UI.Wrappers;
using Moq;


namespace Chess.UI.Test
{
    public class MoveHistoryViewModelTests
    {
        private readonly Mock<IDispatcherQueueWrapper> _dispatcher = new Mock<IDispatcherQueueWrapper>();
        private readonly Mock<IMoveHistoryModel> _model = new Mock<IMoveHistoryModel>();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new Mock<IServiceProvider>();

        public MoveHistoryViewModelTests()
        {
            // Setup service provider mock
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IMoveHistoryModel)))
                .Returns(_model.Object);

            _serviceProviderMock.Setup(s => s.GetService(typeof(IDispatcherQueueWrapper))).Returns(_dispatcher.Object);

            // Initialize the App.Current.Services mock
            SetupAppServiceProvider(_serviceProviderMock.Object);
        }

        [Fact]
        public void AddMove_DistributesMovesAcrossColumns()
        {
            // Arrange
            var viewModel = new MoveHistoryViewModel(_dispatcher.Object, _model.Object);

            // Act
            viewModel.AddMove("e4");
            viewModel.AddMove("e5");
            viewModel.AddMove("Nf3");

            // Assert
            Assert.Single(viewModel.MoveHistoryColumns[0]);
            Assert.Single(viewModel.MoveHistoryColumns[1]);
            Assert.Single(viewModel.MoveHistoryColumns[2]);
            Assert.Equal("e4", viewModel.MoveHistoryColumns[0][0]);
        }

        [Fact]
        public void ClearMoveHistory_RemovesAllMoves()
        {
            // Arrange
            var viewModel = new MoveHistoryViewModel(_dispatcher.Object, _model.Object);
            viewModel.AddMove("e4");
            viewModel.AddMove("e5");

            // Act
            viewModel.ClearMoveHistory();

            // Assert
            Assert.Empty(viewModel.MoveHistoryColumns[0]);
            Assert.Empty(viewModel.MoveHistoryColumns[1]);
            Assert.Empty(viewModel.MoveHistoryColumns[2]);
        }

        [Fact]
        public void RemoveLastMove_CallsModelMethod()
        {
            // Arrange
            var viewModel = new MoveHistoryViewModel(_dispatcher.Object, _model.Object);

            // Act
            viewModel.RemoveLastMove();

            // Assert
            _model.Verify(m => m.RemoveLastMove(), Times.Once);
        }


        // Helper method to setup App.Current.Services
        private void SetupAppServiceProvider(IServiceProvider serviceProvider)
        {
            // Create a mock App for testing
            if (App.Current == null)
            {
                var appField = typeof(App).GetField("_current",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.NonPublic);

                if (appField != null)
                {
                    var mockApp = new Mock<App>();
                    mockApp.Setup(a => a.Services).Returns(serviceProvider);
                    appField.SetValue(null, mockApp.Object);
                }
            }
            else
            {
                // If App.Current exists, set its Services property
                var servicesProperty = typeof(App).GetProperty("Services");
                servicesProperty?.SetValue(App.Current, serviceProvider);
            }
        }
    }
}
