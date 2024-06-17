using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickNonStackEntityView : MonoBehaviour
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	private float m_FontMultiplier;

	public void Initialize(TooltipBrickNonStackVm.NonStackEntity entity)
	{
		m_Icon.sprite = entity.Icon;
		m_Title.text = entity.Name;
		m_FontMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}
}
