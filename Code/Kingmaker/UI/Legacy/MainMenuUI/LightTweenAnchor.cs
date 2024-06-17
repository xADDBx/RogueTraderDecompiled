using System;
using CatmullRomSplines;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class LightTweenAnchor : MonoBehaviour
{
	[Serializable]
	public class TweenLightData
	{
		public float RubberBandIntensity = 3f;

		public float RubberBandRange = 3f;

		[ConditionalShow("ShowAnimtime")]
		public float AnimationTime = 2f;

		public bool UseEasing;

		[ConditionalShow("ShowAnimtime")]
		public bool SeparateCurves;

		[ConditionalHide("UseEasing")]
		public VectorSpline AnimationTrack;

		[ConditionalShow("ShowAnimtime")]
		public AnimationCurve AnimationCurve;

		[ConditionalHide("UseEasing")]
		public VectorSpline LootAtTrack;

		[ConditionalShow("ShowLookCurve")]
		public AnimationCurve LookAtCurve;

		private bool ShowAnimtime
		{
			get
			{
				if (!UseEasing && !AnimationTrack)
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

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}, {14}: {15}, {16}: {17}, {18}: {19}, {20}: {21}", "RubberBandIntensity", RubberBandIntensity, "RubberBandRange", RubberBandRange, "AnimationTime", AnimationTime, "UseEasing", UseEasing, "SeparateCurves", SeparateCurves, "AnimationTrack", AnimationTrack, "AnimationCurve", AnimationCurve, "LootAtTrack", LootAtTrack, "LookAtCurve", LookAtCurve, "ShowAnimtime", ShowAnimtime, "ShowLookCurve", ShowLookCurve);
		}
	}

	[Serializable]
	[IKnowWhatImDoing]
	public class TweenLightDataWithPrev : TweenLightData
	{
		public LightTweenAnchor PreviousAnchor;
	}

	[SerializeField]
	private TweenLightData m_DefaultTween;

	[SerializeField]
	private TweenLightDataWithPrev[] m_Tweens;

	private TweenLightData m_Tween;

	public float Intensivity;

	public float Range;

	private float m_PrevIntensivity;

	private float m_PrevRange;

	private float m_Time;

	public void TickTween(MainMenuLightSorce lightLightSorce, float dt)
	{
		if (m_Tween != null)
		{
			float b = lightLightSorce.CurrentTween.Intensivity;
			if (m_Tween.UseEasing)
			{
				float t = m_Tween.AnimationCurve.Evaluate(m_Time / m_Tween.AnimationTime);
				b = Mathf.LerpUnclamped(m_PrevIntensivity, b, t);
			}
			else if ((bool)m_Tween.AnimationTrack && m_Tween.AnimationTime > 0f)
			{
				b = m_Tween.AnimationCurve.Evaluate(m_Time / m_Tween.AnimationTime);
			}
			lightLightSorce.LightSorce.intensity = Mathf.Lerp(lightLightSorce.LightSorce.intensity, b, dt * m_Tween.RubberBandIntensity);
			float b2 = lightLightSorce.CurrentTween.Range;
			if (m_Tween.UseEasing)
			{
				b2 = Mathf.LerpUnclamped(m_PrevRange, b2, m_Tween.AnimationCurve.Evaluate(m_Time / m_Tween.AnimationTime));
			}
			else if ((bool)m_Tween.AnimationTrack && m_Tween.AnimationTime > 0f)
			{
				b2 = m_Tween.AnimationCurve.Evaluate(m_Time / m_Tween.AnimationTime);
			}
			lightLightSorce.LightSorce.range = Mathf.Lerp(lightLightSorce.LightSorce.range, b2, dt * m_Tween.RubberBandRange);
			m_Time += dt;
		}
	}

	public void StartTween(Light lightLightSorce, LightTweenAnchor lightCurrentTween)
	{
		m_PrevIntensivity = lightLightSorce.intensity;
		m_PrevRange = lightLightSorce.range;
		m_Tween = m_Tweens.FirstOrDefault((TweenLightDataWithPrev t) => t.PreviousAnchor == lightCurrentTween) ?? m_DefaultTween;
		m_Time = 0f;
	}
}
