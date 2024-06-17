namespace Owlcat.Runtime.UI.VirtualListSystem;

internal interface IScrollProvider
{
	bool ScrollUpdated();

	float GetScrollValue();

	void SetScrollValue(float value);
}
