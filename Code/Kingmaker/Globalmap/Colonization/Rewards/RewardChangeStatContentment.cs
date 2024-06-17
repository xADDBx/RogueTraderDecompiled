using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("2a09d956d9324196867de1714fc59e1e")]
public class RewardChangeStatContentment : Reward
{
	[SerializeField]
	private int m_ContentmentModifier;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	public int ContentmentModifier => m_ContentmentModifier;

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	public BlueprintColony Colony => m_Colony?.Get();

	private ColonyStatModifierType StatModifierType()
	{
		BlueprintScriptableObject ownerBlueprint = base.OwnerBlueprint;
		if (!(ownerBlueprint is BlueprintQuestContract))
		{
			if (!(ownerBlueprint is BlueprintColonyProject))
			{
				if (!(ownerBlueprint is BlueprintColonyChronicle))
				{
					if (ownerBlueprint is BlueprintColonyEventResult)
					{
						return ColonyStatModifierType.Event;
					}
					return ColonyStatModifierType.Other;
				}
				return ColonyStatModifierType.Chronicles;
			}
			return ColonyStatModifierType.Project;
		}
		return ColonyStatModifierType.Order;
	}

	public override void ReceiveReward(Colony colony = null)
	{
		ColonyStatModifierType modifierType = StatModifierType();
		BlueprintScriptableObject modifier = RewardOwner();
		if (ApplyToAllColonies)
		{
			ApplyRewardToAllColonies(modifierType);
		}
		else if (colony != null)
		{
			colony.ChangeContentment(m_ContentmentModifier, modifierType, modifier);
		}
		else if (Colony != null)
		{
			(Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyData) => colonyData.Colony.Blueprint == Colony)?.Colony)?.ChangeContentment(m_ContentmentModifier, modifierType, modifier);
		}
		EventBus.RaiseEvent(delegate(IColonyStatsHandler h)
		{
			h.HandleColonyStatsChanged();
		});
	}

	private void ApplyRewardToAllColonies(ColonyStatModifierType modifierType)
	{
		BlueprintScriptableObject modifier = RewardOwner();
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
		{
			h.HandleContentmentInAllColoniesChanged(m_ContentmentModifier);
		});
		coloniesState.ContentmentModifiersForAllColonies.Add(new ColonyStatModifier
		{
			ModifierType = modifierType,
			Value = m_ContentmentModifier,
			Modifier = modifier
		});
		foreach (ColoniesState.ColonyData colony in coloniesState.Colonies)
		{
			colony.Colony.ChangeContentment(m_ContentmentModifier, modifierType, modifier);
		}
	}
}
