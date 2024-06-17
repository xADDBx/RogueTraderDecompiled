using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public static class ShadowSpellLogicExtension
{
	public static int TryApplyShadowDamageFactor([NotNull] this RulebookEvent evt, MechanicEntity target, int damageValue)
	{
		return (evt.Reason.Context ?? evt.ConcreteInitiator.GetOptional<UnitPartShadowSummon>()?.Context).TryApplyShadowDamageFactor(target, damageValue);
	}

	private static int TryApplyShadowDamageFactor([CanBeNull] this MechanicsContext context, MechanicEntity target, int damageValue)
	{
		if (damageValue < 1 || context == null || !context.IsShadow || context.TryAffectByShadow(target, checkChance: false))
		{
			return damageValue;
		}
		return Math.Max(1, Mathf.CeilToInt((float)damageValue * context.ShadowFactor));
	}

	public static bool SkipBecauseOfShadow([NotNull] this RulebookEvent evt)
	{
		MechanicsContext context = evt.Reason.Context;
		if (context == null || !context.IsShadow)
		{
			return false;
		}
		MechanicEntity target = ((evt is RulebookTargetEvent rulebookTargetEvent) ? rulebookTargetEvent.ConcreteTarget : evt.ConcreteInitiator);
		return !context.TryAffectByShadow(target, checkChance: true);
	}
}
