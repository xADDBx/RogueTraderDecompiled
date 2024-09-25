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
[TypeId("1545064d92184eef88475cffc5c2c321")]
public class RequirementProfitFactorCost : Requirement
{
	[SerializeField]
	public int ProfitFactorCost;

	public override bool Check(Colony colony = null)
	{
		return Game.Instance.Player.ProfitFactor.Total >= (float)ProfitFactorCost;
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
		Game.Instance.Player.ProfitFactor.AddModifier(-ProfitFactorCost, type, base.OwnerBlueprint);
	}
}
