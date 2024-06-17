using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.MVVM.VM.MainMenu;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Story;

public class NewGamePhaseStoryVM : NewGamePhaseBaseVm
{
	private readonly ReactiveProperty<NewGamePhaseStoryScenarioEntityVM> m_SelectedEntity = new ReactiveProperty<NewGamePhaseStoryScenarioEntityVM>();

	public readonly SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM> SelectionGroup;

	public ReactiveProperty<Sprite> Art { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<bool> IsDlcRequired { get; } = new ReactiveProperty<bool>();


	public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();


	public NewGamePhaseStoryVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		List<NewGamePhaseStoryScenarioEntityVM> list = new List<NewGamePhaseStoryScenarioEntityVM>();
		foreach (NewGameRoot.StoryEntity storyEntity in BlueprintRoot.Instance.NewGameSettings.StoryList)
		{
			NewGamePhaseStoryScenarioEntityVM newGamePhaseStoryScenarioEntityVM = new NewGamePhaseStoryScenarioEntityVM(storyEntity, delegate
			{
				SetStory(storyEntity);
			});
			list.Add(newGamePhaseStoryScenarioEntityVM);
			AddDisposable(newGamePhaseStoryScenarioEntityVM);
		}
		SelectionGroup = new SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM>(list, m_SelectedEntity);
		AddDisposable(SelectionGroup);
		m_SelectedEntity.Value = list.First();
	}

	private void SetStory(NewGameRoot.StoryEntity story)
	{
		Art.Value = story.KeyArt;
		Description.Value = story.Description;
		BlueprintAreaPreset blueprintAreaPreset = story.DlcReward?.Campaign?.StartGamePreset ?? GameStarter.MainPreset;
		bool flag = (story.DlcReward == null || (story.DlcReward?.Campaign?.IsAvailable).GetValueOrDefault()) && blueprintAreaPreset != null;
		MainMenuChargenUnits.Instance.DlcReward = story.DlcReward;
		IsDlcRequired.Value = !flag;
		IsNextButtonAvailable.Value = flag;
	}

	public override void OnNext()
	{
		if (IsNextButtonAvailable.Value)
		{
			Game.NewGamePreset = GetPreset();
			base.OnNext();
		}
	}

	private BlueprintAreaPreset GetPreset()
	{
		return MainMenuChargenUnits.Instance.DlcReward?.Campaign?.StartGamePreset ?? GameStarter.MainPreset;
	}
}
