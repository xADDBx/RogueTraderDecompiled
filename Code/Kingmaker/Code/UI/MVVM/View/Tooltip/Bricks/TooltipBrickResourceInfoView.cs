using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickResourceInfoView : TooltipBaseBrickView<TooltipBrickResourceInfoVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private float m_DefaultFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultFontSizeCount = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeCount = 20f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Image.sprite = base.ViewModel.Icon;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		if (m_Text != null)
		{
			m_Text.text = base.ViewModel.Name;
			m_Text.fontSize = (isControllerMouse ? m_DefaultFontSizeText : m_DefaultConsoleFontSizeText) * FontMultiplier;
		}
		if (m_Count != null)
		{
			TextMeshProUGUI count = m_Count;
			int count2 = base.ViewModel.Count;
			count.text = count2.ToString();
			m_Count.fontSize = (isControllerMouse ? m_DefaultFontSizeCount : m_DefaultConsoleFontSizeCount) * FontMultiplier;
		}
	}
}
