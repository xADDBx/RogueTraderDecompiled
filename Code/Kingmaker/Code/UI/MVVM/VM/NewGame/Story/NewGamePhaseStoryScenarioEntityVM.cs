using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine.Pool;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Story;

public class NewGamePhaseStoryScenarioEntityVM : SelectionGroupEntityVM
{
	private readonly BlueprintCampaignReference m_CampaignReference;

	private Action m_SetStory;

	public readonly List<NewGamePhaseStoryScenarioEntityIntegralDlcVM> IntegralDlcVms;

	private BlueprintCampaign Campaign
	{
		get
		{
			if (m_CampaignReference != null && !m_CampaignReference.IsEmpty())
			{
				return m_CampaignReference.Get();
			}
			return null;
		}
	}

	public BoolReactiveProperty IsStoryIsAvailable { get; } = new BoolReactiveProperty();


	public string Title
	{
		get
		{
			LocalizedString localizedString = Campaign?.Title;
			if (localizedString == null)
			{
				return "I have no story. What is my purpose?";
			}
			return localizedString;
		}
	}

	public NewGamePhaseStoryScenarioEntityVM(BlueprintCampaign campaign, Action setStory)
		: base(allowSwitchOff: false)
	{
		m_CampaignReference = campaign.ToReference<BlueprintCampaignReference>();
		m_SetStory = setStory;
		IntegralDlcVms = new List<NewGamePhaseStoryScenarioEntityIntegralDlcVM>();
		foreach (BlueprintDlc item in campaign.AdditionalContentDlc.Where((BlueprintDlc dlc) => !dlc.HideWhoNotBuyDlc && !dlc.HideDlcForAll))
		{
			IntegralDlcVms.Add(new NewGamePhaseStoryScenarioEntityIntegralDlcVM(campaign, item));
		}
		SetAvailableState();
	}

	protected override void DisposeImplementation()
	{
		m_SetStory = null;
	}

	private void SetAvailableState()
	{
		BlueprintAreaPreset blueprintAreaPreset = Campaign?.StartGamePreset ?? GameStarter.MainPreset;
		BlueprintCampaign campaign = Campaign;
		bool value = campaign != null && campaign.IsAvailable && blueprintAreaPreset != null;
		IsStoryIsAvailable.Value = value;
	}

	public void SelectMe()
	{
		UpdateDlcSelectionStatus();
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_SetStory?.Invoke();
	}

	private void UpdateDlcSelectionStatus()
	{
		if (m_CampaignReference.IsEmpty())
		{
			return;
		}
		HashSet<BlueprintDlc> hashSet = CollectionPool<HashSet<BlueprintDlc>, BlueprintDlc>.Get();
		foreach (BlueprintDlc item in Game.Instance.Player.GetStartNewGameAdditionalContentDlc())
		{
			hashSet.Add(item);
		}
		foreach (BlueprintDlc item2 in m_CampaignReference.Get().AdditionalContentDlc.Where((BlueprintDlc dlc) => !dlc.HideWhoNotBuyDlc && !dlc.HideDlcForAll && dlc.IsAvailable))
		{
			if (!hashSet.Contains(item2))
			{
				Game.Instance.Player.UpdateAdditionalContentDlcStatus(item2, status: true);
			}
			else
			{
				hashSet.Remove(item2);
			}
		}
		foreach (BlueprintDlc item3 in hashSet)
		{
			Game.Instance.Player.RemoveAdditionalContentDlc(item3);
		}
		hashSet.Clear();
		CollectionPool<HashSet<BlueprintDlc>, BlueprintDlc>.Release(hashSet);
	}
}
