using System;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Owlcat.Runtime.UI.MVVM;

public abstract class VirtualListElementVMBase : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVirtualListElementData
{
	protected readonly ReactiveCommand m_ContentChanged = new ReactiveCommand();

	protected ReactiveProperty<bool> m_IsAvailable = new ReactiveProperty<bool>(initialValue: true);

	public BoolReactiveProperty Active = new BoolReactiveProperty(initialValue: true);

	public IReadOnlyReactiveProperty<bool> ActiveInVirtualList => Active;

	public IReadOnlyReactiveProperty<bool> IsAvailable => m_IsAvailable;

	public ReactiveCommand ContentChanged => m_ContentChanged;

	public bool HasView { get; internal set; }
}
