using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public abstract class InventoryDollView<TSlotView> : CharInfoComponentView<InventoryDollVM> where TSlotView : InventoryEquipSlotView
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[Header("Slot groups")]
	[SerializeField]
	private GameObject m_LeftSlots;

	[SerializeField]
	private GameObject m_RightSlots;

	[Header("Body slots")]
	[SerializeField]
	protected TSlotView m_BodyArmor;

	[SerializeField]
	protected TSlotView m_HeadArmor;

	[SerializeField]
	protected TSlotView m_Gloves;

	[SerializeField]
	protected TSlotView m_Boots;

	[SerializeField]
	protected TSlotView m_Back;

	[SerializeField]
	protected TSlotView m_Neck;

	[SerializeField]
	protected TSlotView m_Ring1;

	[SerializeField]
	protected TSlotView m_Ring2;

	[Header("Quick slots")]
	[SerializeField]
	protected TSlotView[] m_QuickSlots;

	[Header("Weapon sets")]
	[SerializeField]
	protected WeaponSetSelectorPCView m_WeaponSetSelector;

	[Header("Encumbrance")]
	[SerializeField]
	private CharInfoEncumbranceView m_EncumbranceView;

	private CharacterDollRoom Room => UIDollRooms.Instance.Or(null)?.CharacterDollRoom;

	protected override void BindViewImplementation()
	{
		if (m_Label != null)
		{
			m_Label.text = UIStrings.Instance.CharacterSheet.Equipment;
		}
		Room.Initialize(m_CharacterController);
		base.BindViewImplementation();
		m_EncumbranceView.Bind(base.ViewModel.EncumbranceVM);
	}

	public IEnumerator DelayedBind(InventoryDollVM viewModel, float seconds)
	{
		SetSlotGroupsVisibility(isVisible: true);
		yield return new WaitForSecondsRealtime(seconds);
		yield return new WaitForEndOfFrame();
		Bind(viewModel);
	}

	public void ClearViewIfNeeded()
	{
		if (!base.IsBinded)
		{
			SetSlotGroupsVisibility(isVisible: false);
		}
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		if (!(Room == null) && base.ViewModel.Unit.Value != null)
		{
			try
			{
				Room.SetupUnit(base.ViewModel.Unit.Value);
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex);
			}
			UpdateSlots();
		}
	}

	private void UpdateSlots()
	{
		m_BodyArmor.Bind(base.ViewModel.Armor);
		m_HeadArmor.Bind(base.ViewModel.Head);
		m_Gloves.Bind(base.ViewModel.Gloves);
		m_Boots.Bind(base.ViewModel.Feet);
		m_Back.Bind(base.ViewModel.Shoulders);
		m_Neck.Bind(base.ViewModel.Neck);
		m_Ring1.Bind(base.ViewModel.Ring1);
		m_Ring2.Bind(base.ViewModel.Ring2);
		for (int i = 0; i < m_QuickSlots.Length; i++)
		{
			m_QuickSlots[i].Bind(base.ViewModel.QuickSlots[i]);
		}
		m_WeaponSetSelector.Bind(base.ViewModel.WeaponSetSelector);
	}

	private void SwitchOnDoll()
	{
		try
		{
			if ((bool)Room)
			{
				Room.Show();
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		SwitchOnDoll();
		SetSlotGroupsVisibility(isVisible: true);
	}

	private void SwitchOffDoll()
	{
		try
		{
			if ((bool)Room)
			{
				Room.Hide();
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		SwitchOffDoll();
		SetSlotGroupsVisibility(isVisible: false);
	}

	private void SetSlotGroupsVisibility(bool isVisible)
	{
		m_LeftSlots.Or(null)?.SetActive(isVisible);
		m_RightSlots.Or(null)?.SetActive(isVisible);
	}
}
