using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public abstract class ScriptableRendererData : ScriptableObject
{
	[SerializeField]
	private List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	[SerializeField]
	internal List<long> m_RendererFeatureMap = new List<long>(10);

	internal bool IsInvalidated { get; set; }

	public List<ScriptableRendererFeature> rendererFeatures => m_RendererFeatures;

	protected abstract ScriptableRenderer Create();

	public new void SetDirty()
	{
		IsInvalidated = true;
	}

	internal ScriptableRenderer InternalCreateRenderer()
	{
		IsInvalidated = false;
		return Create();
	}

	protected virtual void OnValidate()
	{
		SetDirty();
	}

	protected virtual void OnEnable()
	{
		SetDirty();
	}

	internal bool TryGetRendererFeature<T>(out T rendererFeature) where T : ScriptableRendererFeature
	{
		foreach (ScriptableRendererFeature rendererFeature2 in rendererFeatures)
		{
			if (rendererFeature2.GetType() == typeof(T))
			{
				rendererFeature = rendererFeature2 as T;
				return true;
			}
		}
		rendererFeature = null;
		return false;
	}
}
