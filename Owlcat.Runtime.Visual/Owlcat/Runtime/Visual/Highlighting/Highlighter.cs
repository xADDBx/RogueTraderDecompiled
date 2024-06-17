using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Highlighting;

[DisallowMultipleComponent]
public class Highlighter : UpdateableBehaviour
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

	private const float kDoublePI = MathF.PI * 2f;

	private bool m_IsRenderersDirty;

	private readonly List<RendererInfo> m_RendererInfos = new List<RendererInfo>();

	[CanBeNull]
	private List<RendererInfo> m_ExtraRendererInfos;

	private Color m_ConstantColor = Color.white;

	private float m_TransitionTarget;

	private float m_TransitionTime;

	private float m_TransitionValue;

	private Color m_FlashingColorMin = Color.white;

	private Color m_FlashingColorMax = Color.white;

	private float m_FlashingFreq;

	private bool m_Flashing;

	private Color m_CurrentColor = Color.white;

	public bool IsOff => m_TransitionTarget == 0f;

	public Color CurrentColor => m_CurrentColor;

	protected override void OnEnabled()
	{
		m_IsRenderersDirty = true;
	}

	[CanBeNull]
	internal List<RendererInfo> GetRendererInfos()
	{
		if (m_TransitionValue <= 0f || m_CurrentColor.a <= 0f)
		{
			return null;
		}
		UpdateRenderers();
		return m_RendererInfos;
	}

	private void UpdateRenderers()
	{
		foreach (RendererInfo rendererInfo in m_RendererInfos)
		{
			if (rendererInfo.renderer == null)
			{
				m_IsRenderersDirty = true;
				break;
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
			List<HighlighterBlockerHierarchy> value2;
			using (CollectionPool<List<HighlighterBlockerHierarchy>, HighlighterBlockerHierarchy>.Get(out value2))
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

	public void ConstantOn(Color color, float time = 0.25f)
	{
		m_ConstantColor = color;
		m_TransitionTime = ((time >= 0f) ? time : 0f);
		m_TransitionTarget = 1f;
	}

	public void ConstantOff(float time = 0.25f)
	{
		m_TransitionTime = ((time >= 0f) ? time : 0f);
		m_TransitionTarget = 0f;
	}

	public void ConstantOnImmediate(Color color)
	{
		m_ConstantColor = color;
		m_TransitionValue = (m_TransitionTarget = 1f);
	}

	public void ReinitMaterials()
	{
		m_IsRenderersDirty = true;
	}

	public void FlashingOn(Color color1, Color color2, float freq)
	{
		m_FlashingColorMin = color1;
		m_FlashingColorMax = color2;
		m_FlashingFreq = freq;
		m_Flashing = true;
	}

	public void FlashingOff()
	{
		m_Flashing = false;
	}

	public override void DoUpdate()
	{
		UpdateTransition();
	}

	private void UpdateTransition()
	{
		if (m_TransitionValue != m_TransitionTarget)
		{
			if (m_TransitionTime <= 0f)
			{
				m_TransitionValue = m_TransitionTarget;
				return;
			}
			float num = ((m_TransitionTarget > 0f) ? 1f : (-1f));
			m_TransitionValue = Mathf.Clamp01(m_TransitionValue + num * Time.unscaledDeltaTime / m_TransitionTime);
		}
	}

	public void UpdateColors()
	{
		if (m_Flashing)
		{
			m_CurrentColor = Color.Lerp(m_FlashingColorMin, m_FlashingColorMax, 0.5f * Mathf.Sin(Time.realtimeSinceStartup * m_FlashingFreq * (MathF.PI * 2f)) + 0.5f);
		}
		else if (m_TransitionValue > 0f)
		{
			m_CurrentColor = m_ConstantColor;
			m_CurrentColor.a *= m_TransitionValue;
		}
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

	private bool ShouldIgnoreRenderer(Renderer candidate, List<HighlighterBlockerHierarchy> blockers)
	{
		if (candidate.GetComponent<VisualEffect>() != null)
		{
			return true;
		}
		Highlighter component = candidate.GetComponent<Highlighter>();
		if (component != null && component != this)
		{
			return true;
		}
		if (candidate.GetComponent<HighlighterBlocker>() != null)
		{
			return true;
		}
		foreach (HighlighterBlockerHierarchy blocker in blockers)
		{
			if (candidate.transform.IsChildOf(blocker.transform))
			{
				return true;
			}
		}
		return false;
	}
}
