using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class PercentsModifiersManager : AbstractModifiersManager
{
	public float Value => 1f + (float)GetSum() / 100f;

	public float Bonus => (float)GetSum() / 100f;

	public int FlatBonus => GetSum();

	public void Add(int value, [NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Add(new Modifier(ModifierType.PctMul, value, source, descriptor));
	}

	public void Add(int value, [NotNull] UnitCommand source, StatType stat)
	{
		Add(new Modifier(ModifierType.PctAdd, value, stat));
	}

	public void Add(int value, RulebookEvent source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(ModifierType.PctAdd, value, descriptor));
	}

	public void Add(int value, RulebookEvent source, StatType stat)
	{
		Add(new Modifier(ModifierType.PctAdd, value, stat));
	}

	public void Add(int value, ItemEntity source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(ModifierType.PctAdd, value, source, descriptor));
	}
}
