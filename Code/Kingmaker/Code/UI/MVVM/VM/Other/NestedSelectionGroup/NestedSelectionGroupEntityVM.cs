using JetBrains.Annotations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;

public abstract class NestedSelectionGroupEntityVM : VirtualListElementVMBase, IVirtualListElementIdentifier
{
	public INestedListSource source;

	private int m_VirtualListTypeId;

	private ReactiveProperty<bool> m_IsExpanded = new ReactiveProperty<bool>(initialValue: false);

	public ReactiveCommand RefreshView = new ReactiveCommand();

	public ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly bool AllowSwitchOff;

	public int VirtualListTypeId => m_VirtualListTypeId;

	public abstract INestedListSource NextSource { get; }

	public INestedListSource Source => source;

	public abstract bool HasNesting { get; }

	public abstract int NestingLimit { get; }

	public IReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public bool IsHidden { get; private set; }

	public NestedSelectionGroupEntityVM([NotNull] INestedListSource source, bool allowSwitchOff)
	{
		AllowSwitchOff = allowSwitchOff;
		int num = 0;
		INestedListSource nestedListSource = source;
		while (nestedListSource.Source != null && num <= NestingLimit)
		{
			num++;
			nestedListSource = nestedListSource.Source;
		}
		m_VirtualListTypeId = num;
		this.source = source;
	}

	public void SetExpanded(bool state)
	{
		if (HasNesting)
		{
			m_IsExpanded.Value = state;
		}
	}

	public void SetSelected(bool state)
	{
		IsSelected.Value = state;
		RefreshView.Execute();
		if (state)
		{
			DoSelectMe();
		}
	}

	public void SetSelectedFromView(bool state)
	{
		if (state || AllowSwitchOff)
		{
			SetSelected(state);
		}
	}

	protected abstract void DoSelectMe();

	protected void SetAvailableState(bool state)
	{
		m_IsAvailable.Value = state;
	}

	protected void SetHiddenState(bool state)
	{
		IsHidden = state;
	}

	protected override void DisposeImplementation()
	{
	}
}
