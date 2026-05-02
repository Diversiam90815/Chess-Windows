using System;
using System.Collections.Generic;
using System.IO;


namespace Chess.UI.Styles
{
    public interface IStyleLoader{
        List<PieceStyleInfo> LoadPieceStyles();
        List<BoardStyleInfo> LoadBoardStyles();
    }
    
    
    public class StyleLoader : IStyleLoader
    {
        private static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string boardsPath = Path.Combine(baseDir, "Assets", "Board");
        private readonly string piecesPath = Path.Combine(baseDir, "Assets", "Pieces");


        private static readonly Dictionary<string, BoardStyle> BoardStyleMapping = new()
        {
            { "Wood", BoardStyle.Wood },
            { "Wood2", BoardStyle.Wood2 },
            { "Plain", BoardStyle.Plain },
            { "Plastic", BoardStyle.Plastic },
            { "Marble", BoardStyle.Marble },
            { "Marble2", BoardStyle.Marble2 },
            { "Glass", BoardStyle.Glass }
        };


        private static readonly Dictionary<string, PieceStyle> PieceStyleMapping = new()
        {
            { "Basic", PieceStyle.Basic },
            { "Standard", PieceStyle.Standard }
        };


        public List<BoardStyleInfo> LoadBoardStyles()
        {
            var boardStyles = new List<BoardStyleInfo>();
            var boardFiles = Directory.GetFiles(boardsPath, "*.png");

            foreach (var boardFile in boardFiles)
            {
                string key = Path.GetFileNameWithoutExtension(boardFile);

                if (BoardStyleMapping.TryGetValue(key, out var styleValue))
                {
                    boardStyles.Add(new BoardStyleInfo
                    {
                        Name = key,
                        Style = styleValue,
                        DisplayName = FormatDisplayName(key)
                    });
                }
            }

            return boardStyles;
        }


        public List<PieceStyleInfo> LoadPieceStyles()
        {
            var pieceStyles = new List<PieceStyleInfo>();
            var themeDirectories = Directory.GetDirectories(piecesPath);

            foreach (var themeDirectory in themeDirectories)
            {
                string styleName = Path.GetFileName(themeDirectory);

                if (PieceStyleMapping.TryGetValue(styleName, out var styleValue))
                {
                    pieceStyles.Add(new PieceStyleInfo
                    {
                        Name = styleName,
                        Style = styleValue,
                        DisplayName = FormatDisplayName(styleName)
                    });
                }
            }

            return pieceStyles;
        }


        private static string FormatDisplayName(string name)
        {
            // Convert "Wood2" to "Wood 2", etc.
            return System.Text.RegularExpressions.Regex.Replace(name, @"(\d+)", " $1").Trim();
        }
    }
}
