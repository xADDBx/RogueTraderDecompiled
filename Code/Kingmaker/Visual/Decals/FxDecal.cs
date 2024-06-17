using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

public class FxDecal : ScreenSpaceDecal
{
	private float m_Time;

	private Queue<FxDecalAnimationSettings> m_AnimationQueue = new Queue<FxDecalAnimationSettings>();

	private Vector3 m_DefaultScale = Vector3.one;

	private Color m_DefaultAlbedoColor = Color.white;

	private Color m_DefaultEmissionColor = Color.white;

	private float m_DefaultEmissionColorScale = 1f;

	private MeshRenderer m_MeshRenderer;

	private bool m_DefaultMeshRenderEnabled;

	[SerializeField]
	private Ramp m_ColorRamp = new Ramp();

	[Header("Delay")]
	public float Delay;

	public bool IsInvisibleWhileDelay;

	[Header("Animation")]
	public List<FxDecalAnimationSettings> Animations = new List<FxDecalAnimationSettings>();

	public bool UseScaleAnimation = true;

	[NonSerialized]
	[HideInInspector]
	public bool AutoDestroy;

	private static readonly int _Color = Shader.PropertyToID("_BaseColor");

	private static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

	private static readonly int _EmissionColorScale = Shader.PropertyToID("_EmissionColorScale");

	private static readonly int _SubstractAlphaFlag = Shader.PropertyToID("_SubstractAlphaFlag");

	public override bool IsVisible
	{
		get
		{
			if (m_Time > Delay)
			{
				if (Application.isPlaying && Animations.Count > 0 && m_AnimationQueue.Count == 0)
				{
					return false;
				}
				return true;
			}
			if (Application.isPlaying)
			{
				return !IsInvisibleWhileDelay;
			}
			return true;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_MeshRenderer = GetComponent<MeshRenderer>();
		if ((bool)m_MeshRenderer)
		{
			m_Material = m_MeshRenderer.sharedMaterial;
			if (Application.isPlaying)
			{
				m_Material = m_MeshRenderer.material;
				m_DefaultMeshRenderEnabled = m_MeshRenderer.enabled;
				if (IsInvisibleWhileDelay && Delay > 0f)
				{
					m_MeshRenderer.enabled = false;
				}
			}
		}
		m_Time = 0f;
		if (UseScaleAnimation)
		{
			m_DefaultScale = base.transform.localScale;
		}
		if (m_Material != null)
		{
			if (m_Material.HasProperty(_Color))
			{
				m_DefaultAlbedoColor = m_Material.GetColor(_Color);
			}
			if (m_Material.HasProperty(_EmissionColor))
			{
				m_DefaultEmissionColor = m_Material.GetColor(_EmissionColor);
			}
			if (m_Material.HasProperty(_EmissionColorScale))
			{
				m_DefaultEmissionColorScale = m_Material.GetFloat(_EmissionColorScale);
			}
		}
		Animations.ForEach(delegate(FxDecalAnimationSettings a)
		{
			a.Reset();
			m_AnimationQueue.Enqueue(a);
		});
		Ramp colorRamp = m_ColorRamp;
		colorRamp.TextureBaked = (Action<Texture2D>)Delegate.Combine(colorRamp.TextureBaked, new Action<Texture2D>(RampBaked));
		if (m_ColorRamp.Enabled)
		{
			UpdateColorRamp(m_ColorRamp.GetRamp());
		}
		Update();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Ramp colorRamp = m_ColorRamp;
		colorRamp.TextureBaked = (Action<Texture2D>)Delegate.Remove(colorRamp.TextureBaked, new Action<Texture2D>(RampBaked));
		m_AnimationQueue.Clear();
		if (UseScaleAnimation)
		{
			base.transform.localScale = m_DefaultScale;
		}
		if (Application.isPlaying && m_MeshRenderer != null)
		{
			m_MeshRenderer.enabled = m_DefaultMeshRenderEnabled;
		}
	}

	private void RampBaked(Texture2D ramp)
	{
		if (m_ColorRamp.Enabled)
		{
			UpdateColorRamp(ramp);
		}
	}

	private void UpdateColorRamp(Texture2D ramp)
	{
		base.MaterialProperties.SetTexture(ShaderProps._ColorAlphaRamp, ramp);
	}

	protected override void Update()
	{
		base.Update();
		if (!Application.isPlaying)
		{
			return;
		}
		m_Time += Time.deltaTime;
		if (m_Time < Delay || Animations.Count == 0)
		{
			return;
		}
		if (IsInvisibleWhileDelay && !m_MeshRenderer.enabled)
		{
			m_MeshRenderer.enabled = true;
		}
		FxDecalAnimationSettings fxDecalAnimationSettings = null;
		while (m_AnimationQueue.Count > 0 && fxDecalAnimationSettings == null)
		{
			fxDecalAnimationSettings = m_AnimationQueue.Peek();
			if (fxDecalAnimationSettings.CurrentTime >= fxDecalAnimationSettings.Lifetime)
			{
				if (fxDecalAnimationSettings.LoopAnimation)
				{
					fxDecalAnimationSettings.CurrentTime = 0f;
					continue;
				}
				m_AnimationQueue.Dequeue();
				fxDecalAnimationSettings = null;
			}
		}
		if (fxDecalAnimationSettings == null && Animations.Count > 0)
		{
			if (AutoDestroy)
			{
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		fxDecalAnimationSettings.CurrentTime += Time.deltaTime;
		float num = fxDecalAnimationSettings.CurrentTime / fxDecalAnimationSettings.Lifetime;
		if (UseScaleAnimation)
		{
			fxDecalAnimationSettings.CurrentScale.x = fxDecalAnimationSettings.ScaleX.EvaluateNormalized(num) * fxDecalAnimationSettings.ScaleXZ.EvaluateNormalized(num);
			fxDecalAnimationSettings.CurrentScale.y = fxDecalAnimationSettings.ScaleY.EvaluateNormalized(num);
			fxDecalAnimationSettings.CurrentScale.z = fxDecalAnimationSettings.ScaleZ.EvaluateNormalized(num) * fxDecalAnimationSettings.ScaleXZ.EvaluateNormalized(num);
		}
		fxDecalAnimationSettings.CurrentAlbedoColor = fxDecalAnimationSettings.AlbedoColorOverLifetime.Evaluate(num);
		fxDecalAnimationSettings.CurrentEmissionColor = fxDecalAnimationSettings.EmissionColorOverLifetime.Evaluate(num);
		fxDecalAnimationSettings.CurrentSubstractAlpha = fxDecalAnimationSettings.SubstractAlphaOverLifetime.Evaluate(num);
		if (UseScaleAnimation)
		{
			base.transform.localScale = Vector3.Scale(m_DefaultScale, fxDecalAnimationSettings.CurrentScale);
		}
		base.MaterialProperties.SetColor(_Color, m_DefaultAlbedoColor * fxDecalAnimationSettings.CurrentAlbedoColor);
		base.MaterialProperties.SetColor(_EmissionColor, m_DefaultEmissionColor * fxDecalAnimationSettings.CurrentEmissionColor);
		base.MaterialProperties.SetFloat(_EmissionColorScale, m_DefaultEmissionColorScale * fxDecalAnimationSettings.CurrentEmissionColor.a);
		base.MaterialProperties.SetFloat(_SubstractAlphaFlag, fxDecalAnimationSettings.CurrentSubstractAlpha);
		if (m_MeshRenderer != null)
		{
			m_MeshRenderer.SetPropertyBlock(base.MaterialProperties);
		}
	}
}
