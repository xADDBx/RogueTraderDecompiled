using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog.Additional;

public class RollSlider : MonoBehaviour
{
	[Header("Slider")]
	[SerializeField]
	private Slider m_SufficientValueSlider;

	[SerializeField]
	private Slider m_CurrentValueSlider;

	[SerializeField]
	private TextMeshProUGUI m_ChanceValueText;

	[SerializeField]
	private Image m_ResultSignImage;

	[SerializeField]
	private Image m_CurrentHandleImage;

	[Header("Sprites")]
	[Space]
	[SerializeField]
	private Sprite m_ResultSignSuccessSprite;

	[SerializeField]
	private Sprite m_ResultSignFailedSprite;

	[Space]
	[SerializeField]
	private Sprite m_HandleSuccessSprite;

	[SerializeField]
	private Sprite m_HandleFailedSprite;

	[Header("Colors")]
	[SerializeField]
	private Color m_OrangeColor;

	[SerializeField]
	private Color m_BlueColor;

	[SerializeField]
	private Color m_LightColor;

	public void SetData(int sufficientValue, int? currentValue)
	{
		m_CurrentValueSlider.gameObject.SetActive(currentValue.HasValue);
		m_ResultSignImage.gameObject.SetActive(currentValue.HasValue);
		m_SufficientValueSlider.value = sufficientValue;
		m_CurrentValueSlider.value = (currentValue.HasValue ? currentValue.Value : 0);
		string text = ((currentValue == sufficientValue) ? "=" : ((currentValue < sufficientValue) ? "<" : ">"));
		string text2 = ((!currentValue.HasValue) ? $"<color=#{ColorUtility.ToHtmlStringRGB(m_OrangeColor)}>{sufficientValue}%</color>" : $"<color=#{ColorUtility.ToHtmlStringRGB(m_BlueColor)}>{currentValue.Value}</color> <color=#{ColorUtility.ToHtmlStringRGB(m_OrangeColor)}>{text} {sufficientValue}%</color>");
		m_ChanceValueText.text = text2;
		bool flag = currentValue <= sufficientValue;
		m_ResultSignImage.sprite = (flag ? m_ResultSignSuccessSprite : m_ResultSignFailedSprite);
		m_ResultSignImage.color = (flag ? m_OrangeColor : m_LightColor);
		m_CurrentHandleImage.sprite = (flag ? m_HandleSuccessSprite : m_HandleFailedSprite);
	}
}
