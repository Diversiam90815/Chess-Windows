using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface IGameConfigurationService
    {
        Task<bool> StartGameAsync(GameConfiguration config);
    }


    public class GameConfigurationService : IGameConfigurationService
    {
        private readonly INavigationService _navigationService;
        private readonly ChessBoardViewModel _chessboardViewModel;


        public GameConfigurationService(INavigationService navigationService, ChessBoardViewModel chessboardViewModel)
        {
            _navigationService = navigationService;
            _chessboardViewModel = chessboardViewModel;
        }


        public async Task<bool> StartGameAsync(GameConfiguration config)
        {
            if(!config.IsValid())
            {
                throw new ArgumentException($"Invalid game configuration for mode : {config.Mode}", nameof(config));
            }

            switch (config.Mode)
            {
                case GameModeSelection.LocalCoop:
                    return await StartLocalCoopGameAsync(config);

                case GameModeSelection.SinglePlayer:
                    return await StartSinglePlayerGameAsync(config);

                case GameModeSelection.MultiPlayer:
                    return await StartMultiplayerGameAsync(config);

                default:
                    throw new NotSupportedException($"Game mode not supported: {config.Mode}");
            }
        }


        public async Task<bool> StartLocalCoopGameAsync(GameConfiguration config)
        {
            _chessboardViewModel.IsMultiplayerGame = false;
            _chessboardViewModel.IsKoopGame = true;

            return await _navigationService.NavigateToChessboardAsync(false, config);
        }


        public async Task<bool> StartSinglePlayerGameAsync(GameConfiguration config)
        {
            _chessboardViewModel.IsMultiplayerGame = false;
            _chessboardViewModel.IsKoopGame = false;

            return await _navigationService.NavigateToChessboardAsync(false, config);
        }


        private async Task<bool> StartMultiplayerGameAsync(GameConfiguration config)
        {
            _chessboardViewModel.IsMultiplayerGame = true;
            _chessboardViewModel.IsKoopGame = false;

            return await _navigationService.NavigateToChessboardAsync(true, config);
        }
    }
}
