namespace Owlcat.Runtime.UI.VirtualListSystem;

internal interface IInternalScrollController : IScrollController
{
	bool ElementIsInScrollZone(VirtualListElement element, out bool needScrollDown);

	void ScrollTowards(VirtualListElement element, float speed);

	void ForceScrollToElement(VirtualListElement element);
}
