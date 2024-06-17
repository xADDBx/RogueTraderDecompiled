using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Owlcat.Runtime.UI.SelectionGroup;

public abstract class SelectionGroupEntityVM : VirtualListElementVMBase
{
	public ReactiveCommand RefreshView = new ReactiveCommand();

	public ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly bool AllowSwitchOff;

	public bool IsHidden { get; private set; }

	public SelectionGroupEntityVM(bool allowSwitchOff)
	{
		AllowSwitchOff = allowSwitchOff;
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
