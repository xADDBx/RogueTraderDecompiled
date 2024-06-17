using Kingmaker.Blueprints.Root;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.Common;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GlossaryColorToDigitalOverrider : MonoBehaviour
{
	private TextMeshProUGUI m_Text;

	private bool m_IsDirty;

	private string m_TextToProcess;

	private GlossaryColors PaperColors => BlueprintRoot.Instance.UIConfig.PaperGlossaryColors;

	private GlossaryColors DigitalColors => BlueprintRoot.Instance.UIConfig.DigitalGlossaryColors;

	private void Awake()
	{
		m_Text = GetComponent<TextMeshProUGUI>();
		m_Text.OnPreRenderText += TextOnOnPreRenderText;
	}

	private void OnDestroy()
	{
		m_Text.OnPreRenderText -= TextOnOnPreRenderText;
	}

	private void TextOnOnPreRenderText(TMP_TextInfo textInfo)
	{
		if (m_TextToProcess != textInfo.textComponent.text && textInfo.textComponent.text.Contains("<color"))
		{
			textInfo.textComponent.text = textInfo.textComponent.text.Replace(PaperColors.GlossaryEmptyHTML, DigitalColors.GlossaryEmptyHTML).Replace(PaperColors.GlossaryDecisionsHTML, DigitalColors.GlossaryDecisionsHTML).Replace(PaperColors.GlossaryDefaultHTML, DigitalColors.GlossaryDefaultHTML)
				.Replace(PaperColors.GlossaryMechanicsHTML, DigitalColors.GlossaryMechanicsHTML)
				.Replace(PaperColors.GlossaryGlossaryHTML, DigitalColors.GlossaryGlossaryHTML);
			m_TextToProcess = textInfo.textComponent.text;
			m_Text.SetAllDirty();
			m_IsDirty = true;
		}
	}

	private void LateUpdate()
	{
		if (m_IsDirty)
		{
			m_Text.enabled = false;
			m_Text.enabled = true;
			m_IsDirty = false;
		}
	}
}
