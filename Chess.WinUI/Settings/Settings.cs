using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.Settings
{
    public class Settings
    {
        #region Configuration

        public Settings()
        {

        }

        public void Init()
        {

        }

        #endregion


        #region User Config

        static public string CurrentPieceTheme
        {
            get => EngineAPI.GetCurrentPieceTheme();
            set => EngineAPI.SetCurrentPieceTheme(value);
        }

        static public string CurrentBoardTheme
        {
            get => EngineAPI.GetCurrentBoardTheme();
            set => EngineAPI.SetCurrentBoardTheme(value);
        }

        #endregion


    }
}
