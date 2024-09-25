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
[TypeId("2aa503c54af44bd5b74037ea4f833dfb")]
public class RewardChangeStatEfficiency : Reward
{
	[SerializeField]
	private int m_EfficiencyModifier;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[HideIf("m_ApplyToAllColonies")]
	[SerializeField]
	private BlueprintColonyReference m_Colony;

	public int EfficiencyModifier => m_EfficiencyModifier;

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	public BlueprintColony Colony => m_Colony?.Get();

	private ColonyStatModifierType StatModifierType()
	{
		BlueprintScriptableObject ownerBlueprint = base.OwnerBlueprint;
		if (!(ownerBlueprint is BlueprintQuestContract))
		{
			if (!(ownerBlueprint is BlueprintColonyProject))
			{
				if (ownerBlueprint is BlueprintColonyChronicle)
				{
					return ColonyStatModifierType.Chronicles;
				}
				return ColonyStatModifierType.Other;
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
			colony.ChangeEfficiency(m_EfficiencyModifier, modifierType, modifier);
		}
		else if (Colony != null)
		{
			(Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyData) => colonyData.Colony.Blueprint == Colony)?.Colony)?.ChangeEfficiency(m_EfficiencyModifier, modifierType, modifier);
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
			h.HandleEfficiencyInAllColoniesChanged(m_EfficiencyModifier);
		});
		coloniesState.ContentmentModifiersForAllColonies.Add(new ColonyStatModifier
		{
			ModifierType = modifierType,
			Value = m_EfficiencyModifier,
			Modifier = modifier
		});
		foreach (ColoniesState.ColonyData colony in coloniesState.Colonies)
		{
			colony.Colony.ChangeEfficiency(m_EfficiencyModifier, modifierType, modifier);
		}
	}
}
