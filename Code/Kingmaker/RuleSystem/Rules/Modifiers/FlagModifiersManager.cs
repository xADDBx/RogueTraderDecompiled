using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class FlagModifiersManager : ValueModifiersManager
{
	public new bool Value => base.Value > 0;

	public void Add([NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Add(1, source, descriptor);
	}

	public void Add(RulebookEvent source, ModifierDescriptor descriptor)
	{
		Add(1, source, descriptor);
	}

	public void Add(RulebookEvent source, StatType stat)
	{
		Add(1, source, stat);
	}

	public void Add(ItemEntity source, ModifierDescriptor descriptor)
	{
		Add(1, source, descriptor);
	}
}
