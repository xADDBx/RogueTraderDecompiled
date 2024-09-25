using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Tutorial.Solvers;

[TypeId("4b5bf1a762b0428e88ec3d6e8917dd3a")]
public class TutorialSolverSpellFromList : TutorialSolverSpellOrUsableItem
{
	[SerializeField]
	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	private BlueprintAbilityReference[] m_Spells;

	public ReferenceArrayProxy<BlueprintAbility> Spells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] spells = m_Spells;
			return spells;
		}
	}

	protected override int GetPriority(AbilityData ability)
	{
		int num = Spells.IndexOf(ability.Blueprint);
		if (num < 0)
		{
			return -1;
		}
		return num;
	}

	protected override int GetPriority(ItemEntity item)
	{
		return -1;
	}
}
