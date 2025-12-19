using System;
using System.Threading.Tasks;


namespace Chess.UI.Audio.Core
{
    public interface IAudioEngine
    {
        // Lifecycle
        Task InitializeAsync();
        void Dispose();

        // Module Management
        void RegisterModule(AudioModuleType moduleType, IAudioModule module);
        T GetModule<T>(AudioModuleType moduleType) where T : class, IAudioModule;
        IAudioModule GetModule(AudioModuleType moduleType);
        void UnregisterModule(AudioModuleType moduleType);

        // Global controls
        void SetMasterVolume(float volume);
        float GetMasterVolume();

        // Events
        event EventHandler<AudioEngineEventArgs> ModuleRegistered;
        event EventHandler<AudioEngineEventArgs> ModuleUnregistered;
        event EventHandler<AudioErrorEventArgs> AudioError;
    }


    public interface IAudioModule
    {
        public string ModuleName { get; }
        public AudioModuleType ModuleType { get; }

        Task InitializeAsync();
        void Dispose();

        void SetModuleVolume(float volume);          // Sets the module's own volume level
        float GetModuleVolume();                     // Gets the module's own volume level
        void SetMasterVolume(float masterVolume);    // Updates master volume for final calculation
        float GetEffectiveVolume();                  // Returns module volume * master volume

        event EventHandler<AudioModuleEventArgs> StatusChanged;
    }


    public class AudioEngineEventArgs : EventArgs
    {
        public string ModuleName { get; }
        public AudioModuleType ModuleType { get; }

        public AudioEngineEventArgs(string moduleName, AudioModuleType moduleType)
        {
            ModuleName = moduleName;
            ModuleType = moduleType;
        }
    }

    public class AudioModuleEventArgs : EventArgs
    {
        public string ModuleName { get; }
        public string Status { get; }

        public AudioModuleEventArgs(string moduleName, string status)
        {
            ModuleName = moduleName;
            Status = status;
        }
    }

    public class AudioErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }
        public Exception Exception { get; }
        public string ModuleName { get; }

        public AudioErrorEventArgs(string errorMessage, Exception exception = null, string moduleName = null)
        {
            ErrorMessage = errorMessage;
            Exception = exception;
            ModuleName = moduleName;
        }
    }


    public enum AudioModuleType
    {
        SFX,
        Music,
        Atmosphere,
    }
}
