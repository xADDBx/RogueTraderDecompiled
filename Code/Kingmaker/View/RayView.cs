using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.View;

[RequireComponent(typeof(LineRenderer))]
public class RayView : MonoBehaviour, IDeactivatableComponent
{
	[SerializeField]
	private BlueprintProjectileTrajectoryReference m_Trajectory;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float m_DistanceBetweenVertices = 1f;

	[SerializeField]
	private bool m_SnapToGround;

	[SerializeField]
	private float m_Delay;

	[SerializeField]
	private float m_DelayAfterHit;

	[SerializeField]
	private float m_FadeInSpeed = 5f;

	[SerializeField]
	private float m_FadeInDistance = 1f;

	[SerializeField]
	private bool m_FadeOutEnabled;

	[SerializeField]
	[ConditionalShow("m_FadeOutEnabled")]
	private float m_FadeOutSpeed = 5f;

	[SerializeField]
	[ConditionalShow("m_FadeOutEnabled")]
	private float m_FadeOutDistance = 1f;

	[SerializeField]
	private AnimationClip m_FinishingAnimation;

	[SerializeField]
	private float m_UV0XSpeed;

	[SerializeField]
	private bool m_ApplyFadeInSpeeedToUV0X;

	[SerializeField]
	private float m_UV0XAlphaSpeedScale;

	private LineRenderer m_LineRenderer;

	private TimeSpan m_StartTime;

	private TimeSpan m_PrevTickTime;

	private const int MaxVerticesCount = 255;

	private readonly Vector3[] m_Vertices = new Vector3[255];

	private int m_VerticesCount;

	private float m_FadeInShift;

	private float m_FadeOutShift;

	private Gradient m_ColorGradientOrigin;

	private bool m_Finishing;

	private float m_UV0XStopTime;

	private float m_RandomShiftOffset;

	private UnityEngine.Animation m_AnimationComponent;

	private Projectile m_Projectile;

	private static readonly List<GradientAlphaKey> AlphaKeys = new List<GradientAlphaKey>();

	private TimeSpan LifeTime => Game.Instance.TimeController.GameTime - m_StartTime;

	private Vector3 TargetPoint => m_Projectile.GetTargetPoint();

	private Vector3 SourcePoint => m_Projectile.LaunchPosition;

	private float FullDistance => (TargetPoint - SourcePoint).magnitude;

	private bool IsFadeInFlow => m_FadeInShift - m_FadeInDistance < FullDistance;

	private bool IsFadeOutEnded => m_FadeOutShift - m_FadeOutDistance >= FullDistance;

	private bool IsFadeOutFlow
	{
		get
		{
			if (m_FadeOutEnabled && m_Projectile.IsHit && (double)m_DelayAfterHit <= LifeTime.TotalSeconds - (double)m_Projectile.PassedTime)
			{
				return !IsFadeOutEnded;
			}
			return false;
		}
	}

	private void Awake()
	{
		m_LineRenderer = GetComponent<LineRenderer>();
		m_ColorGradientOrigin = new Gradient
		{
			mode = m_LineRenderer.colorGradient.mode
		};
		m_ColorGradientOrigin.SetKeys(m_LineRenderer.colorGradient.colorKeys.ToArray(), m_LineRenderer.colorGradient.alphaKeys.ToArray());
		m_AnimationComponent = GetComponent<UnityEngine.Animation>();
	}

	public void Initialize(Projectile projectile)
	{
		m_Projectile = projectile;
		m_PrevTickTime = TimeSpan.Zero;
		m_FadeInShift = 0f;
		m_FadeOutShift = 0f;
		m_Finishing = false;
		m_UV0XStopTime = 0f;
		m_RandomShiftOffset = 0f;
		m_StartTime = Game.Instance.TimeController.GameTime;
		if (m_Trajectory.Get() == null)
		{
			PFLog.Default.Error("No trajectory set on ray {0} in {1}", base.name, projectile.Blueprint.name);
			base.enabled = false;
			return;
		}
		BlueprintProjectileTrajectory blueprintProjectileTrajectory = m_Trajectory.Get();
		if (!blueprintProjectileTrajectory.PlaneOffset.Empty())
		{
			TrajectoryOffset[] planeOffset = blueprintProjectileTrajectory.PlaneOffset;
			foreach (TrajectoryOffset trajectoryOffset in planeOffset)
			{
				trajectoryOffset.OnInitializeStaticOffset = PFStatefulRandom.Visuals.Fx.Range(0f - trajectoryOffset.RandomOffset, trajectoryOffset.RandomOffset);
			}
		}
		if (!blueprintProjectileTrajectory.UpOffset.Empty())
		{
			TrajectoryOffset[] planeOffset = blueprintProjectileTrajectory.UpOffset;
			foreach (TrajectoryOffset trajectoryOffset2 in planeOffset)
			{
				trajectoryOffset2.OnInitializeStaticOffset = PFStatefulRandom.Visuals.Fx.Range(0f - trajectoryOffset2.RandomOffset, trajectoryOffset2.RandomOffset);
			}
		}
	}

