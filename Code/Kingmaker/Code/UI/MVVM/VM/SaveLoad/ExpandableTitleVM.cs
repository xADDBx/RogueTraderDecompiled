using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class ExpandableTitleVM : VirtualListElementVMBase
{
	public string Title;

	private readonly Action<bool> m_Switch;

	public BoolReactiveProperty IsExpanded = new BoolReactiveProperty();

	public bool IsSwitchable => m_Switch != null;

	public ExpandableTitleVM(string title, Action<bool> @switch, bool defaultExpanded = true)
	{
		Title = title;
		m_Switch = @switch;
		if (defaultExpanded)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
	}

	public void Expand()
	{
		IsExpanded.Value = true;
		m_Switch?.Invoke(IsExpanded.Value);
	}

	public void Collapse()
	{
		IsExpanded.Value = false;
		m_Switch?.Invoke(IsExpanded.Value);
	}

	public void Switch()
	{
		IsExpanded.Value = !IsExpanded.Value;
		m_Switch?.Invoke(IsExpanded.Value);
	}

	protected override void DisposeImplementation()
	{
	}
}
