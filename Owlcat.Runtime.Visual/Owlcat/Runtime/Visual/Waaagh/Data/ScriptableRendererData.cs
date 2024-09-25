using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public abstract class ScriptableRendererData : ScriptableObject
{
	[SerializeField]
	internal List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	[SerializeField]
	internal List<long> m_RendererFeatureMap = new List<long>(10);

	internal bool IsInvalidated { get; set; }

	public List<ScriptableRendererFeature> RendererFeatures => m_RendererFeatures;

	internal ScriptableRenderer InternalCreateRenderer()
	{
		IsInvalidated = false;
		return Create();
	}

	protected abstract ScriptableRenderer Create();

	public new void SetDirty()
	{
		IsInvalidated = true;
	}

	protected virtual void OnValidate()
	{
		SetDirty();
	}

	protected virtual void OnEnable()
	{
		SetDirty();
	}
}
