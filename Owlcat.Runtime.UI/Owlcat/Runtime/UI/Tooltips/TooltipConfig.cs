using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.UI.Tooltips;

public struct TooltipConfig
{
	public InfoCallPCMethod InfoCallPCMethod;

	public InfoCallConsoleMethod InfoCallConsoleMethod;

	public bool IsGlossary;

	public bool IsEncyclopedia;

	public RectTransform TooltipPlace;

	public int MaxHeight;

	public int PreferredHeight;

	public int Width;

	public List<Vector2> PriorityPivots;

	public TooltipConfig(InfoCallPCMethod infoCallPCMethod = InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod infoCallConsoleMethod = InfoCallConsoleMethod.LongRightStickButton, bool isGlossary = false, bool isEncyclopedia = false, RectTransform tooltipPlace = null, int maxHeight = 0, int preferredHeight = 0, int width = 0, List<Vector2> priorityPivots = null)
	{
		InfoCallPCMethod = infoCallPCMethod;
		InfoCallConsoleMethod = infoCallConsoleMethod;
		IsGlossary = isGlossary;
		IsEncyclopedia = isEncyclopedia;
		TooltipPlace = tooltipPlace;
		MaxHeight = maxHeight;
		PreferredHeight = preferredHeight;
		Width = width;
		PriorityPivots = priorityPivots;
	}
}
