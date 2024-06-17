using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ShipCustomization;
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
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnClose();
		}));
		AddDisposable(m_InfoButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			ShowCurrentInstoledInfo();
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
		m_InfoButtonText.text = UIStrings.Instance.ContextMenu.Information;
		m_UnequipButtonText.text = UIStrings.Instance.ContextMenu.TakeOff;
		m_ConfirmButtonText.text = UIStrings.Instance.ContextMenu.Equip;
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
			m_SelectedEquipped.Value = CanUnequip(entity);
			m_InfoButton.gameObject.SetActive(m_CurrentEntity.Item != ShipItemSelectorWindowVM.EquippedSlot.Item.Value && ShipItemSelectorWindowVM.EquippedSlot.Item.Value != null);
			m_ConfirmButton.gameObject.SetActive(!m_SelectedEquipped.Value);
			m_UnequipButton.gameObject.SetActive(!m_ConfirmButton.gameObject.activeSelf);
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
