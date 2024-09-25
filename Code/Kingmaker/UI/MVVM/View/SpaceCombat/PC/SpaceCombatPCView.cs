using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.UI.MVVM.View.Bark.PC;
using Kingmaker.UI.MVVM.View.ExitBattlePopup.PC;
using Kingmaker.UI.MVVM.View.SystemMap;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class SpaceCombatPCView : CommonStaticComponentView<SpaceCombatVM>
{
	[SerializeField]
	private ShipWeaponsPanelPCView m_ShipWeaponsPanelPCView;

	[SerializeField]
	private ShipPostsPanelPCView m_ShipPostsPanelPCView;

	[SerializeField]
	private SpaceCombatServicePanelPCView m_SpaceCombatServicePanelPCView;

	[SerializeField]
	private ExitBattlePopupPCView m_ExitBattlePopupPCView;

	[SerializeField]
	private CircleArcsView m_SpaceCombatCircleArcsView;

	[SerializeField]
	private StarSystemSpaceBarksHolderPCView m_SpaceCombatBarksHolderPCView;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_ExitBattlePopupPCView.Initialize();
		m_SpaceCombatServicePanelPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_ShipWeaponsPanelPCView.Bind(base.ViewModel.ShipWeaponsPanelVM);
		m_ShipPostsPanelPCView.Bind(base.ViewModel.ShipPostsPanelVM);
		m_SpaceCombatServicePanelPCView.Bind(base.ViewModel.SpaceCombatServicePanelVM);
		m_ExitBattlePopupPCView.Bind(base.ViewModel.ExitBattlePopupVM);
		m_SpaceCombatCircleArcsView.Bind(base.ViewModel.SpaceCombatCircleArcsVM);
		m_SpaceCombatBarksHolderPCView.Bind(base.ViewModel.SpaceCombatBarksHolderVM);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}
