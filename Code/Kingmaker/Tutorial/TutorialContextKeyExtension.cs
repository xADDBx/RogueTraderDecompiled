using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Tutorial;

public static class TutorialContextKeyExtension
{
	private static readonly TutorialContextKey[] Unit = new TutorialContextKey[3]
	{
		TutorialContextKey.SourceUnit,
		TutorialContextKey.SolutionUnit,
		TutorialContextKey.TargetUnit
	};

	private static readonly TutorialContextKey[] Ability = new TutorialContextKey[4]
	{
		TutorialContextKey.SourceAbility,
		TutorialContextKey.SolutionAbility,
		TutorialContextKey.SourceItemOrAbility,
		TutorialContextKey.SolutionItemOrAbility
	};

	private static readonly TutorialContextKey[] Item = new TutorialContextKey[4]
	{
		TutorialContextKey.SourceItem,
		TutorialContextKey.SolutionItem,
		TutorialContextKey.SourceItemOrAbility,
		TutorialContextKey.SolutionItemOrAbility
	};

	private static readonly TutorialContextKey[] Int = new TutorialContextKey[7]
	{
		TutorialContextKey.SourceRoll,
		TutorialContextKey.SourceRollPlusBonus,
		TutorialContextKey.SourceRollDC,
		TutorialContextKey.TargetRoll,
		TutorialContextKey.TargetRollPlusBonus,
		TutorialContextKey.TargetRollDC,
		TutorialContextKey.Damage
	};

	private static readonly TutorialContextKey[] String = new TutorialContextKey[6]
	{
		TutorialContextKey.SourceRollDetails,
		TutorialContextKey.TargetRollDetails,
		TutorialContextKey.SourceRollDCDetails,
		TutorialContextKey.TargetRollDCDetails,
		TutorialContextKey.DamageDetails,
		TutorialContextKey.Descriptor
	};

	public static bool IsUnit(this TutorialContextKey key)
	{
		return Unit.HasItem(key);
	}

	public static bool IsAbility(this TutorialContextKey key)
	{
		return Ability.HasItem(key);
	}

	public static bool IsItem(this TutorialContextKey key)
	{
		return Item.HasItem(key);
	}

	public static bool IsInt(this TutorialContextKey key)
	{
		return Int.HasItem(key);
	}

	public static bool IsString(this TutorialContextKey key)
	{
		return String.HasItem(key);
	}

	public static TutorialContextKey GetPaired(this TutorialContextKey key)
	{
		switch (key)
		{
		case TutorialContextKey.SourceItem:
		case TutorialContextKey.SourceAbility:
			return TutorialContextKey.SourceItemOrAbility;
		case TutorialContextKey.SolutionItem:
		case TutorialContextKey.SolutionAbility:
			return TutorialContextKey.SolutionItemOrAbility;
		default:
			return TutorialContextKey.Invalid;
		}
	}
}
