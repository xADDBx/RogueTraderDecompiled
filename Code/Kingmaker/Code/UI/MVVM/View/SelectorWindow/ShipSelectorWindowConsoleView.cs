using System.Collections;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.UI.MVVM.View.ShipCustomization.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class ShipSelectorWindowConsoleView : SelectorWindowConsoleView<ShipComponentItemSlotConsoleView, ShipComponentItemSlotVM>
{
	private new readonly BoolReactiveProperty m_SelectedEquipped = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_NeedShowInfo = new BoolReactiveProperty();

	private Coroutine m_UnequippedItemTooltipCo;

	private ShipComponentItemSlotConsoleView m_CurrentEntity;

	private bool m_IsCurrentItemInfoShown;

	protected override LocalizedString ConfirmText => UIStrings.Instance.ContextMenu.Equip;

	protected override bool ShouldCloseOnConfirm => true;

	private ShipItemSelectorWindowVM ShipItemSelectorWindowVM => base.ViewModel as ShipItemSelectorWindowVM;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Header.text = UIStrings.Instance.InventoryScreen.ChooseItem;
		AddDisposable(m_SelectedEquipped.Subscribe(delegate(bool value)
		{
			CanEquip.Value = !value;
		}));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			Unequip();
		}, 11, m_SelectedEquipped);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.ContextMenu.TakeOff));
		AddDisposable(inputBindStruct);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		if (m_UnequippedItemTooltipCo != null)
		{
			StopCoroutine(m_UnequippedItemTooltipCo);
			m_UnequippedItemTooltipCo = null;
		}
	}

	protected override void EntityFocused(IConsoleEntity entity)
	{
		base.EntityFocused(entity);
		m_CurrentEntity = entity as ShipComponentItemSlotConsoleView;
		if ((bool)m_CurrentEntity)
		{
			m_NeedShowInfo.Value = m_CurrentEntity.GetVM.Item != ShipItemSelectorWindowVM.EquippedSlot.Item.Value && ShipItemSelectorWindowVM.EquippedSlot.Item.Value != null;
		}
		else
		{
			m_NeedShowInfo.Value = false;
		}
		RefreshSelectedState(entity);
		if (m_UnequippedItemTooltipCo != null)
		{
			StopCoroutine(m_UnequippedItemTooltipCo);
		}
	}

	protected void Unequip()
	{
		ShipItemSelectorWindowVM?.Unequip();
		if (m_NavigationBehaviour.DeepestNestedFocus is ShipComponentItemSlotConsoleView shipComponentItemSlotConsoleView)
		{
			m_UnequippedItemTooltipCo = StartCoroutine(UpdateUnequippedItemTooltipCo(shipComponentItemSlotConsoleView.GetVM.Item));
			OnClose();
		}
	}

	private void RefreshSelectedState(IConsoleEntity entity)
	{
		if (entity is ShipComponentItemSlotConsoleView shipComponentItemSlotConsoleView)
		{
			m_SelectedEquipped.Value = shipComponentItemSlotConsoleView.GetVM.Item.Owner != null;
		}
		else
		{
			m_SelectedEquipped.Value = false;
		}
	}

	public void ShowCurrentInstoledInfo()
	{
		if (ShipItemSelectorWindowVM.EquippedSlot.Item.Value != null)
		{
			if (!m_IsCurrentItemInfoShown)
			{
				base.ViewModel.InfoSectionVM.SetTemplate(ShipItemSelectorWindowVM.EquippedSlot.Tooltip.Value.FirstOrDefault());
				m_IsCurrentItemInfoShown = true;
			}
			else
			{
				base.ViewModel.InfoSectionVM.SetTemplate(m_CurrentEntity.GetVM.Tooltip.Value);
				m_IsCurrentItemInfoShown = false;
			}
		}
	}

	private IEnumerator UpdateUnequippedItemTooltipCo(ItemEntity item)
	{
		while (item.Owner != null)
		{
			yield return null;
		}
		if (m_NavigationBehaviour.DeepestNestedFocus is EquipSelectionSlotConsoleView equipSelectionSlotConsoleView && equipSelectionSlotConsoleView.EquipVM.Item == item)
		{
			equipSelectionSlotConsoleView.EquipVM.RefreshTooltip(forceUpdate: true);
			EntityFocused(equipSelectionSlotConsoleView);
		}
	}
}