	public void Update()
	{
		if (m_Projectile == null)
		{
			PFLog.Default.Warning("RayView has no reference to projectile");
		}
		else if (!(m_PrevTickTime == Game.Instance.TimeController.GameTime))
		{
			m_PrevTickTime = Game.Instance.TimeController.GameTime;
			if (!((double)m_Delay >= LifeTime.TotalSeconds))
			{
				Tick();
			}
		}
	}

	private void Tick()
	{
		if ((double)m_DelayAfterHit <= LifeTime.TotalSeconds - (double)m_Projectile.PassedTime && (m_Finishing || m_Projectile.IsHit))
		{
			if ((!m_FadeOutEnabled || IsFadeOutEnded) && (!m_FinishingAnimation || !m_AnimationComponent || (m_Finishing && !m_AnimationComponent.isPlaying)))
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			if (!m_Finishing && (bool)m_AnimationComponent)
			{
				m_AnimationComponent.clip = m_FinishingAnimation;
				m_AnimationComponent.Play();
				m_Finishing = true;
			}
		}
		Vector3 sourcePoint = SourcePoint;
		Vector3 targetPoint = TargetPoint;
		float fullDistance = FullDistance;
		Vector3 direction = targetPoint - sourcePoint;
		UpdateVertices(sourcePoint, fullDistance, direction);
		UpdateAlpha(fullDistance);
	}

	private static float? DistanceToGround(Vector3 source, out Vector3 normal)
	{
		if (Physics.Raycast(new Ray(source + new Vector3(0f, 10f, 0f), Vector3.down), out var hitInfo, 20f, 2359553))
		{
			normal = hitInfo.normal;
			return hitInfo.distance - 10f;
		}
		normal = Vector3.zero;
		return null;
	}

	private void UpdateVertices(Vector3 source, float fullDistance, Vector3 direction)
	{
		m_VerticesCount = Mathf.Min(255, Mathf.CeilToInt(fullDistance / m_DistanceBetweenVertices) + 1);
		float num = fullDistance / (float)(m_VerticesCount - 1);
		float num2 = 0f;
		if (m_SnapToGround)
		{
			num2 = Math.Min(1f, DistanceToGround(source, out var _) ?? 1f);
		}
		float num3 = 0f;
		float progress = 0f;
		for (int i = 0; i < m_VerticesCount; i++)
		{
			Vector3 vector = TrajectoryCalculator.CalculateShift(m_Trajectory.Get(), direction, fullDistance, num3, progress, (float)LifeTime.TotalSeconds, m_Projectile.InvertUpDirectionForTrajectories);
			Vector3 vector2 = source + direction.normalized * num3;
			if (m_SnapToGround && i > 0)
			{
				Vector3 normal2;
				float? num4 = DistanceToGround(vector2, out normal2);
				if (num4.HasValue)
				{
					vector2.y -= num4.Value;
					vector2 += normal2 * num2;
				}
				else if (i > 0)
				{
					vector2.y = m_Vertices[i - 1].y;
				}
			}
			m_Vertices[i] = vector2 + vector;
			num3 = Math.Min(fullDistance, num3 + num);
			progress = num3 / fullDistance;
		}
		m_LineRenderer.positionCount = m_VerticesCount;
		m_LineRenderer.SetPositions(m_Vertices);
	}

	private void UpdateAlpha(float fullDistance)
	{
		float num = 0f - m_UV0XSpeed;
		float num2 = 0f - (m_ApplyFadeInSpeeedToUV0X ? (m_FadeInSpeed * m_UV0XAlphaSpeedScale) : 0f);
		if (IsFadeInFlow)
		{
			m_FadeInShift += m_FadeInSpeed * Time.deltaTime;
			UpdateFadeIn(fullDistance);
			m_LineRenderer.material.SetTextureOffset(ShaderProps._BaseMap, new Vector2((num2 + num) * Time.time, 0f));
			m_UV0XStopTime = Time.time;
		}
		else
		{
			m_LineRenderer.material.SetTextureOffset(ShaderProps._BaseMap, new Vector2(num2 * m_UV0XStopTime + num * Time.time, 0f));
		}
		if (IsFadeOutFlow)
		{
			m_FadeOutShift += m_FadeOutSpeed * Time.deltaTime;
			UpdateFadeOut(fullDistance);
		}
	}

