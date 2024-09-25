using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public interface IVirtualListElementView
{
	RectTransform RectTransform { get; }

	VirtualListLayoutElementSettings LayoutSettings { get; }

	bool NeedRebuildToGetSize { get; }

	void BindVirtualList(IVirtualListElementData data);

	void UnbindVirtualList();

	IVirtualListElementView Instantiate();
}
