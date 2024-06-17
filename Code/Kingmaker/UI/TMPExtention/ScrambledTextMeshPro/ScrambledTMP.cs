using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScrambledTMP : MonoBehaviour
{
	[SerializeField]
	private float m_Duration = 1f;

	[SerializeField]
	private int m_FramesPerChar = 10;

	private TextMeshProUGUI m_TextComponent;

	private TextScrambler m_TextScrambler;

	public string Text => m_TextComponent.Or(null)?.text ?? string.Empty;

	public void Initialize()
	{
		if (!(m_TextComponent != null) || m_TextScrambler == null)
		{
			m_TextComponent = GetComponent<TextMeshProUGUI>();
			m_TextScrambler = new TextScrambler(new TextScramblerParams
			{
				TargetText = m_TextComponent,
				Duration = m_Duration,
				FramesPerChar = m_FramesPerChar
			});
		}
	}

	private void Update()
	{
		m_TextScrambler?.Tick();
	}

	public void OnDisable()
	{
		StopText();
	}

	public void OnDestroy()
	{
		StopText();
	}

	public void SetText(string startText, string endText)
	{
		Initialize();
		m_TextScrambler?.SetText(startText, endText);
	}

	public void StopText()
	{
		m_TextScrambler?.Dispose();
	}
}
