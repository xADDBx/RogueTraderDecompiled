using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public class CargoCollectionComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new CargoCollectionComparer();
	}

	private CargoCollectionComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1 is GameLogEventCargoCollection)
		{
			return evn2 is GameLogEventCargoCollection;
		}
		return false;
	}
}
