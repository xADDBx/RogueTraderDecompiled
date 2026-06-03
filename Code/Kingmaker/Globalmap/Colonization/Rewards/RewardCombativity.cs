using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowedOn(typeof(BlueprintCue))]
[TypeId("39fc49304a55e72448f1ea90f131949a")]
public class RewardCombativity : Reward
{
	[SerializeField]
	private int m_Combativity;

	public int Combativity => m_Combativity;

	protected ProfitFactorModifierType ModifierType()
	{
		BlueprintScriptableObject ownerBlueprint = base.OwnerBlueprint;
		if (!(ownerBlueprint is BlueprintQuestContract))
		{
			if (!(ownerBlueprint is BlueprintColonyProject))
			{
				if (!(ownerBlueprint is BlueprintColonyChronicle))
				{
					if (!(ownerBlueprint is BlueprintColonyEventResult))
					{
						if (!(ownerBlueprint is BlueprintAnswer))
						{
							if (ownerBlueprint is BlueprintCue)
							{
								return ProfitFactorModifierType.Cue;
							}
							return ProfitFactorModifierType.Other;
						}
						return ProfitFactorModifierType.Answer;
					}
					return ProfitFactorModifierType.Event;
				}
				return ProfitFactorModifierType.Chronicles;
			}
			return ProfitFactorModifierType.Project;
		}
		return ProfitFactorModifierType.Order;
	}

	public override void ReceiveReward(Colony colony = null)
	{
		ProfitFactorModifierType profitFactorModifierType = ModifierType();
		float num = 1f;
		if (colony != null && profitFactorModifierType == ProfitFactorModifierType.Project && m_Combativity > 0)
		{
			num = 1f + (float)colony.Contentment.Value / 10f;
		}
		Game.Instance.Player.Combativity.AddModifier(Mathf.CeilToInt((float)m_Combativity * num), profitFactorModifierType, RewardOwner());
	}
}
