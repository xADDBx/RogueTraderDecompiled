using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBricksGroupLayoutParams
{
	public TooltipBricksGroupLayoutType LayoutType;

	public int ColumnCount = 2;

	public float? PreferredElementHeight;

	public RectOffset Padding = new RectOffset(0, 0, 0, 0);

	public Vector2 Spacing;

	public Vector2? CellSize;
}
