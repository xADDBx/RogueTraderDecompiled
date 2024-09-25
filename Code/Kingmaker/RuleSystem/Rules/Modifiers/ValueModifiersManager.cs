using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class ValueModifiersManager : AbstractModifiersManager
{
	public int Value => GetSum();

	public int GetValue(Predicate<Modifier> filter)
	{
		return GetSum(filter);
	}

	public void CopyFrom(ValueModifiersManager other, Predicate<Modifier> pred = null)
	{
		CopyFrom((AbstractModifiersManager)other, pred);
	}

	public void Add(int value, [NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Add(new Modifier(ModifierType.ValAdd, value, source, descriptor));
	}

	public void Add(int value, [NotNull] UnitCommand source, StatType stat)
	{
		Add(new Modifier(ModifierType.ValAdd, value, stat));
	}

	public void Add(int value, RulebookEvent source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(ModifierType.ValAdd, value, descriptor));
	}

	public void Add(int value, RulebookEvent source, StatType stat)
	{
		Add(new Modifier(ModifierType.ValAdd, value, stat));
	}

	public void Add(int value, ItemEntity source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(ModifierType.ValAdd, value, source, descriptor));
	}
}
