using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Grid;

[Serializable]
public class VirtualListLayoutSettingsGrid : IVirtualListLayoutSettings
{
	public VirtualListLayoutPadding Padding;

	public Vector2 Spacing;

	public int ElementsInRow;

	public float Width;

	public float Height;

	[Tooltip("This value expands zone in witch element is considered visible to be spawned")]
	public float VisibleZoneExpansion;

	public bool IsVertical => true;

	public float DefaultSizeInScrollDirection => Height;

	public float DefaultSpacingIsScrollDirection => Spacing.y;

	public float TopPaddingInScrollDirection => Padding.Top;

	public float BottomPaddingInScrollDirection => Padding.Bottom;

	public bool IsEdgeIndex(int index)
	{
		return index % ElementsInRow == 0;
	}
}
