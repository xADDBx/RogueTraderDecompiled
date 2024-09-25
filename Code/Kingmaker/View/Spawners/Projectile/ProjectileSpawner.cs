using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.View.Spawners.Projectile;

public class ProjectileSpawner : MonoBehaviour
{
	private struct TargetInfo
	{
		public BaseUnitEntity Unit;

		public float Range;

		public TargetInfo(BaseUnitEntity unit, float range)
		{
			Unit = unit;
			Range = range;
		}
	}

	public enum TimeModeType
	{
		GameTime,
		ResetOnTargetsLeft
	}

	[Header("Spawner configuration")]
	[Tooltip("Projectile to spawn.")]
	public BlueprintProjectileReference Projectile;

	[Tooltip("If set to true, projectiles will continue to spawn during cutscenes.")]
	public bool ActiveInCutscenes = true;

	[Tooltip("If set to true, projectiles will continue to spawn during dialogues.")]
	public bool ActiveInDialogues;

	[Tooltip("The number of projectiles spawned per second.")]
	public ParticleSystem.MinMaxCurve RateOverTime = new ParticleSystem.MinMaxCurve(0f, 5f);

	[Range(0.1f, 32f)]
	[Tooltip("How often rate over time will be re-evaluated during projectile spawn.")]
	public float RateOverTimeEvaluationPeriod = 0.1f;

	[Tooltip("If set to GameTime - RateOverTime will be evaluated based on current real time. If set to ResetOnTargetsLeft - RateOverTime will be evaluated based on time since target entered fire zone.")]
	public TimeModeType TimeMode;

	[Tooltip("If set to true, will multiply spawn rate by value from range [0,1] based on unit position inside of the area. Value reaches zero right near outer and inner radius borders and reaches 1 right at the middle between them.")]
	public bool AdjustByTargetPosition;

	[Tooltip("If set to true - Evaluated RateOverTime will be multiplied by amount of targets inside of the area.")]
	public bool AdjustByTargetsCount;

	[ShowIf("AdjustByTargetsCount")]
	[Tooltip("Maximum possible multiplier. Evaluated RateOverTime will not be multiplied by value greater than this one, no matter how many characters are in the area.")]
	[Range(1f, 32f)]
	public int TargetsCountMaxMultiplier = 6;

	[Header("Zone configuration")]
	[Min(0f)]
	[Tooltip("Inner radius value. Anything between 0 and InnerRadius is considered to be no-spawn zone.")]
	public float InnerRadius = 2f;

	[Min(0f)]
	[Tooltip("Outer radius value. Anything between OuterRadius and InnerRadius is considered to be spawn zone. Anything between OuterRadius and Infinity is considered to be no-spawn zone.")]
	public float OuterRadius = 10f;

	[Tooltip("Array of points from where projectiles will be spawned in random order.")]
	public List<GameObject> SpawnPoints = new List<GameObject>();

	[Header("Gizmos configuration")]
	public bool DrawGizmos = true;

	public Color SpawnPointGizmosColor = Color.red;

	public Color InnerRadiusGizmosColor = Color.blue;

	public Color OuterRadiusGizmosColor = Color.red;

	private bool m_Paused;

	private float? m_CurrentTargetsEnteredTime;

	private readonly List<TargetInfo> m_CurrentTargets = new List<TargetInfo>();

	private float m_PreviousRateUpdateTime;

	private readonly RateBasedTokenGenerator m_TokenGenerator = new RateBasedTokenGenerator();

	private Vector3 m_CachedPosition;

	public void Resume()
	{
		if (m_Paused)
		{
			m_Paused = false;
		}
	}

	public void Pause()
	{
		if (!m_Paused)
		{
			m_Paused = true;
		}
	}

	private void OnEnable()
	{
		m_CachedPosition = base.transform.position;
		Game.Instance.ProjectileSpawnerController.RegisterSpawner(this);
	}

	private void OnDisable()
	{
		Game.Instance.ProjectileSpawnerController.UnregisterSpawner(this);
	}

	private void OnValidate()
	{
		if (SpawnPoints.Empty())
		{
			SpawnPoints.Add(base.gameObject);
		}
	}

