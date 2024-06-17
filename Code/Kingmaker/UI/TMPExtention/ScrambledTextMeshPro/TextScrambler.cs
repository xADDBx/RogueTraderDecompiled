using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.UI.Sound;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;

public class TextScrambler : IDisposable
{
	private struct CharTask
	{
		public char? From;

		public char? To;

		public char? Current;

		public int StartFrame;

		public int EndFrame;
	}

	private static readonly char[] Chars = "~!@#$%^&*()_+-=[];><0123456789".ToCharArray();

	private readonly System.Random m_Random = new System.Random();

	private readonly List<CharTask> m_Tasks = new List<CharTask>();

	private readonly TextScramblerParams m_Params;

	private readonly TextMeshProUGUI m_TextComponent;

	private readonly int m_FramesPerChar;

	private float m_TimeBetweenFrames;

	private int m_Frame;

	private int m_CompletedTasks;

	private float m_LastAnimateTime;

	private char[] m_Chars = Chars;

	private bool m_AnimationActive;

	public bool AnimationActive => m_AnimationActive;

	public TextScrambler(TextScramblerParams textScramblerParams)
	{
		m_Params = textScramblerParams;
		m_TextComponent = textScramblerParams.TargetText;
		m_FramesPerChar = textScramblerParams.FramesPerChar;
	}

	public void Dispose()
	{
		StopAnimation();
	}

	public void Tick()
	{
		if (m_AnimationActive && Time.unscaledTime > m_LastAnimateTime + m_TimeBetweenFrames)
		{
			Animate();
		}
	}

	public void SetText(string oldText, string newText)
	{
		StopAnimation();
		if (newText != string.Empty)
		{
			m_Chars = new char[newText.Length + Chars.Length];
			newText.ToCharArray().CopyTo(m_Chars, 0);
			Chars.CopyTo(m_Chars, newText.Length);
		}
		else
		{
			m_Chars = Chars;
			m_TextComponent.text = string.Empty;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 1;
		int num4 = 1;
		int num5 = Math.Max(oldText.Length, newText.Length);
		for (int i = 0; i < num5; i++)
		{
			int num6 = num2 + m_FramesPerChar;
			m_Tasks.Add(new CharTask
			{
				From = ((oldText.Length > i) ? new char?(oldText[i]) : null),
				To = ((newText.Length > i) ? new char?(newText[i]) : null),
				StartFrame = num2,
				EndFrame = num6
			});
			if (i >= num4)
			{
				num2 = num6;
				num3 += 2;
				num4 = i + num3;
			}
			num = Math.Max(num, num6);
		}
		StartAnimation(num, string.IsNullOrWhiteSpace(newText));
	}

	private void StartAnimation(int maxFrames, bool isEmptyText)
	{
		m_TimeBetweenFrames = m_Params.Duration / (float)maxFrames;
		m_Frame = (m_CompletedTasks = 0);
		m_AnimationActive = true;
		if (!isEmptyText && m_TextComponent.gameObject.activeInHierarchy)
		{
			UISounds.Instance.Sounds.ScrambledText.ScrambledTextLoopStart.Play();
		}
	}

	private void StopAnimation()
	{
		UISounds.Instance.Sounds.ScrambledText.ScrambledTextLoopStop.Play();
		m_AnimationActive = false;
		m_Tasks.Clear();
	}

	private void Animate()
	{
		m_LastAnimateTime = Time.unscaledTime;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CharTask task in m_Tasks)
		{
			CharTask current = task;
			if (m_Frame >= current.EndFrame)
			{
				if (m_Frame == current.EndFrame)
				{
					m_CompletedTasks++;
				}
				if (current.To.HasValue)
				{
					stringBuilder.Append(current.To);
				}
			}
			else if (m_Frame >= current.StartFrame)
			{
				if (!current.Current.HasValue || m_Random.Next(100) < 30)
				{
					current.Current = m_Chars[m_Random.Next(m_Chars.Length)];
				}
				stringBuilder.Append(current.Current);
			}
			else if (current.From.HasValue)
			{
				stringBuilder.Append(current.From);
			}
		}
		m_TextComponent.text = stringBuilder.ToString();
		if (m_CompletedTasks == m_Tasks.Count)
		{
			StopAnimation();
		}
		else
		{
			m_Frame++;
		}
	}
}
