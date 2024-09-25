using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Updatables;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Highlighting;

public class MultiHighlighter : UpdateableBehaviour
{
	public enum ModeType
	{
		SingleColor,
		Flashing,
		Gradient
	}

	[Serializable]
	public class HighlightSource : IComparable<HighlightSource>
	{
		public float Lifetime;

		public int Priority;

		public ModeType Mode;

		[Header("Single")]
		public Color Color;

		public float OnsetTime;

		public float RemoveTime;

		[Header("Flashing")]
		public Color AlternateColor;

		public float FlashingFrequency;

		[Header("Gradient")]
		public Gradient ColorGradient;

		public float GradientLifetime;

		public bool UnscaledTime;

		[HideInInspector]
		public float AddTime;

		public int CompareTo(HighlightSource other)
		{
			return other.Priority.CompareTo(Priority);
		}
	}

	private readonly List<HighlightSource> m_HighlightSources = new List<HighlightSource>();

	private HighlightSource m_CurrentlyPlaying;

	private Highlighter m_Highlighter;

	public Highlighter Highlighter => m_Highlighter;

	protected HighlightSource CurrentlyPlaying => m_CurrentlyPlaying;

	[UsedImplicitly]
	protected override void OnEnabled()
	{
		base.OnEnabled();
		m_Highlighter = GetComponent<Highlighter>();
	}

	public void Play(HighlightSource source)
	{
		float addTime = (source.UnscaledTime ? Time.unscaledTime : Time.time);
		if (!m_HighlightSources.Contains(source))
		{
			m_HighlightSources.Add(source);
			m_HighlightSources.Sort();
		}
		source.AddTime = addTime;
	}

	public void Stop(HighlightSource source)
	{
		m_HighlightSources.Remove(source);
	}

	[UsedImplicitly]
	public override void DoUpdate()
	{
		m_HighlightSources.RemoveAll(delegate(HighlightSource s)
		{
			float num2 = (s.UnscaledTime ? Time.unscaledTime : Time.time);
			return s.AddTime <= num2 - s.Lifetime && s.Lifetime > 0f;
		});
		HighlightSource highlightSource = ((m_HighlightSources.Count > 0) ? m_HighlightSources[0] : null);
		if (highlightSource != m_CurrentlyPlaying)
		{
			if (m_CurrentlyPlaying != null)
			{
				m_Highlighter.ConstantOff(m_CurrentlyPlaying.RemoveTime);
				m_Highlighter.FlashingOff();
			}
			if (highlightSource != null)
			{
				switch (highlightSource.Mode)
				{
				case ModeType.SingleColor:
					m_Highlighter.ConstantOn(highlightSource.Color, highlightSource.OnsetTime);
					break;
				case ModeType.Flashing:
					m_Highlighter.FlashingOn(highlightSource.Color, highlightSource.AlternateColor, 1f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case ModeType.Gradient:
					break;
				}
			}
			m_CurrentlyPlaying = highlightSource;
		}
		if (m_CurrentlyPlaying != null && m_CurrentlyPlaying.Mode == ModeType.Gradient && m_CurrentlyPlaying.GradientLifetime > 0f)
		{
			float num = Mathf.Repeat((m_CurrentlyPlaying.UnscaledTime ? Time.unscaledTime : Time.time) - m_CurrentlyPlaying.Lifetime, m_CurrentlyPlaying.GradientLifetime);
			m_Highlighter.ConstantOnImmediate(m_CurrentlyPlaying.ColorGradient.Evaluate(num / m_CurrentlyPlaying.GradientLifetime));
		}
	}

	protected void ReapplyColorInCurrentHighlight(HighlightSource baseAnim)
	{
		if (baseAnim == m_CurrentlyPlaying)
		{
			m_Highlighter.ConstantOn(baseAnim.Color, baseAnim.OnsetTime);
		}
	}
}
