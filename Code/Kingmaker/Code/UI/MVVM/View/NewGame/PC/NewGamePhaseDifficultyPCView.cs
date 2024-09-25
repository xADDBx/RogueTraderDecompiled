using System;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.PC;

public class NewGamePhaseDifficultyPCView : NewGamePhaseDifficultyBaseView
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolPCView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownPCView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderPCView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyPCView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSavePCView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[SerializeField]
	private SettingsViews m_SettingsViews;

	[SerializeField]
	private OwlcatButton m_DefaultDifficultyButton;

	public void Initialize()
	{
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.SettingEntities));
		base.BindViewImplementation();
		if (m_DefaultDifficultyButton != null)
		{
			AddDisposable(base.ViewModel.IsEnabled.CombineLatest(base.ViewModel.IsDefaultButtonInteractable, (bool isEnabled, bool defaultButtonInteractable) => new { isEnabled, defaultButtonInteractable }).Subscribe(value =>
			{
				m_DefaultDifficultyButton.SetInteractable(value.defaultButtonInteractable);
				m_DefaultDifficultyButton.gameObject.SetActive(value.isEnabled);
			}));
			AddDisposable(m_DefaultDifficultyButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.OpenDefaultSettingsDialog();
			}));
		}
	}
}
