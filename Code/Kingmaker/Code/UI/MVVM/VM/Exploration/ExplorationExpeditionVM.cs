using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Exploration;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationExpeditionVM : ExplorationComponentBaseVM, IExplorationExpeditionHandler, ISubscriber
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<int> UnlockedRewardIndex = new ReactiveProperty<int>(-1);

	public readonly ReactiveProperty<int> PeopleCount = new ReactiveProperty<int>(1);

	public readonly ReactiveProperty<int> MaxPeopleCount = new ReactiveProperty<int>(1);

	private PointOfInterestExpedition m_PointOfInterest;

	public ExplorationExpeditionVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	public void HandlePointOfInterestExpeditionInteraction(PointOfInterestExpedition pointOfInterest)
	{
		m_PointOfInterest = pointOfInterest;
		MaxPeopleCount.Value = pointOfInterest.Blueprint.MaxExpeditionPeopleCount;
		ShouldShow.Value = true;
	}

	public void Hide()
	{
		ShouldShow.Value = false;
	}

	public void SendExpedition()
	{
		m_PointOfInterest.SendExpedition(PeopleCount.Value);
		UIUtility.ShowMessageBox(UIStrings.Instance.ExplorationTexts.ExpeditionSentDialogMessage, DialogMessageBoxBase.BoxType.Message, OnSendExpedition);
	}

	public void SetPeopleCount(int count)
	{
		if (count >= 1 && count <= MaxPeopleCount.Value)
		{
			PeopleCount.Value = count;
			UpdateRewards();
		}
	}

	private void UpdateRewards()
	{
		if (m_PointOfInterest == null)
		{
			return;
		}
		List<BlueprintPointOfInterestExpedition.ExpeditionLoot> rewards = m_PointOfInterest.Blueprint.Rewards;
		int value = -1;
		float num = 0f;
		for (int i = 0; i < rewards.Count; i++)
		{
			num += Mathf.Floor(rewards[i].ExpeditionProportion * (float)MaxPeopleCount.Value);
			if (num <= (float)PeopleCount.Value)
			{
				value = i;
			}
		}
		UnlockedRewardIndex.Value = value;
	}

	private void OnSendExpedition(DialogMessageBoxBase.BoxButton button)
	{
		ShouldShow.Value = false;
	}
}
