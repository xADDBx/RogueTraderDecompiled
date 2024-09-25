using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Console;

public class DlcManagerModEntityConsoleView : DlcManagerModEntityBaseView, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, IConsoleEntity, INavigationRightDirectionHandler
{
	[SerializeField]
	private ConsoleHint m_ModSettingsHint;

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(m_ModSettingsHint.Bind(inputLayer.AddButton(delegate
		{
			OpenSettings();
		}, 17, IsFocused.And(base.ViewModel.ModSettingsAvailable).ToReactiveProperty())));
	}

	public bool GetAvailableSettings()
	{
		return base.ViewModel.ModSettingsAvailable.Value;
	}

	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public override bool IsValid()
	{
		return base.gameObject.activeSelf;
	}

	public bool HandleLeft()
	{
		SwitchValue();
		return true;
	}

	public bool HandleRight()
	{
		SwitchValue();
		return true;
	}
}
