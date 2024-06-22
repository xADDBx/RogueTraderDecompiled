using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootCollectorPCView : LootCollectorView
{
	[SerializeField]
	private OwlcatButton m_CollectAllButton;

	[SerializeField]
	private GameObject m_CollectAllButtonBlock;

	[SerializeField]
	private TextMeshProUGUI m_CollectAllButtonLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private GameObject m_CloseButtonBlock;

	[SerializeField]
	private TextMeshProUGUI m_CloseButtonText;

	[SerializeField]
	protected OwlcatButton[] m_LootManagerButtons;

	[SerializeField]
	private OwlcatButton m_AllToCargoButton;

	[SerializeField]
	private OwlcatButton m_AllToInventoryButton;

	public override void Initialize()
	{
		base.Initialize();
		m_CollectAllButtonLabel.text = UIStrings.Instance.LootWindow.CollectAll.Text + " [" + UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName("CollectAllLoot")) + "]";
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CollectAllButton, UISounds.ButtonSoundsEnum.LootCollectAllSound);
		m_LootManagerButtons.ForEach(delegate(OwlcatButton b)
		{
			UISounds.Instance.SetClickAndHoverSound(b, UISounds.ButtonSoundsEnum.PlastickSound);
		});
		AddDisposable(m_CollectAllButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CollectAll();
		}));
		AddDisposable(base.ViewModel.NoLoot.Subscribe(SetupButtons));
		if (m_AllToCargoButton != null && m_AllToInventoryButton != null)
		{
			AddDisposable(m_AllToCargoButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				AddAllToCargo();
			}));
			AddDisposable(m_AllToInventoryButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				AddAllToInventory();
			}));
			m_AllToCargoButton.SetHint(UIStrings.Instance.LootWindow.SendAllToCargo.Text);
			m_AllToInventoryButton.SetHint(UIStrings.Instance.LootWindow.SendAllToInventory.Text);
		}
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		m_CloseButtonText.text = UIStrings.Instance.CommonTexts.CloseWindow;
		OwlcatButton[] lootManagerButtons = m_LootManagerButtons;
		foreach (OwlcatButton owlcatButton in lootManagerButtons)
		{
			AddDisposable(owlcatButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.ChangeView();
			}));
			AddDisposable(owlcatButton.SetHint(UIStrings.Instance.LootWindow.LootManager));
		}
		m_ToCargoText.SetHint(UIStrings.Instance.LootWindow.TrashLootObjectDescr.Text);
		m_ToInventoryText.SetHint(UIStrings.Instance.LootWindow.ItemsLootObjectDescr.Text);
		if (base.ViewModel.NoLoot.Value)
		{
			return;
		}
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.CollectAllLoot.name, delegate
		{
			if (!base.ViewModel.NoLoot.Value)
			{
				CollectAll();
			}
		}));
	}

	private void SetupButtons(bool noLoot)
	{
		m_CloseButtonBlock.gameObject.SetActive(noLoot);
		m_CollectAllButtonBlock.gameObject.SetActive(!noLoot);
	}
}
