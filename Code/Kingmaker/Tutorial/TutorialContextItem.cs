using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Tutorial;

public readonly struct TutorialContextItem
{
	public readonly int? Number;

	[CanBeNull]
	public readonly string Text;

	[CanBeNull]
	public readonly BlueprintScriptableObject Blueprint;

	[CanBeNull]
	public readonly Entity Entity;

	[CanBeNull]
	public readonly EntityFact Fact;

	[CanBeNull]
	public readonly AbilityData Ability;

	[CanBeNull]
	public BaseUnitEntity Unit => Entity as BaseUnitEntity;

	[CanBeNull]
	public ItemEntity Item => Entity as ItemEntity;

	public TutorialContextItem(int number)
	{
		this = default(TutorialContextItem);
		Number = number;
	}

	public TutorialContextItem([NotNull] string text)
	{
		this = default(TutorialContextItem);
		Text = text;
	}

	public TutorialContextItem([NotNull] Entity entity)
	{
		this = default(TutorialContextItem);
		Entity = entity;
	}

	public TutorialContextItem([NotNull] EntityFact fact)
	{
		this = default(TutorialContextItem);
		Fact = fact;
	}

	public TutorialContextItem([NotNull] AbilityData ability)
	{
		this = default(TutorialContextItem);
		Ability = ability;
	}

	public TutorialContextItem([NotNull] BlueprintScriptableObject blueprint)
	{
		this = default(TutorialContextItem);
		Blueprint = blueprint;
	}

	public static implicit operator TutorialContextItem(int value)
	{
		return new TutorialContextItem(value);
	}

	public static implicit operator TutorialContextItem(string text)
	{
		return new TutorialContextItem(text);
	}

	public static implicit operator TutorialContextItem(Entity entity)
	{
		return new TutorialContextItem(entity);
	}

	public static implicit operator TutorialContextItem(EntityFact fact)
	{
		return new TutorialContextItem(fact);
	}

	public static implicit operator TutorialContextItem(AbilityData ability)
	{
		return new TutorialContextItem(ability);
	}

	public static implicit operator TutorialContextItem(BlueprintScriptableObject blueprint)
	{
		return new TutorialContextItem(blueprint);
	}

	public static implicit operator TutorialContextItem(StatType type)
	{
		return UIUtility.GetStatText(type);
	}
}
