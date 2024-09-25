using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.LineRenderer;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.RayView;

[RequireComponent(typeof(CompositeLineRenderer))]
public class CompositeRayView : MonoBehaviour
{
	[Serializable]
	public class Emitter
	{
		public Transform StartPoint;

		public Transform EndPoint;

		public Space OffsetSpace;

		public float2 UvOffset;

		public float OffsetCurveBias;

		public float WidthScale = 1f;

		public float FadeWidth = 1f;

		public float FadeAlphaSpeed = 1f;

		public float FadeUvSpeed = 1f;

		[NonSerialized]
		[HideInInspector]
		public int PositonsCount;
	}

	private const float kDensityCoeff = 1.25f;

	private float m_OnLastHitDelayTime = -1f;

	private float m_OnLastFadeOutDelayTime = -1f;

	private float m_OnAnimationFinishedDelayTime = -1f;

	private int m_OnHitCount;

	private int m_OnFadeOutCount;

	private bool m_IsAnimationPlaying;

	private bool m_IsDirty;

	private CompositeLineRenderer m_LineRenderer;

	private RayViewUpdateJob m_UpdateJob;

	private List<LineDescriptor> m_LineDescriptors = new List<LineDescriptor>();

	private Animation m_AnimationComponent;

	private NativeArray<RayDescriptor> m_RayDescriptors;

	private JobAnimationCurve m_PlaneOffsetMainCurve;

	private JobCompositeAnimationCurve m_PlaneOffsetAdditionalCurve;

	private JobAnimationCurve m_UpOffsetMainCurve;

	private JobCompositeAnimationCurve m_UpOffsetAdditionalCurve;

	public float VertexDistance = 0.5f;

	public float DelayOnLastHit;

	public RayAction OnLastHit;

	public float DelayOnLastFadeOut;

	public RayAction OnLastFadeOut;

	public float DelayOnAnimationFinished;

	public RayAction OnAnimationFinished;

	public AnimationClip Animation;

	[Header("----ГОРИЗОНТАЛЬНАЯ КУРВА----")]
	public AnimationCurve PlaneOffsetMain = AnimationCurve.Linear(0f, 0f, 1f, 0f);

	public CompositeAnimationCurve PlaneOffsetAdditional;

	[Header("----ВЕРТИКАЛЬНАЯ КУРВА----")]
	public AnimationCurve UpOffsetMain = AnimationCurve.Linear(0f, 0f, 1f, 0f);

	public CompositeAnimationCurve UpOffsetAdditional;

	[SerializeField]
	[Header("----РОЖДАТЕЛИ----")]
	private List<Emitter> m_Emitters = new List<Emitter>();

	private float m_Time;

	public List<Emitter> Emitters
	{
		get
		{
			return m_Emitters;
		}
		set
		{
			m_Emitters = value;
		}
	}

	private void OnEnable()
	{
		m_LineRenderer = GetComponent<CompositeLineRenderer>();
		CompositeLineRenderer lineRenderer = m_LineRenderer;
		lineRenderer.OnUpdateJobStart = (Func<JobHandle>)Delegate.Combine(lineRenderer.OnUpdateJobStart, new Func<JobHandle>(OnUpdateJobStart));
		m_AnimationComponent = GetComponent<Animation>();
		m_UpdateJob = default(RayViewUpdateJob);
		m_IsDirty = true;
		m_Time = 0f;
		UpdateNativeData();
	}

