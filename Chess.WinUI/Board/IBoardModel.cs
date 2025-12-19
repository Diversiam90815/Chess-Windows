using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Chess.UI.Board
{
    public interface IBoardModel
    {
        int[] GetBoardStateFromNative();
        BoardSquare DecodeBoardState(int index, int[] boardState);
        (Dictionary<int, int>, int[]) UpdateBoardState();

    }
}