using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Horizontal;

[Serializable]
public class VirtualListLayoutSettingsHorizontal : IVirtualListLayoutSettings
{
	public VirtualListLayoutPadding Padding;

	public float Spacing;

	public float Width;

	public float Height;

	[Tooltip("This value expands zone in witch element is considered visible to be spawned")]
	public float VisibleZoneExpansion;

	public bool IsVertical => false;

	public float DefaultSizeInScrollDirection => Width;

	public float DefaultSpacingIsScrollDirection => Spacing;

	public float TopPaddingInScrollDirection => Padding.Left;

	public float BottomPaddingInScrollDirection => Padding.Right;

	public bool IsEdgeIndex(int index)
	{
		return true;
	}
}
