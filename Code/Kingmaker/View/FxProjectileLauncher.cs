using System;
using System.Collections;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.ManualCoroutines;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public class FxProjectileLauncher : MonoBehaviour
{
	[SerializeField]
	public BlueprintProjectileReference Blueprint;

	[SerializeField]
	private bool m_InvertUpDirectionForTrajectories;

	[SerializeField]
	private Transform m_Start;

	[SerializeField]
	[CanBeNull]
	private Transform m_End;

	[SerializeField]
	private float m_Delay;

	private TimeSpan m_StartTime;

	private CoroutineHandler m_Coroutine;

	[CanBeNull]
	public UnitEntityView HitTarget { get; set; }

	[CanBeNull]
	public Projectile Projectile { get; set; }

	private static TimeSpan CurrentTime => Game.Instance.TimeController.GameTime;

	private void OnEnable()
	{
		TimeSpan targetTime = CurrentTime + TimeSpan.FromSeconds(m_Delay);
		m_Coroutine = Game.Instance.CoroutinesController.Start(Launch(targetTime), this);
	}

	private IEnumerator Launch(TimeSpan targetTime)
	{
		while (CurrentTime < targetTime)
		{
			yield return null;
		}
		ProjectileTargetWrapper launcher = new ProjectileTargetWrapper(m_Start.transform);
		Transform obj = ObjectExtensions.Or(m_End, null);
		ProjectileTargetWrapper target = new ProjectileTargetWrapper((((object)obj != null) ? ObjectExtensions.Or(obj.transform, null) : null) ?? base.transform);
		Projectile projectile = new ProjectileLauncher(Blueprint, launcher, target).AttackResult(AttackResult.Hit).Launch();
		projectile.InvertUpDirectionForTrajectories = m_InvertUpDirectionForTrajectories;
		projectile.FollowLauncher = true;
		projectile.TargetUnitForHitSnapFx = ObjectExtensions.Or(HitTarget, null)?.EntityData;
		Projectile = projectile;
	}

	private void OnDisable()
	{
		Game.Instance.CoroutinesController.Stop(ref m_Coroutine);
		Projectile projectile = Projectile;
		if (projectile != null && !projectile.Destroyed && !projectile.Cleared)
		{
			Projectile.Cleared = true;
		}
	}

	private void OnDestroy()
	{
		Game.Instance.CoroutinesController.Stop(ref m_Coroutine);
		Projectile projectile = Projectile;
		if (projectile != null && !projectile.Destroyed && !projectile.Cleared)
		{
			Projectile.Cleared = true;
		}
	}
}
