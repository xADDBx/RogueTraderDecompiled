using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public abstract class ScriptableRendererFeature : ScriptableObject, IDisposable
{
	public abstract string GetFeatureIdentifier();

	public abstract void Create();

	public abstract void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData);

	public abstract void DisableFeature();

	private void OnEnable()
	{
		Create();
	}

	private void OnValidate()
	{
		Create();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
