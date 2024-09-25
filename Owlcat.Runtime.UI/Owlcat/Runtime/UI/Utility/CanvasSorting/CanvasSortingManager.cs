using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Owlcat.Runtime.UI.Utility.CanvasSorting;

public static class CanvasSortingManager
{
	private const int SortingStep = 100;

	private static readonly List<int> SortingIds = new List<int>();

	public static ReactiveCommand UpdateCommand = new ReactiveCommand();

	public static int PushNewId()
	{
		int num = (SortingIds.Any() ? SortingIds.Max() : 0) + 1;
		SortingIds.Add(num);
		return num;
	}

	public static void PopId(int id)
	{
		SortingIds.Remove(id);
	}

	public static int GetSortingOrder(int id)
	{
		return id * 100;
	}
}
