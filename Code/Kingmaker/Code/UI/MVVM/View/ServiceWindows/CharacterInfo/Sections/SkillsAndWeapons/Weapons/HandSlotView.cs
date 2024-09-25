using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Items;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class HandSlotView : ViewBase<EquipSlotVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
{
	[SerializeField]
	private OwlcatMultiButton m_Slot;

	[SerializeField]
	private Image m_ItemImage;

	[SerializeField]
	private GameObject m_EmptyImage;

	[SerializeField]
	private GameObject m_VacantBackground;

	private Action m_ClickAction;

	private bool m_CanConfirmConsoleClick;

	public OwlcatMultiButton Slot => m_Slot;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Item.Subscribe(delegate(ItemEntity item)
		{
			bool flag = item != null;
			m_VacantBackground.SetActive(flag);
			m_EmptyImage.SetActive(!flag);
		}));
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite sprite)
		{
			m_ItemImage.sprite = sprite;
			m_ItemImage.gameObject.SetActive(sprite != null);
		}));
		AddDisposable(base.ViewModel.CanBeFakeItem.Subscribe(delegate(bool isFake)
		{
			m_Slot.SetInteractable(!isFake);
			m_ItemImage.GetComponent<_2dxFX_GrayScale>().EffectAmount = (isFake ? 1f : 0f);
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		AddDisposable(Slot.OnLeftClickAsObservable().Subscribe(delegate
		{
			m_ClickAction?.Invoke();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetClickAction(Action clickAction)
	{
		m_ClickAction = clickAction;
	}

	public void SetCanConfirmClick(bool canConfirm)
	{
		m_CanConfirmConsoleClick = canConfirm;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public void SetFocus(bool value)
	{
		m_Slot.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Slot.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (m_Slot.IsValid())
		{
			return m_CanConfirmConsoleClick;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		m_ClickAction?.Invoke();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
