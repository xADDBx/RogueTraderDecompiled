using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("32bc20084b6582e4ca92d801e3701e4b")]
public class RequirementCombativityCost : Requirement
{
	[SerializeField]
	public int CombativityCost;

	public override bool Check(Colony colony = null)
	{
		return Game.Instance.Player.Combativity.Total >= (float)CombativityCost;
	}

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

	public override void Apply(Colony colony = null)
	{
		ProfitFactorModifierType type = ModifierType();
		Game.Instance.Player.Combativity.AddModifier(-CombativityCost, type, base.OwnerBlueprint);
	}
}
