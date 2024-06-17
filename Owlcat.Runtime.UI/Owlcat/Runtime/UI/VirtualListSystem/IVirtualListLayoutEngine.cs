namespace Owlcat.Runtime.UI.VirtualListSystem;

internal interface IVirtualListLayoutEngine
{
	void SetClear();

	void SetOffsetElement(VirtualListElement element, bool forItself = false);

	void SetOffset(float position);

	void UpdatePosition(VirtualListElement element);

	bool IsInFieldOfView(VirtualListElement element);
}
