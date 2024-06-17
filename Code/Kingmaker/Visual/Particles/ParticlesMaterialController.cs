using System;
using System.Collections.Generic;
using Kingmaker.GradientSystem;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Updatables;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.Particles;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ParticlesMaterialController : UpdateableInEditorBehaviour
{
	public enum GradientUpdate
	{
		Unknown,
		AlreadyBake,
		AutoBake
	}

	private const string DistortionKeyword = "DISTORTION_ON";

	private List<ParticleSystemVertexStream> m_ActiveStreams = new List<ParticleSystemVertexStream>();

	private static List<Vector4> s_CustomData = new List<Vector4>();

	[SerializeField]
	[FormerlySerializedAs("m_ColorAlphaRamp")]
	public Texture2D TexColorAlphaRamp;

	[SerializeField]
	[FormerlySerializedAs("m_TrailColorRamp")]
	public Texture2D TexTrailColorRamp;

	private bool m_IsVisible;

	[SerializeField]
	private GradientUpdate m_TypeUpdate;

	[SerializeField]
	private GradientUpdate m_TrailTypeUpdate;

	[ColorRamp(ColorRampType.Particles)]
	public Gradient ColorAlphaRamp = new Gradient();

	[ColorRamp(ColorRampType.Trails)]
	public Gradient TrailColorAlphaRamp = new Gradient();

	public bool RandomizeNoiseUv = true;

	public bool RandomizeColorRampOffset;

	[HideInInspector]
	public bool UnscaledTime;

	private ParticleSystem m_ParticleSystem;

	private ParticleSystem.TextureSheetAnimationModule m_TextureSheet;

	private ParticleSystemRenderer m_Renderer;

	private bool m_ParticlesDataUpdateNeeded;

	private Vector4 m_RampSt;

	public GradientUpdate TypeUpdate
	{
		get
		{
			if (m_TypeUpdate == GradientUpdate.Unknown)
			{
				m_TypeUpdate = ((!(TexColorAlphaRamp == null)) ? GradientUpdate.AlreadyBake : GradientUpdate.AutoBake);
			}
			return m_TypeUpdate;
		}
	}

	public GradientUpdate TrailTypeUpdate
	{
		get
		{
			if (m_TrailTypeUpdate == GradientUpdate.Unknown)
			{
				m_TrailTypeUpdate = ((!(TexTrailColorRamp == null)) ? GradientUpdate.AlreadyBake : GradientUpdate.AutoBake);
			}
			return m_TrailTypeUpdate;
		}
	}

	private void Awake()
	{
		m_ParticleSystem = GetComponent<ParticleSystem>();
		m_TextureSheet = m_ParticleSystem.textureSheetAnimation;
		m_Renderer = GetComponent<ParticleSystemRenderer>();
		ApplySettings();
		UpdateColorRamp(ColorRampType.Particles, forceUpdateCache: false);
		UpdateColorRamp(ColorRampType.Trails, forceUpdateCache: false);
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		ApplySettings();
		UpdateColorRamp(ColorRampType.Particles, forceUpdateCache: false);
		UpdateColorRamp(ColorRampType.Trails, forceUpdateCache: false);
	}

	public override void DoUpdate()
	{
		if (!m_IsVisible)
		{
			return;
		}
		m_Renderer.GetActiveVertexStreams(m_ActiveStreams);
		if (m_ParticlesDataUpdateNeeded && m_ActiveStreams.Contains(ParticleSystemVertexStream.Custom1XYZW))
		{
			int customParticleData = m_ParticleSystem.GetCustomParticleData(s_CustomData, ParticleSystemCustomData.Custom1);
			for (int i = 0; i < customParticleData; i++)
			{
				Vector4 value = s_CustomData[i];
				if (value.w < 0.5f)
				{
					value.x = (RandomizeNoiseUv ? PFStatefulRandom.Visuals.Particles.value : 0f);
					value.y = ((RandomizeColorRampOffset && Mathf.Abs(m_RampSt.x) > 1E-06f) ? (PFStatefulRandom.Visuals.Particles.value / m_RampSt.x) : 0f);
					value.w = 1f;
				}
				s_CustomData[i] = value;
			}
			m_ParticleSystem.SetCustomParticleData(s_CustomData, ParticleSystemCustomData.Custom1);
		}
		if (UnscaledTime)
		{
			m_Renderer.sharedMaterial.SetFloat(ShaderProps._TimeEditor, Time.unscaledTime - Time.time);
		}
	}

	private void ApplySettings()
	{
		if (m_TextureSheet.enabled && m_TextureSheet.uvChannelMask != UVChannelFlags.UV0)
		{
			m_TextureSheet.uvChannelMask = UVChannelFlags.UV0;
		}
		if (m_Renderer != null)
		{
			Material sharedMaterial = m_Renderer.sharedMaterial;
			if (sharedMaterial != null && sharedMaterial.HasProperty(ShaderProps._ColorAlphaRamp_ST))
			{
				sharedMaterial.SetFloat(ShaderProps._RandomizeRampOffset, RandomizeColorRampOffset ? 1 : 0);
				sharedMaterial.SetFloat(ShaderProps._RandomizeNoiseOffset, RandomizeNoiseUv ? 1 : 0);
				sharedMaterial.SetFloat(ShaderProps._TexSheetEnabled, m_ParticleSystem.textureSheetAnimation.enabled ? 1 : 0);
				m_RampSt = sharedMaterial.GetVector(ShaderProps._ColorAlphaRamp_ST);
				m_ParticlesDataUpdateNeeded = sharedMaterial.IsKeywordEnabled("NOISE0_ON") || sharedMaterial.IsKeywordEnabled("NOISE1_ON") || (sharedMaterial.IsKeywordEnabled("COLOR_ALPHA_RAMP") && RandomizeColorRampOffset);
				SetVertexStreams(sharedMaterial);
			}
		}
	}

	private void SetVertexStreams(Material mat)
	{
		List<ParticleSystemVertexStream> list = new List<ParticleSystemVertexStream>
		{
			ParticleSystemVertexStream.Position,
			ParticleSystemVertexStream.Normal,
			ParticleSystemVertexStream.Tangent,
			ParticleSystemVertexStream.Color,
			ParticleSystemVertexStream.UV,
			ParticleSystemVertexStream.UV2
		};
		if (m_ParticlesDataUpdateNeeded)
		{
			if (list[list.Count - 1] != ParticleSystemVertexStream.Custom1XYZW)
			{
				list.Add(ParticleSystemVertexStream.Custom1XYZW);
			}
		}
		else if (list[list.Count - 1] == ParticleSystemVertexStream.Custom1XYZW)
		{
			list.Remove(ParticleSystemVertexStream.Custom1XYZW);
		}
		m_Renderer.SetActiveVertexStreams(list);
	}

	public void UpdateColorRamp(ColorRampType type, bool forceUpdateCache, bool save = false)
	{
		if (Application.isEditor && Kingmaker.GradientSystem.GradientSystem.IsBakingProcess)
		{
			return;
		}
		ParticleSystemRenderer component = GetComponent<ParticleSystemRenderer>();
		Material material = null;
		Texture2D texture2D = null;
		Gradient gradient = null;
		switch (type)
		{
		case ColorRampType.Particles:
			material = component.sharedMaterial;
			texture2D = TexColorAlphaRamp;
			gradient = ColorAlphaRamp;
			break;
		case ColorRampType.Trails:
			material = component.trailMaterial;
			texture2D = TexTrailColorRamp;
			gradient = TrailColorAlphaRamp;
			break;
		}
		if (component != null && material != null && material.IsKeywordEnabled("COLOR_ALPHA_RAMP"))
		{
			if (texture2D == null)
			{
				Texture2D texture2D2 = null;
				texture2D2 = Kingmaker.GradientSystem.GradientSystem.GetTextureFromGradient(gradient);
				material.SetTexture(ShaderProps._ColorAlphaRamp, texture2D2);
				texture2D = texture2D2;
			}
			else
			{
				material.SetTexture(ShaderProps._ColorAlphaRamp, texture2D);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null && component.sharedMaterial != null)
			{
				TimeSpan timeSpan = DateTime.Now - DateTime.Today;
				component.sharedMaterial.SetFloat(ShaderProps._TimeEditor, (float)timeSpan.TotalSeconds);
			}
		}
	}

	private void OnBecameVisible()
	{
		m_IsVisible = true;
	}

	private void OnBecameInvisible()
	{
		m_IsVisible = false;
	}
}
