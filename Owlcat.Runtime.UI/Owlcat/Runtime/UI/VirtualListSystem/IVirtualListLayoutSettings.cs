namespace Owlcat.Runtime.UI.VirtualListSystem;

public interface IVirtualListLayoutSettings
{
	bool IsVertical { get; }

	float DefaultSizeInScrollDirection { get; }

	float DefaultSpacingIsScrollDirection { get; }

	float TopPaddingInScrollDirection { get; }

	float BottomPaddingInScrollDirection { get; }

	bool IsEdgeIndex(int index);
}
