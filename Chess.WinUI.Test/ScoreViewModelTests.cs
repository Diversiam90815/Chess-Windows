using Chess.UI.Score;
using Chess.UI.Services;
using Chess.UI.ViewModels;
using Chess.UI.Wrappers;
using Moq;
using System.Reflection;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class ScoreViewModelTests
    {
        private readonly Mock<IDispatcherQueueWrapper> _dispatcher = new Mock<IDispatcherQueueWrapper>();
        private readonly Mock<IScoreModel> _model = new Mock<IScoreModel>();
        private readonly Mock<IImageService> _images = new Mock<IImageService>();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new Mock<IServiceProvider>();

        public ScoreViewModelTests()
        {
            // Setup score model with default dictionaries
            _model.Setup(s => s.WhiteCapturedPieces)
                .Returns(new Dictionary<PieceTypeInstance, int>
                {
                    { PieceTypeInstance.Pawn, 0 },
                    { PieceTypeInstance.Bishop, 0 },
                    { PieceTypeInstance.Knight, 0 },
                    { PieceTypeInstance.Rook, 0 },
                    { PieceTypeInstance.Queen, 0 }
                });

            _model.Setup(s => s.BlackCapturedPieces)
                .Returns(new Dictionary<PieceTypeInstance, int>
                {
                    { PieceTypeInstance.Pawn, 0 },
                    { PieceTypeInstance.Bishop, 0 },
                    { PieceTypeInstance.Knight, 0 },
                    { PieceTypeInstance.Rook, 0 },
                    { PieceTypeInstance.Queen, 0 }
                });

            // Setup image service mock
            _images.Setup(i => i.GetCapturedPieceImage(It.IsAny<PlayerColor>(), It.IsAny<PieceTypeInstance>()));

            // Setup service provider
            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IScoreModel)))
                .Returns(_model.Object);

            _serviceProviderMock
                .Setup(s => s.GetService(typeof(IImageService)))
                .Returns(_images.Object);

            // Initialize the App.Current.Services mock
            SetupAppServiceProvider(_serviceProviderMock.Object);
        }

        [Fact]
        public void OnPlayerScoreUpdated_UpdatesScore()
        {
            // Arrange
            var viewModel = new ScoreViewModel(_dispatcher.Object, _model.Object, _images.Object);
            SetPrivateField(viewModel, "_scoreModel", _model.Object);

            // Act
            viewModel.OnPlayerScoreUpdated(new EngineAPI.Score { player = PlayerColor.White, score = 5 });

            // Assert
            Assert.Equal(5, viewModel.WhiteScoreValue);

            // Act again with Black's score
            viewModel.OnPlayerScoreUpdated(new EngineAPI.Score { player = PlayerColor.Black, score = 3 });

            // Assert again
            Assert.Equal(3, viewModel.BlackScoreValue);
        }

        [Fact]
        public void ReinitScoreValues_ResetsAllValues()
        {
            // Arrange
            var viewModel = new ScoreViewModel(_dispatcher.Object, _model.Object, _images.Object);
            SetPrivateField(viewModel, "_scoreModel", _model.Object);

            viewModel.BlackScoreValue = 5;
            viewModel.WhiteScoreValue = 8;

            // Act
            viewModel.ReinitScoreValues();

            // Assert
            Assert.Equal(0, viewModel.BlackScoreValue);
            Assert.Equal(0, viewModel.WhiteScoreValue);
        }


        // Helper method to setup App.Current.Services
        private void SetupAppServiceProvider(IServiceProvider serviceProvider)
        {
            // Same as in MoveHistoryViewModelTests
            // Create a mock App for testing
            if (App.Current == null)
            {
                var appField = typeof(App).GetField("_current",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

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

        private static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.Instance |
                BindingFlags.NonPublic);
            field?.SetValue(obj, value);
        }
    }
}
