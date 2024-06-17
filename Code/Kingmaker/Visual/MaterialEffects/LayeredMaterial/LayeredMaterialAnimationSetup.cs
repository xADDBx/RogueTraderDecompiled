using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class LayeredMaterialAnimationSetup : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	private sealed class FloatPropertyAnimationData
	{
		public string m_PropertyName;

		public AnimationCurve m_Curve;

		[Min(0.01f)]
		public float m_CurveDuration;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = m_Curve != null && m_CurveDuration > 0f && !string.IsNullOrWhiteSpace(m_PropertyName);
			m_CachedAnimationClip = (flag ? new FloatAnimationClip(m_PropertyName, m_Curve, m_CurveDuration) : null);
		}
	}

	[Serializable]
	private sealed class ColorPropertyAnimationData
	{
		public string m_PropertyName;

		public Gradient m_Gradient;

		public GradientWrapMode m_GradientWrapMode;

		[Min(0.01f)]
		public float m_GradientDuration;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = m_Gradient != null && m_GradientDuration > 0f && !string.IsNullOrWhiteSpace(m_PropertyName);
			m_CachedAnimationClip = (flag ? new ColorAnimationClip(m_PropertyName, m_Gradient, m_GradientDuration, m_GradientWrapMode) : null);
		}
	}

	[Serializable]
	private sealed class TexturePropertyAnimationData
	{
		public string m_PropertyName;

		public Texture m_Texture;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = m_Texture != null && !string.IsNullOrWhiteSpace(m_PropertyName);
			m_CachedAnimationClip = (flag ? new TextureAnimationClip(m_PropertyName, m_Texture) : null);
		}
	}

	[Serializable]
	private sealed class TransformPropertyAnimationData
	{
		public string m_WorldToLocalPropertyName;

		public string m_LocalToWorldPropertyName;

		public Transform m_Transform;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = m_Transform != null && (!string.IsNullOrWhiteSpace(m_WorldToLocalPropertyName) || !string.IsNullOrWhiteSpace(m_LocalToWorldPropertyName));
			m_CachedAnimationClip = (flag ? new TransformAnimationClip(m_WorldToLocalPropertyName, m_LocalToWorldPropertyName, m_Transform) : null);
		}
	}

	[Serializable]
	private sealed class TransferMaterialPropertyAnimationData
	{
		public string m_PropertyName;

		public string m_BaseMaterialPropertyName;

		public MaterialPropertyType m_PropertyType;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = !string.IsNullOrWhiteSpace(m_PropertyName) && !string.IsNullOrWhiteSpace(m_BaseMaterialPropertyName);
			m_CachedAnimationClip = (flag ? new TransferMaterialPropertyAnimationClip(m_BaseMaterialPropertyName, m_PropertyName, m_PropertyType) : null);
		}
	}

	[Serializable]
	private sealed class TransferScriptPropertyAnimationData
	{
		public string m_PropertyName;

		public ScriptProperty m_ScriptProperty;

		[Min(0f)]
		public float m_Duration;

		[Min(0f)]
		public float m_Delay;

		[NonSerialized]
		public IAnimationClip m_CachedAnimationClip;

		public void Refresh()
		{
			bool flag = !string.IsNullOrWhiteSpace(m_PropertyName);
			m_CachedAnimationClip = (flag ? new TransferScriptPropertyAnimationClip(m_ScriptProperty, m_PropertyName) : null);
		}
	}

	private enum LegacyRendererTypeFilter
	{
		SkinnedMeshRenderer,
		Any
	}

	private const int kFormatVersion1 = 1;

	private const int kCurrentFormatVersion = 1;

	[SerializeField]
	private int m_FormatVersion;

	[SerializeField]
	private int m_Priority;

	[SerializeField]
	private LegacyRendererTypeFilter m_RendererTypeFilter;

	[SerializeField]
	private RendererType m_RendererTypeMask = RendererType.SkinnedMeshRenderer;

	[SerializeField]
	private Material m_Material;

	[SerializeField]
	private float m_Duration;

	[SerializeField]
	private float m_Delay;

	[SerializeField]
	private FloatPropertyAnimationData[] m_FloatClips;

	[SerializeField]
	private ColorPropertyAnimationData[] m_ColorClips;

	[SerializeField]
	private TexturePropertyAnimationData[] m_TextureClips;

	[SerializeField]
	private TransformPropertyAnimationData[] m_TransformClips;

	[SerializeField]
	private TransferMaterialPropertyAnimationData[] m_TransferMaterialPropertyClips;

	[SerializeField]
	private TransferScriptPropertyAnimationData[] m_TransferScriptPropertyClips;

	[UsedImplicitly]
	private void OnValidate()
	{
		RefreshClips();
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		RefreshClips();
	}

	private void RefreshClips()
	{
		if (m_FloatClips != null)
		{
			FloatPropertyAnimationData[] floatClips = m_FloatClips;
			for (int i = 0; i < floatClips.Length; i++)
			{
				floatClips[i].Refresh();
			}
		}
		if (m_ColorClips != null)
		{
			ColorPropertyAnimationData[] colorClips = m_ColorClips;
			for (int i = 0; i < colorClips.Length; i++)
			{
				colorClips[i].Refresh();
			}
		}
		if (m_TextureClips != null)
		{
			TexturePropertyAnimationData[] textureClips = m_TextureClips;
			for (int i = 0; i < textureClips.Length; i++)
			{
				textureClips[i].Refresh();
			}
		}
		if (m_TransformClips != null)
		{
			TransformPropertyAnimationData[] transformClips = m_TransformClips;
			for (int i = 0; i < transformClips.Length; i++)
			{
				transformClips[i].Refresh();
			}
		}
		if (m_TransferMaterialPropertyClips != null)
		{
			TransferMaterialPropertyAnimationData[] transferMaterialPropertyClips = m_TransferMaterialPropertyClips;
			for (int i = 0; i < transferMaterialPropertyClips.Length; i++)
			{
				transferMaterialPropertyClips[i].Refresh();
			}
		}
		if (m_TransferScriptPropertyClips != null)
		{
			TransferScriptPropertyAnimationData[] transferScriptPropertyClips = m_TransferScriptPropertyClips;
			for (int i = 0; i < transferScriptPropertyClips.Length; i++)
			{
				transferScriptPropertyClips[i].Refresh();
			}
		}
	}

	public bool TryAddTrack(Timeline timeline, out int token)
	{
		if (m_Material == null)
		{
			token = 0;
			return false;
		}
		if (m_RendererTypeMask == (RendererType)0)
		{
			token = 0;
			return false;
		}
		float time = Time.time + m_Delay;
		Track track = timeline.AddTrack(m_Priority, time, m_Duration, m_Material, m_RendererTypeMask);
		if (m_FloatClips != null)
		{
			FloatPropertyAnimationData[] floatClips = m_FloatClips;
			foreach (FloatPropertyAnimationData floatPropertyAnimationData in floatClips)
			{
				if (floatPropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(floatPropertyAnimationData.m_CachedAnimationClip, floatPropertyAnimationData.m_Delay, floatPropertyAnimationData.m_Duration);
				}
			}
		}
		if (m_ColorClips != null)
		{
			ColorPropertyAnimationData[] colorClips = m_ColorClips;
			foreach (ColorPropertyAnimationData colorPropertyAnimationData in colorClips)
			{
				if (colorPropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(colorPropertyAnimationData.m_CachedAnimationClip, colorPropertyAnimationData.m_Delay, colorPropertyAnimationData.m_Duration);
				}
			}
		}
		if (m_TextureClips != null)
		{
			TexturePropertyAnimationData[] textureClips = m_TextureClips;
			foreach (TexturePropertyAnimationData texturePropertyAnimationData in textureClips)
			{
				if (texturePropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(texturePropertyAnimationData.m_CachedAnimationClip, texturePropertyAnimationData.m_Delay, texturePropertyAnimationData.m_Duration);
				}
			}
		}
		if (m_TransformClips != null)
		{
			TransformPropertyAnimationData[] transformClips = m_TransformClips;
			foreach (TransformPropertyAnimationData transformPropertyAnimationData in transformClips)
			{
				if (transformPropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(transformPropertyAnimationData.m_CachedAnimationClip, transformPropertyAnimationData.m_Delay, transformPropertyAnimationData.m_Duration);
				}
			}
		}
		if (m_TransferMaterialPropertyClips != null)
		{
			TransferMaterialPropertyAnimationData[] transferMaterialPropertyClips = m_TransferMaterialPropertyClips;
			foreach (TransferMaterialPropertyAnimationData transferMaterialPropertyAnimationData in transferMaterialPropertyClips)
			{
				if (transferMaterialPropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(transferMaterialPropertyAnimationData.m_CachedAnimationClip, transferMaterialPropertyAnimationData.m_Delay, transferMaterialPropertyAnimationData.m_Duration);
				}
			}
		}
		if (m_TransferScriptPropertyClips != null)
		{
			TransferScriptPropertyAnimationData[] transferScriptPropertyClips = m_TransferScriptPropertyClips;
			foreach (TransferScriptPropertyAnimationData transferScriptPropertyAnimationData in transferScriptPropertyClips)
			{
				if (transferScriptPropertyAnimationData.m_CachedAnimationClip != null)
				{
					track.AddAnimationClip(transferScriptPropertyAnimationData.m_CachedAnimationClip, transferScriptPropertyAnimationData.m_Delay, transferScriptPropertyAnimationData.m_Duration);
				}
			}
		}
		token = track.token;
		return true;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_FormatVersion != 1)
		{
			MigrateFromVersionLessToV1();
			m_FormatVersion = 1;
		}
	}

	private void MigrateFromVersionLessToV1()
	{
		LegacyRendererTypeFilter rendererTypeFilter = m_RendererTypeFilter;
		if (rendererTypeFilter != 0 && rendererTypeFilter == LegacyRendererTypeFilter.Any)
		{
			m_RendererTypeMask = RendererType.MeshRenderer | RendererType.SkinnedMeshRenderer;
		}
		else
		{
			m_RendererTypeMask = RendererType.SkinnedMeshRenderer;
		}
	}
}
