using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public class WeaponSetBaseView : SelectionGroupEntityView<WeaponSetVM>
{
	[SerializeField]
	protected InventoryEquipSlotView m_PrimaryHand;

	[SerializeField]
	protected InventoryEquipSlotView m_SecondaryHand;

	[SerializeField]
	private TextMeshProUGUI[] m_WeaponSetIndexes;

	[SerializeField]
	private OwlcatMultiButton m_MainHandObject;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_PrimaryHand.Bind(base.ViewModel.Primary);
		m_SecondaryHand.Bind(base.ViewModel.Secondary);
		UISounds.Instance.SetClickAndHoverSound(m_Button, UISounds.ButtonSoundsEnum.PlastickSound);
		TextMeshProUGUI[] weaponSetIndexes = m_WeaponSetIndexes;
		for (int i = 0; i < weaponSetIndexes.Length; i++)
		{
			weaponSetIndexes[i].text = UIUtility.ArabicToRoman(base.ViewModel.Index + 1);
		}
		AddDisposable(base.ViewModel.IsEnabled.Subscribe(delegate(bool value)
		{
			base.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.HasMainHand.Subscribe(delegate(bool value)
		{
			m_MainHandObject.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.Primary.Item.Subscribe(delegate(ItemEntity value)
		{
			SetMainHandIcons(value);
		}));
	}

	private void SetMainHandIcons(ItemEntity item)
	{
		if (item is ItemEntityWeapon itemEntityWeapon && base.ViewModel.HasMainHand.Value)
		{
			m_MainHandObject.SetActiveLayer((itemEntityWeapon.Blueprint.Category == WeaponCategory.Pistol) ? 1 : 2);
		}
		else
		{
			m_MainHandObject.SetActiveLayer(0);
		}
	}
}
