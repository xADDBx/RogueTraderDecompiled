using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarPartWeaponsPCView : ViewBase<SurfaceActionBarPartWeaponsVM>
{
	[SerializeField]
	private SurfaceActionBarWeaponSetPCView m_CurrentSet;

	[SerializeField]
	private SurfaceActionBarWeaponSetPCView m_WeaponSetPrefab;

	[SerializeField]
	private OwlcatMultiButton m_ConvertButton;

	[Header("WeaponSets")]
	[SerializeField]
	private RectTransform m_WeaponSetsContainer;

	[SerializeField]
	private MoveAnimator m_WeaponSetsMoveAnimator;

	[SerializeField]
	private FadeAnimator m_WeaponSetsFadeAnimator;

	[Header("Hotkey block")]
	[SerializeField]
	private TextMeshProUGUI m_HotkeyText;

	[SerializeField]
	private GameObject m_HotkeyContainer;

	private string m_BindName;

	private SettingsEntityKeyBindingPair m_Binding;

	private readonly List<SurfaceActionBarWeaponSetPCView> m_WeaponSets = new List<SurfaceActionBarWeaponSetPCView>();

	private readonly BoolReactiveProperty m_WeaponSetsContainerPinned = new BoolReactiveProperty();

	public void Initialize()
	{
		m_CurrentSet.Initialize(setKeyBindings: true);
		m_BindName = "ChangeWeaponSet";
		m_Binding = SettingsRoot.Controls.Keybindings.ActionBar.GetBindingPair("change-weapon-set");
		SetKeyBindLabel();
	}

	protected override void BindViewImplementation()
	{
		UpdateSets();
		AddDisposable(base.ViewModel.UnitChanged.Subscribe(UpdateSets));
		AddDisposable(base.ViewModel.CurrentSet.Subscribe(m_CurrentSet.Bind));
		UISounds.Instance.SetClickAndHoverSound(m_ConvertButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(base.ViewModel.CanSwitchSets.Subscribe(m_ConvertButton.SetInteractable));
		AddDisposable(UniRxExtensionMethods.Subscribe(m_ConvertButton.OnLeftClickAsObservable(), delegate
		{
			m_WeaponSetsContainerPinned.Value = !m_WeaponSetsContainerPinned.Value;
		}));
		AddDisposable(m_WeaponSetsContainerPinned.Subscribe(SwitchWeaponSetsPinnedState));
		AddDisposable(m_ConvertButton.SetHint(UIStrings.Instance.ActionBar.ActionBarConvertWeapons));
		AddDisposable(Game.Instance.Keyboard.Bind(m_BindName, base.ViewModel.ChangeWeaponSet));
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged += OnBindingChanged;
		}
	}

	protected override void DestroyViewImplementation()
	{
		ClearSets();
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged -= OnBindingChanged;
		}
	}

	private void UpdateSets()
	{
		m_WeaponSetsContainerPinned.Value = false;
		for (int i = 0; i < base.ViewModel.Sets.Count; i++)
		{
			SurfaceActionBarPartWeaponSetVM viewModel = base.ViewModel.Sets[i];
			SurfaceActionBarWeaponSetPCView setView;
			if (i >= m_WeaponSets.Count)
			{
				setView = WidgetFactory.GetWidget(m_WeaponSetPrefab);
				setView.transform.SetParent(m_WeaponSetsContainer, worldPositionStays: false);
				m_WeaponSets.Add(setView);
			}
			else
			{
				setView = m_WeaponSets[i];
			}
			setView.Initialize(setKeyBindings: false);
			setView.SetSwitchButtonCallback(delegate
			{
				SwitchButtonPressed(setView);
			});
			setView.Bind(viewModel);
		}
		for (int j = base.ViewModel.Sets.Count; j < m_WeaponSets.Count; j++)
		{
			m_WeaponSets[j].Unbind();
		}
	}

	private void ClearSets()
	{
		m_WeaponSetsContainerPinned.Value = false;
		m_WeaponSets.ForEach(WidgetFactory.DisposeWidget);
		m_WeaponSets.Clear();
	}

	private void SwitchWeaponSetsPinnedState(bool value)
	{
		if (value)
		{
			m_ConvertButton.SetActiveLayer("Active");
			m_WeaponSetsMoveAnimator.AppearAnimation();
			m_WeaponSetsFadeAnimator.Or(null)?.AppearAnimation();
			UISounds.Instance.Sounds.ActionBar.WeaponListOpen.Play();
		}
		else
		{
			m_ConvertButton.SetActiveLayer("Normal");
			m_WeaponSetsMoveAnimator.DisappearAnimation();
			m_WeaponSetsFadeAnimator.Or(null)?.DisappearAnimation();
			UISounds.Instance.Sounds.ActionBar.WeaponListClose.Play();
		}
	}

	private void SwitchButtonPressed(SurfaceActionBarWeaponSetPCView view)
	{
		if (view.IsCurrent)
		{
			m_WeaponSetsContainerPinned.Value = !m_WeaponSetsContainerPinned.Value;
			return;
		}
		view.SwitchWeapon();
		m_WeaponSetsContainerPinned.Value = false;
	}

	private void OnBindingChanged(KeyBindingPair obj)
	{
		SetKeyBindLabel();
	}

	private void SetKeyBindLabel()
	{
		string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(m_BindName));
		m_HotkeyText.text = stringByBinding;
		m_HotkeyContainer.SetActive(stringByBinding.Length > 0);
	}
}
