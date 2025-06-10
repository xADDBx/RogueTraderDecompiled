using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("fb04b5db8f2a4d399495aa6c2cbd6325")]
public class PartyPetByType : AbstractUnitEvaluator
{
	[SerializeField]
	private PetType m_PetType;

	[SerializeField]
	private BlueprintPetOwnerPriorityConfigReference m_PriorityConfig;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		List<UnitPartPetOwner> list = (from u in Game.Instance.Player.Party
			where u != null
			select u.GetOptional<UnitPartPetOwner>() into petOwner
			where petOwner != null && petOwner.PetType == m_PetType
			select petOwner).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		PetOwnerPriorityConfig petOwnerPriorityConfig = m_PriorityConfig?.Get();
		if (petOwnerPriorityConfig == null)
		{
			return list.Random(PFStatefulRandom.Designers).PetUnit;
		}
		for (int i = 0; i < petOwnerPriorityConfig.PriorityOrder.Count; i++)
		{
			UnitPartPetOwner byPriority = petOwnerPriorityConfig.PriorityOrder[i].GetByPriority(list);
			if (byPriority != null)
			{
				return byPriority.PetUnit;
			}
		}
		return null;
	}

	public override string GetCaption()
	{
		return $"Random party pet of type {m_PetType}";
	}
}
