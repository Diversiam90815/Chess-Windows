using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Chess.UI.Audio.Core
{
    public class AudioEngine : IAudioEngine, IDisposable
    {
        private readonly ConcurrentDictionary<AudioModuleType, IAudioModule> _modules;
        private float _masterVolume = 1.0f;
        private bool _isInitialized = false;

        public event EventHandler<AudioEngineEventArgs> ModuleRegistered;
        public event EventHandler<AudioEngineEventArgs> ModuleUnregistered;
        public event EventHandler<AudioErrorEventArgs> AudioError;


        public AudioEngine()
        {
            _modules = new ConcurrentDictionary<AudioModuleType, IAudioModule>();
        }


        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                // initialize all registered modules
                var initTasks = new List<Task>();

                foreach (var module in _modules)
                {
                    initTasks.Add(module.Value.InitializeAsync());
                }

                await Task.WhenAll(initTasks);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                AudioError?.Invoke(this, new AudioErrorEventArgs("Failed to initialize audio engine", ex));
                throw;
            }
        }


        public void RegisterModule(AudioModuleType type, IAudioModule module)
        {
            if (module is null) return;

            if (_modules.TryAdd(type, module))
            {
                module.StatusChanged += OnModuleStatusChanged;
                ModuleRegistered?.Invoke(this, new AudioEngineEventArgs(module.ModuleName, type));
            }

        }


        public T GetModule<T>(AudioModuleType moduleType) where T : class, IAudioModule
        {
            _modules.TryGetValue(moduleType, out var module);
            return module as T;
        }


        public IAudioModule GetModule(AudioModuleType moduleType)
        {
            _modules.TryGetValue(moduleType, out var module);
            return module;
        }


        public void UnregisterModule(AudioModuleType moduleType)
        {
            if (_modules.TryRemove(moduleType, out var module))
            {
                module.StatusChanged -= OnModuleStatusChanged;
                module.Dispose();
                ModuleUnregistered?.Invoke(this, new AudioEngineEventArgs(module.ModuleName, moduleType));
            }
        }


        public void SetMasterVolume(float masterVolume)
        {
            _masterVolume = Math.Clamp(masterVolume, 0.0f, 1.0f);

            // Update all modules with new master volume
            foreach (var module in _modules.Values)
            {
                module.SetMasterVolume(_masterVolume);
            }
        }


        public float GetMasterVolume() => _masterVolume;


        private void OnModuleStatusChanged(object sender, AudioModuleEventArgs e)
        {
            // Propagate event if needed
        }


        public void Dispose()
        {
            foreach (var module in _modules.Values)
            {
                module.Dispose();
            }
            _modules.Clear();
            _isInitialized = false;
        }

    }
}
