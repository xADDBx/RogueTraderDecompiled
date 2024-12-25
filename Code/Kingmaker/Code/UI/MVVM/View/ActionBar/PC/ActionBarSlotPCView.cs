using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class ActionBarSlotPCView : ViewBase<ActionBarSlotVM>
{
	[SerializeField]
	private ActionBarSlotType m_ActionBarSlotType;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[Header("Hotkey block")]
	[SerializeField]
	private TextMeshProUGUI m_HotkeyText;

	[Header("Convert Block")]
	[SerializeField]
	private ActionBarConvertedPCView m_ConvertedView;

	[SerializeField]
	private OwlcatMultiButton m_ConvertButton;

	private IDisposable m_Tooltip;

	private string m_BindName;

	private SettingsEntityKeyBindingPair m_Binding;

	private IDisposable m_KeyBind;

	public IObservable<Unit> OnLeftClickAsObservable => m_MainButton.OnLeftClickAsObservable();

	public IObservable<Unit> OnRightClickAsObservable => m_MainButton.OnRightClickAsObservable();

	public IObservable<PointerEventData> OnPointerEnterAsObservable => m_MainButton.OnPointerEnterAsObservable();

	public IObservable<PointerEventData> OnPointerExitAsObservable => m_MainButton.OnPointerExitAsObservable();

	protected override void BindViewImplementation()
	{
		m_Tooltip = m_MainButton.SetTooltip(base.ViewModel.Tooltip);
		AddDisposable(m_Tooltip);
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnLeftClickAsObservable(), delegate
		{
			OnMainClick();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_MainButton.OnRightClickAsObservable(), delegate
		{
			OnSupportClick();
		}));
		AddDisposable(m_MainButton.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnPointerEnter();
		}));
		AddDisposable(m_MainButton.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnPointerExit();
		}));
		if (!m_ConvertedView)
		{
			return;
		}
		AddDisposable(base.ViewModel.HasConvert.And(base.ViewModel.IsPossibleActive).Subscribe(delegate(bool value)
		{
			m_ConvertedView.gameObject.SetActive(value);
			m_ConvertButton.Or(null)?.gameObject.SetActive(value);
		}));
		AddDisposable(base.ViewModel.ConvertedVm.Subscribe(delegate(ActionBarConvertedVM value)
		{
			m_ConvertedView.Bind(value);
			if (m_ConvertButton != null)
			{
				m_ConvertButton.SetActiveLayer((value != null) ? 1 : 0);
			}
		}));
		if (m_ConvertButton != null)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_ConvertButton.OnLeftClickAsObservable(), delegate
			{
				ShowConvertRequest();
			}));
		}
	}

	public void OnMainClick()
	{
		if (base.ViewModel == null || base.ViewModel.IsInCharScreen)
		{
			return;
		}
		if (!Game.Instance.SelectionCharacter.IsSingleSelected.Value)
		{
			if (PhotonManager.NetGame.CurrentState != NetGame.State.Playing || base.ViewModel.MechanicActionBarSlot.Unit.IsMyNetRole())
			{
				return;
			}
			PhotonManager.Ping.PressPing(delegate
			{
				if (!string.IsNullOrWhiteSpace(base.ViewModel.MechanicActionBarSlot.KeyName))
				{
					PhotonManager.Ping.PingActionBarAbility(base.ViewModel.MechanicActionBarSlot.KeyName, base.ViewModel.MechanicActionBarSlot.Unit, base.ViewModel.Index, base.ViewModel.WeaponSlotType);
				}
			});
		}
		else
		{
			base.ViewModel.OnMainClick();
			TooltipHelper.HideTooltip();
		}
	}

	public void OnSupportClick()
	{
		base.ViewModel.OnSupportClick();
	}

	public void OnPointerEnter()
	{
		if (!base.ViewModel.IsInCharScreen)
		{
			base.ViewModel.OnHoverOn();
		}
	}

	public void OnPointerExit()
	{
		if (!base.ViewModel.IsInCharScreen)
		{
			base.ViewModel.OnHoverOff();
		}
	}

	public void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_Tooltip?.Dispose();
		RemoveDisposable(m_Tooltip);
		m_Tooltip = m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, rectTransform, 0, 0, 0, pivots));
		AddDisposable(m_Tooltip);
	}

	public void SetKeyBinding(int index)
	{
		TryDestroyBinding();
		m_BindName = GetBindName(index);
		m_Binding = GetSettingsEntityKeyBindingPair(index);
		m_KeyBind = Game.Instance.Keyboard.Bind(m_BindName, OnMainClick);
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged += OnBindingChanged;
		}
		SetKeyBindLabel();
	}

	private void TryDestroyBinding()
	{
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged -= OnBindingChanged;
		}
		m_KeyBind?.Dispose();
		m_KeyBind = null;
	}

	private void OnBindingChanged(KeyBindingPair obj)
	{
		SetKeyBindLabel();
	}

	private void SetKeyBindLabel()
	{
		if (!(m_HotkeyText == null))
		{
			string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(m_BindName));
			m_HotkeyText.text = stringByBinding;
		}
	}

	private string GetBindName(int index)
	{
		return m_ActionBarSlotType switch
		{
			ActionBarSlotType.Ability => $"ActionBarAbilityButton{index:D2}", 
			ActionBarSlotType.Consumable => $"ActionBarConsumableButton{index:D2}", 
			ActionBarSlotType.WeaponAbility => $"ActionBarWeaponButton{index:D2}", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private SettingsEntityKeyBindingPair GetSettingsEntityKeyBindingPair(int index)
	{
		return m_ActionBarSlotType switch
		{
			ActionBarSlotType.Ability => SettingsRoot.Controls.Keybindings.ActionBar.GetAbilityBindingPair($"action-bar-ability-button-{index}"), 
			ActionBarSlotType.Consumable => SettingsRoot.Controls.Keybindings.ActionBar.GetConsumableBindingPair($"action-bar-consumable-button-{index}"), 
			ActionBarSlotType.WeaponAbility => SettingsRoot.Controls.Keybindings.ActionBar.GetWeaponBindingPair($"action-bar-weapon-button-{index}"), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override void DestroyViewImplementation()
	{
		TryDestroyBinding();
	}

	private void ShowConvertRequest()
	{
		if (m_ActionBarSlotType == ActionBarSlotType.WeaponAbility && base.ViewModel.MechanicActionBarSlot is MechanicActionBarShipWeaponSlot variantsShipWeaponSlot)
		{
			base.ViewModel.OnShowVariantsConvertRequest(variantsShipWeaponSlot);
		}
		else
		{
			base.ViewModel.OnShowConvertRequest();
		}
	}
}
