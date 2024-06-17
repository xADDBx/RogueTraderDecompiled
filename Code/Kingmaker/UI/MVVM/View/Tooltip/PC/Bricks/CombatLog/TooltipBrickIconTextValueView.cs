using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickIconTextValueView : TooltipBrickCombatLogBaseView<TooltipBrickIconTextValueVM>
{
	[Space]
	[SerializeField]
	private TextMeshProUGUI m_ValueText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ValueText.text = base.ViewModel.Value;
		TextHelper.AppendTexts(m_ValueText);
	}
}
