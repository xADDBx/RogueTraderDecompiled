using System;
using System.Collections.Generic;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[UpdateCanChangeEnabledState]
public class FxFadeOut : UpdateableBehaviour
{
	public interface IOpacitySource
	{
		float GetOpacity();
	}

	[Serializable]
	public struct CustomMaterialPropertyData
	{
		public string PropertyName;

		public float FadeOutValue;

		[NonSerialized]
		public int PropertyId;
	}

	public float Duration;

	public bool VFXGraphStopEmission;

	[Tooltip("Required for level designers, they will use it in cutscene when they want to dissolve FX manually.")]
	public bool StartForceFadeOut;

	[Tooltip("Required for level designers, they will use it in cutscene when they want to fade in FX manually.")]
	public bool StartFadeIn;

	[Tooltip("If true, the FX object will be released to pool after fade out. If false, it will remain in the scene but with opacity set to 0.")]
	public bool DestroyFxObject = true;

	public List<ParticleSystem> StopParticlesEmission = new List<ParticleSystem>();

	[Tooltip("Applied for non standard shaders.")]
	[SerializeField]
	private CustomMaterialPropertyData[] m_CustomMaterialProperties;

	[Tooltip("Ignore external opacity sources (e.g., FxFadeOthers)")]
	[SerializeField]
	private bool m_IgnoreExternalOpacitySources;

	private FxFader m_Fader;

	private bool m_IsAnimationPlaying;

	private float m_AnimationStartTime;

	private bool m_IsFadingIn;

	private readonly List<IOpacitySource> m_OpacitySources = new List<IOpacitySource>();

	private bool m_OpacitySourcesChanged;

	public void AddOpacitySource(IOpacitySource source)
	{
		m_OpacitySources.Add(source);
		m_OpacitySourcesChanged = true;
	}

	public void RemoveOpacitySource(IOpacitySource source)
	{
		m_OpacitySources.Remove(source);
		m_OpacitySourcesChanged = true;
	}

	private void Awake()
	{
		InitializeCustomPropertySettings();
		m_Fader = new FxFader(base.gameObject, StopParticlesEmission, m_CustomMaterialProperties, VFXGraphStopEmission);
	}

	private void InitializeCustomPropertySettings()
	{
		if (m_CustomMaterialProperties == null)
		{
			return;
		}
		int i = 0;
		for (int num = m_CustomMaterialProperties.Length; i < num; i++)
		{
			ref CustomMaterialPropertyData reference = ref m_CustomMaterialProperties[i];
			if (!string.IsNullOrEmpty(reference.PropertyName))
			{
				reference.PropertyId = Shader.PropertyToID(reference.PropertyName);
			}
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		m_Fader.SetOpacity(1f, applyFadeOutCorrection: true);
		m_IsAnimationPlaying = false;
		m_AnimationStartTime = 0f;
	}

	public override void DoUpdate()
	{
		if (StartForceFadeOut)
		{
			StartFadeOut();
			StartForceFadeOut = false;
		}
		if (StartFadeIn)
		{
			StartFadeInFunc();
			StartFadeIn = false;
		}
		bool num = m_IsAnimationPlaying || m_OpacitySources.Count > 0 || m_OpacitySourcesChanged;
		m_OpacitySourcesChanged = false;
		if (num)
		{
			m_Fader.SetOpacity(ResolveOpacity(), m_IsAnimationPlaying && !m_IsFadingIn);
		}
		if (!m_IsAnimationPlaying || (Duration != 0f && !(Time.unscaledTime - m_AnimationStartTime > Duration)))
		{
			return;
		}
		m_IsAnimationPlaying = false;
		m_AnimationStartTime = 0f;
		if (!m_IsFadingIn)
		{
			GetComponent<SoundFx>()?.PlayDestroyEventsManually();
			if (DestroyFxObject)
			{
				ReleaseSelf();
			}
		}
		m_IsFadingIn = false;
	}

	public void StartFadeOut()
	{
		if (m_IsAnimationPlaying && m_IsFadingIn)
		{
			float num = ResolveOpacity();
			m_IsFadingIn = false;
			m_AnimationStartTime = Time.unscaledTime - (1f - num) * Duration;
		}
		else if (!m_IsAnimationPlaying || m_IsFadingIn)
		{
			if (Duration > 0f)
			{
				base.enabled = true;
				m_IsAnimationPlaying = true;
				m_IsFadingIn = false;
				m_AnimationStartTime = Time.unscaledTime;
			}
			else
			{
				ReleaseSelf();
			}
		}
	}

	public void StartFadeInFunc()
	{
		if (m_IsAnimationPlaying && !m_IsFadingIn)
		{
			float num = ResolveOpacity();
			m_IsFadingIn = true;
			m_AnimationStartTime = Time.unscaledTime - num * Duration;
		}
		else if ((!m_IsAnimationPlaying || !m_IsFadingIn) && Duration > 0f)
		{
			base.enabled = true;
			m_IsAnimationPlaying = true;
			m_IsFadingIn = true;
			m_AnimationStartTime = Time.unscaledTime;
		}
	}

	private float ResolveOpacity()
	{
		float a = ((!m_IsAnimationPlaying) ? 1f : ((!m_IsFadingIn) ? (1f - Mathf.Clamp01((Time.unscaledTime - m_AnimationStartTime) / Duration)) : Mathf.Clamp01((Time.unscaledTime - m_AnimationStartTime) / Duration)));
		float num;
		if (m_IgnoreExternalOpacitySources || m_OpacitySources.Count == 0)
		{
			num = 1f;
		}
		else
		{
			num = 0f;
			foreach (IOpacitySource opacitySource in m_OpacitySources)
			{
				num = Mathf.Max(num, opacitySource.GetOpacity());
			}
		}
		return Mathf.Min(a, num);
	}

	private void ReleaseSelf()
	{
		if (DestroyFxObject)
		{
			GameObjectsPool.Release(base.gameObject);
		}
	}
}
