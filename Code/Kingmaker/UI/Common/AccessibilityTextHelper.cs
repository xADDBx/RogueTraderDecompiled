using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Settings;
using TMPro;

namespace Kingmaker.UI.Common;

public class AccessibilityTextHelper : IDisposable
{
	private class TextData
	{
		public readonly float TextInitialSize;

		public readonly float AutoSizeMin;

		public readonly float AutoSizeMax;

		public TextData(TMP_Text text)
		{
			TextInitialSize = text.fontSize;
			AutoSizeMin = text.fontSizeMin;
			AutoSizeMax = text.fontSizeMax;
		}

		public TextData(float textInitialSize, float autoSizeMin, float autoSizeMax)
		{
			TextInitialSize = textInitialSize;
			AutoSizeMin = autoSizeMin;
			AutoSizeMax = autoSizeMax;
		}
	}

	private readonly Dictionary<TMP_Text, TextData> m_TextToInitSizeMap;

	private bool m_IsUpdated;

	public AccessibilityTextHelper(params TMP_Text[] texts)
	{
		m_TextToInitSizeMap = new Dictionary<TMP_Text, TextData>();
		foreach (TMP_Text tMP_Text in texts)
		{
			if (!(tMP_Text == null))
			{
				m_TextToInitSizeMap.TryAdd(tMP_Text, new TextData(tMP_Text));
			}
		}
	}

	public void AppendTexts(params TMP_Text[] texts)
	{
		List<KeyValuePair<TMP_Text, TextData>> list = new List<KeyValuePair<TMP_Text, TextData>>();
		foreach (TMP_Text tMP_Text in texts)
		{
			if (!(tMP_Text == null))
			{
				KeyValuePair<TMP_Text, TextData> item = new KeyValuePair<TMP_Text, TextData>(tMP_Text, new TextData(tMP_Text));
				if (m_TextToInitSizeMap.TryAdd(item.Key, item.Value))
				{
					list.Add(item);
				}
			}
		}
		if (m_IsUpdated)
		{
			UpdateTextInternal(list);
		}
	}

	public void UpdateTextSize()
	{
		if (!m_IsUpdated)
		{
			UpdateTextInternal(m_TextToInitSizeMap.ToList());
			m_IsUpdated = true;
		}
	}

	private void UpdateTextInternal(List<KeyValuePair<TMP_Text, TextData>> textsToUpdate)
	{
		float fontSizeMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		foreach (KeyValuePair<TMP_Text, TextData> item in textsToUpdate)
		{
			item.Deconstruct(out var key, out var value);
			TMP_Text tMP_Text = key;
			TextData textData = value;
			tMP_Text.fontSizeMax = textData.AutoSizeMax * fontSizeMultiplier;
			tMP_Text.fontSizeMin = textData.AutoSizeMin * fontSizeMultiplier;
			tMP_Text.fontSize = textData.TextInitialSize * fontSizeMultiplier;
		}
	}

	public void Dispose()
	{
		m_IsUpdated = false;
		foreach (KeyValuePair<TMP_Text, TextData> item in m_TextToInitSizeMap)
		{
			item.Deconstruct(out var key, out var value);
			TMP_Text tMP_Text = key;
			TextData textData = value;
			tMP_Text.fontSizeMax = textData.AutoSizeMax;
			tMP_Text.fontSizeMin = textData.AutoSizeMin;
			tMP_Text.fontSize = textData.TextInitialSize;
		}
	}
}
