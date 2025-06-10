using System.Collections.Generic;
using Code.Enums;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

public static class PetOwnerPriorityConfigExtensions
{
	public static UnitPartPetOwner GetByPriority(this PetOwnerPriorityConfig.PetOwnerSpecificPriority priorityFilter, List<UnitPartPetOwner> units)
	{
		return priorityFilter.PriorityType switch
		{
			PetOwnerPriority.RogueTrader => units.FirstOrDefault((UnitPartPetOwner u) => u.Owner.IsMainCharacter), 
			PetOwnerPriority.CompanionWithFeature => units.FirstOrDefault((UnitPartPetOwner u) => u.Owner.Facts.Contains((BlueprintUnitFact)priorityFilter.FeatureFilter)), 
			PetOwnerPriority.CurrentSelected => units.FirstOrDefault((UnitPartPetOwner u) => u.Owner.IsSelected), 
			PetOwnerPriority.Any => units.Random(PFStatefulRandom.Designers), 
			_ => null, 
		};
	}
}
