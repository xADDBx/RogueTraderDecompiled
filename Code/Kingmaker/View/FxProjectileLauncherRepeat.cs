using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public class FxProjectileLauncherRepeat : MonoBehaviour
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
	private bool m_Repeat;

	[SerializeField]
	[ShowIf("m_Repeat")]
	private bool m_RandomRepeatDelay;

	[SerializeField]
	[HideIf("m_RandomRepeatDelay")]
	private float m_Delay;

	[SerializeField]
	[ShowIf("RandomRepeatDelayVisible")]
	private float m_MinSecondsDelay;

	[SerializeField]
	[ShowIf("RandomRepeatDelayVisible")]
	private float m_MaxSecondsDelay;

	private List<Projectile> Projectiles = new List<Projectile>();

	private CoroutineHandler m_Coroutine;

	private bool RandomRepeatDelayVisible
	{
		get
		{
			if (m_Repeat)
			{
				return m_RandomRepeatDelay;
			}
			return false;
		}
	}

	[CanBeNull]
	public UnitEntityView HitTarget { get; set; }

	private static TimeSpan CurrentTime => Game.Instance.TimeController.GameTime;

	private void OnEnable()
	{
		m_Coroutine = Game.Instance.CoroutinesController.Start(LaunchProjectileCoroutine());
	}

	private IEnumerator LaunchProjectileCoroutine()
	{
		do
		{
			TimeSpan targetTime = CurrentTime + TimeSpan.FromSeconds(GetDelay());
			while (CurrentTime < targetTime)
			{
				yield return null;
			}
			CreateProjectile();
			CheckFinishedProjectiles();
		}
		while (m_Repeat);
	}

	private float GetDelay()
	{
		if (!m_RandomRepeatDelay)
		{
			return m_Delay;
		}
		return PFStatefulRandom.View.Range(m_MinSecondsDelay, m_MaxSecondsDelay);
	}

	private void CheckFinishedProjectiles()
	{
		for (int num = Projectiles.Count - 1; num >= 0; num--)
		{
			Projectile projectile = Projectiles[num];
			if (projectile != null && projectile.Destroyed && !projectile.Cleared)
			{
				Projectiles[num].Cleared = true;
			}
			if (Projectiles[num].Cleared)
			{
				Projectiles.Remove(Projectiles[num]);
			}
		}
	}

	private void CreateProjectile()
	{
		ProjectileTargetWrapper launcher = new ProjectileTargetWrapper(m_Start.transform);
		Transform obj = ObjectExtensions.Or(m_End, null);
		ProjectileTargetWrapper target = new ProjectileTargetWrapper((((object)obj != null) ? ObjectExtensions.Or(obj.transform, null) : null) ?? base.transform);
		Projectile projectile = new ProjectileLauncher(Blueprint, launcher, target).AttackResult(AttackResult.Hit).Launch();
		projectile.InvertUpDirectionForTrajectories = m_InvertUpDirectionForTrajectories;
		projectile.FollowLauncher = true;
		projectile.TargetUnitForHitSnapFx = ObjectExtensions.Or(HitTarget, null)?.EntityData;
		Projectiles.Add(projectile);
	}

	private void OnDisable()
	{
		Game.Instance.CoroutinesController.Stop(ref m_Coroutine);
		CheckFinishedProjectiles();
		Projectiles.Clear();
	}

	private void OnDestroy()
	{
		Game.Instance.CoroutinesController.Stop(ref m_Coroutine);
		CheckFinishedProjectiles();
		Projectiles.Clear();
	}
}
