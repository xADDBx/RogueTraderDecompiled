using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NavigatorResource.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NavigatorResource.Console;

public class SectorMapBottomHudConsoleView : SectorMapBottomHudBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleHint m_ToVoidshipBridgeHint;

	[SerializeField]
	private ConsoleHint m_OpenMenuHint;

	[SerializeField]
	private ConsoleHint m_SetCameraOnShipHint;

	private readonly BoolReactiveProperty m_IsShipOnTheScreen = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsExitAvailable = new BoolReactiveProperty();

	private BoolReactiveProperty m_SpaceSystemInformationWindowConsoleViewInspectMode = new BoolReactiveProperty();

	public void CreateInputImpl(InputLayer inputLayer, BoolReactiveProperty isInformationWindowMode, BoolReactiveProperty spaceSystemInformationWindowConsoleViewInspectMode, Action focusOnSystemAction)
	{
		m_SpaceSystemInformationWindowConsoleViewInspectMode = spaceSystemInformationWindowConsoleViewInspectMode;
		AddDisposable(base.ViewModel.IsExitAvailable.Subscribe(delegate(bool value)
		{
			m_IsExitAvailable.Value = value && !isInformationWindowMode.Value;
		}));
		AddDisposable(m_SetCameraOnShipHint.Bind(inputLayer.AddButton(delegate
		{
			focusOnSystemAction();
			base.ViewModel.SetCameraOnVoidShip();
		}, 9, m_IsShipOnTheScreen)));
		m_SetCameraOnShipHint.SetLabel(CenterOnShipHintText);
		AddDisposable(m_ToVoidshipBridgeHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ExitToShip();
		}, 9, m_IsExitAvailable, InputActionEventType.ButtonJustLongPressed)));
		m_ToVoidshipBridgeHint.SetLabel(UIStrings.Instance.SpaceCombatTexts.BackToShipBridge);
		AddDisposable(m_OpenMenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 13, m_IsExitAvailable)));
		m_OpenMenuHint.SetLabel(UIStrings.Instance.CommonTexts.Menu);
	}

	protected override void OnUpdateHandler()
	{
		Game instance = Game.Instance;
		if (instance != null && (instance.SectorMapController?.CurrentStarSystem?.Position).HasValue)
		{
			m_IsShipOnTheScreen.Value = !IsPointOnScreen(Game.Instance.SectorMapController.CurrentStarSystem.Position) && !m_SpaceSystemInformationWindowConsoleViewInspectMode.Value;
		}
	}
}
