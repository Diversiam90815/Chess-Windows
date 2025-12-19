using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface IGameConfigurationService
    {
        Task<bool> StartGameAsync(GameConfiguration config);
        Task<bool> StartLocalCoopGameAsync(GameConfiguration config);
        Task<bool> StartCpuGameAsync(GameConfiguration config);
    }


    public class GameConfigurationService : IGameConfigurationService
    {
        private readonly INavigationService _navigationService;


        public GameConfigurationService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }


        public async Task<bool> StartGameAsync(GameConfiguration config)
        {
            return config.Mode switch
            {
                GameModeSelection.LocalCoop => await StartLocalCoopGameAsync(config),
                GameModeSelection.VsCPU => await StartCpuGameAsync(config),
                GameModeSelection.None => false,
                _ => false
            };
        }


        public async Task<bool> StartLocalCoopGameAsync(GameConfiguration config)
        {
            var chessBoardViewModel = App.Current.Services.GetService<ChessBoardViewModel>();
            chessBoardViewModel.IsMultiplayerGame = false;
            chessBoardViewModel.IsKoopGame = true;

            return await _navigationService.NavigateToChessboardAsync(false, config);
        }


        public async Task<bool> StartCpuGameAsync(GameConfiguration config)
        {
            var chessBoardViewModel = App.Current.Services.GetService<ChessBoardViewModel>();
            chessBoardViewModel.IsMultiplayerGame = false;
            chessBoardViewModel.IsKoopGame = false;

            return await _navigationService.NavigateToChessboardAsync(false, config);
        }
    }
}
