using System;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public interface IVirtualListElementTemplate
{
	Type ElementType { get; }

	int Id { get; }

	IVirtualListElementView View { get; }
}
