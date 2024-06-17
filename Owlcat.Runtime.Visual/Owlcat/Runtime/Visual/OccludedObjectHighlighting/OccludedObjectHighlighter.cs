using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.OccludedObjectHighlighting;

[DisallowMultipleComponent]
public class OccludedObjectHighlighter : RegisteredBehaviour
{
	internal readonly struct RendererInfo
	{
		public readonly Renderer renderer;

		public readonly int expectedMaterialsCount;

		public RendererInfo(Renderer renderer, int expectedMaterialsCount)
		{
			this.renderer = renderer;
			this.expectedMaterialsCount = expectedMaterialsCount;
		}
	}

	private bool m_IsRenderersDirty;

	private readonly List<RendererInfo> m_RendererInfos = new List<RendererInfo>();

	[CanBeNull]
	private List<RendererInfo> m_ExtraRendererInfos;

	public Color Color = Color.white;

	public void InvalidateRenderers()
	{
		m_IsRenderersDirty = true;
	}

	protected override void OnEnabled()
	{
		m_IsRenderersDirty = true;
	}

	[NotNull]
	internal List<RendererInfo> GetRendererInfos()
	{
		UpdateRenderers();
		return m_RendererInfos;
	}

	private void UpdateRenderers()
	{
		if (!m_IsRenderersDirty)
		{
			foreach (RendererInfo rendererInfo in m_RendererInfos)
			{
				if (rendererInfo.renderer == null)
				{
					m_IsRenderersDirty = true;
					break;
				}
			}
		}
		if (!m_IsRenderersDirty)
		{
			return;
		}
		m_RendererInfos.Clear();
		List<Renderer> value;
		using (CollectionPool<List<Renderer>, Renderer>.Get(out value))
		{
			List<OccludedObjectHighlighterBlockerHierarchy> value2;
			using (CollectionPool<List<OccludedObjectHighlighterBlockerHierarchy>, OccludedObjectHighlighterBlockerHierarchy>.Get(out value2))
			{
				GetComponentsInChildren(value);
				GetComponentsInChildren(value2);
				foreach (Renderer item in value)
				{
					if (!ShouldIgnoreRenderer(item, value2))
					{
						int expectedMaterialsCount = item.GetExpectedMaterialsCount();
						if (expectedMaterialsCount != 0)
						{
							m_RendererInfos.Add(new RendererInfo(item, expectedMaterialsCount));
						}
					}
				}
			}
		}
		if (m_ExtraRendererInfos != null)
		{
			m_RendererInfos.AddRange(m_ExtraRendererInfos);
		}
		m_IsRenderersDirty = false;
	}

	public void AddExtraRenderer(Renderer r)
	{
		if (m_ExtraRendererInfos == null)
		{
			m_ExtraRendererInfos = new List<RendererInfo>();
		}
		m_ExtraRendererInfos.Add(new RendererInfo(r, r.GetExpectedMaterialsCount()));
		m_IsRenderersDirty = true;
	}

	public void RemoveExtraRenderer(Renderer r)
	{
		if (m_ExtraRendererInfos == null)
		{
			return;
		}
		for (int num = m_ExtraRendererInfos.Count - 1; num >= 0; num--)
		{
			if (m_ExtraRendererInfos[num].renderer == r)
			{
				m_ExtraRendererInfos.RemoveAt(num);
				m_IsRenderersDirty = true;
				break;
			}
		}
	}

	private bool ShouldIgnoreRenderer(Renderer candidate, List<OccludedObjectHighlighterBlockerHierarchy> blockers)
	{
		if (candidate.GetComponent<VisualEffect>() != null)
		{
			return true;
		}
		OccludedObjectHighlighter component = candidate.GetComponent<OccludedObjectHighlighter>();
		if (component != null && component != this)
		{
			return true;
		}
		if (candidate.GetComponent<OccludedObjectHighlighterBlocker>() != null)
		{
			return true;
		}
		foreach (OccludedObjectHighlighterBlockerHierarchy blocker in blockers)
		{
			if (candidate.transform.IsChildOf(blocker.transform))
			{
				return true;
			}
		}
		return false;
	}
}
