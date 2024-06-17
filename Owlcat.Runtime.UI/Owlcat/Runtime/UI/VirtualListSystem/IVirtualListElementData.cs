using UniRx;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public interface IVirtualListElementData
{
	IReadOnlyReactiveProperty<bool> ActiveInVirtualList { get; }

	IReadOnlyReactiveProperty<bool> IsAvailable { get; }

	ReactiveCommand ContentChanged { get; }
}
