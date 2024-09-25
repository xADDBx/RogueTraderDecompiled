using System;
using CatmullRomSplines;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual;

public class CameraTweenAnchor : MonoBehaviour
{
	[Serializable]
	public class TweenData
	{
		public float RubberBandPosition = 3f;

		public float RubberBandRotation = 3f;

		[ConditionalShow("ShowAnimtime")]
		public float AnimationTime = 2f;

		public bool UseEasing;

		[ConditionalShow("ShowAnimtime")]
		public bool SeparateCurves;

		[ConditionalHide("UseEasing")]
		public VectorSpline PositionTrack;

		[ConditionalShow("ShowAnimtime")]
		public AnimationCurve PositionCurve;

		[ConditionalHide("UseEasing")]
		public VectorSpline LootAtTrack;

		[ConditionalShow("ShowLookCurve")]
		public AnimationCurve LookAtCurve;

		private bool ShowAnimtime
		{
			get
			{
				if (!UseEasing && !PositionTrack)
				{
					return LootAtTrack;
				}
				return true;
			}
		}

		private bool ShowLookCurve
		{
			get
			{
				if (ShowAnimtime)
				{
					return SeparateCurves;
				}
				return false;
			}
		}
	}

	[Serializable]
	[IKnowWhatImDoing]
	public class TweenDataWithPrev : TweenData
	{
		public CameraTweenAnchor PreviousAnchor;
	}

	[SerializeField]
	private TweenData m_DefaultTween;

	[SerializeField]
	private TweenDataWithPrev[] m_Tweens;

	private TweenData m_Tween;

	private float m_Time;

	private Vector3 m_PrevPos;

	private Quaternion m_PrevRot;

	public void TickTween(Transform cameraTransform, float dt)
	{
		if (m_Tween != null)
		{
			Vector3 b = base.transform.position;
			if (m_Tween.UseEasing)
			{
				b = Vector3.LerpUnclamped(m_PrevPos, base.transform.position, m_Tween.PositionCurve.Evaluate(m_Time / m_Tween.AnimationTime));
			}
			else if ((bool)m_Tween.PositionTrack && m_Tween.AnimationTime > 0f)
			{
				b = m_Tween.PositionTrack.EvaluatePosition(m_Tween.PositionCurve.Evaluate(m_Time / m_Tween.AnimationTime));
			}
			cameraTransform.position = Vector3.Lerp(cameraTransform.position, b, dt * m_Tween.RubberBandPosition);
			Quaternion b2 = base.transform.rotation;
			AnimationCurve animationCurve = (m_Tween.SeparateCurves ? m_Tween.LookAtCurve : m_Tween.PositionCurve);
			if (m_Tween.UseEasing)
			{
				b2 = Quaternion.Slerp(m_PrevRot, base.transform.rotation, animationCurve.Evaluate(m_Time / m_Tween.AnimationTime));
			}
			else if ((bool)m_Tween.LootAtTrack && m_Tween.AnimationTime > 0f)
			{
				b2 = Quaternion.LookRotation(m_Tween.LootAtTrack.EvaluatePosition(animationCurve.Evaluate(m_Time / m_Tween.AnimationTime)) - cameraTransform.position);
			}
			cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, b2, dt * m_Tween.RubberBandRotation);
			m_Time += dt;
		}
	}

	public void StartTween(Transform cameraTransform, CameraTweenAnchor prevAnchor)
	{
		m_PrevPos = cameraTransform.position;
		m_PrevRot = cameraTransform.rotation;
		m_Tween = m_Tweens.FirstOrDefault((TweenDataWithPrev t) => t.PreviousAnchor == prevAnchor) ?? m_DefaultTween;
		m_Time = 0f;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, 0.04f);
	}
}