	public void Tick(List<BaseUnitEntity> possibleTargets, float currentTime, float deltaTime)
	{
		using (ProfileScope.New("ProjectileSpawner.Tick"))
		{
			if (m_Paused)
			{
				return;
			}
			PopulateCurrentTargets(possibleTargets, currentTime);
			if (m_CurrentTargets.Count != 0 && SpawnPoints.Count != 0)
			{
				m_TokenGenerator.Tick(deltaTime);
				int num = m_TokenGenerator.ConsumeTokens();
				for (int i = 0; i < num; i++)
				{
					Fire();
				}
				float num2 = m_PreviousRateUpdateTime + RateOverTimeEvaluationPeriod;
				if (currentTime > num2)
				{
					float ratePerSec = CalculateSpawnRate(currentTime);
					m_TokenGenerator.ChangeRate(ratePerSec);
					m_PreviousRateUpdateTime = currentTime;
				}
			}
		}
	}

	private void PopulateCurrentTargets(List<BaseUnitEntity> possibleTargets, float gameTime)
	{
		m_CurrentTargets.Clear();
		float num = InnerRadius * InnerRadius;
		float num2 = OuterRadius * OuterRadius;
		foreach (BaseUnitEntity possibleTarget in possibleTargets)
		{
			Vector3 vector = possibleTarget.Position - m_CachedPosition;
			vector.y = 0f;
			float sqrMagnitude = vector.sqrMagnitude;
			if (num <= sqrMagnitude && sqrMagnitude <= num2)
			{
				m_CurrentTargets.Add(new TargetInfo(possibleTarget, Mathf.Sqrt(sqrMagnitude)));
			}
		}
		if (m_CurrentTargetsEnteredTime.HasValue && m_CurrentTargets.Count == 0)
		{
			m_CurrentTargetsEnteredTime = null;
		}
		if (!m_CurrentTargetsEnteredTime.HasValue && m_CurrentTargets.Count > 0)
		{
			m_CurrentTargetsEnteredTime = gameTime;
		}
	}

	private float CalculateSpawnRate(float time)
	{
		float evaluationTime = GetEvaluationTime(time);
		float num = Mathf.Max(0f, RateOverTime.Evaluate(evaluationTime, PFStatefulRandom.View.value));
		if (AdjustByTargetPosition)
		{
			num *= CalculateTargetsPositionsAdjustmentModifier();
		}
		if (AdjustByTargetsCount)
		{
			num *= (float)Mathf.Min(m_CurrentTargets.Count, TargetsCountMaxMultiplier);
		}
		return num;
	}

	private float GetEvaluationTime(float time)
	{
		float num = time;
		if (TimeMode == TimeModeType.ResetOnTargetsLeft)
		{
			num -= m_CurrentTargetsEnteredTime.GetValueOrDefault();
		}
		return num;
	}

	private float CalculateTargetsPositionsAdjustmentModifier()
	{
		float num = (OuterRadius - InnerRadius) / 2f;
		float num2 = InnerRadius + num;
		float num3 = 0f;
		for (int i = 0; i < m_CurrentTargets.Count; i++)
		{
			float num4 = Math.Abs(m_CurrentTargets[i].Range - num2);
			float num5 = Mathf.Lerp(1f, 0f, num4 / num);
			num3 += num5;
		}
		return num3 / (float)m_CurrentTargets.Count;
	}

	private void Fire()
	{
		TargetInfo targetInfo = m_CurrentTargets.Random(PFStatefulRandom.View);
		GameObject gameObject = SpawnPoints.Random(PFStatefulRandom.View);
		new ProjectileLauncher(Projectile, gameObject.transform.position, targetInfo.Unit).AttackResult(AttackResult.Miss).Launch();
	}

	private void OnDrawGizmos()
	{
		if (!DrawGizmos)
		{
			return;
		}
		Gizmos.color = InnerRadiusGizmosColor;
		DebugDraw.DrawCircle(base.transform.position, Vector3.up, InnerRadius);
		Gizmos.color = OuterRadiusGizmosColor;
		DebugDraw.DrawCircle(base.transform.position, Vector3.up, OuterRadius);
		Gizmos.color = SpawnPointGizmosColor;
		foreach (GameObject spawnPoint in SpawnPoints)
		{
			if ((bool)spawnPoint)
			{
				Gizmos.DrawWireCube(spawnPoint.transform.position, new Vector3(1f, 1f, 1f));
			}
		}
	}
}
