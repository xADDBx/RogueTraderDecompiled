using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPlanetTraitsView : ViewBase<TooltipBrickPlanetTraitsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 18f;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public void Initialize(string title, string description)
	{
		base.gameObject.SetActive(value: true);
		m_Title.text = title;
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Title.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * FontSizeMultiplier;
	}

	protected override void BindViewImplementation()
	{
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		WidgetFactory.DisposeWidget(this);
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
