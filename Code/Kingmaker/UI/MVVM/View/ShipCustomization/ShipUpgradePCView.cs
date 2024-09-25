using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SelectorWindow;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipUpgradePCView : ShipUpgradeBaseView<ShipInventoryStashView, ShipComponentSlotPCView, ShipUpgradeStructureSlotPCView, ShipUpgradeProwRamSlotPCView, ShipSelectorWindowPCView>
{
	[SerializeField]
	private OwlcatButton ToDefaultPosition;

	[SerializeField]
	private TextMeshProUGUI ToDefaultPositionText;

	public override void Initialize()
	{
		base.Initialize();
		m_InventoryStashView.Initialize();
		m_SelectorWindowView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_InventoryStashView.Bind(base.ViewModel.ShipInventoryStashVM);
		if ((bool)m_SelectorWindowView)
		{
			AddDisposable(base.ViewModel.ShipSelectorWindowVM.Subscribe(m_SelectorWindowView.Bind));
		}
		AddDisposable(base.ViewModel.CanSetDefaultPosition.Subscribe(delegate(bool val)
		{
			ToDefaultPosition.gameObject.SetActive(val);
		}));
		AddDisposable(ToDefaultPosition.OnLeftClickAsObservable().Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		ToDefaultPositionText.text = UIStrings.Instance.ShipCustomization.ToDefaultPosition;
		base.BindViewImplementation();
		AddDisposable(m_PlasmaDrives.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_VoidShieldGenerator.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_AugerArray.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		AddDisposable(m_ArmorPlating.SlotVM.Item.Subscribe(delegate
		{
			RotateToDefaultPosition();
		}));
		ShipComponentSlotPCView[] weaponSlots = m_WeaponSlots;
		foreach (ShipComponentSlotPCView shipComponentSlotPCView in weaponSlots)
		{
			AddDisposable(shipComponentSlotPCView.SlotVM.Item.Subscribe(delegate
			{
				RotateToDefaultPosition();
			}));
		}
	}

	protected override void UpdateSlots()
	{
		m_PlasmaDrives.Bind(base.ViewModel.PlasmaDrives);
		m_VoidShieldGenerator.Bind(base.ViewModel.VoidShieldGenerator);
		m_AugerArray.Bind(base.ViewModel.AugerArray);
		m_ArmorPlating.Bind(base.ViewModel.ArmorPlating);
		for (int i = 0; i < m_ArsenalSlots.Length; i++)
		{
			m_ArsenalSlots[i].Bind(base.ViewModel.Arsenals[i]);
		}
		for (int j = 0; j < m_WeaponSlots.Length; j++)
		{
			m_WeaponSlots[j].Bind(base.ViewModel.Weapons[j]);
		}
		m_UpgradeStructureSlot.Bind(base.ViewModel.InternalStructure);
		m_UpgradeProwRamSlot.Bind(base.ViewModel.ProwRam);
	}
}
