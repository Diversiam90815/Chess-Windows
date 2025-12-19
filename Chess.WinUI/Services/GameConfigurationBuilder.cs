using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface IGameConfigurationBuilder
    {
        void Reset();
        void SetGameMode(GameModeSelection mode);
        void SetPlayerColor(PlayerColor player);
        void SetCPUDifficulty(CPUDifficulty difficulty);
        GameConfiguration GetConfiguration();
    }
    
    
    public class GameConfigurationBuilder : IGameConfigurationBuilder
    {
        private GameConfiguration _config;


        public GameConfigurationBuilder()
        {
            Reset();
        }


        public void Reset()
        {
            _config = new GameConfiguration
            {
                Mode = GameModeSelection.None,
                PlayerColor = PlayerColor.White,
                CpuDifficulty = 0
            };
        }


        public void SetGameMode(GameModeSelection mode)
        {
            _config.Mode = mode;
        }


        public void SetPlayerColor(PlayerColor player)
        {
            _config.PlayerColor = player;
        }


        public void SetCPUDifficulty(CPUDifficulty difficulty)
        {
            _config.CpuDifficulty = difficulty;
        }


        public GameConfiguration GetConfiguration()
        {
            return _config;
        }
    }
}
