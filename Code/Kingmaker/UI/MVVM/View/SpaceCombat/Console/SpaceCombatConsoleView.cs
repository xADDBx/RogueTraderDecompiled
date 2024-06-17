using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.UI.MVVM.View.Bark.Console;
using Kingmaker.UI.MVVM.View.ExitBattlePopup.Console;
using Kingmaker.UI.MVVM.View.SystemMap;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class SpaceCombatConsoleView : CommonStaticComponentView<SpaceCombatVM>
{
	[SerializeField]
	private ShipWeaponsPanelConsoleView m_ShipWeaponsPanelConsoleView;

	[SerializeField]
	private ShipPostsPanelConsoleView m_ShipPostsPanelConsoleView;

	[SerializeField]
	private SpaceCombatServicePanelConsoleView m_SpaceCombatServicePanelConsoleView;

	[SerializeField]
	private ExitBattlePopupConsoleView m_ExitBattlePopupConsoleView;

	[SerializeField]
	private CircleArcsView m_SpaceCombatCircleArcsView;

	[SerializeField]
	private StarSystemSpaceBarksHolderConsoleView m_SpaceCombatBarksHolderConsoleView;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_ShipWeaponsPanelConsoleView.Initialize();
		m_ShipPostsPanelConsoleView.Initialize();
		m_ExitBattlePopupConsoleView.Initialize();
		m_SpaceCombatServicePanelConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_ShipWeaponsPanelConsoleView.Bind(base.ViewModel.ShipWeaponsPanelVM);
		m_ShipPostsPanelConsoleView.Bind(base.ViewModel.ShipPostsPanelVM);
		m_SpaceCombatServicePanelConsoleView.Bind(base.ViewModel.SpaceCombatServicePanelVM);
		m_ExitBattlePopupConsoleView.Bind(base.ViewModel.ExitBattlePopupVM);
		m_SpaceCombatCircleArcsView.Bind(base.ViewModel.SpaceCombatCircleArcsVM);
		m_SpaceCombatBarksHolderConsoleView.Bind(base.ViewModel.SpaceCombatBarksHolderVM);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		m_SpaceCombatServicePanelConsoleView.AddCombatInput(inputLayer);
		m_ShipWeaponsPanelConsoleView.AddInput(inputLayer);
		m_ShipPostsPanelConsoleView.AddInput(inputLayer);
		m_SpaceCombatBarksHolderConsoleView.AddInput(inputLayer);
	}
}
