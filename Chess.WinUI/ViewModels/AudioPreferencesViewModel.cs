using Chess.UI.Audio.Modules;
using Chess.UI.Audio.Services;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;



namespace Chess.UI.ViewModels
{
    public record AtmosphereScenarioItem
    {
        public string DisplayName { get; }
        public Audio.Modules.AtmosphereScenario Scenario { get; }

        public AtmosphereScenarioItem(Audio.Modules.AtmosphereScenario scenario)
        {
            Scenario = scenario;
            DisplayName = scenario.ToString();
        }
    }


    public class AudioPreferencesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;


        public ObservableCollection<AtmosphereScenarioItem> AtmosphereSoundscapes { get; }

        private readonly IChessAudioService _audioService;

        public event Action ItemSelected;


        public AudioPreferencesViewModel(IDispatcherQueueWrapper dispatcher)
        {
            _dispatcherQueue = dispatcher;

            _audioService = App.Current.Services.GetService<IChessAudioService>();

            AtmosphereSoundscapes = new ObservableCollection<AtmosphereScenarioItem>();

            IntiializeAtmosphereScenarios();
            InitializeValues();
        }


        private void InitializeValues()
        {
            float masterEngineVolume = _audioService.GetMasterVolume();
            int masterVolume = (int)masterEngineVolume * 100;

            float sfxEngineVolume = _audioService.GetSFXVolume();
            int sfxVolume = (int)(sfxEngineVolume * 100);

            float atmosEngineVolume = _audioService.GetAtmosphereVolume();
            int atmosVolume = (int)(atmosEngineVolume * 100);

            bool sfxEnabled = _audioService.GetSFXEnabled();
            bool atmosEnabled = _audioService.GetAtmosphereEnabled();

            AtmosphereScenario currentScenario = _audioService.GetCurrentAtmosphere();

            SfxEnabled = sfxEnabled;
            AtmosEnabled = atmosEnabled;

            MasterVolume = masterVolume;
            SfxVolume = sfxVolume;
            AtmosVolume = atmosVolume;

            SelectedAtmosphereSoundscape = AtmosphereSoundscapes.FirstOrDefault(a => a.Scenario == currentScenario);
        }


        private void IntiializeAtmosphereScenarios()
        {
            foreach (var scenario in Enum.GetValues<AtmosphereScenario>())
            {
                AtmosphereSoundscapes.Add(new AtmosphereScenarioItem(scenario));
            }
        }


        private AtmosphereScenarioItem selectedAtmosphereSoundscape;
        public AtmosphereScenarioItem SelectedAtmosphereSoundscape
        {
            get => selectedAtmosphereSoundscape;
            set
            {
                if (value == selectedAtmosphereSoundscape) return;
                selectedAtmosphereSoundscape = value;
                ItemSelected?.Invoke();
                _audioService.SetAtmosphereAsync(value.Scenario);
            }
        }


        private int masterVolume;
        public int MasterVolume
        {
            get => masterVolume;
            set
            {
                if (value == masterVolume) return;
                masterVolume = value;
                SetMasterVolumeInEngine(masterVolume);
                OnPropertyChanged();
            }
        }


        private int sfxVolume;
        public int SfxVolume
        {
            get => sfxVolume;
            set
            {
                if (value == sfxVolume) return;
                sfxVolume = value;
                SetSfxVolumeInEngine(sfxVolume);
                OnPropertyChanged();
            }
        }


        private int atmosVolume;
        public int AtmosVolume
        {
            get => atmosVolume;
            set
            {
                if (value == atmosVolume) return;
                atmosVolume = value;
                SetAtmosVolumeInEngine(atmosVolume);
                OnPropertyChanged();
            }
        }


        private bool sfxEnabled;
        public bool SfxEnabled
        {
            get => sfxEnabled;
            set
            {
                if (value == sfxEnabled) return;
                sfxEnabled = value;
                _audioService.SetSFXEnabled(value);
                OnPropertyChanged();
            }
        }


        private bool atmosEnabled;
        public bool AtmosEnabled
        {
            get => atmosEnabled;
            set
            {
                if (value == atmosEnabled) return;
                atmosEnabled = value;
                _audioService.SetAtmosphereEnabled(value);
                OnPropertyChanged();
            }
        }


        public void SetSfxVolumeInEngine(int volume)
        {
            double engineVolume = Math.Round(volume / 100.0f, 1);
            _audioService.SetSFXVolume((float)engineVolume);
        }


        public void SetAtmosVolumeInEngine(int volume)
        {
            double engineVolume = Math.Round(volume / 100.0f, 1);
            _audioService.SetAtmosphereVolume((float)engineVolume);
        }


        public void SetMasterVolumeInEngine(int volume)
        {
            double engineVolume = Math.Round(volume / 100.0f, 1);
            _audioService.SetMasterVolume((float)engineVolume);
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
