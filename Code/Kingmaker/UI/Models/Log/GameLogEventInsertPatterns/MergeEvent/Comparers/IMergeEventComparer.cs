using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public interface IMergeEventComparer
{
	bool Compare(GameLogEvent evn1, GameLogEvent evn2);
}
