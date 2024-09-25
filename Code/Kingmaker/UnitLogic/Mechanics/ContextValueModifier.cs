using System;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Facts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValueModifier : ContextValue
{
	[HideInInspector]
	public bool Enabled;

	public void TryApply(AbstractModifiersManager manager, MechanicEntityFact sourceFact, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int value = Calculate(sourceFact.MaybeContext);
			manager.Add(ModifierType.ValAdd, value, sourceFact, descriptor);
		}
	}
}
