using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

public class LevelUpTotalStats
{
	[NotNull]
	public readonly List<Ability> NewAbilities = new List<Ability>();

	[NotNull]
	public readonly List<ActivatableAbility> NewActivatableAbilities = new List<ActivatableAbility>();

	[NotNull]
	public readonly List<UIFeature> NewAbilityFeatures = new List<UIFeature>();

	[NotNull]
	public readonly List<UIFeature> NewFeats = new List<UIFeature>();

	[NotNull]
	public readonly List<UIFeature> NewTraits = new List<UIFeature>();

	[NotNull]
	public readonly List<AbilityData> NewSpells = new List<AbilityData>();

	public LevelUpTotalStats(BaseUnitEntity before, BaseUnitEntity after)
	{
		try
		{
			BuildDifference(CollectAbilities(before), CollectAbilities(after), NewAbilities);
			BuildDifference(CollectActivatableAbilities(before), CollectActivatableAbilities(after), NewActivatableAbilities);
			BuildDifference(CollectAbilityFeatures(before), CollectAbilityFeatures(after), NewAbilityFeatures);
			BuildDifference(CollectFeats(before), CollectFeats(after), NewFeats);
			BuildDifference(CollectTraits(before), CollectTraits(after), NewTraits);
			BuildDifference(CollectSpells(before), CollectSpells(after), NewSpells);
		}
		finally
		{
		}
	}

	private IEnumerable<AbilityData> CollectSpells(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectSpells(unit);
	}

	private IEnumerable<UIFeature> CollectTraits(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectTraits(unit);
	}

	private IEnumerable<UIFeature> CollectFeats(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectFeats(unit);
	}

	private IEnumerable<UIFeature> CollectAbilityFeatures(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectAbilityFeatures(unit);
	}

	private IEnumerable<ActivatableAbility> CollectActivatableAbilities(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectActivatableAbilities(unit);
	}

	private IEnumerable<Ability> CollectAbilities(BaseUnitEntity unit)
	{
		return UIUtilityUnit.CollectAbilities(unit);
	}

	private static void BuildDifference<T>(IEnumerable<T> beforeEnumerable, IEnumerable<T> afterEnumerable, List<T> result) where T : EntityFact
	{
		T[] array = beforeEnumerable.ToArray();
		T[] array2 = afterEnumerable.ToArray();
		int i = 0;
		int j = 0;
		for (; i < array.Length; i++)
		{
			if (j >= array2.Length)
			{
				break;
			}
			if (array[i].Blueprint == array2[j].Blueprint)
			{
				j++;
			}
		}
		for (; j < array2.Length; j++)
		{
			result.Add(array2[j]);
		}
	}

	private static void BuildDifference(IEnumerable<UIFeature> beforeEnumerable, IEnumerable<UIFeature> afterEnumerable, List<UIFeature> result)
	{
		result.AddRange(afterEnumerable.Except(beforeEnumerable, new UIFeatureEqualityComparer()));
	}

	private static void BuildDifference(IEnumerable<AbilityData> beforeEnumerable, IEnumerable<AbilityData> afterEnumerable, List<AbilityData> result)
	{
		HashSet<BlueprintAbility> beforeSpells = new HashSet<BlueprintAbility>();
		beforeSpells.AddRange(beforeEnumerable.Select((AbilityData a) => a.Blueprint));
		result.AddRange(afterEnumerable.Where((AbilityData a) => !beforeSpells.Contains(a.Blueprint)));
	}
}