	private void UpdateFadeIn(float fullDistance)
	{
		float num = Math.Max(0.1f, m_FadeInDistance / fullDistance);
		float num2 = Math.Min(1f + num, m_FadeInShift / fullDistance);
		m_LineRenderer.colorGradient = UpdateFade(m_ColorGradientOrigin, num2 - num, num2, revert: false);
	}

	private void UpdateFadeOut(float fullDistance)
	{
		float num = Math.Max(0.1f, m_FadeOutDistance / fullDistance);
		float num2 = Math.Min(1f + num, m_FadeOutShift / fullDistance);
		m_LineRenderer.colorGradient = UpdateFade(m_ColorGradientOrigin, num2 - num, num2, revert: true);
	}

	private static void ReverseGradientAlphaKeys(GradientAlphaKey[] keys)
	{
		int num = keys.Length;
		for (int i = 0; i < num / 2; i++)
		{
			GradientAlphaKey gradientAlphaKey = keys[num - 1 - i];
			keys[num - 1 - i] = keys[i];
			keys[i] = gradientAlphaKey;
		}
		for (int j = 0; j < num; j++)
		{
			GradientAlphaKey gradientAlphaKey2 = keys[j];
			keys[j] = new GradientAlphaKey(gradientAlphaKey2.alpha, 1f - gradientAlphaKey2.time);
		}
	}

	private static Gradient UpdateFade(Gradient source, float al, float ar, bool revert)
	{
		GradientAlphaKey[] alphaKeys = source.alphaKeys;
		AlphaKeys.Clear();
		List<GradientAlphaKey> alphaKeys2 = AlphaKeys;
		if (revert)
		{
			ReverseGradientAlphaKeys(alphaKeys);
			float num = ar;
			ar = 1f - al;
			al = 1f - num;
		}
		if (alphaKeys.Length < 1)
		{
			alphaKeys2.Add(new GradientAlphaKey(1f, al));
			alphaKeys2.Add(new GradientAlphaKey(0f, ar));
		}
		else
		{
			int num2 = 0;
			int num3 = 1;
			while (num2 < alphaKeys.Length)
			{
				GradientAlphaKey item = alphaKeys[num2];
				GradientAlphaKey gradientAlphaKey = ((num3 < alphaKeys.Length) ? alphaKeys[num3] : new GradientAlphaKey(item.alpha, 1f));
				if (ar <= item.time)
				{
					if (al >= 0f)
					{
						alphaKeys2.Add(new GradientAlphaKey(item.alpha, al));
					}
					else
					{
						float alpha = Mathf.MoveTowards(0f, 1f, ar / (ar - al)) * item.alpha;
						alphaKeys2.Add(new GradientAlphaKey(alpha, 0f));
					}
					alphaKeys2.Add(new GradientAlphaKey(0f, ar));
					break;
				}
				if (al <= item.time)
				{
					if (num2 == 0 && al < item.time)
					{
						if (al >= 0f)
						{
							alphaKeys2.Add(new GradientAlphaKey(item.alpha, al));
						}
						else
						{
							float alpha2 = Mathf.MoveTowards(0f, 1f, ar / (ar - al)) * item.alpha;
							alphaKeys2.Add(new GradientAlphaKey(alpha2, 0f));
						}
					}
					float alpha3 = Mathf.MoveTowards(0f, 1f, (ar - item.time) / (ar - al)) * item.alpha;
					alphaKeys2.Add(new GradientAlphaKey(alpha3, item.time));
				}
				else
				{
					alphaKeys2.Add(item);
				}
				if (al > item.time && al < gradientAlphaKey.time)
				{
					float alpha4 = Mathf.MoveTowards(gradientAlphaKey.alpha, item.alpha, (gradientAlphaKey.time - al) / (gradientAlphaKey.time - item.time));
					alphaKeys2.Add(new GradientAlphaKey(alpha4, al));
				}
				if (ar <= gradientAlphaKey.time)
				{
					alphaKeys2.Add(new GradientAlphaKey(0f, ar));
					break;
				}
				if (num3 == alphaKeys.Length)
				{
					float alpha5 = Mathf.MoveTowards(0f, 1f, (ar - gradientAlphaKey.time) / (ar - al)) * gradientAlphaKey.alpha;
					alphaKeys2.Add(new GradientAlphaKey(alpha5, gradientAlphaKey.time));
					break;
				}
				num2++;
				num3++;
			}
		}
		GradientAlphaKey[] array = alphaKeys2.ToArray();
		if (revert)
		{
			ReverseGradientAlphaKeys(array);
		}
		Gradient gradient = new Gradient();
		gradient.mode = source.mode;
		gradient.SetKeys(source.colorKeys, array);
		return gradient;
	}
}