	private void UpdateNativeData()
	{
		if (m_IsDirty)
		{
			m_PlaneOffsetMainCurve.Dispose();
			m_PlaneOffsetMainCurve = new JobAnimationCurve(PlaneOffsetMain);
			m_PlaneOffsetAdditionalCurve.Dispose();
			m_PlaneOffsetAdditionalCurve = new JobCompositeAnimationCurve(PlaneOffsetAdditional);
			m_UpOffsetMainCurve.Dispose();
			m_UpOffsetMainCurve = new JobAnimationCurve(UpOffsetMain);
			m_UpOffsetAdditionalCurve.Dispose();
			m_UpOffsetAdditionalCurve = new JobCompositeAnimationCurve(UpOffsetAdditional);
			NativeArrayUtils.IncreaseSize(ref m_RayDescriptors, m_Emitters.Count, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < m_Emitters.Count; i++)
			{
				Emitter emitter = m_Emitters[i];
				emitter.PositonsCount = 0;
				m_RayDescriptors[i] = new RayDescriptor
				{
					Start = emitter.StartPoint.position,
					StartRotation = emitter.StartPoint.rotation,
					End = emitter.EndPoint.position,
					OffsetSpace = emitter.OffsetSpace,
					UvOffset = emitter.UvOffset,
					WidthScale = emitter.WidthScale,
					FadeWidth = emitter.FadeWidth,
					FadeAlphaSpeed = emitter.FadeAlphaSpeed,
					FadeUvSpeed = emitter.FadeUvSpeed,
					FadeAlphaDistance = 0f - emitter.FadeWidth,
					OffsetCurveBias = emitter.OffsetCurveBias,
					State = RayState.FadeIn
				};
			}
			m_IsDirty = false;
		}
	}

	private void OnDisable()
	{
		CompositeLineRenderer lineRenderer = m_LineRenderer;
		lineRenderer.OnUpdateJobStart = (Func<JobHandle>)Delegate.Remove(lineRenderer.OnUpdateJobStart, new Func<JobHandle>(OnUpdateJobStart));
		if (m_RayDescriptors.IsCreated)
		{
			m_RayDescriptors.Dispose();
		}
		m_PlaneOffsetMainCurve.Dispose();
		m_PlaneOffsetAdditionalCurve.Dispose();
		m_UpOffsetMainCurve.Dispose();
		m_UpOffsetAdditionalCurve.Dispose();
	}

