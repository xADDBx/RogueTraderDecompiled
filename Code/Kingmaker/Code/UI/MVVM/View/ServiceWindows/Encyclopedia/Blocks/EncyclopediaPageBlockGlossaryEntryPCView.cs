using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockGlossaryEntryPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockGlossaryEntryVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private Color m_MarkColor = new Color(1f, 1f, 0f, 0.3f);

	[SerializeField]
	private float m_DefaultFontSizeTitle = 24f;

	[SerializeField]
	private float m_DefaultFontSizeDescription = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeDescription = 21f;

	private const string MarkFormat = "<mark=#{0}>{1}</mark>";

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		string text = "<sprite=0>" + base.ViewModel.Title + "<rotate=180><sprite=0>";
		m_Title.text = (base.ViewModel.Marked ? $"<mark=#{ColorUtility.ToHtmlStringRGBA(m_MarkColor)}>{text}</mark>" : text);
		m_Description.text = base.ViewModel.Description;
		AddDisposable(m_Title.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_Description.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Title.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * base.ViewModel.FontMultiplier;
		m_Description.fontSize = (isControllerMouse ? m_DefaultFontSizeDescription : m_DefaultConsoleFontSizeDescription) * base.ViewModel.FontMultiplier;
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_Title, m_Description };
	}
}
