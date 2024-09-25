using System;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Menu;

public class NewGameMenuEntityVM : SelectionGroupEntityVM
{
	public readonly string Title;

	public readonly NewGamePhaseBaseVm NewGamePhaseVM;

	private Action m_Callback;

	public NewGameMenuEntityVM(string title, NewGamePhaseBaseVm newGamePhaseVM, Action callback)
		: base(allowSwitchOff: false)
	{
		Title = title;
		NewGamePhaseVM = newGamePhaseVM;
		m_Callback = callback;
	}

	protected override void DoSelectMe()
	{
		m_Callback?.Invoke();
	}

	protected override void DisposeImplementation()
	{
		m_Callback = null;
	}

	public void OnBack()
	{
		NewGamePhaseVM.OnBack();
	}

	public void OnNext()
	{
		NewGamePhaseVM.OnNext();
	}

	public void SetAvailable(bool value)
	{
		SetAvailableState(value);
	}
}
