using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem.Vertical;

[Serializable]
public class VirtualListLayoutSettingsVertical : IVirtualListLayoutSettings
{
	public VirtualListLayoutPadding Padding;

	public float Spacing;

	public float Width;

	public float Height;

	[Tooltip("This value expands zone in witch element is considered visible to be spawned")]
	public float VisibleZoneExpansion;

	public bool IsVertical => true;

	public float DefaultSizeInScrollDirection => Height;

	public float DefaultSpacingIsScrollDirection => Spacing;

	public float TopPaddingInScrollDirection => Padding.Top;

	public float BottomPaddingInScrollDirection => Padding.Bottom;

	public bool IsEdgeIndex(int index)
	{
		return true;
	}
}
