using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.MoveHistory
{
    public interface IMoveHistoryModel
    {
        // Properties
        List<string> MoveHistory { get; }

        // Methods
        void RemoveLastMove();

        // Events
        event Action MoveHistoryUpdated;
    }
}
