using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public readonly struct Modifier
{
	public static readonly ReadonlyList<Modifier> EmptyList = new ReadonlyList<Modifier>(null);

	public readonly ModifierType Type;

	public readonly int Value;

	[CanBeNull]
	public readonly EntityFact Fact;

	public readonly ItemEntity Item;

	public readonly BonusType Bonus;

	public readonly StatType Stat;

	public readonly ModifierDescriptor Descriptor;

	private Modifier(ModifierType type, int value, [CanBeNull] EntityFact fact, [CanBeNull] ItemEntity item, BonusType bonusType = BonusType.None, StatType stat = StatType.Unknown, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Type = type;
		Value = value;
		Fact = fact;
		Item = item;
		Bonus = bonusType;
		Stat = stat;
		Descriptor = descriptor;
	}

	public Modifier(ModifierType type, int value, [NotNull] EntityFact fact, ModifierDescriptor descriptor = ModifierDescriptor.None)
		: this(type, value, fact, null, BonusType.None, StatType.Unknown, descriptor)
	{
	}

	public Modifier(ModifierType type, int value, [NotNull] EntityFact fact, [NotNull] ItemEntity item, ModifierDescriptor descriptor = ModifierDescriptor.None)
		: this(type, value, fact, item, BonusType.None, StatType.Unknown, descriptor)
	{
	}

	public Modifier(ModifierType type, int value, [NotNull] ItemEntity item, ModifierDescriptor descriptor = ModifierDescriptor.None)
		: this(type, value, null, item, BonusType.None, StatType.Unknown, descriptor)
	{
	}

	private Modifier(ModifierType type, int value, BonusType bonus, ModifierDescriptor descriptor = ModifierDescriptor.None)
		: this(type, value, null, null, bonus, StatType.Unknown, descriptor)
	{
	}

	public Modifier(ModifierType type, int value, StatType stat)
		: this(type, value, null, null, BonusType.None, stat)
	{
	}

	public Modifier(ModifierType type, int value, ModifierDescriptor descriptor)
		: this(type, value, null, null, BonusType.None, StatType.Unknown, descriptor)
	{
	}

	public override string ToString()
	{
		return $"{StatModifiersBreakdown.GetBonusSourceText(this)}({Value})[{Descriptor}]";
	}

	public static bool IsPositive(Modifier m)
	{
		return m.Type switch
		{
			ModifierType.ValAdd => m.Value > 0, 
			ModifierType.PctAdd => m.Value > 0, 
			ModifierType.PctMul => m.Value >= 100, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
