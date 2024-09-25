using System;
using JetBrains.Annotations;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics.Facts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextValueModifierWithType : ContextValueModifier
{
	[HideInInspector]
	public ModifierType ModifierType;

	public void TryApply([NotNull] CompositeModifiersManager target, [NotNull] MechanicEntityFact sourceFact, ModifierDescriptor descriptor)
	{
		if (Enabled)
		{
			int value = Calculate(sourceFact.MaybeContext);
			target.Add(ModifierType, value, sourceFact, descriptor);
		}
	}
}
