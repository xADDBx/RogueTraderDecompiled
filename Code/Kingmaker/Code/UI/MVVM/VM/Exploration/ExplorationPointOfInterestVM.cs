using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationPointOfInterestVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInteractable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<BlueprintPointOfInterest> PointOfInterestBlueprintType = new ReactiveProperty<BlueprintPointOfInterest>();

	public readonly ReactiveProperty<bool> IsQuest = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsRumour = new ReactiveProperty<bool>(initialValue: false);

	private readonly BasePointOfInterest m_PointOfInterest;

	private readonly StarSystemObjectEntity m_Entity;

	private bool m_IsFocused;

	public bool IsFocused => m_IsFocused;

	public ExplorationPointOfInterestVM(BasePointOfInterest pointOfInterest, StarSystemObjectEntity entity)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_PointOfInterest = pointOfInterest;
		m_Entity = entity;
		PointOfInterestBlueprintType.Value = pointOfInterest.Blueprint;
		Name.Value = pointOfInterest.Blueprint?.Name;
		Icon.Value = pointOfInterest.Blueprint?.Icon;
		IsExplored.Value = pointOfInterest.Status == BasePointOfInterest.ExplorationStatus.Explored;
		IsVisible.Value = pointOfInterest.IsVisible();
		IsInteractable.Value = pointOfInterest.IsInteractable();
		CheckQuests();
	}

	public void Interact()
	{
		if (!IsInteractable.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ExplorationTexts.ExploNotInteractable, addToLog: true, WarningNotificationFormat.Attention);
			});
			return;
		}
		if (IsExplored.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ExplorationTexts.ExploAlreadyExplored);
			});
			return;
		}
		BlueprintPointOfInterest value = PointOfInterestBlueprintType.Value;
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			PlayInteractionSound(value);
			Game.Instance.GameCommandQueue.PointOfInterestInteract(m_Entity, m_PointOfInterest.Blueprint);
		}
	}

	private void PlayInteractionSound(BlueprintPointOfInterest poi)
	{
		BlueprintUISound sounds = UISounds.Instance.Sounds;
		if (!(poi is BlueprintPointOfInterestBookEvent))
		{
			if (!(poi is BlueprintPointOfInterestCargo) && !(poi is BlueprintPointOfInterestLoot) && !(poi is BlueprintPointOfInterestStatCheckLoot))
			{
				if (!(poi is BlueprintPointOfInterestColonyTrait) && !(poi is BlueprintPointOfInterestExpedition) && !(poi is BlueprintPointOfInterestGroundOperation))
				{
					_ = poi is BlueprintPointOfInterestResources;
				}
			}
			else
			{
				sounds.SpaceExploration.GetLoot.Play();
			}
		}
		else
		{
			sounds.SpaceExploration.DialogEvent.Play();
		}
	}

	public void SetFocus(bool value)
	{
		m_IsFocused = value;
	}

	protected override void DisposeImplementation()
	{
	}

	private void CheckQuests()
	{
		if (!(m_PointOfInterest is PointOfInterestGroundOperation groundPoi))
		{
			return;
		}
		PlanetView planetView = m_Entity.View as PlanetView;
		if (planetView == null)
		{
			PFLog.UI.Log("ExplorationPointOfInterestVM.CheckQuests - current planet is null!");
			return;
		}
		List<QuestObjective> questsForPlanet = UIUtilitySpaceQuests.GetQuestsForPlanet(planetView.Data.Blueprint);
		StringBuilder stringBuilder = new StringBuilder();
		int num = 1;
		foreach (QuestObjective item in questsForPlanet)
		{
			foreach (BlueprintArea area in item.Blueprint.Areas)
			{
				if (m_Entity.IsSsoPointConnectsToArea(area, groundPoi))
				{
					IsQuest.Value = true;
					IsRumour.Value = item.Blueprint.Quest.Group == QuestGroupId.Rumours;
					if (IsQuest.Value && !string.IsNullOrWhiteSpace(item.Blueprint.GetTitile()))
					{
						stringBuilder.AppendLine($"{num}. {item.Blueprint.GetTitile().Text}");
						num++;
					}
				}
			}
		}
		QuestObjectiveName.Value = stringBuilder.ToString();
	}
}
