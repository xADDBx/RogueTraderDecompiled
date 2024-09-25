using System;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager;

public class DlcManagerMenuEntityVM : SelectionGroupEntityVM
{
	public readonly string Title;

	public readonly DlcManagerTabBaseVM DlcManagerTabVM;

	private Action m_Callback;

	public DlcManagerMenuEntityVM(string title, DlcManagerTabBaseVM dlcManagerTabVM, Action callback)
		: base(allowSwitchOff: false)
	{
		Title = title;
		DlcManagerTabVM = dlcManagerTabVM;
		m_Callback = callback;
	}

	protected override void DisposeImplementation()
	{
		m_Callback = null;
	}

	protected override void DoSelectMe()
	{
		m_Callback?.Invoke();
	}
}
