using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Mods.Console;

public class DlcManagerTabModsConsoleView : DlcManagerTabModsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabModsModSelectorConsoleView m_ModSelectorConsoleView;

	[SerializeField]
	private ConsoleHint m_OpenNexusModsHint;

	[SerializeField]
	private ConsoleHint m_OpenSteamWorkshopHint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ModSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint openModSettingsHint, BoolReactiveProperty modSettingsIsAvailable)
	{
		AddDisposable(openModSettingsHint.Bind(inputLayer.AddButton(delegate
		{
		}, 17, base.ViewModel.IsEnabled.And(modSettingsIsAvailable).ToReactiveProperty())));
		openModSettingsHint.SetLabel(UIStrings.Instance.DlcManager.ModSettings);
		AddDisposable(m_OpenNexusModsHint.Bind(inputLayer.AddButton(delegate
		{
			OpenNexusMods();
		}, 10, base.ViewModel.IsEnabled, InputActionEventType.ButtonJustReleased)));
		AddDisposable(m_OpenSteamWorkshopHint.Bind(inputLayer.AddButton(delegate
		{
			OpenSteamWorkshop();
		}, 11, base.ViewModel.IsEnabled.And(base.ViewModel.IsSteam).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		m_ModSelectorConsoleView.CreateInputImpl(inputLayer, hintsWidget);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_ModSelectorConsoleView.GetNavigationEntities();
	}
}
