using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

public class ProjectileLauncher
{
	[NotNull]
	private readonly BlueprintProjectile m_Blueprint;

	[NotNull]
	private readonly TargetWrapper m_Launcher;

	[NotNull]
	private readonly TargetWrapper m_Target;

	[CanBeNull]
	private AbilityData m_Ability;

	[CanBeNull]
	private AttackResult? m_AttackResult;

	[CanBeNull]
	private Vector3? m_LaunchPosition;

	[CanBeNull]
	private RulebookEvent m_OnHitRule;

	[CanBeNull]
	private Action<Projectile> m_OnHitCallback;

	[CanBeNull]
	private int? m_MaxRangeCells;

	[CanBeNull]
	private StarshipHitLocation? m_StarshipHitLocation;

	[CanBeNull]
	private Vector3? m_MisdirectionOffset;

	private int m_Index;

	private bool m_IsCoverHit;

	private bool m_DoNotPlayHitEffect;

	private bool m_DoNotDeliverHit;

	[CanBeNull]
	private RulebookEventContext m_RulebookContext;

	public ProjectileLauncher([NotNull] BlueprintProjectile blueprint, [NotNull] TargetWrapper launcher, [NotNull] TargetWrapper target)
	{
		m_Blueprint = blueprint;
		m_Launcher = launcher;
		m_Target = target;
	}

	public ProjectileLauncher([NotNull] BlueprintProjectile blueprint, [NotNull] MechanicEntity launcher, [NotNull] TargetWrapper target)
		: this(blueprint, new TargetWrapper(launcher), target)
	{
	}

	public Projectile Launch()
	{
		Projectile projectile = new Projectile(m_Blueprint, m_Launcher, m_Target)
		{
			OnHitRule = m_OnHitRule,
			OnHitCallback = m_OnHitCallback,
			SavedContext = m_RulebookContext,
			Ability = m_Ability,
			MaxRangeMeters = (float)m_MaxRangeCells.GetValueOrDefault() * GraphParamsMechanicsCache.GridCellSize,
			Index = m_Index
		};
		projectile.AttackResult = m_AttackResult ?? projectile.AttackResult;
		projectile.IsCoverHit = m_IsCoverHit;
		projectile.StarshipHitLocation = m_StarshipHitLocation ?? projectile.StarshipHitLocation;
		projectile.DoNotPlayHitEffect = m_DoNotPlayHitEffect;
		projectile.DoNotDeliverHit = m_DoNotDeliverHit;
		if (m_MisdirectionOffset.HasValue)
		{
			projectile.SetMisdirectionOffset(m_MisdirectionOffset.Value);
		}
		Vector3? launchPosition = m_LaunchPosition ?? ((m_Launcher.Entity != null) ? null : new Vector3?(m_Launcher.Point));
		Game.Instance.ProjectileController.Launch(projectile, launchPosition);
		return projectile;
	}

	public ProjectileLauncher Ability([CanBeNull] AbilityData ability)
	{
		m_Ability = ability;
		return this;
	}

	public ProjectileLauncher AttackResult([CanBeNull] AttackResult? attackResult)
	{
		m_AttackResult = attackResult;
		return this;
	}

	public ProjectileLauncher LaunchPosition([CanBeNull] Vector3? launchPosition)
	{
		m_LaunchPosition = launchPosition;
		return this;
	}

	public ProjectileLauncher OnHitRule([CanBeNull] RulebookEvent onHitRule)
	{
		m_OnHitRule = onHitRule;
		if (m_RulebookContext == null)
		{
			m_RulebookContext = ((onHitRule != null) ? Rulebook.CurrentContext.Clone() : null);
		}
		return this;
	}

	public ProjectileLauncher OnHitCallback([CanBeNull] Action<Projectile> onHitCallback)
	{
		m_OnHitCallback = onHitCallback;
		if (m_RulebookContext == null)
		{
			m_RulebookContext = ((onHitCallback != null) ? Rulebook.CurrentContext.Clone() : null);
		}
		return this;
	}

	public ProjectileLauncher MaxRangeCells(int? maxRange)
	{
		m_MaxRangeCells = maxRange;
		return this;
	}

	public ProjectileLauncher Index(int index)
	{
		m_Index = index;
		return this;
	}

	public ProjectileLauncher StarshipHitLocation(StarshipHitLocation hitLocation)
	{
		m_StarshipHitLocation = hitLocation;
		return this;
	}

	public ProjectileLauncher MisdirectionOffset(Vector3 offset)
	{
		m_MisdirectionOffset = offset;
		return this;
	}

	public ProjectileLauncher IsCoverHit(bool isCoverHit)
	{
		m_IsCoverHit = isCoverHit;
		return this;
	}

	public ProjectileLauncher SetDoNotPlayHitEffect(bool doNotPlayHitEffect)
	{
		m_DoNotPlayHitEffect = doNotPlayHitEffect;
		return this;
	}

	public ProjectileLauncher SetDoNotDeliverHit(bool doNotDeliverHit)
	{
		m_DoNotDeliverHit = doNotDeliverHit;
		return this;
	}
}
