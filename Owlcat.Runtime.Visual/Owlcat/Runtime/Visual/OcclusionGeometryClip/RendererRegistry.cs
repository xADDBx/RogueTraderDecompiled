using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public sealed class RendererRegistry
{
	private readonly Dictionary<int, List<Renderer>> m_RenderersBySceneHandle = new Dictionary<int, List<Renderer>>();

	private int m_RenderersCount;

	public int RenderersCount => m_RenderersCount;

	public IEnumerable<Renderer> GetRenderers()
	{
		foreach (List<Renderer> value in m_RenderersBySceneHandle.Values)
		{
			foreach (Renderer item in value)
			{
				yield return item;
			}
		}
	}

	public void AddRenderer(Renderer renderer)
	{
		GetOrCreateRendererList(renderer.gameObject.scene.handle).Add(renderer);
		m_RenderersCount++;
	}

	public void RemoveRenderers(Scene scene)
	{
		if (m_RenderersBySceneHandle.Remove(scene.handle, out var value))
		{
			m_RenderersCount -= value.Count;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private List<Renderer> GetOrCreateRendererList(int sceneHandle)
	{
		if (!m_RenderersBySceneHandle.TryGetValue(sceneHandle, out var value))
		{
			value = new List<Renderer>();
			m_RenderersBySceneHandle.Add(sceneHandle, value);
		}
		return value;
	}
}
