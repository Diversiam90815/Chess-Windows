using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.ViewModels
{
    public class MultiplayerInfoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IChessGameService _gameService;
        private readonly IMoveModel _moveModel;


        public MultiplayerInfoViewModel(IDispatcherQueueWrapper dispatcherQueue, IChessGameService gameService, IMoveModel moveModel)
        {
            _dispatcherQueue = dispatcherQueue;
            _gameService = gameService;
            _moveModel = moveModel;

            // Get MultiplayerViewModel for player info
            var multiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();

            // Subscribe to events
            _gameService.ConfigurationChanged += OnConfigurationChanged;
            _moveModel.PlayerChanged += OnPlayerChanged;

            if (multiplayerViewModel != null)
            {
                multiplayerViewModel.PropertyChanged += OnMultiplayerViewModelPropertyChanged;
                LocalPlayer = multiplayerViewModel.LocalPlayer;
            }
        }


        private void OnConfigurationChanged(GameConfiguration config)
        {
            IsVisible = config.Mode == GameModeSelection.MultiPlayer;
        }


        private void OnPlayerChanged(Side currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            IsLocalPlayersTurn = LocalPlayer == currentPlayer;
        }


        private void OnMultiplayerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MultiplayerViewModel.LocalPlayer))
            {
                var mpViewModel = sender as MultiplayerViewModel;
                if (mpViewModel != null)
                {
                    LocalPlayer = mpViewModel.LocalPlayer;
                }
            }
        }


        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        private Side _localPlayer;
        public Side LocalPlayer
        {
            get => _localPlayer;
            set
            {
                if (_localPlayer != value)
                {
                    _localPlayer = value;
                    OnPropertyChanged();
                }
            }
        }


        private Side _currentPlayer;
        public Side CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                if (_currentPlayer != value)
                {
                    _currentPlayer = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isLocalPlayersTurn;
        public bool IsLocalPlayersTurn
        {
            get => _isLocalPlayersTurn;
            set
            {
                if (_isLocalPlayersTurn != value)
                {
                    _isLocalPlayersTurn = value;
                    OnPropertyChanged();
                }
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
