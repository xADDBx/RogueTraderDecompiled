using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class ShipSelectorWindowPCView : SelectorWindowBaseView<ShipComponentItemSlotPCView, ShipComponentItemSlotVM>, ISelectingWindowFocusHandler, ISubscriber
{
	[SerializeField]
	private OwlcatButton m_Button;

	[SerializeField]
	private OwlcatButton m_InfoButton;

	[SerializeField]
	private TextMeshProUGUI m_InfoButtonText;

	[SerializeField]
	private OwlcatButton m_ConfirmButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmButtonText;

	[SerializeField]
	private OwlcatButton m_UnequipButton;

	[SerializeField]
	private TextMeshProUGUI m_UnequipButtonText;

	[SerializeField]
	private GameObject m_Background;

	private ShipComponentItemSlotVM m_CurrentEntity;

	private bool m_IsCurrentItemInfoShown;

	private ShipItemSelectorWindowVM ShipItemSelectorWindowVM => base.ViewModel as ShipItemSelectorWindowVM;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Header.text = UIStrings.Instance.InventoryScreen.ChooseItem;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnClose();
		}));
		AddDisposable(m_ConfirmButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnConfirm();
		}));
		AddDisposable(m_UnequipButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Unequip();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			OnClose();
		}));
		Focus(base.ViewModel.EntitiesCollection.FirstOrDefault((ShipComponentItemSlotVM x) => x != null));
		m_UnequipButtonText.text = UIStrings.Instance.ContextMenu.TakeOff;
		m_ConfirmButtonText.text = UIStrings.Instance.ContextMenu.Equip;
		m_InfoButton.Or(null)?.gameObject.SetActive(value: false);
		m_Background.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		OnClose();
	}

	private void OnConfirm()
	{
		if (base.ViewModel.CurrentSelected != null)
		{
			base.ViewModel.Confirm(base.ViewModel.CurrentSelected);
		}
		OnClose();
	}

	public void Focus(ShipComponentItemSlotVM entity)
	{
		if (entity != null)
		{
			base.ViewModel.SetCurrentSelected(entity);
			entity.SetSelected(state: true);
			base.ViewModel.InfoSectionVM.SetTemplate(entity.Tooltip.Value);
			m_CurrentEntity = entity;
			bool flag = CanUnequip(entity);
			m_SelectedEquipped.Value = flag;
			m_ConfirmButton.gameObject.SetActive(!flag);
			m_UnequipButton.gameObject.SetActive(flag);
		}
	}

	private bool CanUnequip(ShipComponentItemSlotVM entity)
	{
		return entity.Item.Owner != null;
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
				base.ViewModel.InfoSectionVM.SetTemplate(m_CurrentEntity.Tooltip.Value);
				m_IsCurrentItemInfoShown = false;
			}
		}
	}

	protected virtual void Unequip()
	{
		ShipItemSelectorWindowVM.Unequip();
		OnClose();
	}

	protected override void OnClose()
	{
		base.OnClose();
		m_Background.SetActive(value: false);
	}
}
