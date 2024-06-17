using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Space.Console;

public class ZoneExitConsoleView : ZoneExitBaseView
{
	[SerializeField]
	private ConsoleHint m_StopShipHint;

	[SerializeField]
	private ConsoleHint m_ExitToShipHint;

	[SerializeField]
	private ConsoleHint m_ExitToWarpHint;

	[SerializeField]
	private ConsoleHint m_OpenShipCustomizationHint;

	private readonly ReactiveProperty<bool> m_ExitToShipAvailable = new ReactiveProperty<bool>(initialValue: true);

	private readonly ReactiveProperty<bool> m_StopShipAvailable = new ReactiveProperty<bool>(initialValue: false);

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsExitAvailable.CombineLatest(base.ViewModel.ShipIsMoving, (bool exitAvailable, bool shipIsMoving) => new { exitAvailable, shipIsMoving }).Subscribe(value =>
		{
			m_ExitToShipAvailable.Value = value.exitAvailable && !value.shipIsMoving;
			m_StopShipAvailable.Value = value.shipIsMoving;
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddSystemMapInput(InputLayer inputLayer)
	{
		AddDisposable(m_StopShipHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.StopShip();
		}, 9, m_StopShipAvailable)));
		m_StopShipHint.SetLabel(UIStrings.Instance.ActionTexts.Stop);
		AddDisposable(m_ExitToShipHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ExitToShip();
		}, 9, m_ExitToShipAvailable, InputActionEventType.ButtonJustLongPressed)));
		m_ExitToShipHint.SetLabel(UIStrings.Instance.SpaceCombatTexts.BackToShipBridge);
		AddDisposable(m_ExitToWarpHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ExitToWarp();
		}, 17, base.ViewModel.IsWarpJumpAvailable, InputActionEventType.ButtonJustLongPressed)));
		m_ExitToWarpHint.SetLabel(UIStrings.Instance.SpaceCombatTexts.KoronusExpanse);
		AddDisposable(m_OpenShipCustomizationHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenShipCustomization();
		}, 10, base.ViewModel.IsExitAvailable.And(base.ViewModel.HasAccessStarshipInventory).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		m_OpenShipCustomizationHint.SetLabel(UIStrings.Instance.MainMenu.ShipCustomization);
	}
}
