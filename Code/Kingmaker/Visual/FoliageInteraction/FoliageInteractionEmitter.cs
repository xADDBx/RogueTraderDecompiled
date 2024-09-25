using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Visual.FoliageInteraction;

[Obsolete]
public class FoliageInteractionEmitter : MonoBehaviour
{
	[Serializable]
	public class ScaleCurve
	{
		[SerializeField]
		[MinMaxSlider(0f, 1f)]
		private Vector2 m_KeysTime;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Value0;

		[SerializeField]
		[Range(0f, 1f)]
		private float m_Value1;

		public float NormalizedTime0 => m_KeysTime.x;

		public float NormalizedTime1 => m_KeysTime.y;

		public float Value0 => m_Value0;

		public float Value1 => m_Value1;

		public Color ConvertToColor()
		{
			return new Color(m_KeysTime.x, m_Value0, m_KeysTime.y, m_Value1);
		}

		public AnimationCurve ConvertToAnimationCurve()
		{
			AnimationCurve animationCurve = new AnimationCurve();
			if (m_KeysTime.x > 0f)
			{
				animationCurve.AddKey(new Keyframe(0f, 0f, 0f, 0f));
			}
			animationCurve.AddKey(new Keyframe(m_KeysTime.x, Value0, 0f, 0f));
			if (m_KeysTime.y > m_KeysTime.x)
			{
				animationCurve.AddKey(new Keyframe(m_KeysTime.y, Value1, 0f, 0f));
			}
			if (m_KeysTime.y < 1f)
			{
				animationCurve.AddKey(new Keyframe(1f, 0f, 0f, 0f));
			}
			return animationCurve;
		}
	}

	[Serializable]
	public class EmitterSettings
	{
		public float Delay;

		public float Duration;

		public float RateOverTime;

		public float RateOverDistance = 0.3f;

		public Vector2 Lifetime = Vector2.one;

		public Vector2 Size = Vector2.one;

		public float RandomizePositionRadius;

		[MinMaxSlider(-180f, 180f)]
		public Vector2 RandomizeRotation;

		[Range(0f, 1f)]
		public float RadialWeight = 0.5f;

		[MinMaxSlider(-1f, 1f)]
		public Vector2 InOutBalance;

		public Vector2 FrequencyScale = Vector2.one;

		public float RandomizePhaseOffset;

		public ScaleCurve ScaleCurve;
	}

	private float m_Lifetime;

	private Vector3 m_LastPos;

	private Vector4 m_LastPosTime;

	private float m_LocatorScale = 1f;

	public EmitterSettings Settings;

	public void SetLocatorScale(float locatorScale)
	{
		m_LocatorScale = locatorScale;
	}
}
