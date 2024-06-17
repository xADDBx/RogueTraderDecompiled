using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.UI.MVVM.View.ExitBattlePopup.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ExitBattlePopup.PC;

public class ExitBattlePopupPCView : ExitBattlePopupBaseView
{
	[Header("PC")]
	[SerializeField]
	private TextMeshProUGUI m_ExitBattleButtonLabel;

	[SerializeField]
	private OwlcatButton m_ExitBattleButton;

	[SerializeField]
	private TextMeshProUGUI m_UpgradeButtonLabel;

	[SerializeField]
	private OwlcatButton m_UpgradeButton;

	[SerializeField]
	private LootSlotPCView m_SlotPrefab;

	protected override void InitializeImpl()
	{
		m_ItemsSlotsGroup.Initialize(m_SlotPrefab);
		m_ExitBattleButtonLabel.text = UIStrings.Instance.SettingsUI.DialogOk;
		m_UpgradeButtonLabel.text = UIStrings.Instance.ShipCustomization.Attune;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ExitBattleButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ExitBattle();
		}));
		AddDisposable(m_UpgradeButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ExitBattle(forceOpenVoidshipUpgrade: true);
		}));
		AddDisposable(base.ViewModel.IsUpgradeAvailable.Subscribe(m_UpgradeButton.gameObject.SetActive));
	}
}
