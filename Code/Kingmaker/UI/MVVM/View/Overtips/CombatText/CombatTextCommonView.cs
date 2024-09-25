using DG.Tweening;
using Kingmaker.UI.MVVM.VM.Overtips.CombatText;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Overtips.CombatText;

public class CombatTextCommonView : CombatTextEntityBaseView<CombatMessageBase>
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_IconContainer;

	[SerializeField]
	private CanvasGroup m_IconCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Color m_DefaultColor;

	[SerializeField]
	private float m_Spacing = 6f;

	protected override float GetXPos()
	{
		if (!(m_Icon.sprite != null))
		{
			return 0f;
		}
		return (m_IconContainer.transform as RectTransform).rect.width / 2f + m_Spacing;
	}

	public override string GetText()
	{
		return m_Text.text;
	}

	protected override void DoData(CombatMessageBase combatMessage)
	{
		m_Text.text = combatMessage.GetText();
		m_Text.color = combatMessage.GetColor() ?? m_DefaultColor;
		base.Rect.sizeDelta = new Vector2(m_Text.preferredWidth + (m_IconContainer.transform as RectTransform).rect.width + m_Spacing * 2f, base.Rect.rect.height);
		m_Icon.sprite = combatMessage.GetSprite();
		m_Icon.color = ((m_Icon.sprite != null) ? Color.white : Color.clear);
		m_IconCanvasGroup.alpha = ((m_Icon.sprite != null) ? 1f : 0f);
	}

	protected override void DoShow()
	{
		base.CanvasGroup.alpha = 0f;
		base.CanvasGroup.DOFade(1f, ShowFadeTime).SetUpdate(isIndependentUpdate: true);
	}
}
