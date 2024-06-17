using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public class ItemsCollectionComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new ItemsCollectionComparer();
	}

	private ItemsCollectionComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1 is GameLogEventItemsCollection)
		{
			return evn2 is GameLogEventItemsCollection;
		}
		return false;
	}
}
