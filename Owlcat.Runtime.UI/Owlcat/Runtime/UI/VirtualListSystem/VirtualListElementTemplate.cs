using System;
using System.Linq;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class VirtualListElementTemplate<TData> : IVirtualListElementTemplate where TData : IVirtualListElementData
{
	public Type ElementType { get; }

	public int Id { get; }

	public IVirtualListElementView View { get; }

	public VirtualListElementTemplate(IVirtualListElementView view)
	{
		Type typeFromHandle = typeof(TData);
		if (typeFromHandle.GetInterfaces().Contains(typeof(IVirtualListElementIdentifier)))
		{
			throw new Exception($"[VirtualList] {typeFromHandle} implements {typeof(IVirtualListElementIdentifier)}, so you have to use id");
		}
		ElementType = typeFromHandle;
		Id = 0;
		View = view;
	}

	public VirtualListElementTemplate(IVirtualListElementView view, int id)
	{
		Type typeFromHandle = typeof(TData);
		if (!typeFromHandle.GetInterfaces().Contains(typeof(IVirtualListElementIdentifier)))
		{
			throw new Exception($"[VirtualList] If you use id, {typeFromHandle} has to implement {typeof(IVirtualListElementIdentifier)}");
		}
		ElementType = typeFromHandle;
		Id = id;
		View = view;
	}
}