	private JobHandle OnUpdateJobStart()
	{
		if (m_IsDirty)
		{
			UpdateNativeData();
		}
		int num = m_LineRenderer.MaxPositionsPerLine;
		bool flag = false;
		m_LineDescriptors.Clear();
		int num2 = 0;
		m_OnFadeOutCount = 0;
		for (int i = 0; i < m_Emitters.Count; i++)
		{
			Emitter emitter = m_Emitters[i];
			int num3 = 0;
			if (emitter.StartPoint != null && emitter.EndPoint != null)
			{
				num3 = (int)math.round(math.distance(emitter.StartPoint.position, emitter.EndPoint.position) / VertexDistance);
			}
			if (emitter.PositonsCount != num3)
			{
				flag = true;
				emitter.PositonsCount = num3;
			}
			num = math.max(num, num3);
			m_LineDescriptors.Add(new LineDescriptor
			{
				PositionCount = num3,
				PositionsOffset = num2
			});
			RayDescriptor value = m_RayDescriptors[i];
			value.Start = emitter.StartPoint.position;
			value.End = emitter.EndPoint.position;
			value.StartRotation = emitter.StartPoint.rotation;
			value.State = UpdateRayState(value.State);
			m_RayDescriptors[i] = value;
			num2 += num3;
		}
		if (num > m_LineRenderer.MaxPositionsPerLine)
		{
			m_LineRenderer.MaxPositionsPerLine = (int)((float)num * 1.25f);
		}
		if (flag)
		{
			m_LineRenderer.SetLineDescriptors(m_LineDescriptors);
		}
		if (m_OnHitCount == m_Emitters.Count && m_OnLastHitDelayTime < 0f)
		{
			m_OnHitCount = 0;
			if (DelayOnLastHit > 0f)
			{
				m_OnLastHitDelayTime = 0f;
			}
			else
			{
				ExecuteAction(OnLastHit);
			}
		}
		if (m_OnLastHitDelayTime >= 0f)
		{
			m_OnLastHitDelayTime += Time.deltaTime;
			if (m_OnLastHitDelayTime >= DelayOnLastHit)
			{
				m_OnLastHitDelayTime = -1f;
				ExecuteAction(OnLastHit);
			}
		}
		if (m_OnFadeOutCount == m_Emitters.Count && m_OnLastFadeOutDelayTime < 0f)
		{
			m_OnFadeOutCount = 0;
			if (DelayOnLastFadeOut > 0f)
			{
				m_OnLastFadeOutDelayTime = 0f;
			}
			else
			{
				ExecuteAction(OnLastFadeOut);
			}
		}
		if (m_OnLastFadeOutDelayTime >= 0f)
		{
			m_OnLastFadeOutDelayTime += Time.deltaTime;
			if (m_OnLastFadeOutDelayTime >= DelayOnLastFadeOut)
			{
				m_OnLastFadeOutDelayTime = -1f;
				ExecuteAction(OnLastFadeOut);
			}
		}
		if (m_IsAnimationPlaying && !m_AnimationComponent.isPlaying && m_OnAnimationFinishedDelayTime < 0f)
		{
			m_IsAnimationPlaying = false;
			if (DelayOnAnimationFinished > 0f)
			{
				m_OnAnimationFinishedDelayTime = 0f;
			}
			else
			{
				ExecuteAction(OnAnimationFinished);
			}
		}
		if (m_OnAnimationFinishedDelayTime >= 0f)
		{
			m_OnAnimationFinishedDelayTime += Time.deltaTime;
			if (m_OnAnimationFinishedDelayTime >= DelayOnAnimationFinished)
			{
				m_OnAnimationFinishedDelayTime = -1f;
				ExecuteAction(OnAnimationFinished);
			}
		}
		m_Time += Time.deltaTime;
		m_UpdateJob.RayDescriptors = m_RayDescriptors;
		m_UpdateJob.PlaneOffsetMainCurve = m_PlaneOffsetMainCurve;
		m_UpdateJob.PlaneOffsetAdditionalCurve = m_PlaneOffsetAdditionalCurve;
		m_UpdateJob.UpOffsetMainCurve = m_UpOffsetMainCurve;
		m_UpdateJob.UpOffsetAdditionalCurve = m_UpOffsetAdditionalCurve;
		m_UpdateJob.GlobalTime = m_Time;
		m_UpdateJob.DeltaTime = Time.deltaTime;
		return m_LineRenderer.ScheduleUpdateJob(ref m_UpdateJob, 8);
	}

	private void ExecuteAction(RayAction action)
	{
		if (action.HasFlag(RayAction.StartFadeIn))
		{
			StartFadeIn();
		}
		if (action.HasFlag(RayAction.StartFadeOut))
		{
			StartFadeOut();
		}
		if (action.HasFlag(RayAction.PlayAnim))
		{
			StartAnimation();
		}
		if (action.HasFlag(RayAction.Disable))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void StartFadeIn()
	{
		for (int i = 0; i < m_Emitters.Count; i++)
		{
			RayDescriptor value = m_RayDescriptors[i];
			value.SetState(RayState.FadeIn);
			m_RayDescriptors[i] = value;
		}
	}

	private void StartFadeOut()
	{
		for (int i = 0; i < m_Emitters.Count; i++)
		{
			RayDescriptor value = m_RayDescriptors[i];
			value.SetState(RayState.FadeOut);
			m_RayDescriptors[i] = value;
		}
	}

	private void StartAnimation()
	{
		if (m_AnimationComponent != null && Animation != null)
		{
			m_AnimationComponent.clip = Animation;
			m_AnimationComponent.Play();
			m_IsAnimationPlaying = true;
		}
		else
		{
			m_IsAnimationPlaying = false;
		}
	}

	private RayState UpdateRayState(RayState state)
	{
		RayState result = state;
		switch (state)
		{
		case RayState.AfterHit:
			m_OnHitCount++;
			result = RayState.Normal;
			break;
		case RayState.Finished:
			m_OnFadeOutCount++;
			break;
		}
		return result;
	}

	[Conditional("UNITY_EDITOR")]
	public void SetDirtyFromEditor()
	{
		m_IsDirty = true;
	}
}
