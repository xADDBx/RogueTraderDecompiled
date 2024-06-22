using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.PagesMenu;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Stories;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public class CharacterInfoPCView : ViewBase<CharacterInfoVM>
{
	[SerializeField]
	protected CharInfoPagesMenuPCView m_CharInfoPagesMenu;

	[SerializeField]
	protected CharInfoNameAndPortraitPCView m_NameAndPortraitView;

	[SerializeField]
	private CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsAndWeaponsBaseView m_SkillsAndWeaponsView;

	[SerializeField]
	private CharInfoAlignmentWheelPCView m_AlignmentWheelView;

	[SerializeField]
	private CharInfoAbilitiesBaseView m_AbilitiesView;

	[SerializeField]
	private CharInfoChoicesMadeView m_ChoicesMadeView;

	[SerializeField]
	private CharInfoNameAndPortraitPCView m_NameFullPortraitPCView;

	[SerializeField]
	private CharInfoFactionsReputationPCView m_FactionsReputationPCView;

	[SerializeField]
	private CharInfoStoriesView m_BiographyStoriesView;

	[SerializeField]
	protected UnitProgressionCommonView m_ProgressionView;

	[SerializeField]
	private CharInfoSummaryPCView m_SummaryView;

	protected readonly Dictionary<CharInfoComponentType, ICharInfoComponentView> ComponentViews = new Dictionary<CharInfoComponentType, ICharInfoComponentView>();

	public virtual void Initialize()
	{
		m_NameAndPortraitView.Initialize();
		ComponentViews[CharInfoComponentType.NameAndPortrait] = m_NameAndPortraitView;
		m_LevelClassScoresView.Initialize();
		ComponentViews[CharInfoComponentType.LevelClassScores] = m_LevelClassScoresView;
		m_SkillsAndWeaponsView.Initialize();
		ComponentViews[CharInfoComponentType.SkillsAndWeapons] = m_SkillsAndWeaponsView;
		m_AlignmentWheelView.Initialize();
		ComponentViews[CharInfoComponentType.AlignmentWheel] = m_AlignmentWheelView;
		m_AbilitiesView.Initialize();
		ComponentViews[CharInfoComponentType.Abilities] = m_AbilitiesView;
		m_ChoicesMadeView.Initialize();
		ComponentViews[CharInfoComponentType.AlignmentHistory] = m_ChoicesMadeView;
		m_NameFullPortraitPCView.Initialize();
		ComponentViews[CharInfoComponentType.NameFullPortrait] = m_NameFullPortraitPCView;
		m_FactionsReputationPCView.Initialize();
		ComponentViews[CharInfoComponentType.FactionsReputation] = m_FactionsReputationPCView;
		m_BiographyStoriesView.Initialize();
		ComponentViews[CharInfoComponentType.BiographyStories] = m_BiographyStoriesView;
		m_ProgressionView.Initialize(OnProgressionWindowStateChange);
		ComponentViews[CharInfoComponentType.Progression] = m_ProgressionView;
		m_SummaryView.Initialize();
		ComponentViews[CharInfoComponentType.Summary] = m_SummaryView;
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		m_CharInfoPagesMenu.Bind(base.ViewModel.PagesSelectionGroupRadioVM);
		foreach (KeyValuePair<CharInfoComponentType, ICharInfoComponentView> componentView in ComponentViews)
		{
			AddDisposable(base.ViewModel.ComponentVMs[componentView.Key].Subscribe(componentView.Value.BindSection));
		}
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	protected virtual void OnProgressionWindowStateChange(UnitProgressionWindowState state)
	{
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		OnShow();
		UISounds.Instance.Sounds.Character.CharacterInfoShow.Play();
	}

	private void OnShow()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.CharacterScreen);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: true, FullScreenUIType.CharacterScreen);
		});
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	private void HideWindow()
	{
		OnHide();
		ContextMenuHelper.HideContextMenu();
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.Character.CharacterInfoHide.Play();
	}

	private void OnHide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.CharacterScreen);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: false, FullScreenUIType.CharacterScreen);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
	}
}
