using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipComponentSlotBaseView<TItemSlotView> : ItemSlotView<ShipComponentSlotVM>, IShipCustomizationUIHandler, ISubscriber, IHasTooltipTemplates, INewSlotsHandler, IVoidShipRotationHandler, IItemSlotView where TItemSlotView : ItemSlotBaseView
{
	[Header("Common")]
	[SerializeField]
	private CanvasGroup m_SlotCanvasGroup;

	[SerializeField]
	protected TItemSlotView m_ItemSlotPCView;

	[SerializeField]
	private GameObject m_LockState;

	[SerializeField]
	private GameObject m_EmptyPlaceholder;

	[SerializeField]
	private OwlcatButton m_EmptyPlaceholderButton;

	[SerializeField]
	private GameObject m_PossibleTargetHighlight;

	[SerializeField]
	private TextMeshProUGUI m_ShortDescription;

	[SerializeField]
	private CanvasGroup m_LinesBlock;

	[SerializeField]
	private GameObject[] m_ShipLines;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.Item.Subscribe(delegate(ItemEntity value)
		{
			m_EmptyPlaceholder.SetActive(value == null);
		}));
		AddDisposable(m_EmptyPlaceholderButton.SetHint(SetupHint(base.ViewModel.SlotType)));
		AddDisposable(base.ViewModel.IsLocked.Subscribe(delegate(bool val)
		{
			m_LockState.SetActive(val);
		}));
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(m_PossibleTargetHighlight.SetActive));
		}
		SetShipLines();
		SetSlotState(isActive: true);
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
		}
	}

	protected override void DestroyViewImplementation()
	{
		SetSlotState(isActive: false);
	}

	protected void SetSlotState(bool isActive)
	{
		if (base.ViewModel.IsLocked.Value)
		{
			m_ItemSlotPCView.SetLockState();
		}
		m_SlotCanvasGroup.interactable = isActive;
		m_SlotCanvasGroup.alpha = (isActive ? 1f : 0.3f);
		m_ItemSlotPCView.gameObject.SetActive(isActive);
	}

	protected void OnClick()
	{
		if (!base.ViewModel.IsLocked.Value)
		{
			EventBus.RaiseEvent(delegate(IShipComponentItemHandler h)
			{
				h.HandleChangeItem(base.ViewModel);
			});
		}
	}

	protected void OnDoubleClick()
	{
		TryUnequip();
	}

	protected void OnHoverStart()
	{
		EventBus.RaiseEvent(delegate(IInventorySlotHoverHandler h)
		{
			h.HandleHoverStart(base.ViewModel.ItemSlot, base.ViewModel.WeaponSlotType);
		});
	}

	protected void OnHoverEnd()
	{
		EventBus.RaiseEvent(delegate(IInventorySlotHoverHandler h)
		{
			h.HandleHoverStop();
		});
	}

	protected void EquipItem()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryEquip(base.ViewModel);
		});
	}

	public void HandleOpenShipCustomization()
	{
	}

	public void HandleCloseAllComponentsMenu()
	{
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	private string SetupHint(ShipComponentSlotType type)
	{
		switch (type)
		{
		case ShipComponentSlotType.PlasmaDrives:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Engine.Text;
			return UIStrings.Instance.ShipCustomization.PlasmaDrives;
		case ShipComponentSlotType.VoidShieldGenerator:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Shields.Text;
			return UIStrings.Instance.ShipCustomization.VoidShieldGenerator;
		case ShipComponentSlotType.AugerArray:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Auspex.Text;
			return UIStrings.Instance.ShipCustomization.AugerArray;
		case ShipComponentSlotType.ArmorPlating:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Armor.Text;
			return UIStrings.Instance.ShipCustomization.ArmorPlating;
		case ShipComponentSlotType.Dorsal:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Dorsal.Text;
			return UIStrings.Instance.ShipCustomization.Dorsal;
		case ShipComponentSlotType.Prow1:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Prow.Text;
			return UIStrings.Instance.ShipCustomization.Prow;
		case ShipComponentSlotType.Prow2:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Prow.Text;
			return UIStrings.Instance.ShipCustomization.Prow;
		case ShipComponentSlotType.Port:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Port.Text;
			return UIStrings.Instance.ShipCustomization.Port;
		case ShipComponentSlotType.Starboard:
			m_ShortDescription.text = UIStrings.Instance.ShipCustomization.Starboard.Text;
			return UIStrings.Instance.ShipCustomization.Starboard;
		default:
			return string.Empty;
		}
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public void HandleOnRotationStart()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 0f;
		}
	}

	public void HandleOnRotationStop()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
		}
	}

	private void SetShipLines()
	{
		if (m_ShipLines.Length != 0)
		{
			GameObject[] shipLines = m_ShipLines;
			for (int i = 0; i < shipLines.Length; i++)
			{
				shipLines[i].SetActive(value: false);
			}
			switch (base.ViewModel.ShipType)
			{
			case PlayerShipType.SwordClassFrigate:
				m_ShipLines[0].SetActive(value: true);
				break;
			case PlayerShipType.FalchionClassFrigate:
				m_ShipLines[1].SetActive(value: true);
				break;
			case PlayerShipType.FirestormClassFrigate:
				m_ShipLines[2].SetActive(value: true);
				break;
			}
		}
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		base.SetupContextMenu();
		base.ViewModel.ContextMenu.Value = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.TakeOff, delegate
			{
				TryUnequip();
			}, base.ViewModel.IsEquipPossible),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
	}

	protected bool TryUnequip()
	{
		return InventoryHelper.TryUnequip(base.ViewModel);
	}
}
