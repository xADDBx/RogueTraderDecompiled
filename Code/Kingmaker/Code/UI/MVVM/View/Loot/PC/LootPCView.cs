using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootPCView : LootView<InventoryCargoPCView, LootCollectorPCView, InteractionSlotPartPCView, PlayerStashPCView, InventoryStashPCView>
{
	[Header("PC")]
	[SerializeField]
	private OwlcatButton m_Close;

	[SerializeField]
	private OwlcatButton m_ExtendedClose;

	[SerializeField]
	private OwlcatButton m_AcceptButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptButtonText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_Close, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_ExtendedClose, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_Close.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(m_ExtendedClose.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		m_AcceptButtonText.text = UIStrings.Instance.CommonTexts.CloseWindow;
		AddDisposable(base.ViewModel.ExtendedView.Skip(1).Subscribe(m_ExtendedClose.gameObject.SetActive));
	}
}
