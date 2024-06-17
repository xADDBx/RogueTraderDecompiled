using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockClassProgressionPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockClassProgressionVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Description.text = base.ViewModel.Description;
		AddDisposable(m_Description.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_Description };
	}
}
