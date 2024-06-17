using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockTextPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockTextVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = base.ViewModel.Text;
		AddDisposable(m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
		m_Text.text = string.Empty;
		base.DestroyViewImplementation();
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_Text };
	}
}
