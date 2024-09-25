using System;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.ProfilingCounters;
using UnityEngine;

namespace Owlcat.Core.Overlays;

public class Label : OverlayElement
{
	public enum Severity
	{
		Info,
		Warning,
		Error
	}

	public string Caption;

	public Func<string> GetText;

	[CanBeNull]
	public Func<Severity> GetSeverity;

	public Counter Counter;

	public bool AddSeparator;

	public float Draw(float top)
	{
		string text = GetText?.Invoke() ?? GetCounterText();
		string text2 = Caption + ": " + text;
		int num = text2.Length * 8;
		Rect position = new Rect(0f, top, num, 19f);
		Color color = GUI.color;
		if (OverlayService.Instance.DarkenBackground)
		{
			GUI.color = new Color(0f, 0f, 0f, 0.5f);
			GUI.DrawTexture(position, Texture2D.whiteTexture);
			GUI.color = color;
		}
		if (IsError())
		{
			GUI.color = Color.red;
		}
		else if (IsWarning())
		{
			GUI.color = Color.yellow;
		}
		GUI.Label(position, text2);
		GUI.color = color;
		return num;
	}

	private bool IsError()
	{
		if (Counter == null || !(Counter.GetMedian() > Counter.WarningLevel))
		{
			Func<Severity> getSeverity = GetSeverity;
			if (getSeverity == null)
			{
				return false;
			}
			return getSeverity() == Severity.Error;
		}
		return true;
	}

	private bool IsWarning()
	{
		Func<Severity> getSeverity = GetSeverity;
		if (getSeverity == null)
		{
			return false;
		}
		return getSeverity() == Severity.Warning;
	}

	private string GetCounterText()
	{
		if (Counter == null)
		{
			return "-";
		}
		return Counter.GetMedian().ToString("0.00");
	}

	public Label(string name, Func<string> textGetter, Func<Severity> severityGetter = null)
		: base(name)
	{
		Caption = name;
		GetText = textGetter;
		GetSeverity = severityGetter;
	}

	public Label(Counter c)
		: base(c.Name)
	{
		Caption = c.Name;
		Counter = c;
	}
}
