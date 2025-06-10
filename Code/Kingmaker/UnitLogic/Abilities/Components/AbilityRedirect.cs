using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("4579faa6d7964ee9a26cbd576826cb0a")]
public sealed class AbilityRedirect : BlueprintComponent
{
	public TargetType RedirectTargetType = TargetType.Any;

	public bool IncludeClickedTarget;

	public bool CastOnSelfIfNoTargetsAvailable;

	public RestrictionCalculator CasterRestrictions = new RestrictionCalculator();

	public RestrictionCalculator ClickedTargetRestrictions = new RestrictionCalculator();

	public RestrictionCalculator RedirectTargetRestrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintProjectileReference m_Projectile;

	[SerializeField]
	private bool UseCustomPattern;

	[ShowIf("UseCustomPattern")]
	[SerializeField]
	private AoEPattern m_CustomPattern;

	[SerializeField]
	public StatType[] StatsToOverrideForCaster = new StatType[0];

	[CanBeNull]
	public BlueprintProjectile Projectile => m_Projectile;

	[CanBeNull]
	public AoEPattern CustomPattern
	{
		get
		{
			if (!UseCustomPattern)
			{
				return null;
			}
			return m_CustomPattern;
		}
	}

	public IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		Projectile projectile = null;
		if (Projectile != null)
		{
			projectile = new ProjectileLauncher(Projectile, context.ClickedTarget, target).Ability(context.Ability).Launch();
			float distance = projectile.Distance(context.ClickedTarget.Point, target.Point);
			while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
			{
				yield return null;
			}
		}
		yield return new AbilityDeliveryTarget(target)
		{
			Projectile = projectile
		};
	}
}
